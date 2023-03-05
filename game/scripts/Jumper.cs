using System;
using Godot;

public class Jumper {
    private static readonly float CHARGE_TIME = 1.5f;
    private static readonly float JUMP_TIME = 3f;
    private static readonly float ACC_STAGE = .3f;
    private static readonly float FINITE_SPEED = .01f;

    public static bool Charging;
    private static float charged;
    private static float jumpedTime;
    private static Vector2 jumpDirection;
    private static Simplest jumpMana;
    private static bool isFinite;
    static Jumper() {
        charged = 0;
        Charging = false;
    }
    public static void Process(float dt) {
        Vector2 drag = Main.Singleton.MyWorld.GetLocalMousePosition();
        Vector2 mouseDirection = drag.Normalized();
        if (GameState.Transient.Jumping) {
            float ratio = jumpedTime / JUMP_TIME;
            if (ratio >= 1) {
                Main.Singleton.MyMageUI.Position = new Vector2(0, 0);
                if (
                    isFinite && 
                    GameState.Persistent.Location_dist.MyRank == Rank.FINITE
                ) {
                    GameState.Persistent.Location_dist = Simplest.Finite(
                        GameState.Transient.LocationOffset.Length()
                    );
                    GameState.Persistent.Location_theta = GameState.Transient.LocationOffset.Angle();
                }
                GameState.Transient.Jumping = false;
                Director.UnpauseWorld();
                Director.JumpFinished(jumpMana);
                return;
            }
            Main.Singleton.MyMageUI.Position = new Vector2(
                0, (isFinite ? 100 : 600) * (float)(
                    Math.Pow(ratio - .5, 2)
                    - Math.Pow(.5, 2) 
                )
            );
            Vector2 displace;
            if (isFinite) {
                displace = jumpDirection * FINITE_SPEED * dt;
                GameState.Transient.LocationOffset += displace;
                Main.Singleton.MyWorld.ReverseMoveWorld(displace);
            } else {
                if (ratio < ACC_STAGE) {
                    float speed = (float)-Math.Log(1f - ratio / ACC_STAGE);
                    displace = jumpDirection * speed * dt;
                    GameState.Transient.LocationOffset += displace;
                    Main.Singleton.MyWorld.ReverseMoveWorld(displace);
                } else if (ratio > 1 - ACC_STAGE) {
                    float speed = (float)-Math.Log(1f - (1f - ratio) / ACC_STAGE);
                    displace = jumpDirection * speed * dt;
                    GameState.Transient.LocationOffset += displace;
                } else {
                    double dist = Params.INF_DISTANCE + Shared.Rand.NextDouble();
                    double theta = GameState.Persistent.Location_theta;
                    GameState.Transient.LocationOffset = new Vector2(
                        (float) (dist * Math.Cos(theta)), 
                        (float) (dist * Math.Sin(theta))
                    );
                    Main.Singleton.MyWorld.ClearSpawns();
                }
            }
            Main.Singleton.MyWorld.UpdateBack();

            jumpedTime += dt;
        } else {
            Charging = Input.IsActionPressed("jump");
            if (Charging && !GameState.Transient.WorldPaused && (
                GameState.Persistent.Event_JumperStage == 2
                || (
                    Director.NowEvent is MagicEvent.Jumping e
                    && e.JumpDemo
                )
            )) {
                charged += dt;
                Main.Singleton.MyMageUI.Charging();
                if (charged >= CHARGE_TIME) {
                    GameState.Transient.Jumping = true;
                    charged = 0;
                    jumpedTime = 0;
                    Director.PauseWorld();
                    Main.Singleton.MyMageUI.Spinning();
                    jumpDirection = mouseDirection;
                    jumpMana = GameState.Transient.Mana;
                    GameState.Transient.Mana = Simplest.Zero();
                    isFinite = jumpMana.MyRank == Rank.FINITE;
                    if (!isFinite) {
                        if (jumpMana >= GameState.Persistent.Location_dist) {
                            GameState.Persistent.Location_dist = jumpMana;
                            GameState.Persistent.Location_theta = jumpDirection.Angle();
                        }
                    }
                    Director.JumpBegan();
                }
            } else {
                charged = 0;
            }
        }
    }
}
