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
        if (NowEvent != null)
            throw new Shared.ObjectStateIllegal();
        if (!GameState.Persistent.Event_Intro)
        {
            NowEvent = new MagicEvent.Intro();
            NowEvent.NextStep();
            return;
        }
        if (!GameState.Persistent.Event_Staff)
        {
            Wand staff = new Wand.Staff();
            staff.Init();
            GameState.Transient.NextSpawn = (Spawnable)staff;
            GameState.Transient.EnemiesTillNextSpawn = 0;
            return;
        }
    }

    public static void OnSpawn()
    {

    }
    public static void OnDespawn()
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
        if (NowEvent != null)
            NowEvent.Process(dt);
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
}
