using Godot;
using System;

public class Money : Node2D, SpawnableUI
{
    private static readonly float REPEL = 1f;
    private static readonly float FRICTION = 1f;
    public Vector2 Velocity = new Vector2(0, 0);
    public void Step(Vector2 force, float dt)
    {
        Velocity += REPEL * force * dt;
        Velocity *= (float)Math.Exp(-dt * FRICTION);
        Position += Velocity * dt;
    }
}
