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
        if (GameState.Transient.NextSpawn is Wand.Staff)
            return;
        GameState.Transient.NextSpawn = null;
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
}
