using MathNet.Numerics.LinearAlgebra;

public class Particle {
    // immutable
    public Vector<int> Location;
    public Vector<int> Direction;
    public Simplest Mana;
    public Particle(
        Vector<int> location, Vector<int> direction
        Simplest mana
    ) {
        Location = location;
        Direction = direction;
        Mana = mana;
    }
}
