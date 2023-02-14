using System;
using System.Diagnostics;

class CustomGem : Gem
{
    public Simplest MetaLevel;
    public Circuit MyCircuit;  // only one source one drain
    public Simplest CachedAdder;
    public Simplest CachedMultiplier;
    public CustomGem() : base()
    {
        CachedAdder = null;
        CachedMultiplier = null;
    }
    public override Particle Apply(Particle input)
    {
        Debug.Assert(CachedMultiplier != null);
        input.Multiply(CachedMultiplier);
        if (CachedMultiplier != null)
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
        Simplest mse1 = MyCircuit.MinimumSuperpositionEquilibrium(1);
        if (mse1.MyRank == Rank.FINITE)
        {
            Simplest mse0 = MyCircuit.MinimumSuperpositionEquilibrium(0);
            CachedAdder = mse0;
            CachedMultiplier = new Simplest(Rank.FINITE, mse1.K - mse0.K);
        }
        else
        {
            CachedMultiplier = mse1;
        }
    }
}
