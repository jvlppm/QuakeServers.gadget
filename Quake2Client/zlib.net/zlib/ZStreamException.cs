namespace zlib
{
    using System;
    using System.IO;

    [Serializable]
    public class ZStreamException : IOException
    {
        public ZStreamException()
        {
        }

        public ZStreamException(string s) : base(s)
        {
        }
    }
}

