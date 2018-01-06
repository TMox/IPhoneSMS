using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Diagnostics.Trace;

namespace iPhoneSMS
{   
    public class iPhoneBackup
    {
        // note--these are the fields I use; if you need more, add them
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public string Note { get; set; }
            public DateTime Birthday { get; set; }
            public string JobTitle { get; set; }
            public string Organization { get; set; }
            public List<(string type, string label, string value)> Contacts { get; set; } = new List<(string type, string label, string value)>();
            public string FullString
            {
                get
                {
                    return string.Format("{0}{3}{1}{4}{2}", FirstName, LastName, dumpcontacts(), string.IsNullOrEmpty(FirstName) ? "" : " ", string.IsNullOrEmpty(LastName) ? "" : " ");
                }
            }
            string dumpcontacts()
            {
                return string.Join(",", from c in Contacts select string.Format("{0}:{1}:{2}", c.type, c.label, c.value));
            }
            public override string ToString()
            {
                return string.IsNullOrEmpty(FirstName) ? LastName : FirstName;
            }
        }

        public class Message
        {
            public string Subject { get; set; }
            public string Text { get; set; }
            public bool Incoming { get; set; }
            public string Type { get; set; }
            public DateTime Timestamp { get; set; }
            public override string ToString()
            {
                return Timestamp.ToShortDateString();
            }
        }

        public class MessageGroup
        {
            public List<string> Ids { get; set; } = new List<string>();
            public long ChatId { get; set; }
            string _join = null;
            public static Func<List<string>, string> ToStringFilter { get; set; } = null;
            public string Display
            {
                get
                {
                    return ToString();
                }
            }
            public override string ToString()
            {
                if (_join == null)
                {
                    if (ToStringFilter == null)
                        _join = string.Join(", ", Ids);
                    else
                        _join = ToStringFilter(Ids);
                }
                return _join;
            }
        }

        public iPhoneBackup()
        {
            BackupRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Apple Computer\\MobileSync\\Backup");
        }

        public string FileDate { get; set; }
        public string BackupRoot { get; set; }
        public string DatabaseRoot
        {
            get
            {
                return Path.Combine(BackupRoot, DatabaseFolder);
            }
        }
        public string DatabaseFolder { get; set; }

/*
    SMS	        sms.db	                3d0d7e5fb2ce288813306e4d4636395e047a3d28
    Contacts	AddressBook.sqlitedb	31bb7ba8914766d4ba40d6dfb6113c8b614be442
    Calendar	Calendar.sqlitedb	    2041457d5fe04d39d0ab481178355df6781e6858
    Reminders	Calendar.sqlitedb	    2041457d5fe04d39d0ab481178355df6781e6858
    Notes	    notes.sqlite	        ca3bc056d4da0bbf88b5fb3be254f3b7147e639c
    Call hist.	call_history.db	        2b2b0084a1bc3a5ac8c27afdf14afb42c61a19ca
    Locations   consolidated.db	        4096c9ec676f2847dc283405900e284a7c815836
*/
        readonly Dictionary<string, string> _fileNames = new Dictionary<string, string>
        {
            { "SMS", "3d0d7e5fb2ce288813306e4d4636395e047a3d28" },
            { "Contacts", "31bb7ba8914766d4ba40d6dfb6113c8b614be442" },
        };


