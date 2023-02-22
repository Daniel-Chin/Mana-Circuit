using System.IO;
using System.Text;

using System.Collections.Generic;

public interface JSONable
{
    void ToJSON(StreamWriter writer);
}

public class JSON
{
    public static void Store(string s, StreamWriter writer)
    {
        writer.Write('"');
        foreach (char c in s)
        {
            switch (c)
            {
                case '\\':
                    writer.Write("\\\\");
                    break;
                case '\n':
                    writer.Write("\\n");
                    break;
                case '"':
                    writer.Write("\\\"");
                    break;
                default:
                    writer.Write(c);
                    break;
            }
        }
        writer.WriteLine("\",");
    }
    public static string ParseString(StreamReader reader)
    {
        Shared.Assert((char)reader.Read() == '"');
        bool escaping = false;
        StringBuilder sB = new StringBuilder();
        while (true)
        {
            char c = (char)reader.Read();
            if (escaping)
            {
                escaping = false;
                if (c == 'n')
                {
                    sB.Append('\n');
                    continue;
                }
                sB.Append(c);
            }
            else
            {
                if (c == '"')
                    break;
                if (c == '\\')
                {
                    escaping = true;
                    continue;
                }
                sB.Append(c);
            }
        }
        Shared.Assert(reader.ReadLine() == ",");
        return sB.ToString();
    }

    public static void Store(bool b, StreamWriter writer)
    {
        if (b)
        {
            writer.WriteLine("true,");
        }
        else
        {
            writer.WriteLine("false,");
        }
    }
    public static bool ParseBool(StreamReader reader)
    {
        switch (reader.ReadLine())
        {
            case "true,":
                return true;
            case "false,":
                return false;
        }
        throw new Shared.ValueError();
    }

    public static string NoLast(StreamReader reader)
    {
        string x = reader.ReadLine();
        return x.Substring(0, x.Length - 1);
    }

    public static bool DidArrayEnd(StreamReader reader)
    // this function consumes the trailing "],"
    {
        if (reader.Peek() == ']')
        {
            Shared.Assert(reader.ReadLine().Equals("],"));
            return true;
        }
        return false;
    }
}
