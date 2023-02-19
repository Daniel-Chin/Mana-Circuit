using System;
using Godot;
using MathNet.Numerics.LinearAlgebra;

public class Point : IComparable<Point>
{
    public static readonly double EPS = 1e-7;
    public static readonly double THIRD_INVERSE_EPS;
    static Point()
    {
        THIRD_INVERSE_EPS = 1 / EPS / 3;
    }
    public double X
    {
        get; private set;
    }
    public double Y
    {
        get; private set;
    }
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
    public override string ToString()
    {
        return $"({X}, {Y})";
    }
    int IComparable<Point>.CompareTo(Point that)
    {
        if (this.X == that.X)
        {
            if (this.Y == that.Y)
            {
                return 0;
            }
            else if (this.Y < that.Y)
            {
                return -1;
            }
            else
            {
                return +1;
            }
        }
        else if (this.X < that.X)
        {
            return -1;
        }
        else
        {
            return +1;
        }
    }
    public bool LessThan(Point that)
    {
        return ((IComparable<Point>)this).CompareTo(that) < 0;
    }
    public override bool Equals(object obj)
    {
        if (obj is Point other)
        {
            return X == other.X && Y == other.Y;
        }
        else
        {
            throw new Exception("ge8h43q0");
        }
    }
    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }
    public Point Offset05()
    {
        return new Point(X + .5, Y + .5);
    }
    public double ManhattanMag(Point eye)
    {
        return Math.Abs(X - eye.X) + Math.Abs(Y - eye.Y);
    }
    public static Point operator +(Point a, Point b)
    {
        return new Point(a.X + b.X, a.Y + b.Y);
    }
    public static Point operator -(Point a)
    {
        return new Point(-a.X, -a.Y);
    }
    public static Point operator -(Point a, Point b)
    {
        return new Point(a.X - b.X, a.Y - b.Y);
    }
}

public class PointInt : Point
{
    public int IntX
    {
        get; private set;
    }
    public int IntY
    {
        get; private set;
    }
    public PointInt(int x, int y) : base(x, y)
    {
        IntX = x;
        IntY = y;
    }
    public override string ToString()
    {
        return $"({IntX}, {IntY})";
    }
    public void Deconstruct(out int x, out int y)
    {
        x = IntX;
        y = IntY;
    }
    public static PointInt operator +(PointInt a, PointInt b)
    {
        return new PointInt(a.IntX + b.IntX, a.IntY + b.IntY);
    }
    public static PointInt operator -(PointInt a)
    {
        return new PointInt(-a.IntX, -a.IntY);
    }
    public static PointInt operator -(PointInt a, PointInt b)
    {
        return new PointInt(a.IntX - b.IntX, a.IntY - b.IntY);
    }
    public PointInt Rotate90()
    {
        return new PointInt(-IntY, IntX);
    }
    public PointInt Rotate270()
    {
        return new PointInt(IntY, -IntX);
    }
    public static PointInt operator *(int k, PointInt p)
    {
        return new PointInt(k * p.IntX, k * p.IntY);
    }
    public Vector<double> ToVector()
    {
        Vector<double> v = Vector<double>.Build.Dense(2);
        v[0] = IntX;
        v[1] = IntY;
        return v;
    }
    public static PointInt FromVector(Vector<double> v)
    {
        return new PointInt((int)v[0], (int)v[1]);
    }
    public Vector2 ToVector2()
    {
        return new Vector2((float)X, (float)Y);
    }
    private static readonly PointInt[] BASE_VECS = new PointInt[] {
        new PointInt(1, 0),
        new PointInt(0, 1),
        new PointInt(-1, 0),
        new PointInt(0, -1),
    };
    public static PointInt PhaseToBaseVec(int phase)
    {
        return BASE_VECS[phase];
    }
    public static int BaseVecToPhase(PointInt baseVec)
    {
        return Array.IndexOf(BASE_VECS, baseVec);
    }
}
