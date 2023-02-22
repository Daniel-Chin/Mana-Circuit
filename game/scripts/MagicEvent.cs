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
            }
        }
        public override void Process(float dt)
        {
            if (_step <= 2)
            {
                if (GameState.Transient.LocationOffset.Length() >= 0.15)
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
}
