using System;

public abstract class EstimateLFS
{
    public abstract bool SolveRank(Rank rank, int k);
    public Simplest Search()
    {
        if (SolveRank(Rank.TWO_TO_THE_W, -1))
        {
            Console.WriteLine("TWO_TO_THE_W is a solution.");
            ExponentialSearch eSearch = new ExponentialSearch(2333);
            try
            {
                while (true)
                {
                    if (eSearch.Feedback(SolveRank(Rank.W_TO_THE_K, eSearch.Acc)))
                        break;
                }
            }
            catch (ExponentialSearch.SearchFailed)
            {
                return new Simplest(Rank.TWO_TO_THE_W, -1);
            }
            return new Simplest(Rank.W_TO_THE_K, eSearch.High);
        }
        else
        {
            for (int k = 2; k < 69; k++)
            {
                if (SolveRank(Rank.STACK_W, k))
                    return new Simplest(Rank.STACK_W, k);
            }
            throw new Shared.PlayerCreatedEpsilonNaught();
            // and I don't know how they did it
        }
    }
}
