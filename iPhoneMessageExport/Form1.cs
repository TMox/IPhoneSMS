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
using SMSXml;

namespace iPhoneMessageExport
{
    public partial class Form1 : Form
    {
        /* GLOBAL variables */
        //bool _formInitialized = false;
        iPhone _iPhone;
        iPhoneBackup _backup;
        string _outputFolder = @"C:\Work\git\Tools\Output";
        const string vString = "0.01.02";
        List<iPhoneBackup.MessageGroup> _chats;
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

        ///// <summary>
        ///// Returns DataTable of MessageGroups from iPhone backup file.
        ///// </summary>
        ///// <param name="dbFile"></param>
        ///// <returns></returns>
        //private List<iPhoneBackup.MessageGroup> getMessageGroupsFromFile(string dbFile)
        //{
        //    //DataTable dt = new DataTable();
        //    //dt.Columns.Add("Value", typeof(string));
        //    //dt.Columns.Add("Display", typeof(string));
        //    //dt.DefaultView.Sort = "Value ASC";

        //    List<iPhoneBackup.MessageGroup> groups = new List<iPhoneBackup.MessageGroup>();
        //    if (dbFile != null)
        //    {
        //        // open SQLite data file
        //        Stopwatch sw = Stopwatch.StartNew();
        //        SQLiteConnection m_dbConnection;
        //        m_dbConnection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;Read Only=True;FailIfMissing=True;");
        //        m_dbConnection.Open();

        //        // select the data
        //        // joins chat_handle_join to handle on ch.handle_id = h.ROWID
        //        // joins chat_handle_join to chat_message_join via where clause (chj.chat_id = cmj.chat_id) grouped on chj.chat_id
        //        // joins chat_message_join to message on cmj.message_id = m.ROWID
        //        // joins message to handle on m.handle_id = h.ROWID
        //        // h.ROWID <- chj.handle_id,chj.chat_id -> cmj.chat_id,cmj.message_id -> m.ROWID,m.handle_id -> h.ROWID
        //        string sql = "SELECT DISTINCT (SELECT GROUP_CONCAT(h.id) FROM chat_handle_join ch INNER JOIN handle h on h.ROWID = ch.handle_id " +
        //            "WHERE ch.chat_id = cm.chat_id Group By ch.chat_id) as chatgroup, cm.chat_id FROM chat_message_join cm " +
        //            "INNER JOIN message m ON cm.message_id = m.ROWID INNER JOIN handle h ON m.handle_id = h.ROWID ORDER BY chatgroup;";
        //        //sql = //"SELECT " + 
        //        //        "SELECT Distinct GROUP_CONCAT(h.id) As chatgroup " +
        //        //        "FROM chat_handle_join chj " +
        //        //        "INNER JOIN handle h On h.ROWID = chj.handle_id " +
        //        //        //"WHERE chj.chat_id = cmj.chat_id " + 
        //        //        "Inner Join chat_message_join cmj On cmj.chat_id = chj.chat_id " +
        //        //        "GROUP BY chj.chat_id " +
        //        //    //"FROM chat_message_join cmj " +
        //        //    //"INNER JOIN message m ON cmj.message_id = m.ROWID " +
        //        //    //"INNER JOIN handle h ON m.handle_id = h.ROWID " + 
        //        //    "ORDER BY chatgroup;";

        //        // these are all the groups, even those with no messages
        //        // it's at least 3 times faster
        //        //sql = "SELECT GROUP_CONCAT(h.id) as ChatGroup " +
        //        //        "From chat_handle_join chj " +
        //        //        "Inner Join handle h on h.ROWID = chj.handle_id " +
        //        //        "Group By chj.chat_id " +
        //        //        "Order By ChatGroup;";

        //        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        //        SQLiteDataReader row = command.ExecuteReader();
        //        while (row.Read())
        //        {
        //            if (row["chatgroup"].ToString().Trim() != "")
        //            {
        //                List<string> ids = (row["chatgroup"] as string).Split(",".ToCharArray()).ToList();
        //                //ids.Sort();
        //                // add and prettify US 11-digit phone numbers
        //                groups.Add(new iPhoneBackup.MessageGroup() { ChatId = (long)row["chat_id"], Ids = ids });
        //                //dt.Rows.Add(row["chatgroup"] as string, Regex.Replace(row["chatgroup"].ToString(), @"\+1(\d{3})(\d{3})(\d{4})\b", "($1)$2-$3"));
        //            }
        //        }
        //        row.Close();
        //        m_dbConnection.Close();
        //        TraceInformation("groups: {0} ms", sw.ElapsedMilliseconds);
        //    }
        //    return groups;
        //}

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

