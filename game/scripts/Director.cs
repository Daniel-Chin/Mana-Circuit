using System;

public class Director
{
    public static Main MainUI;
    public static MagicEvent NowEvent;
    static Director()
    {
        NowEvent = null;
    }

    public static void CheckEvent()
    {
        if (NowEvent != null) return;
        if (!GameState.Persistent.Event_Intro)
        {
            StartEvent(new MagicEvent.Intro());
            return;
        }
        if (!GameState.Persistent.Event_Staff)
        {
            Wand staff = new Wand.Staff();
            staff.Init();
            GameState.Transient.NextSpawn = (SpawnableSpecial)staff;
            GameState.Transient.EnemiesTillNextSpawn = 0;
            return;
        }
        if (SpawnShopIf(Simplest.Finite(4), (new Gem.AddOne(), 0)))
            return;
        if (SpawnShopIf(Simplest.Finite(6), (new Gem.WeakMult(), 0)))
            return;
        if (SpawnShopIf(Simplest.Finite(9), (new Gem.AddOne(), 1)))
            return;
        if (
            GameState.Persistent.MyWand is Wand.Staff
            && SpawnShopIf(Simplest.Finite(13))
        )
            return;
        if (FillGuitar())
            return;
    }

    public static void StartEvent(MagicEvent e)
    {
        Shared.Assert(NowEvent == null);
        NowEvent = e;
        NowEvent.NextStep();
    }

    public static void SpecialSpawned()
    {
        if (GameState.Transient.NextSpawn is NPC.WandSmith)
            GameState.Transient.NextSpawn = null;
    }
    public static void SpecialDespawned(
        SpawnableSpecial s, bool exposed
    )
    {
        if (! exposed)
            return;
        if (GameState.Transient.NextSpawn == null)
            return;
        if (GameState.Transient.NextSpawn is Wand.Staff)
            return;
        if (GameState.Transient.NextSpawn == s)
            GameState.Transient.EventRejected();
    }

    public static void OnEventStepComplete()
    {
        NowEvent.NextStep();
    }

    public static void Process(float dt)
    {
        if (NowEvent != null) {
            NowEvent.Process(dt);
            return;
        }
    }

    public static void EventFinished()
    {
        NowEvent = null;
        CheckEvent();
    }

    public static bool CanSpawnNonevent()
    {
        if (
            GameState.Transient.NextSpawn != null
            && GameState.Transient.EnemiesTillNextSpawn == 0
        ) return false;
        if (
            !GameState.Persistent.Event_Intro
            || !GameState.Persistent.Event_Staff
        ) return false;
        return true;
    }

    public static void WandAttacked()
    {
        if (NowEvent is MagicEvent.Staff e)
        {
            e.Attacked = true;
            e.NextStep();
        }
    }

    public static void PauseWorld() {
        MainUI.MyMageUI.Resting();
        GameState.Transient.WorldPaused = true;
    }
    public static void UnpauseWorld() {
        GameState.Transient.WorldPaused = false;
    }

    private static bool SpawnShopIf(
        Simplest money, Tuple<Gem, int> maxGems
    ) {
        if (!(GameState.Persistent.Money >= money))
            return false;
        if (maxGems is Tuple<Gem, int> _maxGems) {
            var (gem, num) = _maxGems;
            if (GameState.Persistent.HasGems[gem.Name()] > num)
                return false;
        }
        if (GameState.Transient.NextSpawn is NPC.Shop) 
        {
            ;
        } else {
            Console.WriteLine("Shop as event");
            if (GameState.Transient.NextSpawn == null) {
                GameState.Transient.NextSpawn = new NPC.Shop();
                GameState.Transient.EnemiesTillNextSpawn = 0;
            }
        }
        return true;
    }
    private static bool SpawnShopIf(
        Simplest money
    ) {
        return SpawnShopIf(money, null);
    }
    private static bool SpawnShopIf(
        Simplest money, (Gem, int) maxGems
    ) {
        return SpawnShopIf(money, new Tuple<Gem, int>(
            maxGems.Item1, maxGems.Item2
        ));
    }

    private static bool FillGuitar() {
        if (! (GameState.Persistent.MyWand is Wand.Guitar))
            return false;
        double needMoney = 0;
        Gem gem;
        gem = new Gem.Focus(null);
        if (GameState.Persistent.CountGemsOwned(gem).Equals(Simplest.Zero()))
            needMoney += NPC.Shop.PriceOf(gem).K;
        gem = new Gem.Mirror(false);
        if (GameState.Persistent.CountGemsOwned(gem) <= Simplest.One())
            needMoney += NPC.Shop.PriceOf(gem, 1).K;
        if (GameState.Persistent.CountGemsOwned(gem).Equals(Simplest.Zero()))
            needMoney += NPC.Shop.PriceOf(gem, 0).K;
        if (needMoney == 0)
            return false;
        return SpawnShopIf(Simplest.Finite(needMoney));
    }
}
