using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

public enum TerminalType
{
    NUMBER, OMEGA
}

public class Terminal
{
    public static readonly Terminal w = new Terminal(TerminalType.OMEGA);

    public TerminalType MyTerminalType;
    public double MyNumber;

    public Terminal(TerminalType terminalType)
    {
        MyTerminalType = terminalType;
    }
    public Terminal(double number)
    {
        MyTerminalType = TerminalType.NUMBER;
        MyNumber = number;
    }
    public override string ToString()
    {
        switch (MyTerminalType)
        {
            case TerminalType.NUMBER:
                return MyNumber.ToString("0.0");
            case TerminalType.OMEGA:
                return "w";
            default:
                throw new Shared.ObjectStateIllegal();
        }
    }
}

public enum Operator
{
    PLUS, TIMES, POWER
    // The order must represent precedence.  
}

public class Expression
{
    public static readonly Expression ONE = new Expression(new Terminal(1));
    public static readonly Expression MINUS_ONE = new Expression(new Terminal(-1));
    public static readonly Expression w = new Expression(Terminal.w);

    public bool IsTerminal;
    public Terminal MyTerminal;
    public Operator MyOperator;
    public Expression Left;
    public Expression Right;

    public Expression(Terminal terminal)
    {
        IsTerminal = true;
        MyTerminal = terminal;
    }

    public Expression(
        Expression left, Operator op, Expression right
    )
    {
        IsTerminal = false;
        Left = left;
        MyOperator = op;
        Right = right;
    }

    public override string ToString()
    {
        StringBuilder sB = new StringBuilder();
        BuildString(sB, null, false);
        return sB.ToString();
    }
    public void BuildString(
        StringBuilder sB, Operator? parentOperator,
        bool isLeftNotRight
    )
    {
        if (IsTerminal)
        {
            sB.Append(MyTerminal.ToString());
            return;
        }
        bool doParenthesis = false;
        if (parentOperator != null)
        {
            if (
                parentOperator > MyOperator
                || parentOperator == Operator.POWER
                && isLeftNotRight
            )
            {
                doParenthesis = true;
            }
        }
        if (doParenthesis) sB.Append('('); else sB.Append('<');
        Left.BuildString(sB, MyOperator, true);
        sB.Append(' ');
        sB.Append(OperatorToChar(MyOperator));
        sB.Append(' ');
        Right.BuildString(sB, MyOperator, false);
        if (doParenthesis) sB.Append(')'); else sB.Append('>');
    }
    static public char OperatorToChar(Operator op) {
        switch (op)
        {
            case Operator.PLUS:
                return '+';
            case Operator.TIMES:
                return '*';
            case Operator.POWER:
                return '^';
            default:
                throw new Shared.ObjectStateIllegal();
        }
    }
    public void ExpandAll()
    {
        if (IsTerminal) return;
        if (MyOperator == Operator.TIMES)
        {
            if (! Left .IsTerminal && Left .MyOperator == Operator.PLUS)
            {
                (Left, Right) = (Right, Left);
            }
            if (! Right.IsTerminal && Right.MyOperator == Operator.PLUS)
            {
                (MyOperator, Left, Right) = (
                    Operator.PLUS,
                    new Expression(Left, Operator.TIMES, Right.Left),
                    new Expression(Left, Operator.TIMES, Right.Right)
                );
            }
        }
        Left.ExpandAll();
        Right.ExpandAll();
    }

    public Expression[] Top(Operator op)
    {
        if (!IsTerminal)
        {
            if (MyOperator == op)
            {
                return Left.Top(op).Concat(Right.Top(op)).ToArray();
            }
            else
            {
                Debug.Assert(MyOperator > op);
            }
        }
        return new Expression[] { this };
    }

    public static Expression Sample(double probToTerminate)
    {
        // Debug.Assert(probToTerminate > .5);    // converge
        if (Shared.Rand.NextDouble() < probToTerminate) {
            if (Shared.Rand.NextDouble() < .6) {
                return new Expression(new Terminal(
                    Shared.Rand.NextDouble()
                ));
            } else {
                return Expression.w;
            }
        } else {
            double x = Shared.Rand.NextDouble();
            Operator op;
            if (x < .4) {
                op = Operator.PLUS;
            } else if (x < .7) {
                op = Operator.TIMES;
            } else {
                op = Operator.POWER;
            }
            probToTerminate += (1 - probToTerminate) * .3;
            return new Expression(
                Sample(probToTerminate), op, 
                Sample(probToTerminate)
            );
        }
    }
}

