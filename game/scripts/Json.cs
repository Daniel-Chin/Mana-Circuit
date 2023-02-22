using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

public interface JSONable
{
    void ToJSON(StreamWriter writer);
}

public class JList<T> : List<T>, JSONable
{
    public void ToJSON(StreamWriter writer)
    {
        writer.WriteLine("[");
        foreach (T item in this)
        {
            ((JSONable)item).ToJSON(writer);
        }
        writer.WriteLine("],");
    }
    public JSONable FromJSON(StreamReader reader)
    {
        JList<T> list = new JList<T>();
        Debug.Assert(reader.ReadLine().Equals("["));
        while (true)
        {
            if (reader.Peek() == ']')
            {
                Debug.Assert(reader.ReadLine().Equals("],"));
                return list;
            }
            switch (typeof(T))
            {
                case var value when value == typeof(PointInt):
                    list.Add((T)(object)PointInt.FromJSON(reader));
                    break;
                    // case var value when value == typeof(Gem):
                    //     list.Add((T)(object)Gem.FromJSON(reader));
                    //     break;
            }
        }
    }
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
        Debug.Assert((char)reader.Read() == '"');
        bool escaping = false;
        StringBuilder sB = new StringBuilder();
        while (true)
        {
            char c = (char)reader.Read();
            if (escaping)
            {
                sB.Append(c);
                escaping = false;
                continue;
            }
            if (c == '"')
                break;
            if (c == 'n')
            {
                sB.Append('\n');
                continue;
            }
        }
        Debug.Assert((char)reader.Read() == '\n');
        return sB.ToString();
    }
}
