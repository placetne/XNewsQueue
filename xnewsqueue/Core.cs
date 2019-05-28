namespace xnewsqueue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Xml;

    internal class Core
    {
        public static string DecodeBytes(byte[] bytes, Encoding encoding)
        {
            char[] chars = encoding.GetChars(bytes);
            StringBuilder builder = new StringBuilder();
            foreach (char ch in chars)
            {
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static string DecodeStreamBytes(BinaryReader br, int count)
        {
            return DecodeBytes(br.ReadBytes(count), Encoding.Default);
        }

        public static Article GetArticle(BinaryReader br)
        {
            try
            {
                Article article = new Article();
                article.Unknown0 = br.ReadBytes(4);
                article.CacheOffset = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.ArticleNumber = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.QueueOrder = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.Unknown1 = br.ReadBytes(4);
                article.Unknown2 = br.ReadBytes(4);
                article.Bytes = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.Lines = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.Date = br.ReadBytes(8);
                article.MessageIdCharCount = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.MessageId = DecodeStreamBytes(br, article.MessageIdCharCount);
                article.SubjectCharCount = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.Subject = DecodeStreamBytes(br, article.SubjectCharCount);
                article.UsernameCharCount = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.Username = DecodeStreamBytes(br, article.UsernameCharCount);
                article.EmailCharCount = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.Email = DecodeStreamBytes(br, article.EmailCharCount);
                article.Unknown3 = br.ReadBytes(4);
                article.ServerCharCount = BitConverter.ToInt32(br.ReadBytes(4), 0);
                article.Server = DecodeStreamBytes(br, article.ServerCharCount);
                return article;
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<Article> GetArticles(BinaryReader br)
        {
            Article article;
            var list = new List<Article>();
            do
            {
                if (br.PeekChar() == -1)
                {
                    break;
                }
                article = GetArticle(br);
                if (article != null)
                {
                    list.Add(article);
                }
            }
            while (article != null);
            return list;
        }

        public static Header GetHeader(BinaryReader br)
        {
            try
            {
                Header header = new Header();
                header.Identifier = br.ReadBytes(4);
                header.ArticleCount = br.ReadInt32();
                header.MBXBytes = br.ReadInt32();
                header.Unknown = br.ReadBytes(0x44);
                return header;
            }
            catch
            {
                return null;
            }
        }

        public static string GetHexString(IEnumerable<byte> bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte num in bytes)
            {
                if (string.Format("{0:X}", num).Length == 1)
                {
                    builder.Append("0");
                }
                builder.AppendFormat("{0:X} ", num);
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        public static IEnumerable<Article> ImportNZB(string filename, string server, ProgressBar progress)
        {
            progress.Value = 0;
            var list = new List<Article>();
            XmlDocument document = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    using (StringWriter writer = new StringWriter(sb))
                    {
                        string str;
                        do
                        {
                            str = reader.ReadLine();
                            if (((str != null) && (str != "")) && (str != "\n"))
                            {
                                writer.WriteLine(str);
                            }
                        }
                        while (str != null);
                    }
                }
            }
            catch
            {
                return null;
            }
            try
            {
                document.XmlResolver = null;
                StringReader txtReader = new StringReader(sb.ToString());
                document.Load(txtReader);
                if (document.InnerXml.IndexOf("xmlns=\"http://www.newzbin.com/DTD/2003/nzb\"") > 0)
                {
                    document.InnerXml = document.InnerXml.Replace("xmlns=\"http://www.newzbin.com/DTD/2003/nzb\"", "");
                }
                if (document.InnerXml.IndexOf("xmlns=\"http://www.newzbin.com/DTD/2004/nzb\"") > 0)
                {
                    document.InnerXml = document.InnerXml.Replace("xmlns=\"http://www.newzbin.com/DTD/2004/nzb\"", "");
                }
            }
            catch
            {
                return null;
            }
            if (document.DocumentElement == null)
            {
                return null;
            }
            Path.GetFileNameWithoutExtension(filename);
            progress.Maximum = document.DocumentElement.ChildNodes.Count;
            progress.Update();
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
				if (node.NodeType == System.Xml.XmlNodeType.Comment)
				{
					continue;
				}
                var c = new List<Article>();
                DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0);
                time = time.AddSeconds(double.Parse(node.SelectSingleNode("@date").Value));
                string str3 = node.SelectSingleNode("@poster").Value;
                int index = 0;
                string[] strArray = new string[node.SelectNodes("groups/group").Count];
                foreach (XmlNode node2 in node.SelectNodes("groups/group"))
                {
                    strArray[index] = node2.InnerText;
                    index++;
                }

                var segments = node.SelectNodes("segments/segment");

                var subject = node.SelectSingleNode("@subject").Value;
                var input = Regex.IsMatch(subject, @"\(\d+\/\d+\)") ? subject : subject.TrimEnd() + " (1/" + subject.Count() + ")";

                foreach (XmlNode node3 in segments)
                {
					int.Parse(node3.SelectSingleNode("@number").Value);
                    int num2 = int.Parse(node3.SelectSingleNode("@bytes").Value);
                    string innerText = node3.InnerText;
                    Article article = new Article();
                    article.SeqNumber = int.Parse(node3.SelectSingleNode("@number").Value);
                    article.Unknown0 = new byte[4];
                    article.Unknown1 = new byte[4];
                    article.Unknown2 = new byte[4];
                    article.Unknown3 = new byte[4];
                    article.ArticleNumber = 0;
                    article.CacheOffset = -1;
                    article.QueueOrder = -1;
                    article.Date = BitConverter.GetBytes(time.ToOADate());
                    article.Bytes = num2;
                    article.Lines = num2 / 40;
                    article.Username = str3;
                    article.UsernameCharCount = article.Username.Length;
                    article.Email = str3;
                    article.EmailCharCount = article.Email.Length;
                    article.Server = server + ":" + strArray[0];
                    article.ServerCharCount = article.Server.Length;
                    while (innerText.StartsWith("<"))
                    {
                        innerText = innerText.Remove(0, 1);
                    }
                    while (innerText.EndsWith(">"))
                    {
                        innerText = innerText.Remove(innerText.Length - 1, 1);
                    }
                    article.MessageId = "<" + innerText + ">";
                    article.MessageIdCharCount = article.MessageId.Length;
                    if (c.All(a => a.SeqNumber != article.SeqNumber))
                    {
                        c.Add(article);
                    }
                }
				c.Sort(new ArticleSortClass());
                int count = c.Count;
                Match match = new Regex(@"(^.*\()(\s*\d+\s*/\s*\d+\s*)(\).*$)", RegexOptions.Compiled).Match(input);
                if (match.Success)
                {
                    int num4 = 1;
                    foreach (Article article2 in c)
                    {
                        article2.Subject = string.Concat(new object[] { match.Groups[1].Value, num4.ToString(), "/", count.ToString(), match.Groups[3] });
                        article2.SubjectCharCount = article2.Subject.Length;
                        num4++;
                    }
                }
                else
                {
                    foreach (Article article3 in c)
                    {
                        article3.Subject = input;
                        article3.SubjectCharCount = article3.Subject.Length;
                    }
                }
                list.AddRange(c);
                progress.Value++;
                progress.Update();
            }
            return list;
        }

		private class ArticleSortClass : IComparer<Article>
		{
			// Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            public int Compare(Article x, Article y)
			{
				Article x1 = x;
				Article y1 = y;
				if (x1.SeqNumber < y1.SeqNumber)
				{
					return -1;
				}
				else if (x1.SeqNumber > y1.SeqNumber)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
			//IComparer.Compare
		}

        [STAThread]
        private static void Main(string[] args)
        {
            if (Environment.CurrentDirectory != Application.StartupPath)
            {
                Environment.CurrentDirectory = Application.StartupPath;
            }
            Application.Run(new MainForm());
        }

        public static void OutputArticle(Article article)
        {
            WriteVariable("Unknown", GetHexString(article.Unknown0));
            WriteVariable("MBR file offset", article.CacheOffset);
            WriteVariable("Article number", article.ArticleNumber);
            WriteVariable("Queue order", article.QueueOrder);
            WriteVariable("Unknown", GetHexString(article.Unknown1));
            WriteVariable("Unknown", GetHexString(article.Unknown2));
            WriteVariable("Byte count", article.Bytes);
            WriteVariable("Line count", article.Lines);
            WriteVariable("Date", GetHexString(article.Date));
            WriteVariable("MessageId count", article.MessageIdCharCount);
            WriteVariable("MessageId", article.MessageId);
            WriteVariable("Subject count", article.SubjectCharCount);
            WriteVariable("Subject", article.Subject);
            WriteVariable("Username count", article.UsernameCharCount);
            WriteVariable("Username", article.Username);
            WriteVariable("Email count", article.EmailCharCount);
            WriteVariable("Email", article.Email);
            WriteVariable("Unknown", GetHexString(article.Unknown3));
            WriteVariable("Server count", article.ServerCharCount);
            WriteVariable("Server", article.Server);
        }

        public static void OutputHeader(Header header)
        {
            WriteVariable("Identifier", GetHexString(header.Identifier));
            WriteVariable("Article count", header.ArticleCount);
            WriteVariable("MBX File Size", header.MBXBytes);
            for (int i = 0; i < header.Unknown.Length; i++)
            {
                var list = new List<byte>();
                while (i < header.Unknown.Length)
                {
                    list.Add(header.Unknown[i]);
                    i++;
                    if (((i + 1) % 9) == 0)
                    {
                        break;
                    }
                }
                WriteVariable("Unknown", GetHexString(list));
            }
        }

        public static bool WriteArticle(BinaryWriter bw, Article article)
        {
            try
            {
                bw.Write(article.Unknown0);
                bw.Write(BitConverter.GetBytes(article.CacheOffset));
                bw.Write(BitConverter.GetBytes(article.ArticleNumber));
                bw.Write(BitConverter.GetBytes(article.QueueOrder));
                bw.Write(article.Unknown1);
                bw.Write(article.Unknown2);
                bw.Write(BitConverter.GetBytes(article.Bytes));
                bw.Write(BitConverter.GetBytes(article.Lines));
                bw.Write(article.Date);
                bw.Write(BitConverter.GetBytes(article.MessageIdCharCount));
                bw.Write(Encoding.Default.GetBytes(article.MessageId));
                bw.Write(BitConverter.GetBytes(article.SubjectCharCount));
                bw.Write(Encoding.Default.GetBytes(article.Subject));
                bw.Write(BitConverter.GetBytes(article.UsernameCharCount));
                bw.Write(Encoding.Default.GetBytes(article.Username));
                bw.Write(BitConverter.GetBytes(article.EmailCharCount));
                bw.Write(Encoding.Default.GetBytes(article.Email));
                bw.Write(article.Unknown3);
                bw.Write(BitConverter.GetBytes(article.ServerCharCount));
                bw.Write(Encoding.Default.GetBytes(article.Server));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WriteHeader(BinaryWriter bw, Header header)
        {
            try
            {
                bw.Write(header.Identifier);
                bw.Write(header.ArticleCount);
                bw.Write(header.MBXBytes);
                bw.Write(header.Unknown);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void WriteQueue(BinaryWriter bw, Header header, IEnumerable<Article> articles)
        {
            header.ArticleCount = articles.Count();
            WriteHeader(bw, header);
            foreach (Article article in articles)
            {
                if (!WriteArticle(bw, article))
                {
                    return;
                }
            }
        }

        private static void WriteVariable(string name, object towrite)
        {
            name = name + ":";
            Console.WriteLine(name.PadRight(0x12, ' ') + towrite);
        }
    }
}
