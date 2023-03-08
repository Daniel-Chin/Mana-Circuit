using System;
using System.Threading;

public class Screenshot {
    // intentionally thread-unsafe
    public static readonly bool ACTIVE = false;
    public static Godot.Image Data;
    public static int Acc;
    public static bool Continue;
    public static bool Finished;
    // public static Semaphore Sema;
    private static Thread _thread;
    private static int acc;
    private static float accTime;
    static Screenshot() {
        Data = new Godot.Image();
        Data.Create(
            (int)Shared.RESOLUTION.x, 
            (int)Shared.RESOLUTION.y, 
            false, Godot.Image.Format.Rgbah
        );
        Data.Fill(Godot.Colors.Black);
        Acc = 1;
        Continue = true;
        Finished = false;
        // Sema = new Semaphore(1, 1);

        acc = 0;
        accTime = 0;
    }
    public static void Start() {
        if (ACTIVE) {
            _thread = new Thread(new ThreadStart(Worker));
            _thread.Start();
        }
    }
    public static void Join() {
        if (ACTIVE) {
            _thread.Join();
        }
    }
    public static void Once() {
        Godot.Image img = Main.Singleton.GetViewport().GetTexture().GetData();
        img.Resize(
            (int)Shared.RESOLUTION.x, (int)Shared.RESOLUTION.y
            , Godot.Image.Interpolation.Nearest
        );
        Data = img;
    }
    public static void Worker() {
        // Sema.WaitOne();
        while (Continue) {
            Once();
            Acc ++;
            acc ++;
            if (Main.MainTime >= accTime + 1) {
                accTime ++;
                Console.Write("Screenshots ");
                Console.Write(acc);
                Console.WriteLine(" / sec.");
                acc = 0;
            }
        }
        // Sema.Release();
        Console.WriteLine("Screenshot worker exit");
        Finished = true;
    }

    public static Godot.Image Rect(Godot.Control control) {
        float flippedY = (
            Shared.RESOLUTION.y - control.RectGlobalPosition.y
            - control.RectSize.y
        );
        Godot.Image img = Data.GetRect(new Godot.Rect2(
            new Godot.Vector2(
                control.RectGlobalPosition.x, flippedY
            ), 
            control.RectSize
        ));
        img.FlipY();
        return img;
    }
}