        public List<Person> GetAddressList()
        {
            string file = _fileNames["Contacts"];
            string path = Path.Combine(DatabaseRoot, file.Substring(0, 2));
            file = Path.Combine(path, file);
            Sqlitedb backup = new Sqlitedb(file);

            //// getting everything at once in a complicated query is barely quicker than 
            //// getting each table separately an splicing the data together, so I elect to do it the easier way
            //// the time difference, in my situation, is 200 ms for this query (unfinished) and 260 ms for getting all the talbes
            //// since the value labels don't have an index, just a row number, we can't query that directly
            //// instead, we have to generate a lookup table and use that outside of the query
            //string sql = @"Select trim(person.First) as First, trim(person.Last) as Last, 
            //                trim(person.Middle) as Middle, trim(person.Organization) as Organization, 
            //                person.Birthday, trim(person.JobTitle) as JobTitle, trim(person.Note) as Note,
            //                (Select Group_Concat(
            //                    Case value.property 
            //                        When 3 Then 'phone'
            //                        When 4 Then 'email'
            //                        When 5 Then 'address'
            //                        When 22 Then 'homepage'
            //                    End || ':' || value.label || ':' || value.value
            //                )
            //                From ABMultiValue value
            //                Where value.record_id = person.ROWID
            //                Group By record_id) as type
            //                From ABPerson person;";
            //DataTable dt = backup.Query(sql);
            //Sqlitedb.Table labels = backup.GetTableData("ABMultiValueLabel");
            //Sqlitedb.Table adrLbls = backup.GetTableData("ABMultiValueEntryKey");
            //List<string> lText = (from l in labels.Items select Strip(l["value"] as string)).ToList();
            //List<string> lAddress = (from l in adrLbls.Items select l["value"] as string).ToList();
            //return new List<Person>();

            Sqlitedb.Table people = backup.GetTableData("ABPerson");
            Sqlitedb.Table values = backup.GetTableData("ABMultiValue");
            Sqlitedb.Table labels = backup.GetTableData("ABMultiValueLabel");
            Sqlitedb.Table adrLbls = backup.GetTableData("ABMultiValueEntryKey");
            Sqlitedb.Table addresses = backup.GetTableData("ABMultiValueEntry");


            // get a list of labels for a Person's Contacts
            List<string> lText = (from l in labels.Items select Strip(l["value"] as string)).ToList();
            List<string> lAddress = (from l in adrLbls.Items select l["value"] as string).ToList();
            const int PHONE = 3;
            const int EMAIL = 4;
            const int ADDRESS = 5;
            const int HOMEPAGE = 22;

            List<Person> pList = new List<Person>(people.Items.Count);
            long id;
            Person p;
            object tmp;
            int i;
            string type;
            foreach (var item in people.Items)
            {
                p = new Person();
                id = (long)item["ROWID"];
                p.FirstName = item["First"] as string;
                if (!string.IsNullOrEmpty(p.FirstName))
                    p.FirstName = p.FirstName.Trim();
                p.LastName = item["Last"] as string;
                if (!string.IsNullOrEmpty(p.LastName))
                    p.LastName = p.LastName.Trim();
                p.MiddleName = item["Middle"] as string;
                if (!string.IsNullOrEmpty(p.MiddleName))
                    p.MiddleName = p.MiddleName.Trim();
                p.Organization = item["Organization"] as string;
                if (!string.IsNullOrEmpty(p.Organization))
                    p.Organization = p.Organization.Trim();
                if (!((tmp = item["Birthday"]) is DBNull))
                    p.Birthday = new DateTime(2001, 1, 1, 0, 0, 0).AddSeconds((long)Convert.ToDouble(tmp));
                p.JobTitle = item["JobTitle"] as string;
                if (!string.IsNullOrEmpty(p.JobTitle))
                    p.JobTitle = p.JobTitle.Trim();
                p.Note = item["Note"] as string;
                if (!string.IsNullOrEmpty(p.Note))
                    p.Note = p.Note.Trim();
                foreach (var val in values.Items.Where(q => (long)q["record_id"] == id))
                {
                    tmp = (long)val["property"];
                    switch ((long)tmp)
                    {
                        case HOMEPAGE:
                            type = "homepage";
                            break;
                        case PHONE:
                            type = "phone";
                            break;
                        case EMAIL:
                            type = "email";
                            break;
                        case ADDRESS:
                            type = "address";
                            break;
                        default:
                            continue;
                    }
                    tmp = val["label"];
                    i = tmp is DBNull ? -1 : (int)((long)tmp - 1);
                    tmp = val["value"];
                    if (tmp is DBNull)
                        tmp = string.Join(";", from v in addresses.Items where (long)v["parent_id"] == (long)val["UID"] select string.Format("{0}:{1}", lAddress[((int)(long)v["key"]) - 1], v["value"]));
                    else
                        tmp = (tmp as string).Trim();
                    p.Contacts.Add((type, (long)i == -1 ? "" : lText[i], handle(type, tmp as string)));
                }
                pList.Add(p);
            }
            return pList;
        }

        Regex _handleReplace = new Regex(@"[\s()-]");
        string handle(string type, string value)
        {
            if (type != "phone")
                return value;
            // convert phone number into a message "handle" which is +lllll
            var s =  (value.StartsWith("+") ? "" : "+1") + _handleReplace.Replace(value, "");
            return s;
        }

        string Strip(string s)
        {
            int i = s.IndexOf("<");
            int j = s.IndexOf(">");
            if (i == -1 || j == -1)
                return s;
            return s.Substring(i + 1, j - i - 1);
        }


