using Godot;

public class Enemy : Spawnable
{
    public Simplest HP;
    public int HitTimes;
    public Vector2 ScreenLocation { get; set; }
    public Enemy(Simplest hp)
    {
        HP = hp;
        HitTimes = 0;
    }
}
