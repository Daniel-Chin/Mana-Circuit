using Godot;
using System;
using System.Collections.Generic;

public class ManaTrail : Node2D
{
    // code-defined
    private static readonly float RADIUS = 25;
    private static readonly Vector2 OFFSET = new Vector2(-RADIUS * 2, -RADIUS);
    public float Lifetime;
    public float LineWidth;
    private Queue<LineWithAge> _lines;
    private RichTextLabel _label;
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

    public ManaTrail(bool hasLabel) : base()
    {
        _lines = new Queue<LineWithAge>();
        if (hasLabel) {
            _label = new RichTextLabel();
            AddChild(_label);
            _label.BbcodeEnabled = true;
            _label.RectMinSize = new Vector2(RADIUS * 4, RADIUS * 2);
            _label.ScrollActive = false;
            _label.Theme = Shared.THEME;
            _label.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
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
        if (_label != null)
            _label.RectPosition = location + OFFSET;
    }
    public void SetMana(Simplest s) {
        if (_label == null)
            return;
        _label.Clear();
        _label.PushAlign(RichTextLabel.Align.Center);
        _label.PushColor(Colors.Cyan);
        _label.AppendBbcode(MathBB.Build(s));
        _label.Pop();
        _label.Pop();
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
        if (_label != null) 
            _label.Hide();
    }
}
