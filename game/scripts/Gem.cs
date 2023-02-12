using MathNet.Numerics.LinearAlgebra;

public abstract class Gem
{
    public Vector<int> Location;
    public Vector<int> Size;
    public Gem()
    {
        Size = Vector<int>.Build.DenseOfArray(new int[2] { 1, 1 });
    }
    public abstract Particle Apply(Particle input);

    public class Source : Gem
    {
        Vector<int> Direction;
        public override Particle Apply(Particle input)
        {
            input.Direction = Direction;
            return input;
        }
    }
    public class Drain : Gem
    {
        public override Particle Apply(Particle input)
        {
            return null;
        }
    }
    public class Wall : Gem
    {
        public override Particle Apply(Particle input)
        {
            return null;
        }
    }
    public class AddOne : Gem
    {
        public override Particle Apply(Particle input)
        {
            input.Mana[0] = Simplest.Eval(
                input.Mana[0], Operator.PLUS, Simplest.One()
            );
            return input;
        }
    }
    public class WeakMult : Gem
    {
        public override Particle Apply(Particle input)
        {
            input.Multiply(1.4);    // 1.4**2 < 2
            return input;
        }
    }
    public class Doubler : Gem
    {
        Doubler() : base()
        {
            Size = Vector<int>.Build.DenseOfArray(new int[2] { 2, 2 });
        }
        public override Particle Apply(Particle input)
        {
            input.Multiply(2);
            return input;
        }
    }
    public class Focus : Gem
    {
        Vector<int> Direction;
        public override Particle Apply(Particle input)
        {
            input.Direction = Direction;
            return input;
        }
    }
    public class Mirror : Gem
    {
        Matrix<int> Transform;
        public Mirror(bool orientation) : base()
        {
            if (orientation)
            {
                Transform = Matrix<int>.Build.DenseOfArray(new int[2, 2] {
                    {0, 1},
                    {1, 0},
                });
            }
            else
            {
                Transform = Matrix<int>.Build.DenseOfArray(new int[2, 2] {
                    {0, -1},
                    {-1, 0},
                });
            }
        }
        public override Particle Apply(Particle input)
        {
            input.Direction = Transform * input.Direction;
            return input;
        }
    }
    public class Stochastic : Mirror
    {
        public Stochastic(bool orientation) : base(orientation) { }
        public override Particle Apply(Particle input)
        {
            if (Shared.Rand.Next() % 2 == 0)
            {
                return input;
            }
            else
            {
                return base.Apply(input);
            }
        }
        public Particle[] Superposition(Particle input)
        {
            Particle[] particles = new Particle[2];
            input.Multiply(.5);
            particles[0] = input.Copy();
            particles[1] = base.Apply(input);
            return particles;
        }
    }
}
