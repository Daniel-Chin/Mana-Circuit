using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class MagicProblem
{
    public int N;
    public Simplest[,] AWithoutDiag;
    public Simplest[] MinusB;

    public MagicProblem(int n)
    {
        N = n;
        AWithoutDiag = Simplest.Zeros(N, N);
        MinusB = Simplest.Zeros(N);
    }

    public Vector<double> SolveFinite()
    {
        Simplest[] solution = new Simplest[N];
        Matrix<double> A = Matrix<double>.Build.Dense(N, N);
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Simplest s = AWithoutDiag[i, j];
                if (s.MyRank != Rank.FINITE)
                    return null;
                A[i, j] = s.K;
                if (i == j)
                    A[i, j]--;
            }
        }
        Vector<double> b = Vector<double>.Build.Dense(N);
        for (int i = 0; i < N; i++)
        {
            b[i] = -MinusB[i].K;
        }
        Vector<double> x = A.Solve(b);
        if (double.IsNaN(x.Minimum()) || x.Minimum() < 0)
            return null;    // reject NaN & negative solutions
        return x;
    }

    public static void Test()
    {
        // Matrix<double> A = DenseMatrix.OfArray(new double[,] {
        //         {1,1,1,1},
        //         {1,2,3,4},
        //         {4,3,2,1}});
        // Vector<double>[] nullspace = A.Kernel();

        // // verify: the following should be approximately (0,0,0)
        // var x = (A * (2*nullspace[0] - 3*nullspace[1]));
        // Console.Write("x = ");
        // Console.WriteLine(x);

        Matrix<double> A = DenseMatrix.OfArray(new double[,] {
            {1,1,1},
            {1,2,3},
            {4,3,2}
        });
        Vector<double> b = DenseVector.OfArray(new double[] {
            0, 0, 0
        });
        var x = A.Solve(b);
        Console.WriteLine(x);
        x[1] = 3;
        Console.WriteLine(x);
        Console.WriteLine(x.Minimum());
        Console.ReadKey();
    }

    public void Print()
    {
        Console.WriteLine("MagicProblem BEGIN");
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Console.Write(AWithoutDiag[i, j]);
                Console.Write(" ");
                Console.Write((char)((int)'A' + j));
                Console.Write(" + ");
            }
            Console.Write(MinusB[i]);
            Console.Write(" = ");
            Console.Write((char)((int)'A' + i));
            Console.WriteLine();
        }
        Console.WriteLine("MagicProblem END");
    }
}
