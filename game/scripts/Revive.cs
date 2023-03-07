using Godot;
using System;

public class Revive : PanelContainer
{
    public Button Button1;
    public Button Button2;
    public Button Button3;
    public RichTextLabel MoneyLabel;
    public RichTextLabel DistanceLabel;
    public override void _Ready()
    {
        Button1 = GetNode<Button>("VBox/Button1");
        Button2 = GetNode<Button>("VBox/Button2");
        Button3 = GetNode<Button>("VBox/Button3");
        MoneyLabel = GetNode<RichTextLabel>("VBox/Money");
        DistanceLabel = GetNode<RichTextLabel>("VBox/Distance");
        Button1.Connect(
            "pressed", this, "Button1Pressed"
        );
        Button2.Connect(
            "pressed", this, "Button2Pressed"
        );
        Button3.Connect(
            "pressed", this, "Button3Pressed"
        );
    }

    public void Activate()
    {
        Button2.Disabled = true;
        Button3.Disabled = true;
        Visible = true;
        GameState.Transient.Mana = Simplest.Zero();
        GameState.Transient.NextSpawn = null;
        Director.NowEvent = null;
        Director.PauseWorld();
        Main.Singleton.VBoxLowPanel.Visible = false;
        Update();
        Main.Singleton.MySidePanel.Update();
    }

    public Simplest Downgrade(Simplest s) {
        if (s.Equals(Simplest.Bottom(s.MyRank)))
        {
            return new Simplest(
                s.MyRank - 1, 2
            );
        }
        else
        {
            return new Simplest(
                s.MyRank, s.K - 1
            );
        }
    }

    public void Button1Pressed()
    {
        Simplest money = GameState.Persistent.Money;
        GameState.Persistent.Sema.WaitOne();
        if (money.MyRank == Rank.FINITE) {
            GameState.Persistent.Money = Simplest.Finite(Math.Ceiling(money.K * .1));
        } else {
            GameState.Persistent.Money = Downgrade(money);
        }
        GameState.Persistent.Sema.Release();
        Update();
        Button2.Disabled = false;

        Main.Singleton.MySidePanel.Update();
    }
    public void Button2Pressed()
    {
        Simplest dist = GameState.Persistent.Location_dist;
        GameState.Persistent.Sema.WaitOne();
        if (dist.MyRank == Rank.FINITE)
        {
            GameState.Persistent.Location_dist = new Simplest(
                Rank.FINITE, Math.Max(
                    0, dist.K - 2
                )
            );
        }
        else
        {
            GameState.Persistent.Location_dist = Downgrade(dist);
        }
        GameState.Persistent.Sema.Release();
        Update();
        Button3.Disabled = false;

        GameState.Transient.NextSpawn = null;
        GameState.Transient.Update();
        Main.Singleton.MyWorld.ClearSpawns();
        Main.Singleton.MyWorld.UpdateBack();
    }
    public void Button3Pressed()
    {
        Visible = false;
        Director.UnpauseWorld();
        if (GameState.Persistent.MyWand is Wand.Ricecooker)
            GameState.Transient.NextSpawn = new NPC.WandSmith();
            GameState.Transient.EnemiesTillNextSpawn = 0;
    }

    public new void Update()
    {
        string money = MathBB.Build(GameState.Persistent.Money);
        MoneyLabel.BbcodeText = $"[center]Money: [color=yellow]${money}[/color][/center]";
        string distance = MathBB.Build(GameState.Persistent.Location_dist);
        DistanceLabel.BbcodeText = $"[center]Distance: {distance}[/center]";
    }
}
