using Godot;

public class SFX {
    public static readonly AudioStreamOGGVorbis JUMP = GD.Load<AudioStreamOGGVorbis>(
        "res://sfx/jump.ogg"
    );

    public static void Play(AudioStream x) {
        Main.Singleton.AudioPlayer.Stream = x;
        Main.Singleton.AudioPlayer.Play(0);
    }
}
