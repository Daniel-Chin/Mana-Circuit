using System;
using Godot;

public abstract class MagicEvent : Godot.Object
{
    // Part of the game state is in the event. 
    public abstract void NextStep();
    public virtual void Process(float dt) {}
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
                        "You instinctively know to *hold* the *Right Mouse Button*."
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
            GameState.Persistent.Sema.WaitOne();
            GameState.Persistent.Event_Intro = true;
            GameState.Persistent.Sema.Release();
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
                        "On one end, a *radiator* [img=30]res://texture/gem/source.png[/img] . A rare ore that naturally emits weak mana."
                    );
                    _step++;
                    break;
                case 2:
                    Director.MainUI.MyLowPanel.Display(
                        "On the other end, a *crystalizer* [img=30]res://texture/gem/drain.png[/img] . A crystal to store unlimited mana."
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
                        GameState.Persistent.Sema.WaitOne();
                        GameState.Persistent.Event_Staff = true;
                        GameState.Persistent.Sema.Release();
                        SaveLoad.SaveAsync();
                        Director.MainUI.VBoxLowPanel.Visible = false;
                        Director.EventFinished();
                    }
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }
    }

    public class Shopping : MagicEvent {
        public const int SHOP_TIP_TOP = 100;
        private int _step;
        private NPC _npc;
        private Gem _strongMult;
        public Shopping(NPC npc)
        {
            _step = 0;
            _npc = npc;
        }
        public override void NextStep() {

            switch (_step)
            {
                case 0:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.SetFace(_npc);
                    Director.MainUI.MyLowPanel.Display(
                        "Wanna spend some [color=yellow]$[/color]?"
                    );
                    Director.MainUI.MyLowPanel.SetButtons(
                        "Buy gems", "Upgrade wand"
                    );
                    Director.MainUI.MyLowPanel.AcceptClick = true;
                    _step++;
                    break;
                case 1:
                    // cancelled selection
                    _step = 6;
                    NextStep();
                    break;
                case 6:
                    SaveLoad.SaveAsync();
                    Director.UnpauseWorld();
                    if (GameState.Persistent.HasGems["addOne"] == 0) {
                        GameState.Transient.EventRejected();
                        // maybe: reject only when shop despawns? to do that, comment out the line above.
                    } else {
                        GameState.Persistent.Sema.WaitOne();
                        GameState.Persistent.Loneliness_WandSmith = Params.NPC_LINELINESS_MAX;
                        GameState.Persistent.Sema.Release();
                        if (GameState.Transient.NextSpawn is NPC.Shop)
                            GameState.Transient.NextSpawn = null;
                    }
                    Director.EventFinished();
                    break;
                case 10:
                    Director.MainUI.MyLowPanel.Display(
                        "Wait... Your wand is already max-tier."
                    );
                    _step++;
                    break;
                case 11:
                    if (GameState.Transient.Mana >= Simplest.Finite(4000)) {
                        if (GameState.Transient.Mana.MyRank == Rank.FINITE) {
                            if (GameState.Persistent.HasCustomGems.ContainsKey(0)) {
                                Director.MainUI.MyLowPanel.Display(
                                    "You should focus on customizing gems."
                                );
                            } else {
                                Director.MainUI.MyLowPanel.Display(
                                    "Buy some better gems, I guess?"
                                );
                            }
                        } else {
                            Director.MainUI.MyLowPanel.Display(
                                "Only the Gem Expert can help you now."
                            );
                        }
                    } else {
                        Director.MainUI.MyLowPanel.Display(
                            "To realize its true potential, try redesigning its circuit."
                        );
                    }
                    _step++;
                    break;
                case 12:
                    Director.UnpauseWorld();
                    Director.EventFinished();
                    break;
                case 20:
                    Director.MainUI.MyLowPanel.Display(
                        "*" + GameState.Persistent.MyWand.DisplayName()
                        + "*. The ultimate wand."
                    );
                    _step++;
                    break;
                case 21:
                    Director.MainUI.MyLowPanel.Display(
                        "Only few mages can realize its true potential."
                    );
                    _step++;
                    break;
                case 22:
                    SaveLoad.SaveAsync();
                    Director.UnpauseWorld();
                    GameState.Persistent.Sema.WaitOne();
                    GameState.Persistent.Loneliness_WandSmith = Params.NPC_LINELINESS_MAX;
                    GameState.Persistent.Sema.Release();
                    if (GameState.Transient.NextSpawn is NPC.Shop)
                        GameState.Transient.NextSpawn = null;
                    Director.EventFinished();
                    break;
                case 30:
                    Director.MainUI.MyLowPanel.Display(
                        "*" + GameState.Persistent.MyWand.DisplayName()
                        + "*... It has three radiators. Concentrate their mana into the neck!"
                    );
                    _step = 22;
                    break;
                case 40:
                    Director.MainUI.MyLowPanel.SetFace(_npc);
                    Director.MainUI.MyLowPanel.Display(
                        "You can see Custom Gems now?"
                    );
                    _step++;
                    break;
                case 41:
                    Director.MainUI.MyLowPanel.Display(
                        "It's time. You will need this:"
                    );
                    _step++;
                    break;
                case 42:
                    _strongMult = new Gem.StrongMult();
                    Director.MainUI.MyLowPanel.SetFace(null);
                    Director.MainUI.MyLowPanel.Display(
                        $"+ *{_strongMult.DisplayName()}* obtained! +", 
                        true
                    );
                    GameState.Persistent.Sema.WaitOne();
                    GameState.Persistent.HasGems[_strongMult.Name()] = 1;
                    GameState.Persistent.Sema.Release();
                    _step++;
                    break;
                case 43:
                    Director.MainUI.MyLowPanel.SetFace(_npc);
                    Director.MainUI.MyLowPanel.Display(
                        $"There is only one {_strongMult.DisplayName()} in the world. Use it wisely."
                    );
                    _step = 6;
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
                _step = 5;
                GemListScene gemListScene = new GemListScene();
                Director.MainUI.AddChild(gemListScene);
                gemListScene.ListBuyable();
                gemListScene.Connect(
                    "finished", this, "Bye"
                );
                gemListScene.PopupCentered();
            } else if (buttonID == 1) {
                // Upgrade wand
                if (GameState.Persistent.MyWand.UpgradeInto() == null) {
                    _step = 10;
                    NextStep();
                    return;
                }
                UpgradeWand upgradeWand = new UpgradeWand();
                Director.MainUI.AddChild(upgradeWand);
                upgradeWand.Connect(
                    "finished", this, "ByeUpgradeWand"
                );
                upgradeWand.PopupCentered();
            }
        }

        public void Bye() {
            if (
                GameState.Persistent.CountGemsOwned(
                    new CustomGem(Simplest.Zero())
                ) >= Simplest.One()
                && GameState.Persistent.HasGems[
                    new Gem.StrongMult().Name()
                ] == 0
            ) {
                _step = 40;
                NextStep();
                return;
            }
            _step++;
            GameState.Persistent.Sema.WaitOne();
            string tip = Tip();
            GameState.Persistent.Sema.Release();
            if (tip == null) {
                NextStep();
                return;
            }
            Director.MainUI.MyLowPanel.SetFace(_npc);
            Director.MainUI.MyLowPanel.Display(tip);
        }
        public void ByeUpgradeWand(bool didBuy) {
            if (didBuy && GameState.Persistent.MyWand is Wand.Ricecooker) {
                _step = 20;
                NextStep();
            } else if (didBuy && GameState.Persistent.MyWand is Wand.Guitar) {
                _step = 30;
                NextStep();
            } else {
                _step = 5;
                Bye();
            }
        }

        private string Tip() {
            switch (GameState.Persistent.ShopTip) {
                case 0:
                    GameState.Persistent.ShopTip ++;
                    return "There are more shops ahead. Don't worry --- they appear quite frequently.";
                case 1:
                    GameState.Persistent.ShopTip ++;
                    return "Remember: The further you go, the stronger the enemies!";
                case 2:
                    if (GameState.Persistent.HasGems[new Gem.Stochastic(false).Name()] != 0) {
                        GameState.Persistent.ShopTip ++;
                        return "A good mage needs 1% grinding and 99% thinking.";
                    }
                    return null;
                case 3:
                    if (GameState.Persistent.MadeInfMeanWand) {
                        GameState.Persistent.ShopTip = 10;
                        return Tip();
                    }
                    if (GameState.Persistent.KillsSinceStochastic >= 15) {
                        GameState.Persistent.ShopTip ++;
                        return "Stop playing. Give yourself one minute. Think: how do you create truely immense mana?";
                    }
                    return null;
                case 4:
                    if (GameState.Persistent.MadeInfMeanWand) {
                        GameState.Persistent.ShopTip = 10;
                        return Tip();
                    }
                    if (GameState.Persistent.KillsSinceStochastic >= 28) {
                        GameState.Persistent.ShopTip ++;
                        return "My friend once guided her mana into a loop. It produced strong mana, but the loop had no exit...";
                    }
                    return null;
                case 5:
                    if (GameState.Persistent.MadeInfMeanWand) {
                        GameState.Persistent.ShopTip = 10;
                        return Tip();
                    }
                    return null;
                case 10:
                    GameState.Persistent.ShopTip = SHOP_TIP_TOP;
                    return "A quiz for you: what is the theoretical mean of your mana output rate?";
                case SHOP_TIP_TOP:
                    // fixed point
                    return null;
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
            _npcUI = npcUI;
            JumpDemo = false;
            if (GameState.Persistent.Event_JumperStage == 0) {
                _step = 0;
            } else {
                _step = 100;
            }
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
                        "You can now jump over great distances! *Jumper MK-I* converts 10% of your mana to jumping power."
                    );
                    _step++;
                    break;
                case 6:
                    Director.MainUI.MyLowPanel.Display(
                        "Try it! Hold J to jump."
                    );
                    Director.UnpauseWorld();
                    _step++;
                    break;
                case 7:
                    break;
                case 8:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.Display(
                        "What do you think?"
                    );
                    Director.MainUI.MyLowPanel.SetButtons(
                        "It's pathetic.", 
                        "Hmm, I like the sound it makes."
                    );
                    _step++;
                    break;
                case 20:
                    Director.MainUI.MyLowPanel.Display(
                        "Hehe, exactly my thoughts."
                    );
                    _step++;
                    break;
                case 21:
                    Director.MainUI.MyLowPanel.Display(
                        "I'll take some time to improve the deisgn. You just wait and see!"
                    );
                    _step = 40;
                    break;
                case 30:
                    Director.MainUI.MyLowPanel.Display(
                        "I know right!"
                    );
                    _step++;
                    break;
                case 31:
                    Director.MainUI.MyLowPanel.Display(
                        "..."
                    );
                    _step++;
                    break;
                case 32:
                    Director.MainUI.MyLowPanel.Display(
                        "Still, the jumping power is way too low. I need to take it back home and improve the design."
                    );
                    _step = 40;
                    break;
                case 40:
                    _sprite = new Sprite();
                    _sprite.Texture = GD.Load<Texture>("res://texture/misc/jumper.png");
                    _sprite.Scale = Params.SPRITE_SCALE;
                    Main.Singleton.MyWorld.AddChild(_sprite);
                    _step++;
                    animationTime = 0;
                    JumpDemo = false;
                    Main.Singleton.MySidePanel.Update();
                    Process(0);
                    break;
                case 43:
                    GameState.Persistent.Sema.WaitOne();
                    GameState.Persistent.Event_JumperStage = 1;
                    GameState.Persistent.Sema.Release();
                    SaveLoad.SaveAsync();
                    Director.UnpauseWorld();
                    Main.Singleton.MyWorld.DespawnSpecial(_npcUI);
                    GameState.Transient.NextSpawn = null;
                    Director.EventFinished();
                    break;

                case 100:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.SetFace((NPC)_npcUI.MySpawnable);
                    Director.MainUI.MyLowPanel.Display(
                        "I.               \nAM.               \nBACK."
                    );
                    _step++;
                    break;
                case 101:
                    Director.MainUI.MyLowPanel.Display(
                        "With the all new --- *Jumper MK-II*!!!"
                    );
                    _step++;
                    break;
                case 102:
                    Director.MainUI.MyLowPanel.Display(
                        "*Jumper MK-II* revolutionarily converts 12% of your mana to jumping power, "
                    );
                    _step++;
                    break;
                case 103:
                    Director.MainUI.MyLowPanel.Display(
                        "i.e., a jarring 2% increase in efficiency."
                    );
                    _step++;
                    break;
                case 104:
                    Director.MainUI.MyLowPanel.Display(
                        "It's so powerful, it essentially ends the series."
                    );
                    _step++;
                    break;
                case 105:
                    Director.MainUI.MyLowPanel.Display(
                        "As my beta tester, you get one for free. "
                    );
                    _step++;
                    break;
                case 106:
                    Director.MainUI.MyLowPanel.Display(
                        "Try it! I'd like to see just how much it has improved."
                    );
                    _step++;
                    break;
                case 107:
                    _sprite = new Sprite();
                    _sprite.Texture = GD.Load<Texture>("res://texture/misc/jumper.png");
                    _sprite.Scale = Params.SPRITE_SCALE;
                    Main.Singleton.MyWorld.AddChild(_sprite);
                    _step++;
                    animationTime = 0;
                    Process(0);
                    break;
                case 109:
                    Director.UnpauseWorld();
                    _step++;
                    break;
                case 110:
                    Director.UnpauseWorld();
                    break;
                case 115:
                    Director.MainUI.MyLowPanel.Display(
                        "Hold J. In case you forgot."
                    );
                    _step = 110;
                    break;
                case 120:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.Display(
                        "Unbelievable. You decreased your mana and used my Jumper MK-II, just like Author predicted!"
                    );
                    _step++;
                    break;
                case 121:
                    Director.MainUI.MyLowPanel.Display(
                        "How does he know everything?"
                    );
                    _step = 110;
                    break;
                case 130:
                    GameState.Persistent.Sema.WaitOne();
                    GameState.Persistent.Event_JumperStage = 2;
                    GameState.Persistent.Sema.Release();
                    SaveLoad.SaveAsync();
                    Director.UnpauseWorld();
                    GameState.Transient.NextSpawn = null;
                    Director.EventFinished();
                    break;
                default:
                    Console.WriteLine("_step=" + _step);
                    throw new Shared.ValueError();
            }
        }
        public override void Process(float dt) {
            if (!IsInstanceValid(_npcUI))
                return;
            if (_step == 3 || _step == 108) {
                float ratio = animationTime / ANIMATION_LENGTH;
                if (ratio > 1f) {
                    animationTime = 0;
                    _step ++;
                    _sprite.QueueFree();
                    if (_step == 109)
                        NextStep();
                    JumpDemo = true;
                    Main.Singleton.MySidePanel.Update();
                    return;
                }
                _sprite.Position = _npcUI.Position * (1f - ratio);
                animationTime += dt;
            } else if (_step == 4) {
                if (animationTime > 1) {
                    animationTime = 0;
                    _step ++;
                    NextStep();
                    return;
                }
                animationTime += dt;
            } else if (_step == 41) {
                float ratio = animationTime / ANIMATION_LENGTH;
                if (ratio > 1f) {
                    animationTime = 0;
                    _step ++;
                    _sprite.QueueFree();
                    return;
                }
                _sprite.Position = _npcUI.Position * ratio;
                animationTime += dt;
            } else if (_step == 42) {
                _npcUI.Position = new Vector2(
                    _npcUI.Position.x, 
                    _npcUI.Position.y + dt * 1000f
                );
                if (animationTime > 1) {
                    animationTime = 0;
                    _step ++;
                    NextStep();
                    return;
                }
                animationTime += dt;
            }
            if (JumpDemo) {
                float theta = _npcUI.Position.Angle();
                float distance = _npcUI.Position.Length();
                if (distance > 300f) {
                    distance = 300f;
                    _npcUI.Position = new Vector2(
                        (float) (distance * Math.Cos(theta)), 
                        (float) (distance * Math.Sin(theta))
                    );
                }
            }
        }

        public void JumpBegan() {
            Main.Singleton.VBoxLowPanel.Visible = false;
        }
        public void JumpFinished(Simplest jumpMana) {
            if (GameState.Persistent.Event_JumperStage == 0) {
                _step ++;
                NextStep();
            } else {
                Shared.Assert(_step == 110);
                if (jumpMana.MyRank == Rank.FINITE) {
                    _step = 120;
                    NextStep();
                } else {
                    _step = 130;
                    NextStep();
                }
            }
        }
        public override void ButtonClicked(int buttonID) {
            Director.MainUI.MyLowPanel.NoButtons();
            if (buttonID == 0) {
                _step = 20;
                NextStep();
            } else {
                _step = 30;
                NextStep();
            }
        }
        public void CollideAgain() {
            Shared.Assert(JumpDemo);
            if (GameState.Persistent.Event_JumperStage == 0) {
                _step = 6;
                NextStep();
            } else {
                _step = 115;
                NextStep();
            }
        }
    }

    public class Experting : MagicEvent {
        private int _step;
        private NPCUI _npcUI;
        private float _accTime;
        private Simplest _metaLevel;
        private CustomGem _cG;
        public Experting(NPCUI npcUI)
        {
            _step = 0;
            _npcUI = npcUI;
        }
        public override void NextStep() {

            switch (_step)
            {
                case 0:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.SetFace((NPC)_npcUI.MySpawnable);
                    if (GameState.Persistent.MyTypelessGem == null) {
                        Director.MainUI.MyLowPanel.Display(
                            "Everyone is so tremendously weak! How unbearably boring!!!"
                        );
                        _step++;
                    } else {
                        Director.MainUI.MyLowPanel.Display(
                            "zzzzzz..."
                        );
                        _step = 200;
                    }
                    break;
                case 1:
                    Main.Singleton.MyWorld.ClearAttacks();
                    Director.MainUI.MyLowPanel.Display(
                        "You there! Show me your strongest attack!"
                    );
                    _step++;
                    break;
                case 2:
                    Director.UnpauseWorld();
                    _step++;
                    _accTime = 0;
                    break;
                case 5:
                    Director.MainUI.MyLowPanel.Display(
                        "Attack me!"
                    );
                    _step = 2;
                    _accTime = 0;
                    break;
                case 10:
                    // unlock meta^0
                    Director.MainUI.MyLowPanel.Display(
                        $"Not bad. You just produced a {AttackClass()}."
                    );
                    _step++;
                    break;
                case 11:
                    _cG = new CustomGem(_metaLevel);
                    Director.MainUI.MyLowPanel.Display(
                        $"As your reward, I will now unlock \n*{_cG.DisplayName()}s* for you."
                    );
                    _step++;
                    break;
                case 12:
                    for (int i = 0; i <= _metaLevel.K; i++) {
                        if (!GameState.Persistent.HasCustomGems.ContainsKey(i)) {
                            _cG = new CustomGem(Simplest.Finite(i));
                            break;
                        }
                    }
                    if (_cG.MetaLevel.K >= 4) {
                        GameState.Persistent.Sema.WaitOne();
                        for (int i = 0; i <= _metaLevel.K; i++) {
                            if (i >= 5)
                                break;
                            if (!GameState.Persistent.HasCustomGems.ContainsKey(i)) {
                                _cG = new CustomGem(Simplest.Finite(i));
                                GameState.Persistent.HasCustomGems[i] = (
                                    0, _cG
                                );
                            }
                        }
                        GameState.Persistent.Sema.Release();
                        _step = 16;
                        _cG = new CustomGem(_metaLevel);
                        Director.MainUI.MyLowPanel.SetFace(null);
                        Director.MainUI.MyLowPanel.Display(
                            "... and so on. Eventually you unlocked up to \n"
                            + _cG.DisplayName() + ". "
                        );
                        break;
                    }
                    Director.MainUI.MyLowPanel.SetFace(null);
                    Director.MainUI.MyLowPanel.Display(
                        $"+ *{_cG.DisplayName()}* unlocked! +", 
                        true
                    );
                    GameState.Persistent.Sema.WaitOne();
                    GameState.Persistent.HasCustomGems[(int)_cG.MetaLevel.K] = (
                        0, _cG
                    );
                    GameState.Persistent.Sema.Release();
                    _step++;
                    break;
                case 13:
                    Director.MainUI.MyLowPanel.SetFace((NPC)_npcUI.MySpawnable);
                    _step++;
                    if (
                        Simplest.Finite(1) <= _cG.MetaLevel
                        && _cG.MetaLevel <= Simplest.Finite(3)
                    ) {
                        CustomGem subCG = new CustomGem(Simplest.Finite(_cG.MetaLevel.K - 1));
                        Director.MainUI.MyLowPanel.Display(
                            $"{_cG.DisplayName()}s can contain {subCG.DisplayName()}s."
                        );
                    } else {
                        NextStep();
                    }
                    break;
                case 14:
                    _step++;
                    if (
                        _cG.MetaLevel.Equals(Simplest.Zero())
                    ) {
                        Director.MainUI.MyLowPanel.Display(
                            "A Custom Gem can be placed in your "
                            + GameState.Persistent.MyWand.DisplayName() 
                            + " while also containing other gems in the Custom Gem's own circuit."
                        );
                    } else {
                        NextStep();
                    }
                    break;
                case 15:
                    if (_metaLevel.Equals(_cG.MetaLevel)) {
                        _step++;
                    } else {
                        _step = 12;
                    }
                    if (
                        _cG.MetaLevel.Equals(Simplest.Zero())
                    ) {
                        Director.MainUI.MyLowPanel.Display(
                            "Custom Gems take the theoretical mean of anything stochastic inside them."
                        );
                    } else {
                        NextStep();
                    }
                    break;
                case 16:
                    _step++;
                    if (
                        _metaLevel.MyRank == Rank.FINITE
                        && _metaLevel.K >= 2
                    ) {
                        string w = new MathBB.RaisableChar(
                            'w', 0, Main.Singleton.MyLowPanel.FontHeight
                        ).ToString();
                        string x = new MathBB.RaisableChar(
                            'x', 1, Main.Singleton.MyLowPanel.FontHeight
                        ).ToString();
                        Director.MainUI.MyLowPanel.SetFace((NPC)_npcUI.MySpawnable);
                        Director.MainUI.MyLowPanel.Display(
                            $"You see, if you hit me with a class-\n{w}{x} attack, I will unlock Meta{x} Custom Gems."
                        );
                    } else {
                        NextStep();
                    }
                    break;
                case 17:
                    Director.MainUI.MyLowPanel.Display(
                        $"You have potential. However, \na {AttackClass()} is nothing I'd call exciting. "
                    );
                    _step++;
                    break;
                case 18:
                    Director.MainUI.MyLowPanel.Display(
                        "Find me again when you are not this weak."
                    );
                    _step = 200;
                    break;
                case 20:
                    Director.MainUI.MyLowPanel.Display(
                        "!!!"
                    );
                    _step++;
                    break;
                case 21:
                    Director.MainUI.MyLowPanel.Display(
                        "You just struck me with             \n"
                        + $"a {AttackClass()}."
                    );
                    _step = 11;
                    break;
                case 40:
                    Director.MainUI.MyLowPanel.SetFace((NPC)_npcUI.MySpawnable);
                    Director.MainUI.MyLowPanel.Display(
                        "Nice try..."
                    );
                    _step++;
                    break;
                case 41:
                    Director.MainUI.MyLowPanel.Display(
                        $"But I'm afraid that was still just as strong as a {AttackClass()}."
                    );
                    _step = 200;
                    break;
                case 100:
                    Director.MainUI.MyLowPanel.Display(
                        "!!!"
                    );
                    _step++;
                    break;
                case 101:
                    Director.MainUI.MyLowPanel.Display(
                        "You just struck me with          \n"
                        + $"A {AttackClass()}."
                    );
                    _step++;
                    break;
                case 102:
                    Director.MainUI.MyLowPanel.Display(
                        "..."
                    );
                    _step++;
                    break;
                case 103:
                    Director.MainUI.MyLowPanel.Display(
                        "......"
                    );
                    _step++;
                    break;
                case 104:
                    Director.MainUI.MyLowPanel.Display(
                        "Very well."
                    );
                    _step++;
                    break;
                case 105:
                    _cG = new CustomGem(_metaLevel);
                    {
                        string w = new MathBB.RaisableChar(
                            'w', 1, Main.Singleton.MyLowPanel.FontHeight
                        ).ToString();
                        Director.MainUI.MyLowPanel.Display(
                            "As per our agreement, I now grant you the ability to make          \n"
                            + $"*Meta{w} Custom Gems*."
                        );
                    }
                    _step++;
                    break;
                case 106:
                    {
                        string w = new MathBB.RaisableChar(
                            'w', 1, Main.Singleton.MyLowPanel.FontHeight
                        ).ToString();
                        string minus = new MathBB.RaisableChar(
                            '-', 1, Main.Singleton.MyLowPanel.FontHeight
                        ).ToString();
                        string one = new MathBB.RaisableChar(
                            '1', 1, Main.Singleton.MyLowPanel.FontHeight
                        ).ToString();
                        Director.MainUI.MyLowPanel.Display(
                            $"*Meta{w} Custom Gems* can contain \n*Meta{w}{minus}{one} Custom Gems* --- "
                            + $"which are just *Meta{w} Custom Gems*."
                        );
                    }
                    _step++;
                    break;
                case 107:
                    Director.MainUI.MyLowPanel.Display(
                        "Because they can self-embed, mages also call them *"
                        + _cG.DisplayName() + "s*."
                    );
                    _step++;
                    break;
                case 108:
                    Director.MainUI.MyLowPanel.SetFace(null);
                    Director.MainUI.MyLowPanel.Display(
                        $"+ *{_cG.DisplayName()}* unlocked! +", 
                        true
                    );
                    GameState.Persistent.Sema.WaitOne();
                    GameState.Persistent.MyTypelessGem = _cG;
                    GameState.Persistent.Sema.Release();
                    Main.Singleton.MyTypelessCache.Start();
                    _step = 115;
                    break;
                case 115:
                    Director.MainUI.MyLowPanel.SetFace((NPC)_npcUI.MySpawnable);
                    Director.MainUI.MyLowPanel.Display(
                        "You know, you may just be strong enough to achieve what I couldn't."
                    );
                    _step++;
                    break;
                case 116:
                    Director.MainUI.MyLowPanel.Display(
                        "For all our lives, mages like me seek a way to produce epsilon_naught amount of mana. "
                    );
                    _step++;
                    break;
                case 117:
                    Director.MainUI.MyLowPanel.Display(
                        "It is generally suspected to be related to the typeless custom gems."
                    );
                    _step = 120;
                    break;
                case 120:
                    Director.MainUI.MyLowPanel.Display(
                        "However, nobody knows for sure. "
                    );
                    _step++;
                    break;
                case 121:
                    Director.MainUI.MyLowPanel.Display(
                        "Even Author has no idea how to produce epsilon_naught, or whether that is possible in this game."
                    );
                    _step++;
                    break;
                case 122:
                    Director.MainUI.MyLowPanel.Display(
                        "Still, if you manage to obtain epsilon_naught, the game has a mechanism to check for that."
                    );
                    _step++;
                    break;
                case 123:
                    Director.MainUI.MyLowPanel.Display(
                        "Well. I have taught you all that I know."
                    );
                    for (int i = 0; i < 5; i++)
                    {
                        if (!GameState.Persistent.HasCustomGems.ContainsKey(i)) {
                            GameState.Persistent.Sema.WaitOne();
                            GameState.Persistent.HasCustomGems[i] = (
                                0, new CustomGem(Simplest.Finite(i))
                            );
                            GameState.Persistent.Sema.Release();
                        }
                    }
                    _step++;
                    break;
                case 124:
                    Director.MainUI.MyLowPanel.Display(
                        "Time to go to sleep."
                    );
                    _step = 200;
                    break;
                case 200:
                    Director.EventFinished();
                    SaveLoad.SaveAsync();
                    Director.UnpauseWorld();
                    GameState.Persistent.Sema.WaitOne();
                    GameState.Persistent.Loneliness_Shop = Params.NPC_LINELINESS_MAX;
                    GameState.Persistent.Sema.Release();
                    if (GameState.Transient.NextSpawn is NPC.GemExpert)
                        GameState.Transient.NextSpawn = null;
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }

        public override void Process(float dt)
        {
            _accTime += dt;
            if (_step == 3) {
                // waiting for attack
                if (_accTime >= 4) {
                    _step = 5;
                    NextStep();
                    return;
                }
            }
        }

        public void Attacked(Attack attack) {
            Shared.Assert(_step == 2 | _step == 3);
            Director.PauseWorld();
            if (attack.Mana.MyRank == Rank.FINITE) {
                _metaLevel = Simplest.Zero();
            } else if (attack.Mana.MyRank == Rank.W_TO_THE_K) {
                _metaLevel = Simplest.Finite(attack.Mana.K);
            } else {
                if (attack.Mana.MyRank == Rank.TWO_TO_THE_W) {
                    _metaLevel = Simplest.W();
                } else {
                    // stack w
                    _metaLevel = new Simplest(Rank.STACK_W, attack.Mana.K - 1);
                }
                _step = 100;
                NextStep();
                return;
            }
            if (GameState.Persistent.HasCustomGems.ContainsKey((int)_metaLevel.K)) {
                _step = 40;
            } else {
                if (_metaLevel.Equals(Simplest.Zero())) {
                    _step = 10;
                } else {
                    _step = 20;
                }
            }
            NextStep();
        }

        private string AttackClass() {
            string x;
            if (_metaLevel.Equals(Simplest.Zero())) {
                x = "1";
            } else if (_metaLevel.MyRank == Rank.FINITE) {
                x = MathBB.Build(
                    new Simplest(Rank.W_TO_THE_K, _metaLevel.K), 
                    Main.Singleton.MyLowPanel.FontHeight
                );
            } else {
                int k;
                if (_metaLevel.Equals(Simplest.W())) {
                    k = 1;
                } else {
                    // stack w
                    k = (int)_metaLevel.K;
                }
                x = MathBB.Build(
                    new Simplest(Rank.STACK_W, k + 1), 
                    Main.Singleton.MyLowPanel.FontHeight
                );
            }
            return $"*class-{x} attack*";
        }
    }

    public class PickUpGem : MagicEvent {
        private int _step;
        private Gem _gem;
        public PickUpGem(DroppedGem dGem)
        {
            _gem = dGem.MyGem;
            Shared.Assert(_gem is Gem.Stochastic);
            _step = 0;
        }
        public override void NextStep()
        {
            switch (_step) {
                case 0:
                    Director.PauseWorld();
                    Director.MainUI.MyLowPanel.SetFace(null);
                    Director.MainUI.MyLowPanel.Display(
                        "You pick up the *" + _gem.DisplayName() + "*."
                    );
                    _step++;
                    break;
                case 1:
                    Director.MainUI.MyLowPanel.Display(
                        "Rumour has it that Author dropped the gem here becuase you still haven't bought any "
                        + _gem.DisplayName() + "s from the shop."
                    );
                    _step++;
                    break;
                case 2:
                    Director.MainUI.MyLowPanel.Display(
                        "However, you should NOT expect any more gems lying on the ground."
                    );
                    _step++;
                    break;
                case 3:
                    Director.UnpauseWorld();
                    Director.EventFinished();
                    SaveLoad.SaveAsync();
                    break;
                default:
                    throw new Shared.ValueError();
            }
        }
    }
}
