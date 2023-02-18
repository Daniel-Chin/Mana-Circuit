using System;
using System.Text;

public class Particle
{
    public PointInt Location;
    public PointInt Direction;
    public Simplest[] Mana; // Mana[0] is 0th-order
    public Gem LastGem;
    public Particle(
        PointInt location, PointInt direction,
        Simplest[] mana
    )
    {
        Location = location;
        Direction = direction;
        Mana = mana;
        LastGem = null;
    }

    public override string ToString()
    {
        StringBuilder sB = new StringBuilder();
        sB.Append("<Particle @ ");
        sB.Append(Location);
        sB.Append(" mana = ");
        foreach (Simplest s in Mana)
        {
            sB.Append(s);
            sB.Append(", ");
        }
        sB.Append(">");
        return sB.ToString();
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
        Particle p = new Particle(
            Location, Direction, (Simplest[])Mana.Clone()
        );
        p.LastGem = LastGem;
        return p;
    }
}
