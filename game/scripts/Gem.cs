using MathNet.Numerics.LinearAlgebra;

public abstract class Gem
{
    public PointInt Location;
    public PointInt Size;
    public Gem()
    {
        Size = new PointInt(1, 1);
    }
    public Gem Place(PointInt location)
    {
        Location = location;
        return this;
    }
    public abstract Particle Apply(Particle input);

    public class Source : Gem
    {
        public PointInt Direction;
        public Source(PointInt direction) : base()
        {
            Direction = direction;
        }
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
        public Doubler() : base()
        {
            Size = new PointInt(2, 2);
        }
        public override Particle Apply(Particle input)
        {
            input.Multiply(2);
            return input;
        }
    }
    public class Focus : Gem
    {
        public PointInt Direction;
        public Focus(PointInt direction) : base()
        {
            Direction = direction;
        }
        public override Particle Apply(Particle input)
        {
            input.Direction = Direction;
            return input;
        }
    }
    public class Mirror : Gem
    {
        public bool Orientation;
        private Matrix<double> _transform;
        public Mirror(bool orientation) : base()
        {
            Orientation = orientation;
            if (orientation)
            {
                _transform = Matrix<double>.Build.DenseOfArray(
                    new double[2, 2] {
                        {0, 1},
                        {1, 0},
                    }
                );
            }
            else
            {
                _transform = Matrix<double>.Build.DenseOfArray(
                    new double[2, 2] {
                        {0, -1},
                        {-1, 0},
                    }
                );
            }
        }
        public override Particle Apply(Particle input)
        {
            input.Direction = PointInt.FromVector(
                _transform * input.Direction.ToVector()
            );
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
