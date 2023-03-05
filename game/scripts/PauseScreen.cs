using Godot;
using System;

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
    }

    public void Resume() {
        Hide();
        Director.UnpauseWorld();
    }
    public void SaveAndQuit() {
        SaveLoad.Save();
        GetTree().Quit();
    }
}
