using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using iPhoneSMS;
using static System.Diagnostics.Trace;
using System.Text;
using System.Diagnostics;

namespace iPhoneMessageExport
{
    public partial class Form1 : Form
    {
        /* GLOBAL variables */
        //bool _formInitialized = false;
        iPhoneBackup _backup;
        List<iPhoneBackup.MessageGroup> _chats;
        List<iPhoneBackup.Person> _people;
        Dictionary<string, string> _contacts;
        DataTable dtMessageFiles;
        string dbFile = null;
        string dbFileDate = null;
        string dbFileDir = null;
        //string messageGroup = null;
        //string htmlFile = null;
        string backupPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Apple Computer\MobileSync\Backup";
        string formTitle = null;

        /// <summary>
        /// Returns DataTable of iPhone message backup files given iPhone backup directory.
        /// </summary>
        /// <param name="dirBackup"></param>
        /// <returns></returns>
        private DataTable getBackupFiles(DirectoryInfo dirBackup)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Timestamp", typeof(int));
            dt.Columns.Add("Path", typeof(string));
            dt.Columns.Add("FullDate", typeof(string));
            dt.DefaultView.Sort = "Timestamp DESC";

            DirectoryInfo[] dirBackups = dirBackup.GetDirectories("*.", SearchOption.TopDirectoryOnly);
            //FileInfo[] files = null;
            foreach (DirectoryInfo dir in dirBackups)
            {
                dt.Rows.Add(MiscUtil.datetimeToTimestamp(dir.CreationTime), dir.FullName, dir.CreationTime.ToString("f"));
                //// check that it contains the messages file (3d0d7e5fb2ce288813306e4d4636395e047a3d28)
                ////tmox: file not in top folder, so descend
                //files = dir.GetFiles("3d0d7e5fb2ce288813306e4d4636395e047a3d28", SearchOption.AllDirectories);
                //if (files != null)
                //{
                //    foreach (System.IO.FileInfo fi in files)
                //    {
                //        // Unix Timestamp will overflow in the year 2038
                //        dt.Rows.Add(MiscUtil.datetimeToTimestamp(fi.CreationTime), fi.FullName, fi.CreationTime.ToString("f"));
                //    }
                //}
            }
            return dt;
        }

