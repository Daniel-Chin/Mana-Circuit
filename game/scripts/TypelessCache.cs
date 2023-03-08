using Godot;

public class TypelessCache : Viewport {
    public static readonly Vector2 RESOLUTION = new Vector2(256, 256);
    public CircuitUI CUI;
    public ImageTexture ImgTexture;
    public TypelessCache() {
        Size = RESOLUTION;
        Hdr = false;
        Usage = UsageEnum.Usage2d;
        RenderTargetUpdateMode = UpdateMode.Always;
        if (GameState.Persistent.MyTypelessGem != null) {
            Start();
        }
    }

    public void Start() {
        CUI = new CircuitUI(
            GameState.Persistent.MyTypelessGem, 0, 
            false, true
        );
        AddChild(CUI);
        CUI.Rebuild();
    }

    public override void _Process(float delta)
    {
        Image img = GetTexture().GetData();
        img.FlipY();
        ImageTexture iT = new ImageTexture();
        iT.CreateFromImage(img);
        ImgTexture = iT;
    }
}
