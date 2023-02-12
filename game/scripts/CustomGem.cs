using System.Diagnostics;

class CustomGem : Gem {
    Simplest MetaLevel;
    Circuit circuit;    // only one source one drain
    Simplest CachedMultiplier;
    public override Particle Apply(Particle input) {
        Debug.Assert(CachedMultiplier != null);
        return new Particle(
            input.Location, input.Direction, 
            Simplest.Eval(input.Mana, Operator.TIMES, CachedMultiplier)
        );
    }
    public Simplest MinimumSuperpositionEquilibrium(int input) {
        Source source = circuit.FindSources()[0];
    }
}
