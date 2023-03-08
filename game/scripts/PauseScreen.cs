using System;
using System.Text;
using Godot;

public class PauseScreen : WindowDialog
{
    public override void _Ready()
    {
        GetNode<Button>("VBox/HBox/ResumeButton").Connect(
            "pressed", this, "Resume"
        );
        GetNode<Button>("VBox/HBox/QuitButton").Connect(
            "pressed", this, "SaveAndQuit"
        );
        StringBuilder sB = new StringBuilder();
        sB.Append("# Game save \n");
        sB.Append("Location: \n");
        sB.Append(Godot.OS.GetUserDataDir());
        sB.Append("\n");
        sB.Append("Delete all files there to reset the game. \n");
        GetNode<TextEdit>("VBox/TextHBox/TextEdit").Text += sB.ToString();
    }

    public void Resume() {
        Hide();
        Director.UnpauseWorld();
    }
    public void SaveAndQuit() {
        SaveLoad.Save();
        Main.Singleton.Quit();
    }
}
