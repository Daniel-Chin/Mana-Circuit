public class Enemy : SpawnableSpecial
{
    public Simplest HP;
    public int HitTimes;
    public Enemy(Simplest hp)
    {
        HP = hp;
        HitTimes = 0;
    }
}
