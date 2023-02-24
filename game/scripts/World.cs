using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class World : Node2D
{
    [Signal] public delegate void new_wand();
    public TextureRect BackRect;
    public MageUI MyMageUI;
    ShaderMaterial BackShader;
    public float AspectRatio;
    public List<Money> Moneys;
    public List<SpawnableUI> SpawnedUIs;

    public override void _Ready()
    {
        BackRect = GetNode<TextureRect>("Background");
        MyMageUI = GetNode<MageUI>("MageUI");
        BackShader = (ShaderMaterial)BackRect.Material;
        AspectRatio = BackRect.RectMinSize.y / BackRect.RectMinSize.x;
        BackShader.SetShaderParam("aspect_ratio", AspectRatio);
        UpdateBack();
        MyMageUI.Resting();
        MyMageUI.Hold(GameState.Persistent.MyWand);
        Moneys = new List<Money>();
        SpawnedUIs = new List<SpawnableUI>();
    }

    private static readonly float SOFTZONE = 0;
    public override void _Process(float delta)
    {
        if (GameState.Transient.NPCPausedWorld)
            return;
        if (Input.IsMouseButtonPressed(((int)ButtonList.Right)))
        {
            Vector2 drag = GetLocalMousePosition();
            Vector2 direction = drag.Normalized();
            float l = drag.Length() / SOFTZONE;
            if (l < 1)
                direction *= l;
            Vector2 displace = (
                delta * Params.SPEED * direction
            );
            if (GameState.Persistent.Location_dist.MyRank == Rank.FINITE)
            {
                GameState.Transient.LocationOffset += displace;
                GameState.Persistent.Location_dist = new Simplest(
                    Rank.FINITE,
                    GameState.Transient.LocationOffset.Length()
                );
                GameState.Persistent.Location_theta = GameState.Transient.LocationOffset.Angle();
            }
            GameState.Transient.Update();
            UpdateBack();
            MyMageUI.Walking();
            foreach (SpawnableUI s in SpawnedUIs)
            {
                s.Position -= displace * BackRect.RectSize.x;
            }
            OnWalk(direction);
        }
        else
        {
            MyMageUI.Resting();
        }
        UpdateMoneys(delta);
    }

    private void OnWalk(Vector2 direction)
    {
        if (
            GameState.Transient.NextSpawn != null
            && GameState.Transient.EnemiesTillNextSpawn == 0
        )
        {
            // spawn event
            bool alreadyThere = false;
            foreach (var ui in SpawnedUIs)
            {
                if (ui.MySpawnable == GameState.Transient.NextSpawn)
                {
                    alreadyThere = true;
                    break;
                }

            }
            if (!alreadyThere)
            {
                Console.WriteLine("Spawning " + GameState.Transient.NextSpawn);
                Spawn(GameState.Transient.NextSpawn, direction);
                Director.OnSpawn();
            }
        }
        if (GameState.Transient.CanSpawnNonevent)
        {
            if ((
                GameState.Transient.LastLocationNoneventSpawn
                - GameState.Transient.LocationOffset
            ).Length() >= Params.SPAWN_EVERY_DISTANCE)
            {
                // if event shows can spawn shops, spawn w/ low prob
                Simplest hp;
                Simplest d = GameState.Persistent.Location_dist;
                if (d.MyRank == Rank.FINITE)
                {
                    hp = new Simplest(Rank.FINITE, Math.Exp(d.K));
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
        // check NPC collision
        foreach (var ui in new List<SpawnableUI>(SpawnedUIs))
        {
            float distance = ui.Position.Length();
            if (ui is EnemyUI enemyUI)
            {
                if (distance < Params.Enemy_COLLISION_RANGE)
                {
                    CollidedWithEnemy(enemyUI);
                }
                continue;
            }
            if (distance < Params.NPC_COLLISION_RANGE)
            {
                if (ui.Position.Normalized().Dot(direction) > 0)
                {
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
        }
    }

    private void Spawn(Spawnable s, Vector2 direction)
    {
        SpawnableUI ui;
        switch (s)
        {
            case Enemy e:
                ui = new EnemyUI(e);
                break;
            case Wand.Staff staff:
                ui = new DroppedItemUI(staff);
                ui.MySprite.Texture = GD.Load<Texture>("res://texture/wand/staff.png");
                break;
            default:
                throw new Shared.TypeError();
        }
        Vector2 location = direction * .6f + new Vector2(
            (float)Shared.Rand.NextDouble() - .5f,
            (float)Shared.Rand.NextDouble() - .5f
        ) * 2 * .3f;
        location = location.Normalized();
        ui.Position = location * .7f * BackRect.RectSize.x;
        SpawnedUIs.Add(ui);
        // Console.Write("SpawnedUIs ");
        // Shared.PrintList(SpawnedUIs);
        AddChild(ui);
    }

    private void UpdateMoneys(float dt)
    {
        for (int i = 0; i < Moneys.Count; i++)
        {
            Money m0 = Moneys[i];
            foreach (Money m1 in Moneys.Skip(i + 1))
            {
                Vector2 displace = m0.Position - m1.Position;
                Vector2 force = displace.Normalized() / displace.Length();
                m0.Step(force, dt);
                m1.Step(-force, dt);
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
        // todo
    }
    private void CollidedWithNPC(NPCUI npcUI)
    {
        // todo
    }
    private void CollidedWithDroppedItem(DroppedItemUI dUI)
    {
        Shared.Assert(dUI.MySpawnable is Wand.Staff);
        Shared.Assert(GameState.Persistent.MyWand == null);
        GameState.Persistent.MyWand = (Wand)dUI.MySpawnable;
        SpawnedUIs.Remove(dUI);
        dUI.QueueFree();
        EmitSignal("new_wand");
        GameState.Persistent.Event_Staff = true;
        GameState.Transient.NextSpawn = null;
        SaveLoad.Save();
    }
}
