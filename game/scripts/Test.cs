using System;
using System.Diagnostics;
using System.Text;

public class Test
{
    static public void Main()
    {
        Console.WriteLine("begin");
        TestMath();
        Console.WriteLine("end");
        Console.ReadKey();
    }

    static void TestMath() {
        for (int i = 0; i < 32; i++)
        {
            Console.WriteLine("=== ONE ITER ===");
            // Expression ww = new Expression(
            //     Expression.w, Operator.POWER, Expression.w
            // );
            // Expression wp1 = new Expression(
            //     Expression.w, Operator.PLUS, Expression.ONE
            // );
            // Expression wp1tw = new Expression(
            //     wp1, Operator.TIMES, Expression.w
            // );
            // Expression wp1twp1 = new Expression(
            //     wp1, Operator.TIMES, wp1
            // );
            // Console.WriteLine(wp1twp1);
            // wp1twp1.ExpandAll();
            // Console.WriteLine(wp1twp1);
            Expression e = Expression.Sample(0);
            // Expression e = new Expression(
            //     new Expression(new Terminal(3.4)), Operator.POWER, ww
            // );
            Console.WriteLine(e);
            Console.WriteLine();
            Simplest s = Simplest.FromExpression(e, true);
            Console.WriteLine();
            e.ExpandAll();
            Console.WriteLine(e);
            Console.WriteLine();
            Simplest es = Simplest.FromExpression(e, false);
            if (! s.Equals(es)) {
                Console.WriteLine("Oh NOOOO");
                throw new ApplicationException();
            }
            Console.WriteLine(s);
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
