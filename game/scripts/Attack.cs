using System;
using Godot;

public class Attack : ManaTrail, SpawnableUI
{
    private static readonly float DISTANCE = 20;
    public Vector2 Direction { get; set; }
    public Vector2 Head { get; set; }
    public Simplest Mana { get; set; }
    private double _time_since_last_advance;

    public Attack() : base()
    {
        Head = new Vector2(0, 0);
        _head = Head;
        _time_since_last_advance = 0;
        Lifetime = 1;
    }

    public bool Advect(float dt)
    {
        _time_since_last_advance += dt;
        if (CheckAdvance())
        {
            ArriveAt(Head + Direction * DISTANCE);
            Head = (Vector2)_head;
            return true;
        }
        return false;
    }

    private bool CheckAdvance()
    {
        if (Mana.MyRank != Rank.FINITE)
        {
            _time_since_last_advance = 0;
            return true;
        }
        double interval = 1 / (
            // Math.Log(Mana.K) + .3
            Math.Pow(Math.Min(500.0, Mana.K), .8) * .3
            // Mana.K * .3
            + Params.ATTACK_FLY_BASE_SPEED
        );
        if (_time_since_last_advance >= interval)
        {
            _time_since_last_advance -= interval;
            return true;
        }
        return false;
    }

    public override void _Process(float delta)
    {
        if (GameState.Transient.WorldPaused)
            return;
        base._Process(delta);
    }
}