        /// <summary>
        /// Returns DataTable of MessageGroups from iPhone backup file.
        /// </summary>
        /// <param name="dbFile"></param>
        /// <returns></returns>
        private List<iPhoneBackup.MessageGroup> getMessageGroupsFromFile(string dbFile)
        {
            //DataTable dt = new DataTable();
            //dt.Columns.Add("Value", typeof(string));
            //dt.Columns.Add("Display", typeof(string));
            //dt.DefaultView.Sort = "Value ASC";

            List<iPhoneBackup.MessageGroup> groups = new List<iPhoneBackup.MessageGroup>();
            if (dbFile != null)
            {
                // open SQLite data file
                Stopwatch sw = Stopwatch.StartNew();
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;Read Only=True;FailIfMissing=True;");
                m_dbConnection.Open();

                // select the data
                // joins chat_handle_join to handle on ch.handle_id = h.ROWID
                // joins chat_handle_join to chat_message_join via where clause (chj.chat_id = cmj.chat_id) grouped on chj.chat_id
                // joins chat_message_join to message on cmj.message_id = m.ROWID
                // joins message to handle on m.handle_id = h.ROWID
                // h.ROWID <- chj.handle_id,chj.chat_id -> cmj.chat_id,cmj.message_id -> m.ROWID,m.handle_id -> h.ROWID
                string sql = "SELECT DISTINCT (SELECT GROUP_CONCAT(h.id) FROM chat_handle_join ch INNER JOIN handle h on h.ROWID = ch.handle_id " +
                    "WHERE ch.chat_id = cm.chat_id Group By ch.chat_id) as chatgroup, cm.chat_id FROM chat_message_join cm " +
                    "INNER JOIN message m ON cm.message_id = m.ROWID INNER JOIN handle h ON m.handle_id = h.ROWID ORDER BY chatgroup;";
                //sql = //"SELECT " + 
                //        "SELECT Distinct GROUP_CONCAT(h.id) As chatgroup " +
                //        "FROM chat_handle_join chj " +
                //        "INNER JOIN handle h On h.ROWID = chj.handle_id " +
                //        //"WHERE chj.chat_id = cmj.chat_id " + 
                //        "Inner Join chat_message_join cmj On cmj.chat_id = chj.chat_id " +
                //        "GROUP BY chj.chat_id " +
                //    //"FROM chat_message_join cmj " +
                //    //"INNER JOIN message m ON cmj.message_id = m.ROWID " +
                //    //"INNER JOIN handle h ON m.handle_id = h.ROWID " + 
                //    "ORDER BY chatgroup;";

                // these are all the groups, even those with no messages
                // it's at least 3 times faster
                //sql = "SELECT GROUP_CONCAT(h.id) as ChatGroup " +
                //        "From chat_handle_join chj " +
                //        "Inner Join handle h on h.ROWID = chj.handle_id " +
                //        "Group By chj.chat_id " +
                //        "Order By ChatGroup;";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader row = command.ExecuteReader();
                while (row.Read())
                {
                    if (row["chatgroup"].ToString().Trim() != "")
                    {
                        List<string> ids = (row["chatgroup"] as string).Split(",".ToCharArray()).ToList();
                        //ids.Sort();
                        // add and prettify US 11-digit phone numbers
                        groups.Add(new iPhoneBackup.MessageGroup() { ChatId = (long)row["chat_id"], Ids = ids });
                        //dt.Rows.Add(row["chatgroup"] as string, Regex.Replace(row["chatgroup"].ToString(), @"\+1(\d{3})(\d{3})(\d{4})\b", "($1)$2-$3"));
                    }
                }
                row.Close();
                m_dbConnection.Close();
                TraceInformation("groups: {0} ms", sw.ElapsedMilliseconds);
            }
            return groups;
        }

        //string Reorder(string s)
        //{
        //    List<string> t = s.Split(",".ToCharArray()).ToList();
        //    t.Sort();
        //    return string.Join(", ", t);
        //}

        /// <summary>
        /// 
        /// </summary>
        private void PreviewGroup(iPhoneBackup.MessageGroup group)
        {
            if (_backup == null)
                return;

            bool isGroupMessage = group.Ids.Count > 1;
            lbPreview.Items.Clear();

            Stopwatch sw = Stopwatch.StartNew();
            List<iPhoneBackup.Message> messages = _backup.GetMessages(group.ChatId, group.ToString());
            TraceInformation("messages: {0} ms", sw.ElapsedMilliseconds);

            lbPreview.Items.Add(string.Format("Group {0}: {1}", group.ChatId, group.ToString()));

            // fields: date, service, direction, id, text, filereflist
            List<string> list = new List<string>(messages.Count);
            foreach (var m in messages)
            {
                if (m.Incoming)
                    list.Add("< " + m.Text);
                else
                    list.Add("> " + m.Text);
            }
            lbPreview.Items.AddRange(list.ToArray());
        }

        string GroupNames(List<string> ids)
        {
            List < string > names = new List<string>(ids.Count);
            string s;
            ids.ForEach((i) => { if (!_contacts.TryGetValue(i, out s)) names.Add(i); else names.Add(s); });
            return string.Join(", ", names);
        }


