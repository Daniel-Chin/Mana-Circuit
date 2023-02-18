using System;
using System.Diagnostics;

public abstract class EstimateLFS
{
    public abstract bool SolveRank(Rank rank, int k);
    public Simplest Search()
    {
        for (int k = 1; k < 69; k++)
        {
            if (SolveRank(Rank.W_TO_THE_K, k))
                return new Simplest(Rank.W_TO_THE_K, k);
        }
        if (SolveRank(Rank.TWO_TO_THE_W, -1))
            return new Simplest(Rank.TWO_TO_THE_W, -1);
        for (int k = 2; k < 69; k++)
        {
            if (SolveRank(Rank.STACK_W, k))
                return new Simplest(Rank.STACK_W, k);
        }
        throw new Shared.PlayerCreatedEpsilonNaught();
        // and I don't know how they did it
    }
}
