using Godot;
using System;
using System.Collections.Generic;

public class ManaTrail : Node2D
{
    // code-defined
    public float Lifetime;
    public float LineWidth;
    private Queue<LineWithAge> _lines;
    protected Vector2? _head;
    private bool _freed;

    public class LineWithAge : Line2D
    {
        public float Remaining;
        public LineWithAge(float lifetime) : base()
        {
            Remaining = lifetime;
        }
    }

    public ManaTrail() : base()
    {
        _lines = new Queue<LineWithAge>();
        _head = null;
        _freed = false;
        LineWidth = 10;  // default
        Lifetime = .2f;  // default
    }
    public void ArriveAt(Vector2 location)
    {
        if (_head is Vector2 head)
        {
            LineWithAge line = new LineWithAge(Lifetime);
            AddChild(line);
            _lines.Enqueue(line);
            line.AddPoint(head);
            line.AddPoint(location);
            // line.DefaultColor = Colors.White;
            line.Width = LineWidth;
        }
        _head = location;
    }

    public override void _Process(float delta)
    {
        foreach (LineWithAge line in _lines)
        {
            line.Remaining -= delta;
            float x = Math.Max(0f, line.Remaining / Lifetime);
            line.DefaultColor = Color.FromHsv(.55f, 1f, 1f, x);
        }
        while (_lines.Count != 0 && _lines.Peek().Remaining < 0)
        {
            _lines.Dequeue().QueueFree();
        }
        if (_freed && _lines.Count == 0)
        {
            base.QueueFree();
            // Console.WriteLine("Free mana trial");
        }
    }

    public new void QueueFree()
    {
        _freed = true;
    }
}
