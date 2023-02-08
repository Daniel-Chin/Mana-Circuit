using System;
using System.Diagnostics;
using System.Linq;

public class Function {
  public Expression Body;
}

public class TerminalType : EnumClass {
  public static readonly TerminalType NUMBER   = new TerminalType(0);
  public static readonly TerminalType OMEGA    = new TerminalType(1);
  public static readonly TerminalType X        = new TerminalType(2);
  public static readonly TerminalType FUNCTION = new TerminalType(3);
}

public class Terminal {
  public static readonly Terminal w = new Terminal(TerminalType.OMEGA);

  public TerminalType MyTerminalType;
  public float MyNumber;
  public Function MyFunction;

  public Terminal(TerminalType terminalType) {
    MyTerminalType = terminalType;
  }
  public Terminal(Function function) {
    MyTerminalType = TerminalType.FUNCTION;
    MyFunction = function;
  }
  public Terminal(float number) {
    MyTerminalType = TerminalType.NUMBER;
    MyNumber = number;
  }
}

public class Operator : EnumClass {
  public static readonly Operator PLUS  = new Operator(0);
  public static readonly Operator TIMES = new Operator(1);
  public static readonly Operator POWER = new Operator(2);
  public static readonly Operator APPLY = new Operator(3);
}

public class Expression {
  public static readonly Expression w = new Expression(Terminal.w);
  public static readonly Expression ONE = new Expression(new Terminal(1));
  public static readonly Expression MINUS_ONE = new Expression(new Terminal(-1));

  public bool IsTerminal;
  public Terminal MyTerminal;
  public Operator MyOperator;
  public Expression Left; 
  public Expression Right; 

  public Expression(Terminal terminal) {
    IsTerminal = true;
    MyTerminal = terminal;
  }

  public Expression(
    Expression left, Operator op, Expression right
  ) {
    IsTerminal = false;
    Left = left;
    MyOperator = op;
    Right = right;
  }

  public void ExpandAll() {
    if (IsTerminal) return;
    if (MyOperator == Operator.TIMES) {
      if (Left.MyOperator == Operator.PLUS) {
        (Left, Right) = (Right, Left);
      }
      if (Right.MyOperator == Operator.PLUS) {
        (MyOperator, Left, Right) = (
          Operator.PLUS, 
          new Expression(Left, Operator.TIMES, Right.Left), 
          new Expression(Left, Operator.TIMES, Right.Right)
        );
      }
    }
    Left .ExpandAll();
    Right.ExpandAll();
  }

  public Expression[] Top(Operator op) {
    if (! IsTerminal) {
      if (MyOperator == op) {
        return Left.Top(op).Concat(Right.Top(op)).ToArray();
      } else {
        Debug.Assert(MyOperator.Id > op.Id);
      }
    }
    return new Expression[]{ this };
  }

  public bool DoesContain(Function function) {
    if (IsTerminal) {
      return (
        MyTerminal.MyTerminalType == TerminalType.FUNCTION && 
        MyTerminal.MyFunction == function
      );
    } else {
      return Left.DoesContain(function) || Right.DoesContain(function);
    }
  }
}

public class Equation {
  public Expression Left;
  public Expression Right;

  public Expression SolveFor(Function function) {
    // Only used when assuming class-1 solutions.  
    Expression newLeft = new Expression(Left, Operator.PLUS, new Expression(
      Expression.MINUS_ONE, Operator.TIMES, Right
    ));
    newLeft.ExpandAll();
    List<Expression> k = new List<Expression>();
    List<Expression> b = new List<Expression>();
    foreach (var part in newLeft.Top(Operator.PLUS)) {
      if (part.DoesContain(function)) {
        k.Add(ParsePart(part, function));
      } else {
        b.Add(part);
      }
    }
    b = new Expression(
      Expression.MINUS_ONE, Operator.TIMES, b
    );
  }

  private Expression ParsePart(Expression expression, Function function) {
    Debug.Assert(! expression.IsTerminal);
    Expression[] parts = expression.Top(Operator.TIMES);
    Expression factor = Expression.ONE;
    int acc = 0;
    foreach (var part in parts) {
      if (part.DoesContain(funciton)) {
        acc ++;
        if (part.IsTerminal)
          throw new CannotIsolateFunction("wt9q28");
        if (part.MyOperator != Operator.APPLY)
          throw new CannotIsolateFunction("t8932rg");
        if (part.Left .MyTerminal.MyFunction != function)
          throw new CannotIsolateFunction("v83r0ht");
        if (part.Right.MyTerminal.MyTerminalType != TerminalType.X)
          throw new CannotIsolateFunction("gr38024h");
      } else {
        factor = new Expression(
          factor, Operator.TIMES, part
        );
      }
    }
    if (acc != 1)
      throw new CannotIsolateFunction("5tn3oe0");
    return factor;
  }

  public class CannotIsolateFunction : Exception { }
}

public class EquationSystem {
  public Equation[] Equations;
  public Function[] Functions;

  public void Solve() {
    Debug.Assert(Equations.Length == Functions.Length);
    // brute force the least fixed point
    // class-1
    if (TryClassOne()) {
      return;
    }
    // class-w
  }

  private bool TryClassOne() {
    foreach (var function in Functions) {
      function.Body = null;
    }
    foreach (var (equation, function) in Equations.Zip(Functions)) {
      
    }
  }
}
