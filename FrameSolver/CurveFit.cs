using System;
using MathNet.Numerics.LinearAlgebra;

namespace FrameSolver
{
    public class CurveFit
    {
        /// <summary>
        /// Fits a 2nd degree polynom to the given points.
        /// </summary>
        public static void Polynomial2(double x1, double y1, double t1, double x2, double y2, double t2, out double a, out double b, out double c)
        {
            Matrix<double> x = Matrix<double>.Build.DenseOfArray(new double[,] { { x1 * x1, x1, 1, 0 },
                                                                                 { x2 * x2, x2, 1, 0 },
                                                                                 { 2 * x1, 1, 0,0 },
                                                                                 { 2 * x2, 1, 0,0 } });
            Vector<double> y = Vector<double>.Build.DenseOfArray(new double[] { y1, y2, t1, t2 });

            // Solve [x][p]=[y]
            Vector<double> p = x.Solve(y);
            a = p[0];
            b = p[1];
            c = p[2];
        }

        /// <summary>
        /// Fits a 3rd degree polynom to the given points.
        /// </summary>
        public static void Polynomial3(double x1, double y1, double t1, double x2, double y2, double t2, out double a, out double b, out double c, out double d)
        {
            Matrix<double> x = Matrix<double>.Build.DenseOfArray(new double[,] { { x1 * x1 * x1, x1 * x1, x1, 1 },
                                                                                 { x2 * x2 * x2, x2 * x2, x2, 1 },
                                                                                 { 3 * x1 * x1, 2 * x1, 1, 0 },
                                                                                 { 3 * x2 * x2, 2 * x2, 1, 0 } });
            Vector<double> y = Vector<double>.Build.DenseOfArray(new double[] { y1, y2, t1, t2 });

            // Solve [x][p]=[y]
            Vector<double> p = x.Solve(y);
            a = p[0];
            b = p[1];
            c = p[2];
            d = p[3];
        }

    }
}