        /// <summary>
        /// Export all messages for MessageGroup in backup file into HTML file. (THREAD)
        /// </summary>
        private void exportHTMLForMessageGroup(iPhoneBackup.MessageGroup grp)
        {
            StringBuilder sb = new StringBuilder();
            string group = string.Join(",", grp.Ids);
            bool isGroupMessage = (group.Contains(",")) ? true : false;

            // query database
            if (dbFile != null)
            {
                // open SQLite data file
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;Read Only=True;FailIfMissing=True;");
                m_dbConnection.Open();

                int totalMessages = 1;
                // get count of messages for progress bar
                string sql = "SELECT count(*) as count, (SELECT GROUP_CONCAT(h.id) FROM chat_handle_join ch " +
                    "INNER JOIN handle h on h.ROWID = ch.handle_id WHERE ch.chat_id = cm.chat_id GROUP BY ch.chat_id) as chatgroup " +
                    "FROM chat_message_join cm INNER JOIN message m ON cm.message_id = m.ROWID INNER JOIN handle h ON m.handle_id = h.ROWID " +
                    "WHERE chatgroup = \"" + group + "\" LIMIT 1";
                SQLiteDataAdapter adpt = new SQLiteDataAdapter(sql, m_dbConnection);
                DataSet set = new DataSet();
                adpt.Fill(set);
                totalMessages = int.Parse(set.Tables[0].Rows[0]["count"].ToString());

                // select the data
                sql = "SELECT cm.chat_id, (SELECT GROUP_CONCAT(h.id) FROM chat_handle_join ch INNER JOIN handle h on h.ROWID = ch.handle_id " +
                    "WHERE ch.chat_id = cm.chat_id GROUP BY ch.chat_id) as chatgroup, datetime(m.date+978307200,\"unixepoch\",\"localtime\") as date, " +
                    "m.service, CASE m.is_from_me WHEN 1 THEN \"SENT\" WHEN 0 THEN \"RCVD\" END as direction, h.id, " +
                    "CASE m.type WHEN 1 THEN \"GROUP\" WHEN 0 THEN \"1 - ON - 1\" END as type, replace(m.text,cast(X'EFBFBC' as text),\"[MEDIA]\") as text, " +
                    "(SELECT GROUP_CONCAT(\"MediaDomain-\"||substr(a.filename,3)) FROM message_attachment_join ma " +
                    "JOIN attachment a ON ma.attachment_id = a.ROWID WHERE ma.message_id = m.ROWID GROUP BY ma.message_id) as filereflist " +
                    "FROM chat_message_join cm INNER JOIN message m ON cm.message_id = m.ROWID INNER JOIN handle h ON m.handle_id = h.ROWID " +
                    "WHERE chatgroup = \"" + group + "\" ORDER BY date";
                set.Dispose();
                adpt.SelectCommand.CommandText = sql;
                set = new DataSet();
                adpt.Fill(set);
                Stopwatch sw = Stopwatch.StartNew();
                sb.AppendLine(iPhoneSMS.Properties.Resources.HTMLHeaders); // add html headers
                sb.AppendLine("<BODY>\n");
                sb.AppendLine("<H1>Messages from " + grp.ToString() + "</H1>\n");
                sb.AppendLine("<H2>as of " + dbFileDate + "</H2>\n");
                sb.AppendLine("<DIV id=\"messages\">\n");
                // fields: date, service, direction, id, text, filereflist
                int i = 0;
                string mFile;
                foreach (DataRow row in set.Tables[0].Rows)
                {
                    string content = (string)row["text"];
                    sb.AppendLine("<DIV class=\"message message-" + row["direction"] + "-" + row["service"] + "\">");
                    sb.AppendLine("<DIV class=\"timestamp-placeholder\"></DIV><DIV class=\"timestamp\">" + row["date"] + "</DIV>");
                    if (isGroupMessage && row["direction"].ToString() == "RCVD")
                        sb.AppendLine("<DIV class=\"sender\">" + row["id"] + "</DIV>");
                    // replace image placeholders (ï¿¼) with image files 
                    if (row["filereflist"].ToString().Length > 0)
                    {
                        List<string> mediaFileList = row["filereflist"].ToString().Split(',').ToList();
                        foreach (string mediaFile in mediaFileList)
                        {
                            string replace = null;
                            // get extension of mediaFile
                            switch (mediaFile.Substring(mediaFile.LastIndexOf('.')).ToLower())
                            {
                                // image 
                                case ".jpeg":
                                case ".jpg":
                                case ".png":
                                    mFile = MiscUtil.getSHAHash(mediaFile);
                                    mFile = mFile.Substring(0, 2) + "\\" + mFile;
                                    replace = "<img src=\"" + Path.Combine(dbFileDir, mFile) + "\"><!-- " + mediaFile + " //-->";
                                    break;
                                case ".mov":
                                    mFile = MiscUtil.getSHAHash(mediaFile);
                                    mFile = mFile.Substring(0, 2) + "\\" + mFile;
                                    replace = string.Format("<video controls='controls' width='300' name='video' src='{0}'></video>", Path.Combine(dbFileDir, mFile));
                                    break;
                                case ".vcf":
                                    mFile = MiscUtil.getSHAHash(mediaFile);
                                    mFile = mFile.Substring(0, 2) + "\\" + mFile;
                                    mFile = Path.Combine(dbFileDir, mFile);
                                    string contents = File.ReadAllText(mFile);
                                    int j = contents.IndexOf("FN:");
                                    int k = contents.IndexOf("\r", j + 3);
                                    contents = contents.Substring(j + 3, k - j - 3);
                                    replace = "Contact: " + contents;
                                    break;
                                case ".pluginpayloadattachment":
                                    mFile = MiscUtil.getSHAHash(mediaFile);
                                    mFile = mFile.Substring(0, 2) + "\\" + mFile;
                                    mFile = Path.Combine(dbFileDir, mFile);
                                    replace = string.Format("<p>Link: {0}</p>", mFile);
                                    replace = content;
                                    break;
                                default:
                                    replace = "";
                                    break;
                            }
                            // do switch statement for replacement string
                            Regex rgx = new Regex(@"\[MEDIA\]");
                            content = rgx.Replace(content, replace, 1);
                        }
                    }
                    sb.AppendLine("<DIV class=\"content\">" + content + "</DIV>\n");
                    sb.AppendLine("</DIV>\n");

                    i++;
                    backgroundWorker1.ReportProgress(i * 100 / totalMessages);
                }
                sb.AppendLine("</DIV>\n");
                sb.AppendLine("</BODY>\n");
                sb.AppendLine("</HTML>\n");

                TraceInformation("Creating html: {0} ms", sw.ElapsedMilliseconds);
                //4895
                set.Dispose();
                adpt.Dispose();
                m_dbConnection.Close();
            }

            // regex replacements
            // REGEX: change phone number format (+12257490000 => (225)749-0000)
            string htmlOutput = sb.ToString();
            htmlOutput = Regex.Replace(htmlOutput, @"\+1(\d{3})(\d{3})(\d{4})\b", "($1) $2-$3");
            // REGEX: change date format (2015-01-01 00:00 => 01/01/2015 12:00am)
            htmlOutput = Regex.Replace(htmlOutput, @"(\d{4})-(\d{2})-(\d{2})\s(\d{2}):(\d{2}):(\d{2})", delegate (Match match)
            {
                int hour = int.Parse(match.ToString().Substring(11, 2));
                string suffix = (hour < 12) ? "am" : "pm";
                if (hour == 0) hour += 12;
                else if (hour > 12) hour -= 12;
                string replace = "$2/$3/$1 " + hour + ":$5" + suffix;
                return match.Result(replace);
            });

            // output html data
            string file = Path.GetTempFileName() + ".html";
            File.WriteAllText(file, htmlOutput);
            ShowBrowser(file);
            return;

        }

