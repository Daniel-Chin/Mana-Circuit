using MathNet.Numerics.LinearAlgebra;

public class Particle
{
    public Vector<int> Location;
    public Vector<int> Direction;
    public Simplest[] Mana; // Mana[0] is 0th-order
    public Gem LastGem;
    public Particle(
        Vector<int> location, Vector<int> direction,
        Simplest[] mana
    )
    {
        Location = location;
        Direction = direction;
        Mana = mana;
        LastGem = null;
    }

    public void Multiply(double k)
    {
        Simplest s = new Simplest(Rank.FINITE, k);
        Multiply(s);
    }
    public void Multiply(Simplest s)
    {
        for (int i = 0; i < Mana.Length; i++)
        {
            Mana[i] = Simplest.Eval(Mana[i], Operator.TIMES, s);
        }
    }

    public Particle Copy()
    {
        Particle p = new Particle(Location, Direction, Mana);
        p.LastGem = LastGem;
        return p;
    }
}
