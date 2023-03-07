using Godot;

public class DroppedGem : SpawnableSpecial {
    public Gem MyGem { get; set; }

    public override string ToString()
    {
        return $"<dropped {MyGem}>";
    }
}
