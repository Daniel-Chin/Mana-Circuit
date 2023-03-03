using System;
using Godot;

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
                        "I sell, you buy! Welcome~"
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
                        // maybe: reject only when shop despawns? to do that, comment out the line above.
                    } else {
                        GameState.Transient.NextSpawn = new NPC.WandSmith();
                    }
                    Director.EventFinished();
                    break;
                default:
                    Console.WriteLine("_step " + _step);
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

    public class Jumping : MagicEvent {
        public bool JumpDemo;
        private static readonly float ANIMATION_LENGTH = 2;
        private int _step;
        private float animationTime;
        private SpawnableSpecialUI _npcUI;
        private Sprite _sprite;
        public Jumping(SpawnableSpecialUI npcUI)
        {
            _step = 0;
            _npcUI = npcUI;
            JumpDemo = false;
        }
        public override void NextStep() {

            switch (_step)
            {
                case 0:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.SetFace((NPC)_npcUI.MySpawnable);
                    Director.MainUI.MyLowPanel.Display(
                        "I make skateboards as a hobby."
                    );
                    _step++;
                    break;
                case 1:
                    Director.MainUI.MyLowPanel.Display(
                        "Check out my latest design: *Jumper MK-I*!!!"
                    );
                    _step++;
                    break;
                case 2:
                    _sprite = new Sprite();
                    _sprite.Texture = GD.Load<Texture>("res://texture/misc/jumper.png");
                    _sprite.Scale = Params.SPRITE_SCALE;
                    Main.Singleton.MyWorld.AddChild(_sprite);
                    _step++;
                    animationTime = 0;
                    Process(0);
                    break;
                case 5:
                    Director.MainUI.MyLowPanel.Display(
                        "It converts 10% of your mana to jumping power."
                    );
                    _step++;
                    break;
                case 6:
                    Director.MainUI.MyLowPanel.Display(
                        "Try it! Hold J to jump."
                    );
                    Director.UnpauseWorld();
                    JumpDemo = true;
                    _step++;
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }
        public override void Process(float dt) {
            if (_step == 3) {
                float ratio = animationTime / ANIMATION_LENGTH;
                if (ratio > 1f) {
                    _step ++;
                    animationTime = 0;
                    _sprite.QueueFree();
                    return;
                }
                _sprite.Position = _npcUI.Position * (1f - ratio);
                animationTime += dt;
            } else if (_step == 4) {
                if (animationTime > 1) {
                    _step ++;
                    animationTime = 0;
                    NextStep();
                    return;
                }
                animationTime += dt;
            }
        }
    }
}
