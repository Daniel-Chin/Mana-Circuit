using Godot;
using System;
using System.Collections.Generic;

public class ManaTrail : Node2D
{
    private static readonly float LIFETIME = .5f;

    // public bool IsReady = false;
    private Queue<LineWithAge> _lines;
    private Vector2? _head;

    public class LineWithAge : Line2D
    {
        public float Remaining;
        public LineWithAge() : base()
        {
            Remaining = LIFETIME;
        }
    }

    public override void _Ready()
    {
        // IsReady = true;
        _lines = new Queue<LineWithAge>();
        _head = null;
    }
    public void ArriveAt(Vector2 location)
    {
        if (_head is Vector2 head)
        {
            LineWithAge line = new LineWithAge();
            AddChild(line);
            _lines.Enqueue(line);
            line.AddPoint(head);
            line.AddPoint(location);
            // line.DefaultColor = Colors.White;
            line.Width = 3;
        }
        _head = location;
    }

    public override void _Process(float delta)
    {
        foreach (LineWithAge line in _lines)
        {
            line.Remaining -= delta;
            float x = Math.Max(0f, line.Remaining / LIFETIME);
            line.DefaultColor = Color.FromHsv(.55f, 1f, 1f, x);
        }
        while (_lines.Count != 0 && _lines.Peek().Remaining < 0)
        {
            _lines.Dequeue();
        }
    }
}
