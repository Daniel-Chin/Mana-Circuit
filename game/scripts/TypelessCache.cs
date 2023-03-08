using System;
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
        } else {
            ImgTexture = new ImageTexture();
            ImgTexture.Create(8, 8, Image.Format.Rgbah);
        }
    }

    public void Start() {
        CUI = new CircuitUI(
            GameState.Persistent.MyTypelessGem, 1, 
            false, true
        );
        AddChild(CUI);
        CUI.RectMinSize = RESOLUTION;
        CUI.Rebuild();
        _Process(0);
    }

    public override void _Process(float delta)
    {
        if (CUI == null)
            return;
        Image img = GetTexture().GetData();
        img.FlipY();
        ImageTexture iT = new ImageTexture();
        iT.CreateFromImage(img);
        ImgTexture = iT;
    }

    public void Rebuild() {
        if (CUI != null)
            CUI.Rebuild();
    }
}
