using Godot;

public class Params
{
    public static readonly float TEXT_ROLL_SPEED = 34;
    public static readonly float WALK_SPEED = .1f;
    public static readonly float ENEMY_SPEED = .1f;
    public static readonly float SPAWN_EVERY_DISTANCE = .3f;
    public static readonly float NPC_COLLISION_RANGE = 140;
    public static readonly float ENEMY_COLLISION_RANGE = 50;
    public static readonly float MONEY_COLLISION_RANGE = 50;
    public static readonly int WAND_MAX_PARTICLES = 10;
    public static readonly float WAND_EMIT_INTERVAL = .3f;
    public static readonly float WAND_ADVECT_INTERVAL = .03f;
    public static readonly float ATTACK_PROXIMITY = 40;
    public static readonly double ATTACK_FLY_BASE_SPEED = 4;
    public static readonly double BASE_ENEMY_HP = 3;
    public static readonly double MONEY_DROP_MULT = .1;
    public static readonly int N_ENEMIES_AFTER_REJECT_EVENT = 2;
    public static readonly float LMB_HOLD_DEADZONE = .3f;
    public static readonly Vector2 SPRITE_SCALE = new Vector2(4, 4);
    public static readonly double INF_DISTANCE = 1e3;   // 45 minutes of walk time
    public static readonly int NPC_LINELINESS_MAX = 7;
    public static readonly double PROB_ENEMY_INF = .2;
    public static readonly int INF_AFTER_KILLS_AFTER_STRONG_MULT = 10;
}
