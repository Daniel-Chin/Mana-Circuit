public class Enemy : SpawnableSpecial
{
    public Simplest Money;
    public Simplest HP;
    public int HitTimes;
    // public Enemy(Simplest hp) : this(hp, hp) { }
    public Enemy(Simplest hp, Simplest money)
    {
        Money = money;
        HP = hp;
        HitTimes = 0;
    }
}
