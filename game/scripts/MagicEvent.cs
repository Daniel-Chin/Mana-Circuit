public abstract class MagicEvent : Godot.Object
{
    // Part of the game state is in the event. 
    public abstract void NextStep();
    public abstract void Process(float dt);
    public virtual void ButtonClicked(int buttonID) {
        throw new Shared.ObjectStateIllegal();
    }
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
                    Director.UnpauseWorld();
                    _step++;
                    break;
                case 1:
                    Director.UnpauseWorld();
                    _step++;
                    // wait for movement
                    break;
                case 2:
                    Director.MainUI.MyLowPanel.Display(
                        "You feel urged to discover this game's true content."
                    );
                    Director.PauseWorld();
                    _step++;
                    break;
                case 3:
                    Director.MainUI.MyLowPanel.Display(
                        "However, you can't shake off the suspicion that your previous thought was injected by the narrator."
                    );
                    Director.PauseWorld();
                    _step++;
                    break;
                case 4:
                    Director.UnpauseWorld();
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
                    Director.PauseWorld();
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
                    Director.UnpauseWorld();
                    _step++;
                    break;
                case 5:
                    if (Attacked)
                    {
                        GameState.Persistent.Event_Staff = true;
                        SaveLoad.Save();
                        Director.MainUI.VBoxLowPanel.Visible = false;
                        Director.EventFinished();
                    }
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }

        public override void Process(float dt) { }
    }

    public class Shopping : MagicEvent {
        private int _step;
        private NPC _npc;
        public Shopping(NPC npc)
        {
            _step = 0;
            _npc = npc;
        }
        public override void Process(float dt) { }
        public override void NextStep() {

            switch (_step)
            {
                case 0:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.SetFace(_npc);
                    Director.MainUI.MyLowPanel.Display(
                        "Welcome to the shop!"
                    );
                    Director.MainUI.MyLowPanel.SetButtons(
                        "Buy gems", "Upgrade wand"
                    );
                    _step++;
                    break;
                case 2:
                    SaveLoad.Save();
                    Director.UnpauseWorld();
                    if (GameState.Persistent.HasGems["addOne"] == 0) {
                        GameState.Transient.EventRejected();
                    } else {
                        GameState.Transient.NextSpawn = null;
                    }
                    Director.EventFinished();
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }

        public override void ButtonClicked(int buttonID)
        {
            Director.MainUI.MyLowPanel.NoButtons();
            if (buttonID == 0) {
                // Buy gems
                GemListScene gemListScene = new GemListScene();
                Director.MainUI.AddChild(gemListScene);
                gemListScene.ListBuyable();
                gemListScene.Connect(
                    "finished", this, "Bye"
                );
                gemListScene.PopupCentered();
            } else if (buttonID == 1) {
                // Upgrade wand
                UpgradeWand upgradeWand = new UpgradeWand();
                Director.MainUI.AddChild(upgradeWand);
                upgradeWand.Connect(
                    "finished", this, "Bye"
                );
                upgradeWand.PopupCentered();
            }
        }

        public void Bye() {
            Director.MainUI.MyLowPanel.SetFace(_npc);
            Director.MainUI.MyLowPanel.Display(Tip());
            _step++;
        }

        private string Tip() {
            GameState.Persistent.ShopTip ++;
            GameState.Persistent.ShopTip %= 2;
            switch (GameState.Persistent.ShopTip) {
                case 1:
                    if (GameState.Persistent.Money <= Simplest.Finite(8))
                        return "There are more shops ahead. Don't worry --- they appear quite frequently.";
                    return "A good mage needs 1% grinding and 99% thinking.";
                case 0:
                    return "Remember: The further you go, the stronger the enemies!";
                default:
                    throw new Shared.ValueError();
            }
        }
    }
}
