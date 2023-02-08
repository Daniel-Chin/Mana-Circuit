using System;
using System.Diagnostics;
using System.Linq;

public class Variable {}
public class Function {
  public Expression Body;
}

public class TerminalType : EnumClass {
  public static readonly TerminalType NUMBER   = new TerminalType(0);
  public static readonly TerminalType OMEGA    = new TerminalType(1);
  public static readonly TerminalType VARIABLE = new TerminalType(2);
  public static readonly TerminalType FUNCTION = new TerminalType(3);
}

public class Terminal {
  public static readonly Terminal w = new Terminal(TerminalType.OMEGA);

  public TerminalType MyTerminalType;
  public float MyNumber;
  public Variable MyVariable;
  public Function MyFunction;

  public Terminal(TerminalType terminalType) {
    MyTerminalType = terminalType;
  }
  public Terminal(Variable variable) {
    MyTerminalType = TerminalType.VARIABLE;
    MyVariable = variable;
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
    if (! IsTerminal && MyOperator == op) {
      return Left.Top(op).Concat(Right.Top(op)).ToArray();
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
      new Expression(new Terminal(-1)), Operator.TIMES, Right
    ));
    newLeft.ExpandAll();
    List<Expression> k = new List<Expression>();
    List<Expression> b = new List<Expression>();
    foreach (var part in newLeft.Top(Operator.PLUS)) {
      if (part.DoesContain(function)) {
        ParsePart(part, function);
      } else {
        b.Add(part);
      }
    }
    // TODO: negate b
  }

  private Expression ParsePart(Expression part, Function function) {
    Debug.Assert(! part.IsTerminal);
    Expression container = null;
    if (part.Left.DoesContain(function)) {
      container = Left;
    }
    if (part.Right.DoesContain(function)) {
      if (container is null) {
        container = Right;
      } else {
        throw CannotIsolateFunction("Both Left and Right contain `function`.");
      }
    }
    Debug.Assert(container is object);
    if (container.IsTerminal) {
      Debug.Assert(part.MyOperator == Operator.APPLY);
      Debug.Assert(container.MyTerminal.MyFunction == function);
      return new Expression(new Terminal(1));
    }
    if (container.MyOperator != Operator.APPLY)
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
