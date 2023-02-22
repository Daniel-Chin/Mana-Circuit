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
        }
    }

    public static void OnEventStepCompelte()
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
    }
}
