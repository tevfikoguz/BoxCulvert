using System;
using MathNet.Numerics.LinearAlgebra;

namespace FrameSolver
{
    public abstract class Element : SerializableItem
    {
        public abstract double Angle { get; }
    }

    public abstract class TwoNodeElement : Element
    {
        public Node NodeI { get; set; }
        public Node NodeJ { get; set; }
        public double Length { get { return Math.Sqrt((NodeI.X - NodeJ.X) * (NodeI.X - NodeJ.X) + (NodeI.Z - NodeJ.Z) * (NodeI.Z - NodeJ.Z)); } }
        public override double Angle { get { return Math.Atan2(NodeJ.Z - NodeI.Z, NodeJ.X - NodeI.X); } }
        public Node Centroid { get { return new Node((NodeI.X + NodeJ.X) / 2, (NodeI.Z + NodeJ.Z) / 2); } }

        public int Index { get; internal set; }

        public abstract Matrix<double> LocalElementStiffness { get; }

        protected TwoNodeElement() { ; }

        public TwoNodeElement(Node nodei, Node nodej)
        {
            NodeI = nodei;
            NodeJ = nodej;
        }

        /// <summary>
        /// Returns the element stiffness matrix in global coordinates.
        /// </summary>
        public Matrix<double> ElementStiffness
        {
            get
            {
                Matrix<double> k = LocalElementStiffness;
                Matrix<double> t = Transformation;
                Matrix<double> tt = t.Transpose();
                Matrix<double> kt = tt * k * t;

                return kt;
            }
        }

        /// <summary>
        /// Returns the affine transformation matrix.
        /// </summary>
        public Matrix<double> Transformation
        {
            get
            {
                Matrix<double> m = Matrix<double>.Build.Dense(6, 6, 0);
                double L = Length;
                double c = (NodeJ.X - NodeI.X) / L;
                double s = (NodeJ.Z - NodeI.Z) / L;

                m[0, 0] = c;
                m[0, 1] = s;
                m[1, 0] = -s;
                m[1, 1] = c;
                m[2, 2] = 1;

                m[3, 3] = c;
                m[3, 4] = s;
                m[4, 3] = -s;
                m[4, 4] = c;
                m[5, 5] = 1;

                return m;
            }
        }
    }
}
