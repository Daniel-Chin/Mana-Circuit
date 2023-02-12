using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

public abstract class Gem
{
    public Vector<int> Location;
    public Vector<int> Size;
    public Gem() {
        Size = Vector<int>.Build.DenseOfArray(new int[2] {1, 1});
    }
    public abstract Particle Apply(Particle input);

    public class Source : Gem {
        Vector<int> Direction;
        public override Particle Apply(Particle input) {
            Debug.Assert(input == null);
            return new Particle(
                Location, Direction, 
                Simplest.One()
            );
        }
    }
    public class Drain : Gem {
        public override Particle Apply(Particle input) {
            return null;
        }
    }
    public class Wall : Gem {
        public override Particle Apply(Particle input) {
            return null;
        }
    }
    public class AddOne : Gem {
        public override Particle Apply(Particle input) {
            return new Particle(
                input.Location, input.Direction, 
                Simplest.Eval(input.Mana, Operator.PLUS, Simplest.One())
            );
        }
    }
    public class WeakMult : Gem {
        public static readonly double MULT = 1.4;   // 1.4**2 < 2
        public override Particle Apply(Particle input) {
            return new Particle(
                input.Location, input.Direction, 
                Simplest.Eval(input.Mana, Operator.TIMES, new Simplest(
                    Rank.FINITE, MULT
                ))
            );
        }
    }
    public class Doubler : Gem
    {
        Doubler() : base() {
            Size = Vector<int>.Build.DenseOfArray(new int[2] {2, 2});
        }
        public override Particle Apply(Particle input) {
            return new Particle(
                input.Location, input.Direction, 
                Simplest.Eval(input.Mana, Operator.TIMES, new Simplest(
                    Rank.FINITE, 2
                ))
            );
        }
    }
    public class Focus : Gem {
        Vector<int> Direction;
        public override Particle Apply(Particle input) {
            return new Particle(
                input.Location, Direction, 
                input.Mana
            );
        }
    }
    public class Mirror : Gem {
        Matrix<int> Transform;
        public Mirror(bool orientation) : base() {
            if (orientation) {
                Transform = Matrix<int>.Build.DenseOfArray(new int[2,2] {
                    {0, 1}, 
                    {1, 0}, 
                });
            } else {
                Transform = Matrix<int>.Build.DenseOfArray(new int[2,2] {
                    {0, -1}, 
                    {-1, 0}, 
                });
            }
        }
        public override Particle Apply(Particle input) {
            return new Particle(
                input.Location, Transform * input.Direction, 
                input.Mana
            );
        }
    }
    public class Stochastic : Mirror {
        public Stochastic(bool orientation) : base(orientation) { }
        public override Particle Apply(Particle input) {
            if (Shared.Rand.Next() % 2 == 0) {
                return input;
            } else {
                return base.Apply(input);
            }
        }
        public Particle[] Superposition(Particle input) {
            Particle[] particles = new Particle[2];
            Simplest mana = Simplest.Eval(input.Mana, Operator.TIMES, new Simplest(
                Rank.FINITE, 0.5
            ));
            particles[0] = new Particle(
                input.Location, input.Direction, mana
            );
            particles[1] = base.Apply(input);
            particles[1] = new Particle(
                particles[1].Location, particles[1].Direction, 
                mana
            );
            return particles;
        }
    }
}
