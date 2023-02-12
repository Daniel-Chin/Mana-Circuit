using System;

public class ExponentialSearch
{
    public class SearchFailed : Exception { }

    public int Acc;
    private int StopAt;
    private bool StillExponential;
    public int Low;
    public int High;

    public ExponentialSearch(int stopAt)
    {
        StopAt = stopAt;
        Acc = 1;
        StillExponential = true;
        Low = 1;
    }

    public bool Feedback(bool tooLarge)
    {
        if (StillExponential)
        {
            if (tooLarge)
            {
                StillExponential = false;
                High = Acc;
                Acc = (Low + High) / 2;
                return false;
            }
            else
            {
                Low = Acc;
                Acc *= 2;
                if (Acc >= StopAt) throw new SearchFailed();
                return false;
            }
        }
        else
        {
            if (tooLarge)
            {
                High = Acc;
            }
            else
            {
                Low = Acc;
            }
            Acc = (Low + High) / 2;
            return High <= Low + 1;
        }
    }
}
