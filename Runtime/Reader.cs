using System;
using System.Text.RegularExpressions;

namespace Sand
{
    public class Reader
    {
        public string Buffer { get; private set; }
        private readonly string delimiter;
        private readonly Action<string> onReadLine;

        public Reader(Action<string> onReadLine, string delimiter)
        {
            this.onReadLine = onReadLine;
            this.delimiter = delimiter; ;
        }

        public void OnReceiveData(string data)
        {
            string stringData = Buffer + data;
            string[] lines = Regex.Split(stringData, delimiter);
            int length = lines.Length - 1;
            
            if (length == 0)
            {
                Buffer = stringData;
            }
            else if (lines[length] != "")
            {
                Buffer = lines[length];
            }
            else
            {
                Buffer = "";
            }

            if (onReadLine != null && length > 0)
                for (int i = 0; i < length; i++)
                    onReadLine(lines[i]);
        }
    }
}
