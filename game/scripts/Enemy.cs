public class Enemy : SpawnableSpecial
{
    public Simplest MaxHP;
    public Simplest HP;
    public int HitTimes;
    public Enemy(Simplest hp)
    {
        MaxHP = hp;
        HP = hp;
        HitTimes = 0;
    }
}
