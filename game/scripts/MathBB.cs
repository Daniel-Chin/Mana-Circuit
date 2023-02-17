using System.Diagnostics;
using System.Text;
using Godot;

public class MathBB
{
    static readonly int MAX_RAISE = 3;
    private class RaisableChar
    {
        public char Char;
        public int Raised;
        public RaisableChar(char _char, int raised)
        {
            Char = _char;
            Raised = raised;
        }
        public override string ToString()
        {
            StringBuilder sB = new StringBuilder();
            sB.Append("res://texture/math/");
            switch (Raised)
            {
                case 0:
                    break;
                case 1:
                    sB.Append("raised_");
                    break;
                default:
                    Debug.Assert(Raised <= MAX_RAISE);
                    sB.Append(new string('r', Raised));
                    break;
            }
            sB.Append(Char);
            sB.Append(".png");
            string imagePath = sB.ToString();
            float width = Shared.FONT_SCALE * GD.Load<Texture>(
                imagePath
            ).GetWidth();
            return $"[img={width}]{imagePath}[/img]";
        }
    }
    public static string Build(Simplest simplest)
    {
        StringBuilder sB = new StringBuilder();
        double k = simplest.K;
        switch (simplest.MyRank)
        {
            case Rank.FINITE:
                sB.Append(k);
                break;
            case Rank.W_TO_THE_K:
                sB.Append(new RaisableChar('w', 0));
                if (k >= 2)
                    foreach (char c in k.ToString())
                    {
                        sB.Append(new RaisableChar(c, 1));
                    }
                break;
            case Rank.TWO_TO_THE_W:
                sB.Append(2);
                sB.Append(new RaisableChar('w', 1));
                break;
            case Rank.STACK_W:
                for (int i = 0; i < k; i++)
                {
                    if (k - 1 <= MAX_RAISE)
                    {
                        sB.Append(new RaisableChar('w', i));
                    }
                    else
                    {
                        if (i != 0)
                            sB.Append('^');
                        sB.Append(new RaisableChar('w', 0));
                    }
                }
                break;
        }
        return sB.ToString();
    }
}
