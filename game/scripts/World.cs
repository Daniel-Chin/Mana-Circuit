using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class World : Node2D
{
    [Signal] public delegate void wand_replaced();
    [Signal] public delegate void player_died();
    private static readonly int Z_INDEX_RANGE = 512;
    public TextureRect BackRect;
    public MageUI MyMageUI;
    ShaderMaterial BackShader;
    public float AspectRatio;
    public List<SpawnableSpecialUI> SpawnedSpecialUIs;
    public List<Money> Moneys;
    public List<Attack> Attacks;
    public float Time;

    private float _lastLeftUp;
    private bool _isLeftDownLastFrame;

    public override void _Ready()
    {
        BackRect = GetNode<TextureRect>("Background");
        MyMageUI = GetNode<MageUI>("MageUI");
        // MyMageUI.ZIndex = ZIndexOf(MyMageUI);
        BackShader = (ShaderMaterial)BackRect.Material;
        AspectRatio = BackRect.RectMinSize.y / BackRect.RectMinSize.x;
        BackShader.SetShaderParam("aspect_ratio", AspectRatio);
        UpdateBack();
        MyMageUI.Resting();
        MyMageUI.Hold(GameState.Persistent.MyWand);
        Moneys = new List<Money>();
        SpawnedSpecialUIs = new List<SpawnableSpecialUI>();
        Attacks = new List<Attack>();

        _lastLeftUp = 0;
        Time = 0;
    }

    private static readonly float SOFTZONE = 0;
    public override void _Process(float delta)
    {
        // foreach (var ui in SpawnedSpecialUIs) {
        //     ui.ZIndex = ZIndexOf(ui);
        // }
        if (GameState.Transient.WorldPaused)
            return;
        Vector2 drag = GetLocalMousePosition();
        Vector2 direction = drag.Normalized();
        Time += delta;
        BackShader.SetShaderParam("worldTime", Time);
        bool walking = false;
        if (! Jumper.Charging) {
            if (Input.IsMouseButtonPressed(((int)ButtonList.Right)))
            {
                walking = true;
                float l = drag.Length() / SOFTZONE;
                if (l < 1)
                    direction *= l;
                Vector2 displace = (
                    delta * Params.WALK_SPEED * direction
                );
                GameState.Transient.LocationOffset += displace;
                if (GameState.Persistent.Location_dist.MyRank == Rank.FINITE)
                {
                    GameState.Persistent.Location_dist = new Simplest(
                        Rank.FINITE,
                        GameState.Transient.LocationOffset.Length()
                    );
                    GameState.Persistent.Location_theta = GameState.Transient.LocationOffset.Angle();
                    GameState.Transient.Update();
                }
                UpdateBack();
                MyMageUI.Walking();
                ReverseMoveWorld(displace);
                OnWalk(direction);
            }
            else
            {
                MyMageUI.Resting();
            }
            // attack
            bool isLeftDown = Input.IsMouseButtonPressed(((int)ButtonList.Left));
            bool risingEdge = (!_isLeftDownLastFrame) && isLeftDown;
            _isLeftDownLastFrame = isLeftDown;
            if (!isLeftDown)
                _lastLeftUp = Main.MainTime;
            if (
                (
                    risingEdge || (
                        isLeftDown 
                        && Main.MainTime - _lastLeftUp >= Params.LMB_HOLD_DEADZONE
                    )
                )
                && !GameState.Transient.Mana.Equals(Simplest.Zero())
            )
            {
                Attack attack = new Attack()
                {
                    Direction = direction,
                    Mana = GameState.Transient.Mana,
                };
                attack.Head = direction * Params.ENEMY_COLLISION_RANGE;
                GameState.Transient.Mana = Simplest.Zero();
                Main.Singleton.MySidePanel.Update(); // Ideally, a signal
                attack.LineWidth = 3;
                Attacks.Add(attack);
                AddChild(attack);
                Director.WandAttacked();
            }
        }
        UpdateMoneys(delta);
        UpdateAttacks(delta);
        UpdateEnemies(delta);
        // despawn specials
        foreach (var ui in new List<SpawnableSpecialUI>(SpawnedSpecialUIs))
        {
            if (ShouldDespawn(ui.Position))
            {
                if (ui is EnemyUI)
                    continue;
                DespawnSpecial(ui);
            }
        }
        // check SpawnableSpecial collision
        foreach (var ui in new List<SpawnableSpecialUI>(SpawnedSpecialUIs))
        {
            float distance = ui.Position.Length();
            if (ui is EnemyUI enemyUI)
            {
                if (distance < Params.ENEMY_COLLISION_RANGE)
                {
                    CollidedWithEnemy(enemyUI);
                }
                continue;
            }
            if (
                walking 
                && distance < Params.NPC_COLLISION_RANGE
                && ui.Position.Normalized().Dot(direction) > .7
            ) {
                switch (ui)
                {
                    case DroppedItemUI dUI:
                        CollidedWithDroppedItem(dUI);
                        break;
                    case NPCUI npcUI:
                        CollidedWithNPC(npcUI);
                        break;
                    default:
                        throw new Shared.TypeError();
                }
            }
        }
        // check Money collision
        foreach (Money money in new List<Money>(Moneys))
        {
            float distance = money.Position.Length();
            if (distance < Params.MONEY_COLLISION_RANGE)
            {
                Moneys.Remove(money);
                money.QueueFree();
                GameState.Persistent.Money = Simplest.Eval(
                    GameState.Persistent.Money, Operator.PLUS,
                    money.Amount
                );
                Main.Singleton.MySidePanel.Update(); // Ideally, a signal
            }
        }
    }

    private void OnWalk(Vector2 direction)
    {
        // spawn event
        if (
            GameState.Transient.NextSpawn is SpawnableSpecial s
            && GameState.Transient.EnemiesTillNextSpawn == 0
        )
        {
            bool alreadyThere = false;
            foreach (var ui in SpawnedSpecialUIs)
            {
                if (ui.MySpawnable == s)
                {
                    alreadyThere = true;
                    break;
                }

            }
            if (!alreadyThere)
            {
                Console.WriteLine("Spawning " + s);
                Spawn(s, direction);
            }
        }
        // spawn non-event
        if (Director.CanSpawnNonevent())
        {
            if ((
                GameState.Transient.LastLocationNoneventSpawn
                - GameState.Transient.LocationOffset
            ).Length() >= Params.SPAWN_EVERY_DISTANCE)
            {
                GameState.Transient.LastLocationNoneventSpawn = GameState.Transient.LocationOffset;
                if (! TrySpawnNPCAsNonEvent(direction)) {
                    Simplest hp;
                    Simplest d = GameState.Persistent.Location_dist;
                    if (d.MyRank == Rank.FINITE)
                    {
                        hp = new Simplest(Rank.FINITE, Math.Ceiling(
                            // Math.Pow(d.K, 3) + Params.BASE_ENEMY_HP
                            Math.Exp(d.K + Math.Log(Params.BASE_ENEMY_HP))
                        ));
                    }
                    else
                    {
                        hp = d;
                    }
                    Spawn(new Enemy(hp), direction);
                    if (GameState.Transient.EnemiesTillNextSpawn > 0)
                        GameState.Transient.EnemiesTillNextSpawn--;
                }
            }
        }
    }

    private void Spawn(SpawnableSpecial s, Vector2 direction)
    {
        SpawnableSpecialUI ui;
        switch (s)
        {
            case Enemy e:
                ui = new EnemyUI(e);
                break;
            case Wand.Staff staff:
                ui = new DroppedItemUI(staff);
                ui.MySprite.Texture = GD.Load<Texture>("res://texture/wand/staff.png");
                break;
            case NPC npc:
                // unique
                foreach (var _ui in SpawnedSpecialUIs) {
                    if (s.GetType() == _ui.MySpawnable.GetType())
                        return;
                }
                ui = new NPCUI(npc);
                break;
            default:
                throw new Shared.TypeError();
        }
        Vector2 location = direction * .6f + new Vector2(
            (float)Shared.Rand.NextDouble() - .5f,
            (float)Shared.Rand.NextDouble() - .5f
        ) * 2 * .3f;
        location = location.Normalized();
        ui.Position = location * SpawnRadius();
        SpawnedSpecialUIs.Add(ui);
        // Console.Write("SpawnedUIs ");
        // Shared.PrintList(SpawnedUIs);
        AddChild(ui);
        Director.SpecialSpawned(s);
    }

    private void UpdateMoneys(float dt)
    {
        // despawn
        foreach (Money money in new List<Money>(Moneys))
        {
            if (ShouldDespawn(money.Position))
            {
                Moneys.Remove(money);
                money.QueueFree();
            }
        }
        // repel
        foreach (Money m0 in Moneys)
        {
            Vector2 force = new Vector2(0, 0);
            List<Node2D> repellers = new List<Node2D>();
            repellers.AddRange(Moneys);
            repellers.AddRange(SpawnedSpecialUIs);
            foreach (Node2D repeller in repellers)
            {
                if (m0 == repeller) continue;
                Vector2 displace = m0.Position - repeller.Position;
                float dist = Math.Max(3f, displace.Length());
                force += displace.Normalized() / (float)Math.Pow(dist, 2) * (
                    repeller is NPCUI ? 40 : 1
                );
            }
            m0.Step(force, dt);
        }
    }

    private void UpdateAttacks(float dt)
    {
        foreach (Attack attack in new List<Attack>(Attacks))
        {
            UpdateAttack(dt, attack);
        }
    }

    private void UpdateAttack(float dt, Attack attack)
    {
        while (attack.Advect(dt))
        {
            if (ShouldDespawn(attack.Head + attack.Position))
            {
                Attacks.Remove(attack);
                attack.QueueFree();
                return;
            }
            foreach (var ui in SpawnedSpecialUIs)
            {
                if ((ui.Position - (
                    attack.Head + attack.Position
                )).Length() >= Params.ATTACK_PROXIMITY)
                    continue;
                if (ui is EnemyUI enemyUI)
                {
                    HitEnemy(attack, enemyUI);
                } else if (ui is NPCUI npcUI) {
                    if (!HitNPC(attack, npcUI))
                        continue;
                }
                Attacks.Remove(attack);
                attack.QueueFree();
                return;
            }
        }
    }

    private void UpdateEnemies(float dt)
    {
        foreach (var ui in SpawnedSpecialUIs)
        {
            if (ui is EnemyUI eUI)
            {
                eUI.Position -= (
                    eUI.Position.Normalized() * Params.ENEMY_SPEED
                    * dt * BackRect.RectSize.x
                );
            }
        }
    }

    public void UpdateBack()
    {
        BackShader.SetShaderParam(
            "offset_g", GameState.Transient.LocationOffset
            - new Vector2(.5f, .5f * AspectRatio)
        );
    }

    private void CollidedWithEnemy(EnemyUI enemyUI)
    {
        EmitSignal("player_died");
    }
    private void CollidedWithNPC(NPCUI npcUI)
    {
        switch (npcUI.MySpawnable) {
            case NPC npc:
                npc.Collided(npcUI);
                break;
            default:
                throw new Shared.TypeError();
        }
    }
    private void CollidedWithDroppedItem(DroppedItemUI dUI)
    {
        Shared.Assert(dUI.MySpawnable is Wand.Staff);
        Shared.Assert(GameState.Persistent.MyWand == null);
        GameState.Persistent.MyWand = (Wand)dUI.MySpawnable;
        SpawnedSpecialUIs.Remove(dUI);
        dUI.QueueFree();
        EmitSignal("wand_replaced");
        GameState.Transient.NextSpawn = null;
        Director.StartEvent(new MagicEvent.Staff());
    }

    private float SpawnRadius()
    {
        return .75f * BackRect.RectSize.x;
    }

    public void DespawnSpecial(SpawnableSpecialUI ui)
    {
        SpawnedSpecialUIs.Remove(ui);
        ui.QueueFree();
        Director.SpecialDespawned(
            ui.MySpawnable, ui.Exposed
        );
    }

    private void HitEnemy(Attack attack, EnemyUI enemyUI)
    {
        Enemy enemy = (Enemy)enemyUI.MySpawnable;
        if (attack.Mana >= enemy.HP)
        {
            SpawnedSpecialUIs.Remove(enemyUI);
            enemyUI.QueueFree();
            DropMoneyCluster(enemy.MaxHP, enemyUI.Position);
            Director.CheckEvent();
        }
        else
        {
            enemy.HP = Simplest.Eval(
                enemy.HP, Operator.MINUS, attack.Mana
            );
            enemyUI.UpdateHP();
        }
    }
    private bool HitNPC(Attack attack, NPCUI npcUI) {
        if (npcUI.MySpawnable is NPC.GemExpert) {
            if (Director.NowEvent is MagicEvent.Experting e) {
                e.Attacked(attack);
                return true;
            // } else {
                // Just don't allow. I'd be confusing if the player randomly hit Expert. 
                // if (
                //     GameState.Persistent.HasCustomGems.ContainsKey(0)
                //     && GameState.Persistent.MyTypelessGem == null
                // ) {
                //     MagicEvent.Experting ex = new MagicEvent.Experting(npcUI);
                //     Director.NowEvent = ex;
                //     ex.Attacked(attack);
                //     return true;
                // }
            }
        }
        return false;
    }

    private bool ShouldDespawn(Vector2 location)
    {
        return location.Length() >= SpawnRadius() * 1.1;
    }

    private void DropMoneyCluster(Simplest enemyHP, Vector2 location)
    {
        Simplest amount = enemyHP;
        if (enemyHP.MyRank == Rank.FINITE)
        {
            amount = new Simplest(Rank.FINITE, Math.Ceiling(
                enemyHP.K * Params.MONEY_DROP_MULT
            ));
            if (amount.K < 7)
            {
                for (int i = 0; i < amount.K; i++)
                {
                    DropOneMoney(
                        Simplest.One(),
                        location + new Vector2(
                            (float)Shared.Rand.NextDouble() - .5f,
                            (float)Shared.Rand.NextDouble() - .5f
                        )
                    );
                }
                return;
            }
        }
        DropOneMoney(amount, location);
    }

    private Money DropOneMoney(Simplest amount, Vector2 location)
    {
        Money money = new Money(amount);
        AddChild(money);
        Moneys.Add(money);
        money.Position = location;
        return money;
    }

    public void ClearAttacks()
    {
        Shared.QFreeList<Attack>(Attacks);
        Attacks.Clear();
    }
    public void ClearSpawns()
    {
        Shared.QFreeList<SpawnableSpecialUI>(SpawnedSpecialUIs);
        SpawnedSpecialUIs.Clear();
        Shared.QFreeList<Money>(Moneys);
        Moneys.Clear();
        ClearAttacks();
    }

    public void ReverseMoveWorld(Vector2 displace) {
        Vector2 halfSize = BackRect.RectSize * .5f;
        foreach (SpawnableSpecialUI s in SpawnedSpecialUIs)
        {
            s.Position -= displace * BackRect.RectSize.x;
            s.Moved(halfSize);
        }
        foreach (Money m in Moneys)
        {
            m.Position -= displace * BackRect.RectSize.x;
        }
        foreach (Attack a in Attacks)
        {
            a.Position -= displace * BackRect.RectSize.x;
        }
    }

    private int ZIndexOf(Node2D ui) {
        return (int)((
            ui.Position.y / BackRect.RectMinSize.y + 1f
        ) * Z_INDEX_RANGE);
    }

    private bool TrySpawnNPCAsNonEvent(Vector2 direction) {
        Console.WriteLine("dan");
        if (!GameState.Persistent.Event_Staff)
            return false;
        if (!GameState.Persistent.HasAnyNonCGem())
            return false;
        Console.WriteLine("dan2");
        Console.WriteLine(GameState.Persistent.Loneliness_WandSmith);
        if (
            GameState.Persistent.Loneliness_WandSmith >= Params.NPC_LINELINESS_MAX
        ) {
            Spawn(new NPC.WandSmith(), direction);
            return true;
        }
        if (
            GameState.Persistent.Loneliness_Shop >= Params.NPC_LINELINESS_MAX
        ) {
            Spawn(new NPC.Shop(), direction);
            return true;
        }
        if (!GameState.Persistent.HasCustomGems.ContainsKey(0))
            return false;
        if (
            GameState.Persistent.Loneliness_GemExpert >= Params.NPC_LINELINESS_MAX
        ) {
            Spawn(new NPC.GemExpert(), direction);
            return true;
        }
        return false;
    }
}