        public List<MessageGroup> GetChats()
        {
            string file = _fileNames["SMS"];
            string path = Path.Combine(DatabaseRoot, file.Substring(0, 2));
            file = Path.Combine(path, file);
            Sqlitedb backup = new Sqlitedb(file);
            string sql = @"
SELECT h.id,chj.chat_id,
 Case When cmj.message_date > 1000000000 
  Then cmj.message_date / 1000000000 
  Else cmj.message_date 
 End as date
From chat_handle_join chj
Inner Join handle h on h.ROWID = chj.handle_id
Inner Join chat_message_join cmj on cmj.chat_id = chj.chat_id
Group By chj.chat_id,h.id
Order By date Desc;";
            //string sql = @"SELECT h.id,chj.chat_id,cmj.message_date as date
            //                From chat_handle_join chj
            //                Inner Join handle h on h.ROWID = chj.handle_id
            //                Inner Join chat_message_join cmj on cmj.chat_id = chj.chat_id
            //                Group By chj.chat_id
            //                Order By date Desc;"; 
            DataRowCollection rows = backup.Query(sql).Rows;
            List<MessageGroup> groups = new List<MessageGroup>();
            MessageGroup group = null;
            long chatId = -1;
            foreach (DataRow item in rows)
            {
                if ((long)item[1] != chatId)
                {
                    chatId = (long)item[1];
                    group = new MessageGroup() { ChatId = chatId };
                    groups.Add(group);
                }
                group.Ids.Add(item[0] as string);
            }
            //groups.Sort((a, b) => a.Ids[0].CompareTo(b.Ids[0]));
            //bugbug--this isn't needed, I'm just using it so the output text file will be in orthagraphical order (comparing to old output)
            //groups.Sort((a, b) => a.ToString().CompareTo(b.ToString()));
            return groups;
        }

        public List<Message> GetMessages(long chatId, string group = null)
        {
            string file = _fileNames["SMS"];
            string path = Path.Combine(DatabaseRoot, file.Substring(0, 2));
            file = Path.Combine(path, file);
            Sqlitedb backup = new Sqlitedb(file);
 
            // select the data
            // a problem with is original query: it assumes that a chatgroup has ids ordered the same way every time, which may not be true
            //string sql = "SELECT cm.chat_id, (SELECT GROUP_CONCAT(h.id) FROM chat_handle_join ch " +
            //    "INNER JOIN handle h on h.ROWID = ch.handle_id " +
            //    "WHERE ch.chat_id = cm.chat_id " +
            //    "GROUP BY ch.chat_id) as chatgroup, m.date, " + //datetime(m.date+978307200,\"unixepoch\",\"localtime\") as date, " +
            //        "m.service, CASE m.is_from_me WHEN 1 THEN \"SENT\" WHEN 0 THEN \"RCVD\" END as direction, h.id, " +
            //        "CASE m.type WHEN 1 THEN \"GROUP\" WHEN 0 THEN \"1 - ON - 1\" END as type, replace(m.text,cast(X'EFBFBC' as text),\"[MEDIA]\") as text, " +
            //        "(SELECT GROUP_CONCAT(\"MediaDomain-\"||substr(a.filename,3)) FROM message_attachment_join ma " +
            //    "JOIN attachment a ON ma.attachment_id = a.ROWID WHERE ma.message_id = m.ROWID GROUP BY ma.message_id) as filereflist " +
            //    "FROM chat_message_join cm INNER JOIN message m ON cm.message_id = m.ROWID INNER JOIN handle h ON m.handle_id = h.ROWID " +
            //    "WHERE chatgroup = \"" + group + "\" ORDER BY date";
            //DataRowCollection messages = backup.Query(sql).Rows;
            //return (from DataRow m in messages select new Message { Text = m["text"] as string, Type = m["service"] as string, Incoming = m["direction"].Equals("RCVD"), Timestamp = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime().AddMilliseconds((long)m["date"] / 1000000) }).ToList();

            string sql = string.Format("Select cmj.chat_id, m.ROWID as id, m.service, m.is_from_me, m.date, replace(m.text,cast(X'EFBFBC' as text),\"[MEDIA]\") as text, CASE m.type WHEN 1 THEN \"GROUP\" WHEN 0 THEN \"1 - ON - 1\" END as type, " +
                    "(SELECT GROUP_CONCAT(\"MediaDomain-\"||substr(a.filename,3)) FROM message_attachment_join maj " +
                     "JOIN attachment a ON maj.attachment_id = a.ROWID WHERE maj.message_id = m.ROWID GROUP BY maj.message_id) as filereflist " +
                    "From chat_message_join cmj " +
                    "Inner Join message m On m.ROWID = cmj.message_id " +
                    "Where cmj.chat_id = {0} " + 
                    "Order By m.date;", chatId);
            DataRowCollection messages = backup.Query(sql).Rows;
            return (from DataRow m in messages select new Message { Text = m["text"] as string, Type = m["service"] as string, Incoming = (long)m["is_from_me"] == 0, Timestamp = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime().AddMilliseconds((long)m["date"] / 1000000) }).ToList();
        }

        public List<Message> GetAllMessages()
        {
            string file = _fileNames["SMS"];
            string path = Path.Combine(DatabaseRoot, file.Substring(0, 2));
            file = Path.Combine(path, file);
            Sqlitedb backup = new Sqlitedb(file);
            Sqlitedb.Table messages = backup.GetTableData("message");
            return (from m in messages.Items select new Message { Text = m["text"] as string, Subject = m["subject"] as string, Type = m["service"] as string, Incoming = ((long)m["is_from_me"] == 0), Timestamp = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime().AddMilliseconds((long)m["date"] / 1000000) }).ToList();
        }
    }
}
