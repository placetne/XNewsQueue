namespace xnewsqueue
{
    using System;

    internal class Article
    {
        public int ArticleNumber;
        public int Bytes;
        public int CacheOffset;
        public byte[] Date;
        public string Email;
        public int EmailCharCount;
        public int Lines;
        public string MessageId;
        public int MessageIdCharCount;
        public int QueueOrder;
		public int SeqNumber;
        public string Server;
        public int ServerCharCount;
        public string Subject;
        public int SubjectCharCount;
        public byte[] Unknown0;
        public byte[] Unknown1;
        public byte[] Unknown2;
        public byte[] Unknown3;
        public string Username;
        public int UsernameCharCount;
    }
}

