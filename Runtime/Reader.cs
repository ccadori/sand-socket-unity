using System;

public class Reader
{
    private string buffer = "";
    private readonly char[] delimiter;
    private readonly Action<string> onReadLine;

    public Reader(Action<string> onReadLine, char[] delimiter)
    {
        this.onReadLine = onReadLine;
        this.delimiter = delimiter; ;
    }

    public void OnReceiveData(string data)
    {
        string stringData = buffer + data;
        string[] lines = stringData.Split(delimiter);
        int length = lines.Length - 1;
        buffer = length > 1 ? lines[length] : "";

        if (onReadLine != null)
            for (int i = 0; i < length; i++)
                onReadLine(lines[i]);
           
    }
}
