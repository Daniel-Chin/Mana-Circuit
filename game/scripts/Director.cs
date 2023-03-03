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
        if (
            GameState.Persistent.Money >= Simplest.Finite(4) &&
            GameState.Persistent.HasGems["addOne"] == 0
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

    public static void StartEvent(MagicEvent e)
    {
        Shared.Assert(NowEvent == null);
        NowEvent = e;
        NowEvent.NextStep();
    }

    public static void OnSpecialSpawn()
    {

    }
    public static void OnSpecialDespawn(SpawnableSpecial s)
    {
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
}
