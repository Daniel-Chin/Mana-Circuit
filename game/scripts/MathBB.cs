
using System.Text;
using Godot;

public class MathBB
{
    static readonly int MAX_RAISE = 3;
    public class RaisableChar
    {
        public char Char;
        public int Raised;
        private float _fontHeight;
        public RaisableChar(char _char, int raised, float fontHeight)
        {
            Char = _char;
            Raised = raised;
            _fontHeight = fontHeight;
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
                    Shared.Assert(Raised <= MAX_RAISE);
                    sB.Append(new string('r', Raised));
                    break;
            }
            sB.Append(Char);
            sB.Append(".png");
            string imagePath = sB.ToString();
            float width = _fontHeight / 7f / 2f * GD.Load<Texture>(
                imagePath
            ).GetWidth();
            // 7f is the font's # of pixels in height
            return $"[img={width}]{imagePath}[/img]";
        }
    }
    public static void BuildExp(double ex, StringBuilder sB, float fontHeight)
    {
        if (ex >= 2)
        {
            foreach (char c in ex.ToString())
            {
                sB.Append(new RaisableChar(c, 1, fontHeight));
            }
        }
    }
    public static string Build(Simplest simplest) {
        return Build(simplest, Shared.FONT.GetHeight());
    }
    public static string Build(Simplest simplest, float fontHeight)
    {
        StringBuilder sB = new StringBuilder();
        double k = simplest.K;
        switch (simplest.MyRank)
        {
            case Rank.FINITE:
                if (k > 1000000000)
                {
                    sB.Append(k.ToString("e1"));
                    break;
                }
                if (k > 1000000)
                {
                    sB.Append((int)k / 1000000);
                    sB.Append('M');
                    break;
                }
                if (k > 1000)
                {
                    sB.Append((int)k / 1000);
                    sB.Append('K');
                    break;
                }
                if (k % 1 == 0)
                {
                    sB.Append((int)k);
                    break;
                }
                sB.Append(k.ToString("#.#"));
                break;
            case Rank.W_TO_THE_K:
                sB.Append(new RaisableChar('w', 0, fontHeight));
                BuildExp(k, sB, fontHeight);
                break;
            case Rank.TWO_TO_THE_W:
                sB.Append(2);
                sB.Append(new RaisableChar('w', 1, fontHeight));
                break;
            case Rank.STACK_W:
                for (int i = 0; i < k; i++)
                {
                    if (k - 1 <= MAX_RAISE)
                    {
                        sB.Append(new RaisableChar('w', i, fontHeight));
                    }
                    else
                    {
                        if (i != 0)
                            sB.Append('^');
                        sB.Append(new RaisableChar('w', 0, fontHeight));
                    }
                }
                break;
        }
        return sB.ToString();
    }
}
