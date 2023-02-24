public abstract class MagicEvent
{
    // Part of the game state is in the event. 
    public abstract void NextStep();
    public abstract void Process(float dt);
    public class Intro : MagicEvent
    {
        private int _step;
        public Intro()
        {
            _step = 0;
        }
        public override void NextStep()
        {
            switch (_step)
            {
                case 0:
                    Director.MainUI.MyLowPanel.SetFace(null);
                    Director.MainUI.MyLowPanel.Display(
                        "You instictively know to *hold* the *Right Mouse Button*."
                    );
                    GameState.Transient.NPCPausedWorld = false;
                    _step++;
                    break;
                case 1:
                    GameState.Transient.NPCPausedWorld = false;
                    _step++;
                    // wait for movement
                    break;
                case 2:
                    Director.MainUI.MyLowPanel.Display(
                        "You feel urged to discover this game's true content."
                    );
                    GameState.Transient.NPCPausedWorld = true;
                    _step++;
                    break;
                case 3:
                    Director.MainUI.MyLowPanel.Display(
                        "However, you can't shake off the suspicion that your previous thought was injected by the narrator."
                    );
                    GameState.Transient.NPCPausedWorld = true;
                    _step++;
                    break;
                case 4:
                    GameState.Transient.NPCPausedWorld = false;
                    EventFinished();
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }
        public override void Process(float dt)
        {
            if (_step <= 2)
            {
                if (GameState.Transient.LocationOffset.Length() >= 0.1)
                {
                    NextStep();
                }
            }
        }
        private void EventFinished()
        {
            GameState.Persistent.Event_Intro = true;
            Director.EventFinished();
        }
    }

    public class Staff : MagicEvent
    {
        private int _step;
        public bool Attacked;
        public Staff()
        {
            _step = 0;
            Attacked = false;
        }
        public override void NextStep()
        {
            switch (_step)
            {
                case 0:
                    Director.MainUI.MyLowPanel.SetFace(null);
                    Director.MainUI.MyLowPanel.Display(
                        "You pick up the *staff*."
                    );
                    GameState.Transient.NPCPausedWorld = true;
                    _step++;
                    break;
                case 1:
                    Director.MainUI.MyLowPanel.Display(
                        "On one end, a *radiator*. A rare ore that naturally emits weak mana."
                    );
                    _step++;
                    break;
                case 2:
                    Director.MainUI.MyLowPanel.Display(
                        "On the other end, a *crystalizer*. A crystal to store unlimited mana."
                    );
                    _step++;
                    break;
                case 3:
                    Director.MainUI.MyLowPanel.Display(
                        "To use it..."
                    );
                    _step++;
                    break;
                case 4:
                    Director.MainUI.MyLowPanel.Display(
                        "*Hold* the left mouse button!"
                    );
                    GameState.Transient.NPCPausedWorld = false;
                    _step++;
                    break;
                case 5:
                    if (Attacked)
                    {
                        GameState.Persistent.Event_Staff = true;
                        SaveLoad.Save();
                        Director.MainUI.MyLowPanel.Visible = false;
                        Director.EventFinished();
                    }
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }

        public override void Process(float dt) { }
    }
}
