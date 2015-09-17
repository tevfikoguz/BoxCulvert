using System;
using MathNet.Numerics.LinearAlgebra;

namespace FrameSolver
{
    public class Spring : TwoNodeElement
    {
        public double AxialStiffness { get; set; }
        public double ShearStiffness { get; set; }
        public double RotationalStiffness { get; set; }

        protected Spring() { ; }

        public Spring(double axialStiffness, double shearStiffness, double rotationalStiffness, Node nodei, Node nodej)
            : base(nodei, nodej)
        {
            AxialStiffness = axialStiffness;
            RotationalStiffness = rotationalStiffness;
        }

        /// <summary>
        /// Returns the element stiffness matrix in local coordinates.
        /// </summary>
        public override Matrix<double> LocalElementStiffness
        {
            get
            {
                Matrix<double> m = Matrix<double>.Build.Dense(6, 6, 0);

                double K = AxialStiffness;
                double Kv = ShearStiffness;
                double Km = RotationalStiffness;

                m[0, 0] = K;
                m[3, 0] = m[0, 3] = -K;
                m[3, 3] = K;

                m[1, 1] = Kv;
                m[4, 1] = m[1, 4] = -Kv;
                m[4, 4] = Kv;

                m[2, 2] = Km;
                m[5, 2] = m[2, 5] = Km / 2;
                m[5, 5] = Km;

                return m;
            }
        }
    }
}
