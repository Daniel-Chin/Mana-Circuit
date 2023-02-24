public class Enemy : Spawnable
{
    public Simplest HP;
    public int HitTimes;
    public Enemy(Simplest hp)
    {
        HP = hp;
        HitTimes = 0;
    }
}