            //bool isGroupMessage = group.Ids.Count > 1;
            lbPreview.Items.Clear();

            Stopwatch sw = Stopwatch.StartNew();
            List<iPhoneBackup.Message> messages = _backup.GetMessages(group.ChatId);
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

        /// <summary>
        /// Export all messages for MessageGroup in backup file into HTML file. (THREAD)
        /// </summary>
        private void exportHTMLForMessageGroup(iPhoneBackup.MessageGroup group)
        {
            StringBuilder sb = new StringBuilder();
            //string group = string.Join(",", grp.Ids);
            //bool isGroupMessage = group.Ids.Count > 1; // (group.Contains(",")) ? true : false;

            // query database
            if (dbFile != null)
            {
                // open SQLite data file
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;Read Only=True;FailIfMissing=True;");
                m_dbConnection.Open();

                int totalMessages = 1;
                Stopwatch sw = Stopwatch.StartNew();
                List<iPhoneBackup.Message> msgs = _backup.GetMessages(group.ChatId);
                List<iPhoneBackup.MessageGroup> chats = _backup.GetChats();
                iPhoneBackup.MessageGroup chat;
                totalMessages = msgs.Count;
                TraceInformation("get messages: {0} ms", sw.ElapsedMilliseconds);
                sw.Restart();
                sb.AppendLine(iPhoneSMS.Properties.Resources.HTMLHeaders); // add html headers
                sb.AppendLine("<BODY>");
                sb.AppendFormat("<H1>Messages from {0}</H1>\n", group.ToString());
                sb.AppendFormat("<H2>as of {0}</H2>\n", dbFileDate);
                sb.AppendLine("<DIV id=\"messages\">");
                // fields: date, service, direction, id, text, filereflist
                int i = 0;
                string mFile;
                foreach (iPhoneBackup.Message row in msgs)
                {
                    string content = row.Text; // (string)row["text"];
                    sb.AppendFormat("<DIV class=\"message message-{0}-{1}\">\n", row.Incoming ? "RCVD" : "SENT", row.Type); // row["direction"], row["service"]);
                    sb.AppendFormat("<DIV class=\"timestamp-placeholder\"></DIV><DIV class=\"timestamp\">{0}</DIV>\n", row.Timestamp); // row["date"]);
                    //Assert(isGroupMessage == row.IsGroup);
                    chat = chats.Find(q => q.ChatId == row.ChatId);
                    if (chat.Ids.Count > 1) // it's a group chat
                        sb.AppendFormat("<DIV class=\"sender\">{0}</DIV>\n", _iPhone.IdToName(row.Sender)); // row["id"]);
                    // replace image placeholders (ï¿¼) with image files 
                    //if (row["filereflist"].ToString().Length > 0)
                    if (row.Attachments != null && row.Attachments.Count > 0)
                    {
                        //List<string> mediaFileList = row["filereflist"].ToString().Split(',').ToList();
                        foreach (var mediaFile in row.Attachments)
                        {
                            mFile = mediaFile.FileName;
                            if (mFile.StartsWith("~"))
                                mFile = "MediaDomain-" + mFile.Substring(2);
                            else
                                mFile = "MediaDomain-" + mFile.Substring(12);
                            mFile = MiscUtil.getSHAHash(mFile);
                            mFile = mFile.Substring(0, 2) + "\\" + mFile;
                            mFile = Path.Combine(dbFileDir, mFile);
                            string replace = null;
                            // get extension of mediaFile
                            switch (mediaFile.FileName.Substring(mediaFile.FileName.LastIndexOf('.')).ToLower())
                            {
                                // image 
                                case ".jpeg":
                                case ".jpg":
                                case ".png":
                                    replace = string.Format("<img src=\"{0}\"><!-- {1}//-->", mFile, mediaFile);
                                    break;
                                case ".mov":
                                    replace = string.Format("<video controls='controls' width='300' name='video' src='{0}'></video>", mFile);
                                    break;
                                case ".vcf":
                                    string contents = File.ReadAllText(mFile);
                                    int j = contents.IndexOf("FN:");
                                    int k = contents.IndexOf("\r", j + 3);
                                    contents = contents.Substring(j + 3, k - j - 3);
                                    replace = "Contact: " + contents;
                                    break;
                                case ".pluginpayloadattachment":
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
                    sb.AppendFormat("<DIV class=\"content\">{0}</DIV>\n", content);
                    sb.AppendLine("</DIV>");

                    i++;
                    backgroundWorker1.ReportProgress(i * 100 / totalMessages);
                }
                sb.AppendLine("</DIV>");
                sb.AppendLine("</BODY>");
                sb.AppendLine("</HTML>");

                TraceInformation("Creating html: {0} ms", sw.ElapsedMilliseconds);
                //set.Dispose();
                //adpt.Dispose();
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
            if (!Directory.Exists(_outputFolder))
            {
                Directory.CreateDirectory(_outputFolder);
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
            _iPhone = new iPhone();
            _iPhone.GetContacts(_backup);
            TraceInformation("GetAddressList {0} ms", sw.ElapsedMilliseconds);
            //List<iPhoneBackup.Message> messages = _backup.GetAllMessages();
            sw.Restart();
            _chats = _backup.GetChats();

            FillMessageGroupsListbox(_chats);

            TraceInformation("{1} chats: {0} ms", sw.ElapsedMilliseconds, _chats.Count);
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
                iPhoneBackup.MessageGroup.ToStringFilter = _iPhone.GroupNames;
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


        void BackupToXML()
        {
            var foo = GetiPhoneBackupMessages();

            //int count = 0;
            //int good = 0;
            //foreach (var item in foo.mms)
            //{
            //    if (item.Parts.Count == 1 && item.Parts[0].ct == "test/plain")
            //        continue;
            //    foreach (var p in item.Parts)
            //    {
            //        string f = MiscUtil.getSHAHash(p.cd);
            //        f = f.Substring(0, 2) + "\\" + f;
            //        f = Path.Combine(dbFileDir, f);
            //        if (File.Exists(f))
            //            good++;
            //    }
            //    count += item.Parts.Count;
            //}
            //WriteLine(string.Format("{0} attachments found, {1} good.", count, good));

            AndroidXml xml = new AndroidXml() { MMSMessages = foo.mms, SMSMessages = foo.sms };
            xml.SortMMS();
            xml.SortSMS();
            string mFile;
            string path = Path.Combine(_outputFolder, "Files");
            string file;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            bool changedname = false;
            foreach (AndroidMMS item in xml.WriteAndroidXMLFillingMMS(Path.Combine(_outputFolder, string.Format("sms_iPhoneSMS_{0:yyMMddhhmmss}.xml", DateTime.Now))))
            {
                foreach(Part part in item.Parts)
                {
                    changedname = false;
                    if (item.Parts.Count == 1 && item.Parts[0].ct == "text/plain" || string.IsNullOrEmpty(part.name))
                        // this is a text-only mms group message
                        continue;
                    mFile = MiscUtil.getSHAHash(part.cd);
                    mFile = mFile.Substring(0, 2) + "\\" + mFile;
                    mFile = Path.Combine(dbFileDir, mFile);
                    if (!File.Exists(mFile))
                    {
                        // almost all the missing files are vcards or vlocations; nothing to do about this--iPhone doesn't keep them apparently
                        WriteLine("Couldn't find file " + mFile);
                        part.cd = "null";
                        continue;
                    }
                    file = Path.Combine(path, item.date.ToString("yyMMddhhmmss"));
                    if (!Directory.Exists(file))
                        Directory.CreateDirectory(file);
                    file = Path.Combine(file, part.name);
                    while (!File.Exists(file))
                    {
                        File.Copy(mFile, file);

                        //changedname = true;
                        //int i = file.LastIndexOf(".");
                        //int j = i - 1;
                        //while (j > 0 && file[j] != '(')
                        //    j--;
                        //if (j == 0)
                        //    file = file.Substring(0, i) + "(2)" + file.Substring(i);
                        //else
                        //    file = file.Substring(0, j) + "(" + (int.Parse(file.Substring(j + 1, i - 2 - j)) + 1).ToString() + ")" + file.Substring(i);
                    }
                    //if (changedname)
                    //    WriteLine(file);
                    part.cd = "null";
                    switch (part.ct) {
                        case "audio/amr":
                        case "video/3gpp":
                        case "video/mp4":
                        case "image/gif":
                        case "image/jpeg":
                        case "image/png":
                        case "video/quicktime":
                            //part.data = Convert.ToBase64String(File.ReadAllBytes(mFile));
                            break;
                        case "text/plain":
                        case "text/vcard":
                        case "text/x-vcard":
                        case "text/x-vlocation":
                            break;
                    }
                }
            }
            //xml.WriteAndroidXML(Path.Combine(_outputFolder, string.Format("sms_iPhoneSMS_{0}.xml", DateTime.Now)));
        }


        //        void WriteSMS()
        //        string xmlHeader = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
        //<!--File Created By TMox iPhoneSMS v{3} on {2}-->
        //<?xml-stylesheet type=""text/xsl"" href=""sms.xsl""?>
        //<smses count=""{1}"" backup_set=""{0}"" backup_date=""{4}"">
        //";
        //        string xmlTail = @"</smses>";
        //        string sms = "<sms protocol=\"0\" address=\"{0}\" date=\"{1}\" type=\"{2}\" subject=\"{3}\" body={4} toa=\"null\" sc_toa=\"null\" service_center=\"null\" read=\"1\" status=\"-1\" locked=\"0\" date_sent=\"{7}\" readable_date=\"{5:MMM d, yyyy h:mm:ss tt}\" contact_name=\"{6}\" />\n";
        //        StringBuilder sb = new StringBuilder();
        //        AndroidXml xml = new AndroidXml() { AndroidMMSMessages = mmsMessages, AndroidSMSMessages = smsMessages };
        //        xml.WriteAndroidXML(Path.Combine(_outputFolder, string.Format("sms_iPhoneSMS.xml", DateTime.Now)));

        (List<AndroidSMS> sms, List<AndroidMMS> mms) GetiPhoneBackupMessages()
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<iPhoneBackup.Message> msgs = _backup.GetMessages(-1);
            TraceInformation("Get all messages: {0} ms", sw.ElapsedMilliseconds);
            sw.Restart();
            // we need to put all the individual messages into chats
            List<iPhoneBackup.MessageGroup> chats = _backup.GetChats();
            TraceInformation("Get all chats: {0} ms", sw.ElapsedMilliseconds);

            List<AndroidSMS> smsMessages = new List<AndroidSMS>();
            List<AndroidMMS> mmsMessages = new List<AndroidMMS>();
            iPhoneBackup.MessageGroup chat;
            bool isGroup, hasAttachment;
            //int count = 0;
            foreach (var item in msgs)
            {
                chat = chats.Find(q => q.ChatId == item.ChatId);
                isGroup = chat.Ids.Count > 1;
                hasAttachment = item.Attachments != null && item.Attachments.Count > 0;
                if (isGroup || hasAttachment)   
                {
                    AndroidMMS msg = new AndroidMMS()
                    {
                        address = chat.Ids,
                        // have to set msg_box because that's the incoming/outgoing indicator (1/2)
                        msg_box = item.Incoming ? 1 : 2,
                        readable_date = string.Format("{0:MMM d, yyyy h:mm:ss tt}", item.Timestamp),
                        contact_name = (from cid in chat.Ids select _iPhone.IdToName(cid)).ToList(),
                        date = item.Timestamp,
                        date_sent = (item.Sent == DateTime.MinValue) ? item.Sent : item.Sent,
                    };
                    mmsMessages.Add(msg);
                    msg.Addresses = (from a in chat.Ids select new Address { address = a, type = 151 }).ToList();
                    msg.Addresses[0].type = 137;
                    msg.Parts = new List<Part>();
                    int i = 0;
                    if (item.Attachments == null || item.Attachments.Count == 0 || !string.IsNullOrEmpty(item.Text))
                    {
                        //Assert(isGroup);
                        msg.Parts.Add(
                            new Part
                            {
                                seq = 0,
                                ct = "text/plain",
                                name = null,
                                chset = "106",
                                fn = "part-0",
                                cid = null,
                                cl = null,
                                text = item.Text,
                            }
                        );
                        i++;
                    }
                    if (item.Attachments != null)
                    { 
                        foreach (var a in item.Attachments)
                        {
                            // these are all the mime types I get
                            //if (a.MimeType != null && a.MimeType != "audio/amr" && a.MimeType != "video/3gpp" && a.MimeType != "video/mp4" && a.MimeType != "image/gif" && a.MimeType != "image/jpeg" && a.MimeType != "text/vcard" && a.MimeType != "video/quicktime" && a.MimeType != "text/x-vcard" && a.MimeType != "text/x-vlocation" && a.MimeType != "image/png")
                            //    WriteLine("stop");
                            msg.text_only = false; // text_only only shows up if there are two attachments: 1) a smil & 2) text. iPhone doesn't do smil files
                            string fn = null;
                            string path = null;
                            try
                            {
                                path = a.FileName;
                                if (path.StartsWith("~"))
                                    path = "MediaDomain-" + path.Substring(2);
                                else
                                    path = "MediaDomain-" + path.Substring(12);
                                fn = Path.GetFileName(path);
                            }
                            catch (Exception ex)
                            {
                                WriteLine("Error: " + ex.Message);
                            }
                            bool txt = isText(a.MimeType);
                            msg.Parts.Add(
                                new Part
                                {
                                    seq = i,
                                    cd = path,
                                    ct = a.MimeType,
                                    name = fn,
                                    chset = txt ? "106" : null,
                                    fn = "part-" + i,
                                    cid = string.Format("<{0}>", fn),
                                    cl = fn,
                                    text = txt ? item.Text : null,
                                    //bugbug have to actually get the data file (no text files for iPhone)
                                    data = txt ? null : "data"
                                });
                            i++;
                        }
                    }
                }
                else
                {
                    smsMessages.Add(new AndroidSMS()
                    {
                        address = item?.Sender ?? chat.Ids[0],
                        body = item.Text,
                        contact_name = _iPhone.IdToNameUnknown(item?.Sender ?? chat.Ids[0]),
                        date = item.Timestamp,
                        date_sent = (item.Sent == DateTime.MinValue) ? item.Sent : item.Sent,
                        readable_date = string.Format("{0:MMM d, yyyy h:mm:ss tt}", item.Timestamp),
                        subject = item.Subject,
                        type = item.Incoming ? 1 : 2
                    });
                }
            }

            TraceInformation("{0} SMS, {1} MMS", smsMessages.Count, mmsMessages.Count());
            btnBackupXML.Enabled = true;
            return (smsMessages, mmsMessages);
        }

        bool isText(string mimetype)
        {
            switch (mimetype)
            {
                case "audio/amr":
                case "video/3gpp":
                case "video/mp4":
                case "image/gif":
                case "image/jpeg":
                case "image/png":
                case "video/quicktime":
                    return false;
                case "text/plain":
                case "text/vcard":
                case "text/x-vcard":
                case "text/x-vlocation":
                    return true;
                default:
                    WriteLine("stop");
                    break;
            }
            return false;
        }

        string Encode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "\"\"";
            s = Regex.Replace(s, @"\p{Cs}", "");
            s = s.Replace("&", "&amp;").Replace("\n", "&#10;").Replace("\r", "&#13;").Replace("<", "&lt;").Replace(">", "&gt;");
            if (s.Contains("\""))
                return "'" + s.Replace("'", "&apos;") + "'";
            return "\"" + s + "\"";
        }
        string MediaString(string s, List<string> files)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            int i = 0;
            int j;
            while ((j = s.IndexOf("[MEDIA]")) != -1)
            {
                s = s.Substring(0, j) + string.Format("[Attachment: {0}]", files != null && i < files.Count ? file(files[i++]) : "null") + s.Substring(j + 7);
            }
            return s;
        }
        string file(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            int i = s.LastIndexOf("/");
            if (i == -1)
                return s;
            return s.Substring(i + 1);
        }
        private void btnBackupXML_Click(object sender, EventArgs e)
        {
            btnBackupXML.Enabled = false;
            BackupToXML();
            btnBackupXML.Enabled = true;
        }
    }
}