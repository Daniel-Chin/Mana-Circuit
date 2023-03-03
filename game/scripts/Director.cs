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
        Shared.Assert(NowEvent == null);
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
    }

    public static void StartEvent(MagicEvent e)
    {
        Shared.Assert(NowEvent == null);
        NowEvent = e;
        NowEvent.NextStep();
    }

    public static void OnSpecialSpawn()
    {

    }
    public static void OnSpecialDespawn()
    {
        if (GameState.Transient.NextSpawn is Wand.Staff)
            return;
        GameState.Transient.EnemiesTillNextSpawn = Shared.Rand.Next(2, 4);
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
        if (
            GameState.Persistent.Money >= Simplest.Finite(4) &&
            GameState.Persistent.HasGems["addOne"].Equals(Simplest.Zero())
        )
        {
            if (GameState.Transient.NextSpawn is NPC.Shop) 
            {
                ;
            } else {
                Shared.Assert(GameState.Transient.NextSpawn == null);
                GameState.Transient.NextSpawn = new NPC.Shop();
                GameState.Transient.EnemiesTillNextSpawn = 0;
            }
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
}