class Simplest
{
    public static readonly Simplest w = new Simplest(1);
    public static readonly Simplest TwoToThew = new Simplest(3);
    public static readonly Simplest ww = new Simplest(4);
    public static readonly Simplest www = new Simplest(5);

    public int Rank;
    public bool IsNumber;
    public bool IsWToTheK;
    public bool IsWWWW;
    public double K;
    protected Simplest(
        bool isNumber, bool isWToTheK, bool isWWWW, double k
    )
    {
        IsNumber = isNumber;
        IsWToTheK = isWToTheK;
        IsWWWW = isWWWW;
        K = k;
        if (isNumber) Rank = 0;
        if (isWToTheK) Rank = 2;
        if (isWWWW) Rank = 6;
    }
    protected Simplest(int rank)
    {
        Rank = rank;
        if (rank == 1) K = 1;
        if (rank == 4) K = 2;
        if (rank == 5) K = 3;
    }
    public static Simplest FromExpression(
        Expression expression, bool verbose
    )
    {
        if (expression.IsTerminal)
        {
            TerminalType terminalType = expression.MyTerminal.MyTerminalType;
            if (terminalType == TerminalType.NUMBER)
                return new Simplest(true, false, false, expression.MyTerminal.MyNumber);
            if (terminalType == TerminalType.OMEGA)
                return w;
        }
        Simplest left  = FromExpression(expression.Left , verbose);
        Simplest right = FromExpression(expression.Right, verbose);
        if (verbose) {
            Console.Write(left);
            Console.Write(Expression.OperatorToChar(expression.MyOperator));
            Console.Write(right);
            Console.WriteLine();
        }

        Simplest result = Eval(
            left, expression.MyOperator, right
        );
        if (verbose) {
            Console.Write(" = ");
            Console.WriteLine(result);
        }
        return result;
    }
    public static Simplest Eval(
        Simplest left, Operator op, Simplest right
    )
    {
        Simplest a; Simplest b;
        if (left.Rank < right.Rank)
        {
            a = left; b = right;
        }
        else
        {
            b = left; a = right;
        }
        switch (op)
        {
            case Operator.PLUS:
                if (a.Rank < b.Rank)
                    return b;
                if (a.IsNumber)
                    return new Simplest(
                        true, false, false, a.K + b.K
                    );
                return b;
            case Operator.TIMES:
                if (a.IsNumber)
                {
                    if (b.IsNumber)
                        return new Simplest(
                            true, false, false, a.K * b.K
                        );
                    return b;
                }
                if (a.Rank == 1)
                {
                    if (b.Rank < 3)
                        return new Simplest(
                            false, true, false, 1 + b.K
                        );
                    return b;
                }
                if (a.Rank == 2)
                {
                    if (b.Rank < 3)
                        return new Simplest(
                            false, true, false, a.K + b.K
                        );
                    return b;
                }
                if (a.Rank >= 3)
                {
                    return b;
                }
                break;
            case Operator.POWER:
                if (a.IsNumber)
                {
                    if (b.IsNumber)
                        return new Simplest(
                            true, false, false, Math.Pow(left.K, right.K)
                        );
                    if (b.Rank == 1)
                    {
                        if (a.K < 1) 
                            return new Simplest(
                                true, false, false, 0
                            );
                        if (a.K == 1) 
                            return new Simplest(
                                true, false, false, 1
                            );
                        return TwoToThew;
                    }
                    if (b.Rank < 5)
                        return www;
                    if (b.Rank == 5)
                        return new Simplest(
                            false, false, true, 4
                        );
                    if (b.Rank == 6)
                        return new Simplest(
                            false, false, true, b.K + 1
                        );
                } else {
                    if (b.Rank == 1)
                        return ww;
                    if (b.Rank < 5)
                        return www;
                    if (b.Rank == 5)
                        return new Simplest(
                            false, false, true, 4
                        );
                    if (b.Rank == 6)
                        return new Simplest(
                            false, false, true, b.K + 1
                        );
                }
                break;
        }
        throw new Shared.ObjectStateIllegal();
    }

    public override string ToString()
    {
        if (this == w)
        {
            return "w";
        }
        else if (this == TwoToThew)
        {
            return "2^w";
        }
        else if (this == ww)
        {
            return "w^w";
        }
        else if (this == www)
        {
            return "w^w^w";
        }
        else
        {
            if (IsNumber)
            {
                return K.ToString();
            }
            else if (IsWToTheK)
            {
                return $"w^{K}";
            }
            else if (IsWWWW)
            {
                return new String('w', (int)K);
            }
            else
                throw new Shared.ObjectStateIllegal();
        }
    }
}
