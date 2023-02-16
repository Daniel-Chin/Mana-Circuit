using System.Collections.Generic;

namespace GameState
{
    public static class Persistent
    {
        public static Simplest Money;
        public static (Simplest, Simplest) Location;
        public static int WandID;
        public static Dictionary<string, int> HasGems;
        public static Dictionary<int, CustomGem> HasCustomGems;
        public static CustomGem HasTypelessGem;
        public static void Init()
        {
            HasGems = new Dictionary<string, int>();
            HasCustomGems = new Dictionary<int, CustomGem>();
            HasTypelessGem = null;

            // debug
            HasGems.Add("addOne", 1);
            HasGems.Add("weakMult", 9);
            HasGems.Add("focus", 99);
            HasGems.Add("mirror", 99);
            HasGems.Add("stochastic", 99);
            HasCustomGems.Add(0, new CustomGem(new Simplest(Rank.FINITE, 0)));
            HasCustomGems.Add(1, new CustomGem(new Simplest(Rank.FINITE, 1)));
            HasCustomGems.Add(3, new CustomGem(new Simplest(Rank.FINITE, 3)));
            HasCustomGems.Add(6, new CustomGem(new Simplest(Rank.FINITE, 6)));
            HasTypelessGem = new CustomGem(new Simplest(Rank.W_TO_THE_K, 1));
        }
        public static void WriteDisk() { }
        public static void LoadDisk() { }
    }
    public static class Transient
    {
        public static void Init() { }
    }
}