        delegate void dlgString(string s);
        void ShowBrowser(string file)
        {
            var foo = Process.Start(file);
            foo.WaitForInputIdle();
            File.Delete(file);
        }
        // FORM LOGIC

        /// <summary>
        /// Set tooltip on load button and populate combobox with backup files.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // save form title
            formTitle = this.Text;

            // set tooltip on load button
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.btnLoad, "Load Backup File");

            // POPULATE COMBOBOX WITH BACKUP FILES
            DirectoryInfo dirBackup = new DirectoryInfo(backupPath);
            if (!dirBackup.Exists)
            {
                MessageBox.Show("Fatal Error: Cannot find iPhone backup folder.");
                this.Close();
                return; // the path does not exist
            }

            // get backup files
            dtMessageFiles = getBackupFiles(dirBackup);

            DataView dvMessageFiles = dtMessageFiles.DefaultView;
            comboBackups.DataSource = new BindingSource(dvMessageFiles, null);
            comboBackups.DisplayMember = "FullDate";
            comboBackups.ValueMember = "Path";

            // enable comboBackups
            if (comboBackups.Items.Count > 0)
            {
                comboBackups.Enabled = true;
                btnLoad.Enabled = true;
            }
            // disable export button until listbox is populated
            btnExport.Enabled = false;

            iPhoneBackup.MessageGroup.ToStringFilter = GroupNames;
        }


        /// <summary>
        /// Populate ListBox with MessageGroup from dB 
        /// </summary>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (comboBackups.SelectedItem == null)
            {
                MessageBox.Show("Select an item from the list.");
                return;
            }
            GetChatGroups();
        }

        private void GetChatGroups()
        {
            Stopwatch sw = Stopwatch.StartNew();
            _people = _backup.GetAddressList();
            _contacts = new Dictionary<string, string>();
            _people.ForEach((p) => p.Contacts.ForEach((c) => _contacts[c.value] = p.FirstName));
            TraceInformation("GetAddressList {0} ms", sw.ElapsedMilliseconds);
            //List<iPhoneBackup.Message> messages = _backup.GetAllMessages();
            sw.Restart();
            _chats = _backup.GetChats();

            FillMessageGroupsListbox(_chats);

            TraceInformation("{1} chats: {0} ms", sw.ElapsedMilliseconds, _chats.Count);
            //File.WriteAllText(@"C:\Work\git\Tools\IPhoneSMS\b.txt", string.Join("\r\n", from c in _chats select /*c.ChatId + ": " + */ c.ToString()));
        }

        void FillMessageGroupsListbox(List<iPhoneBackup.MessageGroup> groups)
        { 
            //DataTable dt = new DataTable();
            //dt.Columns.Add("Value", typeof(long));
            //dt.Columns.Add("Display", typeof(string));
            //dt.Columns.Add("Group", typeof(string));
            //dt.DefaultView.Sort = "Display ASC";
            //string s;
            //foreach (var item in groups)
            //{
            //    s = item.ToString();
            //    dt.Rows.Add(item.ChatId, Regex.Replace(s, @"\+1(\d{3})(\d{3})(\d{4})\b", "($1) $2-$3"), s); ;
            //}
            //DataView dvMessageGroups = dt.DefaultView;
            lbMessageGroup.DataSource = new BindingSource(groups, null);
            lbMessageGroup.DisplayMember = "Display";
            lbMessageGroup.ValueMember = "ChatId";
            lblGroupCount.Text = groups.Count.ToString();
            lblGroupCount.Left = lbMessageGroup.Right - lblGroupCount.Width + 1;
        }

        //private void OpenBackup()
        //{
        //    // get message groups from file
        //    List<iPhoneBackup.MessageGroup> MessageGroups = getMessageGroupsFromFile(dbFile);
        //    FillMessageGroupsListbox(MessageGroups);
        //    //DataView dvMessageGroups = dtMessageGroups.DefaultView;
        //    //lbMessageGroup.DataSource = new BindingSource(dvMessageGroups, null);
        //    //lbMessageGroup.DisplayMember = "Display";
        //    //lbMessageGroup.ValueMember = "Value";
        //    //lblGroupCount.Text = dvMessageGroups.Count.ToString();
        //    //lblGroupCount.Left = lbMessageGroup.Right - lblGroupCount.Width + 1;
        //    //var foo = (from c in MessageGroups select c.ToString()).ToList();
        //    //foo.Sort();
        //    //File.WriteAllText(@"C:\Work\git\Tools\IPhoneSMS\a.txt", string.Join("\r\n", foo /*from g in MessageGroups select string.Join(",", g.Ids)*/ ));
        //}

        /// <summary>
        /// Enable HTML export button
        /// </summary>
        private void lbMessageGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnExport.Enabled = true;
            if (lbMessageGroup.SelectedItem == null)
                return;
            // for some reason, sometimes the selectedvalue is the selecteditem; if this is the case, we have to get the value from the item
            if (lbMessageGroup.SelectedValue is iPhoneBackup.MessageGroup)
                //messageGroup = lbMessageGroup.SelectedValue.ToString();
                PreviewGroup(_chats.Find(q => q.ChatId == (lbMessageGroup.SelectedValue as iPhoneBackup.MessageGroup).ChatId));
            else
                PreviewGroup(_chats.Find(q => q.ChatId == (long)lbMessageGroup.SelectedValue));
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            //// show dialog for where to save html file
            //SaveFileDialog htmlFileDialog = new SaveFileDialog();
            //htmlFileDialog.Filter = "HTML File|*.htm,*.html";
            //htmlFileDialog.Title = "Save an HTML File";
            //htmlFileDialog.ShowDialog();

            //// If the file name is not an empty string open it for saving.
            //if (htmlFileDialog.FileName == "")
            //{
            //    return;
            //}
            //htmlFile = htmlFileDialog.FileName;

            // disable interface while exporting
            comboBackups.Enabled = false;
            lbMessageGroup.Enabled = false;
            btnLoad.Enabled = false;
            btnExport.Enabled = false;

            // Export messages from MessageGroup into HTML file
            //messageGroup = ;
            backgroundWorker1.RunWorkerAsync(lbMessageGroup.SelectedItem as iPhoneBackup.MessageGroup);
            //Thread exportThread = new Thread(new ThreadStart(exportHTMLForMessageGroup));
            //exportThread.Start();

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            exportHTMLForMessageGroup(e.Argument as iPhoneBackup.MessageGroup);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show("HTML Export completed!");

            // re-enable interface after export completes
            comboBackups.Enabled = true;
            lbMessageGroup.Enabled = true;
            btnLoad.Enabled = true;
            btnExport.Enabled = true;
            this.Text = formTitle;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Text = formTitle + " - Exporting... (" + e.ProgressPercentage.ToString() + "%)";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form1_Resize(null, null);
            if (comboBackups.Items.Count == 1)
            {
                // save selected dbFile for later use
                dbFileDir = comboBackups.SelectedValue as string;
                dbFileDate = comboBackups.GetItemText(comboBackups.SelectedItem);
                dbFile = Path.Combine(dbFileDir, "3d\\3d0d7e5fb2ce288813306e4d4636395e047a3d28");
                _backup = new iPhoneBackup() { FileDate = dbFileDate, DatabaseFolder = Path.GetFileName(dbFileDir), BackupRoot = Path.GetDirectoryName(dbFileDir) };

                GetChatGroups();
                //OpenBackup();
                //_formInitialized = true;
                //// force the indexchanged to occur prior to this SelectedValue wasn't set correctly
                //lbMessageGroup_SelectedIndexChanged(null, null);
                lbMessageGroup.Select();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int MARGIN = lbMessageGroup.Left;
            lbMessageGroup.Size = new System.Drawing.Size(ClientSize.Width / 2 - 2 * MARGIN, btnExport.Top - lbMessageGroup.Top - MARGIN);
            lbPreview.Left = lbMessageGroup.Right + 2 * MARGIN;
            lbPreview.Size = new System.Drawing.Size(lbMessageGroup.Width, lbMessageGroup.Height);
            lblGroupCount.Left = lbMessageGroup.Right - lblGroupCount.Width + 1;
        }
    }
}