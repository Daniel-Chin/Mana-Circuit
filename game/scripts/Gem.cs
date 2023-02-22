using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

public abstract class Gem : MagicItem
{
    public List<PointInt> Locations { get; set; }
    public PointInt Size { get; set; }
    static Gem()
    {
        // verify consistency
        // FromID(N_IDS - 1);
        // bool ok = true;
        // try
        // {
        //     FromID(N_IDS);
        //     ok = false;
        // }
        // catch (Shared.ValueError) { }
        // Debug.Assert(ok);
    }
    public Gem()
    {
        Size = new PointInt(1, 1);
        Locations = new List<PointInt>();
    }
    public abstract Particle Apply(Particle input);
    public abstract string Explain();

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
        public override string Name()
        {
            return "source";
        }
        public override string DisplayName()
        {
            return "Radiator Gem";
        }
        public override string Explain()
        {
            return "";
        }
    }
    public class Drain : Gem
    {
        public override Particle Apply(Particle input)
        {
            return null;
        }
        public override string Name()
        {
            return "drain";
        }
        public override string DisplayName()
        {
            return "Mana Crystalizer";
        }
        public override string Explain()
        {
            return "";
        }
    }
    public class Wall : Gem
    {
        public override Particle Apply(Particle input)
        {
            return null;
        }
        public override string Name()
        {
            return "wall";
        }
        public override string DisplayName()
        {
            return "Blockage";
        }
        public override string Explain()
        {
            return "";
        }
    }
    public class RemoveGem : Gem
    {
        public override Particle Apply(Particle input)
        {
            return input;
        }
        public override string Name()
        {
            return "transparent";
        }
        public override string DisplayName()
        {
            return "RemoveGem";
        }
        public override string Explain()
        {
            return "Remove gem.";
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
        public override string Name()
        {
            return "addOne";
        }
        public override string DisplayName()
        {
            return "+1 Gem";
        }
        public override string Explain()
        {
            return "Add one mana.";
        }
    }
    public class WeakMult : Gem
    {
        private static readonly double MULT = 1.4;  // 1.4**2 < 2
        public override Particle Apply(Particle input)
        {
            input.Multiply(MULT);
            return input;
        }
        public override string Name()
        {
            return "weakMult";
        }
        public override string DisplayName()
        {
            return $"x{MULT} Gem";
        }
        public override string Explain()
        {
            return $"Multiply mana by {MULT:#.#}.";
        }
    }
    // public class Doubler : Gem
    // {
    //     public Doubler() : base()
    //     {
    //         Size = new PointInt(2, 2);
    //     }
    //     public override Particle Apply(Particle input)
    //     {
    //         input.Multiply(2);
    //         return input;
    //     }
    // }
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
        public override string Name()
        {
            return "focus";
        }
        public override string DisplayName()
        {
            return "Focus Gem";
        }
        public override string Explain()
        {
            return "Focus mana from different directions.";
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
        public override string Name()
        {
            return "mirror";
        }
        public override string DisplayName()
        {
            return "Mirror Gem";
        }
        public override string Explain()
        {
            return "Reflect mana.";
        }
    }
    public class Stochastic : Mirror
    {
        public Stochastic(bool orientation) : base(orientation) { }
        public Particle ApplyMirror(Particle input)
        {
            return base.Apply(input);
        }
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
        public override string Name()
        {
            return "stochastic";
        }
        public override string DisplayName()
        {
            return "Stochastic Gem";
        }
        public override string Explain()
        {
            return "Reflect mana with probability = 50%.";
        }
    }

    // public static readonly int N_IDS = 7;
    // public static Gem FromID(int id)
    // {
    //     switch (id)
    //     {
    //         case 0:
    //             return new Source(new PointInt(0, 1));
    //         case 1:
    //             return new Drain();
    //         case 2:
    //             return new AddOne();
    //         case 3:
    //             return new WeakMult();
    //         case 4:
    //             return new Focus(new PointInt(0, 1));
    //         case 5:
    //             return new Mirror(true);
    //         case 6:
    //             return new Stochastic(true);
    //         default:
    //             throw new Shared.ValueError();
    //     }
    // }
}
