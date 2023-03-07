using System;
using System.Text;

public class CustomGem : Gem
{
    public Simplest MetaLevel { get; set; }
    public Circuit MyCircuit { get; set; }  // only one source one drain
    public Simplest CachedAdder { get; set; }
    public Simplest CachedMultiplier { get; set; }
    public CustomGem(Simplest metaLevel) : base()
    {
        MetaLevel = metaLevel;
        CachedAdder = null;
        CachedMultiplier = null;
        initCircuit();
    }
    private void initCircuit()
    {
        int n = 5;
        MyCircuit = new Circuit(new PointInt(n, n));
        for (int i = 0; i < n; i++)
        {
            MyCircuit.Add(new Gem.Wall(), new PointInt(0, i), true);
            MyCircuit.Add(new Gem.Wall(), new PointInt(n - 1, i), true);
            MyCircuit.Add(new Gem.Wall(), new PointInt(i, 0), true);
            MyCircuit.Add(new Gem.Wall(), new PointInt(i, n - 1), true);
        }

        PointInt location;
        location = new PointInt(0, n / 2);
        MyCircuit.Remove(location);
        MyCircuit.Add(new Gem.Source(new PointInt(1, 0)), location);

        location = new PointInt(n - 1, n / 2);
        MyCircuit.Remove(location);
        MyCircuit.Add(new Gem.Drain(), location);
    }
    public override string Name()
    {
        return "custom";
    }
    public override Particle Apply(Particle input)
    {
        Shared.Assert(CachedMultiplier != null);
        input.Multiply(CachedMultiplier);
        if (CachedAdder != null)
        {
            input.Mana[0] = Simplest.Eval(
                input.Mana[0], Operator.PLUS, CachedAdder
            );
        }
        return input;
    }
    public void Eval()
    {
        CachedAdder = null;
        if (MetaLevel.MyRank != Rank.FINITE)
        {
            // typeless custom gem
            // recursion makes the system of eq. non-linear, 
            // so I can't solve for finite solutions just by 
            // matrix division. Give up. Just brute force. 
            CachedMultiplier = new MagicProblem(MyCircuit).SolveTypeless(this);
            return;
        }
        // typed custom gem
        Simplest lfs1 = new MagicProblem(MyCircuit).SolveTyped(1);
        if (lfs1.MyRank == Rank.FINITE)
        {
            Simplest lfs0 = new MagicProblem(MyCircuit).SolveTyped(0);
            CachedAdder = lfs0;
            CachedMultiplier = new Simplest(Rank.FINITE, lfs1.K - lfs0.K);
        }
        else
        {
            CachedMultiplier = lfs1;
        }
    }

    public override string DisplayName()
    {
        if (MetaLevel.MyRank != Rank.FINITE)
            return "Typeless Custom Gem";
        int k = (int)MetaLevel.K;
        if (k == 0)
            return "Custom Gem";
        if (k == 1)
            return "Meta Custom Gem";
        if (k == 2)
            return "Meta Meta Custom Gem";
        StringBuilder sB = new StringBuilder();
        sB.Append("Meta");
        MathBB.BuildExp(k, sB, Shared.FONT.GetHeight());
        sB.Append(" Custom Gem");
        return sB.ToString();
    }
    public override string Explain(bool inCG)
    {
        if (MetaLevel.MyRank == Rank.FINITE)
        {
            StringBuilder sB = new StringBuilder();
            sB.Append(DisplayName());
            sB.Append(". Can embed ");
            switch (MetaLevel.K)
            {
                case 0:
                    sB.Append("gems. ");
                    break;
                case 1:
                    sB.Append("gems and Custom Gems. ");
                    break;
                case 2:
                    sB.Append("gems, Custom Gems, and Meta Custom Gems. ");
                    break;
                default:
                    sB.Append("up to ");
                    sB.Append(new CustomGem(new Simplest(
                        Rank.FINITE, MetaLevel.K - 1
                    )).DisplayName());
                    sB.Append("s. ");
                    break;
            }
            return sB.ToString();
        }
        else
        {
            return "Typeless Custom Gem. Can embed up to Typeless Custom Gems.";
        }
    }
}
