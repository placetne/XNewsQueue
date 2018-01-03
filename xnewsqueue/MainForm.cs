namespace xnewsqueue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Resources;
    using System.Timers;
    using System.Windows.Forms;
    using System.Xml;

    public class MainForm : Form
    {
        private Button Button_About;
        private Button Button_Exit;
        private Button Button_Import;
        private Button Button_ImportNZB;
        private Button Button_Path;
        private ComboBox Combo_Queue;
        private ComboBox Combo_Server;
        private Container components = null;
        private FolderBrowserDialog Folder_Path;
        private Label Label_Import;
        private Label Label_Path;
        private Label Label_Queue;
        private Label Label_Server;
        private Label Label_Status;
        private string[] NZBPaths;
        private bool NZBPathsValid;
        private OpenFileDialog OpenFile_Import;
        private ProgressBar Progress_Import;
        private string[] Queues;
        private bool QueueValid;
        private string[] Servers;
        private bool ServerValid;
        private System.Timers.Timer t;
        private TextBox Text_Path;
        private TextBox TextBox_Import;
        private string XnewsPath;
        private bool XnewsPathValid;

        public MainForm()
        {
            this.InitializeComponent();
        }

        private void Button_About_Click(object sender, EventArgs e)
        {
            Form form = new AboutForm();
            form.ShowDialog(this);
            form.Dispose();
        }

        private void Button_Exit_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void Button_Import_Click(object sender, EventArgs e)
        {
            if (this.OpenFile_Import.ShowDialog() == DialogResult.OK)
            {
                this.NZBPaths = this.OpenFile_Import.FileNames;
                string str = "";
                foreach (string str2 in this.NZBPaths)
                {
                    str = str + str2 + ", ";
                }
                if (str != "")
                {
                    str = str.Remove(str.Length - 2, 2);
                }
                this.TextBox_Import.Text = str;
            }
        }

        private void Button_ImportNZB_Click(object sender, EventArgs e)
        {
            Header header;
            string[] strArray;
            if (this.Combo_Queue.SelectedItem == null)
            {
                MessageBox.Show("Please select a folder before importing.", "Error");
                return;
            }
            if (this.Combo_Server.SelectedItem == null)
            {
                MessageBox.Show("Please select a server before importing.", "Error");
                return;
            }
            List<Article> list = new List<Article>();
            string path = this.XnewsPath + @"\folders\" + this.Combo_Queue.SelectedItem.ToString() + ".qdr";
            string str2 = this.XnewsPath + @"\folders\" + this.Combo_Queue.SelectedItem.ToString() + ".mxb";
            if (File.Exists(path))
            {
                new FileInfo(path);
            }
            string server = this.Combo_Server.SelectedItem.ToString();
            this.Label_Status.Text = "Importing...";
            this.Label_Status.Refresh();
            if (File.Exists(path))
            {
                try
                {
                    this.Label_Status.Text = "Creating backup";
                    this.Label_Status.Refresh();
                    File.Copy(path, path + ".bak", true);
                }
                catch
                {
                    MessageBox.Show("Failed to make backup copy of file: " + path, "Error");
                    return;
                }
                try
                {
                    this.Label_Status.Text = "Reading existing queue...";
                    this.Label_Status.Refresh();
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            header = Core.GetHeader(reader);
                            var articles = Core.GetArticles(reader);
                            list.AddRange(articles);
                        }
                    }
                    goto Label_021B;
                }
                catch
                {
                    MessageBox.Show("Failed to read existing queue: " + path, "Error");
                    return;
                }
            }
            header = new Header();
            header.Identifier = new byte[] { 230, 0xeb, 0x1f, 0 };
            if (File.Exists(str2))
            {
                FileInfo info = new FileInfo(str2);
                header.MBXBytes = Convert.ToInt32(info.Length);
            }
            else
            {
                header.MBXBytes = 0;
            }
            header.Unknown = new byte[0x44];
            for (int i = 0; i < header.Unknown.Length; i++)
            {
                header.Unknown[i] = 0;
            }
        Label_021B:
            strArray = this.NZBPaths;
            for (int j = 0; j < strArray.Length; j++)
            {
                string str4 = strArray[j];
                this.Label_Status.Text = "Importing: " + str4;
                this.Label_Status.Refresh();
                if (File.Exists(str4))
                {
                    var c = Core.ImportNZB(str4, server, this.Progress_Import);
                    list.AddRange(c);
                }
            }
            try
            {
                this.Label_Status.Text = "Writing queue...";
                this.Label_Status.Refresh();
                using (FileStream stream2 = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream2))
                    {
                        Core.WriteQueue(writer, header, list);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to write to queue: " + path, "Error");
                return;
            }
            this.Progress_Import.Value = 0;
            this.Label_Status.Text = "Complete";
            this.Label_Status.Refresh();
            this.t = new System.Timers.Timer();
            this.t.AutoReset = false;
            this.t.Enabled = true;
            this.t.Interval = 3000.0;
            this.t.Elapsed += new ElapsedEventHandler(this.t_Elapsed);
            this.t.Start();
        }

        private void Button_Path_Click(object sender, EventArgs e)
        {
            if (this.Folder_Path.ShowDialog() == DialogResult.OK)
            {
                this.XnewsPath = this.Folder_Path.SelectedPath;
                this.Text_Path.Text = this.Folder_Path.SelectedPath;
                this.UpdateServers();
                this.UpdateQueues();
            }
        }

        private void CheckValid()
        {
            if (this.Combo_Queue.Items.Count > 0)
            {
                this.QueueValid = true;
            }
            else
            {
                this.QueueValid = false;
            }
            if (this.Combo_Server.Items.Count > 0)
            {
                this.ServerValid = true;
            }
            else
            {
                this.ServerValid = false;
            }
            if ((this.NZBPaths != null) && (this.NZBPaths.Length > 0))
            {
                this.NZBPathsValid = true;
            }
            else
            {
                this.NZBPathsValid = false;
            }
            if ((this.XnewsPathValid && this.QueueValid) && (this.ServerValid && this.NZBPathsValid))
            {
                this.Button_ImportNZB.Enabled = true;
            }
            else
            {
                this.Button_ImportNZB.Enabled = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ResourceManager manager = new ResourceManager(typeof(MainForm));
            this.Label_Server = new Label();
            this.Label_Queue = new Label();
            this.Label_Import = new Label();
            this.Combo_Server = new ComboBox();
            this.Combo_Queue = new ComboBox();
            this.Button_Import = new Button();
            this.Button_ImportNZB = new Button();
            this.Button_Exit = new Button();
            this.OpenFile_Import = new OpenFileDialog();
            this.Label_Path = new Label();
            this.Button_Path = new Button();
            this.Text_Path = new TextBox();
            this.Folder_Path = new FolderBrowserDialog();
            this.Button_About = new Button();
            this.Progress_Import = new ProgressBar();
            this.Label_Status = new Label();
            this.TextBox_Import = new TextBox();
            base.SuspendLayout();
            this.Label_Server.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.Label_Server.Location = new Point(12, 0x20);
            this.Label_Server.Name = "Label_Server";
            this.Label_Server.Size = new Size(0x60, 0x15);
            this.Label_Server.TabIndex = 0;
            this.Label_Server.Text = "Server:";
            this.Label_Server.TextAlign = ContentAlignment.MiddleLeft;
            this.Label_Queue.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.Label_Queue.Location = new Point(12, 60);
            this.Label_Queue.Name = "Label_Queue";
            this.Label_Queue.Size = new Size(0x60, 0x15);
            this.Label_Queue.TabIndex = 1;
            this.Label_Queue.Text = "Queue:";
            this.Label_Queue.TextAlign = ContentAlignment.MiddleLeft;
            this.Label_Import.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.Label_Import.Location = new Point(12, 0x58);
            this.Label_Import.Name = "Label_Import";
            this.Label_Import.Size = new Size(0x60, 0x15);
            this.Label_Import.TabIndex = 2;
            this.Label_Import.Text = "NZB File:";
            this.Label_Import.TextAlign = ContentAlignment.MiddleLeft;
            this.Combo_Server.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Combo_Server.Location = new Point(0x58, 0x20);
            this.Combo_Server.Name = "Combo_Server";
            this.Combo_Server.Size = new Size(0xb8, 0x15);
            this.Combo_Server.TabIndex = 3;
            this.Combo_Queue.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Combo_Queue.Location = new Point(0x58, 60);
            this.Combo_Queue.Name = "Combo_Queue";
            this.Combo_Queue.Size = new Size(0xb8, 0x15);
            this.Combo_Queue.TabIndex = 4;
            this.Button_Import.FlatStyle = FlatStyle.Flat;
            this.Button_Import.Location = new Point(0xf8, 0x58);
            this.Button_Import.Name = "Button_Import";
            this.Button_Import.Size = new Size(0x18, 20);
            this.Button_Import.TabIndex = 6;
            this.Button_Import.Text = "...";
            this.Button_Import.Click += new EventHandler(this.Button_Import_Click);
            this.Button_ImportNZB.FlatStyle = FlatStyle.Flat;
            this.Button_ImportNZB.Location = new Point(4, 0x9c);
            this.Button_ImportNZB.Name = "Button_ImportNZB";
            this.Button_ImportNZB.Size = new Size(0x55, 0x18);
            this.Button_ImportNZB.TabIndex = 7;
            this.Button_ImportNZB.Text = "Import NZB";
            this.Button_ImportNZB.Click += new EventHandler(this.Button_ImportNZB_Click);
            this.Button_Exit.FlatStyle = FlatStyle.Flat;
            this.Button_Exit.Location = new Point(0xbc, 0x9c);
            this.Button_Exit.Name = "Button_Exit";
            this.Button_Exit.Size = new Size(0x55, 0x18);
            this.Button_Exit.TabIndex = 8;
            this.Button_Exit.Text = "Exit";
            this.Button_Exit.Click += new EventHandler(this.Button_Exit_Click);
            this.OpenFile_Import.Filter = "NZB Files|*.nzb";
            this.OpenFile_Import.Multiselect = true;
            this.OpenFile_Import.RestoreDirectory = true;
            this.OpenFile_Import.Title = "Import NZB";
            this.Label_Path.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.Label_Path.Location = new Point(12, 4);
            this.Label_Path.Name = "Label_Path";
            this.Label_Path.Size = new Size(0x60, 0x15);
            this.Label_Path.TabIndex = 0;
            this.Label_Path.Text = "XNews:";
            this.Label_Path.TextAlign = ContentAlignment.MiddleLeft;
            this.Button_Path.FlatStyle = FlatStyle.Flat;
            this.Button_Path.Location = new Point(0xf8, 4);
            this.Button_Path.Name = "Button_Path";
            this.Button_Path.Size = new Size(0x18, 20);
            this.Button_Path.TabIndex = 1;
            this.Button_Path.Text = "...";
            this.Button_Path.Click += new EventHandler(this.Button_Path_Click);
            this.Text_Path.Location = new Point(0x58, 4);
            this.Text_Path.Name = "Text_Path";
            this.Text_Path.Size = new Size(160, 20);
            this.Text_Path.TabIndex = 0;
            this.Text_Path.Text = "";
            this.Text_Path.TextChanged += new EventHandler(this.Text_Path_TextChanged);
            this.Button_About.FlatStyle = FlatStyle.Flat;
            this.Button_About.Location = new Point(0x60, 0x9c);
            this.Button_About.Name = "Button_About";
            this.Button_About.Size = new Size(0x55, 0x18);
            this.Button_About.TabIndex = 9;
            this.Button_About.Text = "About";
            this.Button_About.Click += new EventHandler(this.Button_About_Click);
            this.Progress_Import.Location = new Point(4, 0x84);
            this.Progress_Import.Name = "Progress_Import";
            this.Progress_Import.Size = new Size(0x10c, 0x10);
            this.Progress_Import.TabIndex = 10;
            this.Label_Status.BorderStyle = BorderStyle.FixedSingle;
            this.Label_Status.Location = new Point(4, 0x74);
            this.Label_Status.Name = "Label_Status";
            this.Label_Status.Size = new Size(0x10c, 0x10);
            this.Label_Status.TabIndex = 11;
            this.Label_Status.Text = "Idle";
            this.Label_Status.TextAlign = ContentAlignment.MiddleCenter;
            this.TextBox_Import.Location = new Point(0x58, 0x58);
            this.TextBox_Import.Name = "TextBox_Import";
            this.TextBox_Import.ReadOnly = true;
            this.TextBox_Import.Size = new Size(160, 20);
            this.TextBox_Import.TabIndex = 5;
            this.TextBox_Import.Text = "";
            this.TextBox_Import.TextChanged += new EventHandler(this.TextBox_Import_TextChanged);
            this.AutoScaleBaseSize = new Size(5, 13);
            base.ClientSize = new Size(0x116, 0xb7);
            base.Controls.Add(this.Label_Status);
            base.Controls.Add(this.Progress_Import);
            base.Controls.Add(this.Button_About);
            base.Controls.Add(this.Button_Path);
            base.Controls.Add(this.Text_Path);
            base.Controls.Add(this.Button_Exit);
            base.Controls.Add(this.Button_ImportNZB);
            base.Controls.Add(this.Button_Import);
            base.Controls.Add(this.TextBox_Import);
            base.Controls.Add(this.Combo_Queue);
            base.Controls.Add(this.Combo_Server);
            base.Controls.Add(this.Label_Path);
            base.Controls.Add(this.Label_Import);
            base.Controls.Add(this.Label_Queue);
            base.Controls.Add(this.Label_Server);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.MaximizeBox = false;
            base.Name = "MainForm";
            this.Text = "Xnews NZB Importer";
            base.Closing += new CancelEventHandler(this.MainForm_Closing);
            base.Load += new EventHandler(this.MainForm_Load);
            base.ResumeLayout(false);
        }

        private void LoadSettings(string filename)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.XmlResolver = null;
                document.Load(filename);
                if (document.DocumentElement != null)
                {
                    XmlNode node = document.SelectSingleNode("Options");
                    if (node["XnewsPath"] != null)
                    {
                        this.XnewsPath = node["XnewsPath"].InnerXml;
                        this.Text_Path.Text = this.XnewsPath;
                        if (this.XnewsPath != "")
                        {
                            this.UpdateServers();
                            this.UpdateQueues();
                        }
                    }
                    int x = -1;
                    int y = -1;
                    if (node["WindowX"] != null)
                    {
                        x = int.Parse(node["WindowX"].InnerXml);
                    }
                    if (node["WindowY"] != null)
                    {
                        y = int.Parse(node["WindowY"].InnerXml);
                    }
                    if ((x >= 0) && (y >= 0))
                    {
                        base.SetDesktopLocation(x, y);
                    }
                }
            }
            catch
            {
            }
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            string filename = "Settings.xml";
            this.SaveSettings(filename);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string path = "Settings.xml";
            if (File.Exists(path))
            {
                this.LoadSettings(path);
            }
            this.CheckValid();
        }

        private void SaveSettings(string filename)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.CreateXmlDeclaration("1.0", "utf-8", "");
                document.XmlResolver = null;
                XmlElement node = document.CreateElement("Options");
                this.XmlAddElement(node, "XnewsPath", this.XnewsPath);
                this.XmlAddElement(node, "WindowX", base.Location.X.ToString());
                this.XmlAddElement(node, "WindowY", base.Location.Y.ToString());
                document.AppendChild(node);
                document.Save(filename);
            }
            catch
            {
            }
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Label_Status.Text = "Idle";
            this.Label_Status.Refresh();
            this.t.Stop();
        }

        private void Text_Path_TextChanged(object sender, EventArgs e)
        {
            this.XnewsPath = this.Text_Path.Text;
            if ((Directory.Exists(this.XnewsPath) && File.Exists(this.XnewsPath + @"\Xnews.exe")) && (File.Exists(this.XnewsPath + @"\folders.ini") && File.Exists(this.XnewsPath + @"\servers.ini")))
            {
                this.UpdateQueues();
                this.UpdateServers();
                this.XnewsPathValid = true;
                this.CheckValid();
            }
            else
            {
                this.Combo_Queue.Items.Clear();
                this.Combo_Server.Items.Clear();
                this.XnewsPathValid = false;
                this.CheckValid();
            }
        }

        private void TextBox_Import_TextChanged(object sender, EventArgs e)
        {
            this.CheckValid();
        }

        private void UpdateQueues()
        {
            if (File.Exists(this.XnewsPath + @"\folders.ini"))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(this.XnewsPath + @"\folders.ini"))
                    {
                        string str;
                        do
                        {
                            str = reader.ReadLine();
                            if (str == null)
                            {
                                return;
                            }
                        }
                        while (!str.Trim().ToLower().StartsWith("[folders]"));
                        str = reader.ReadLine();
                        int num = 0;
                        if (str.StartsWith("Count="))
                        {
                            num = Convert.ToInt32(str.Substring(str.IndexOf("Count=") + "Count=".Length));
                        }
                        if (str.StartsWith("count="))
                        {
                            num = Convert.ToInt32(str.Substring(str.IndexOf("count=") + "count=".Length));
                        }
                        this.Queues = new string[num];
                        for (int i = 0; i < num; i++)
                        {
                            str = reader.ReadLine();
                            this.Queues[i] = str.Substring(str.IndexOf("=") + "=".Length);
                        }
                    }
                    this.Combo_Queue.Items.Clear();
                    this.Combo_Queue.Items.AddRange(this.Queues);
                    this.Combo_Queue.SelectedIndex = 0;
                }
                catch
                {
                    this.Combo_Queue.Items.Clear();
                }
            }
            else
            {
                this.Combo_Queue.Items.Clear();
            }
        }

        private void UpdateServers()
        {
            if (File.Exists(this.XnewsPath + @"\servers.ini"))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(this.XnewsPath + @"\servers.ini"))
                    {
                        string str;
                        do
                        {
                            str = reader.ReadLine();
                            if (str == null)
                            {
                                return;
                            }
                        }
                        while (!str.Trim().ToLower().StartsWith("[profiles]"));
                        str = reader.ReadLine();
                        int num = 0;
                        int num2 = 1;
                        if (str.StartsWith("Count="))
                        {
                            num = Convert.ToInt32(str.Substring(str.IndexOf("Count=") + "Count=".Length));
                        }
                        if (str.StartsWith("count="))
                        {
                            num = Convert.ToInt32(str.Substring(str.IndexOf("count=") + "count=".Length));
                        }
                        if (str.StartsWith("Default="))
                        {
                            num2 = Convert.ToInt32(str.Substring(str.IndexOf("Default=") + "Default=".Length));
                        }
                        if (str.StartsWith("default="))
                        {
                            num2 = Convert.ToInt32(str.Substring(str.IndexOf("default=") + "default=".Length));
                        }
                        str = reader.ReadLine();
                        if (str.StartsWith("Count="))
                        {
                            num = Convert.ToInt32(str.Substring(str.IndexOf("Count=") + "Count=".Length));
                        }
                        if (str.StartsWith("count="))
                        {
                            num = Convert.ToInt32(str.Substring(str.IndexOf("count=") + "count=".Length));
                        }
                        if (str.StartsWith("Default="))
                        {
                            num2 = Convert.ToInt32(str.Substring(str.IndexOf("Default=") + "Default=".Length));
                        }
                        if (str.StartsWith("default="))
                        {
                            num2 = Convert.ToInt32(str.Substring(str.IndexOf("default=") + "default=".Length));
                        }
                        this.Servers = new string[num];
                        for (int i = 0; i < num; i++)
                        {
                            str = reader.ReadLine();
                            this.Servers[i] = str.Substring(str.IndexOf("=") + "=".Length, (str.IndexOf(",") - str.IndexOf("=")) - 1);
                        }
                        this.Combo_Server.Items.Clear();
                        this.Combo_Server.Items.AddRange(this.Servers);
                        this.Combo_Server.SelectedIndex = num2 - 1;
                    }
                }
                catch
                {
                    this.Combo_Server.Items.Clear();
                }
            }
            else
            {
                this.Combo_Server.Items.Clear();
            }
        }

        private void XmlAddAttr(XmlNode node, string name, string value)
        {
            XmlAttribute attribute = node.OwnerDocument.CreateAttribute(name);
            attribute.Value = value;
            node.Attributes.Append(attribute);
        }

        private void XmlAddElement(XmlNode node, string name, string value)
        {
            XmlElement newChild = node.OwnerDocument.CreateElement(name);
            newChild.InnerText = value;
            node.AppendChild(newChild);
        }
    }
}

