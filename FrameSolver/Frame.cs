using System;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace FrameSolver
{
    public class Frame : TwoNodeElement
    {
        public FrameSection Section { get; set; }
        public double WeightPerLength { get { return Section.Area * Section.Material.UnitWeight; } }

        public List<FrameLoad> Loads { get; internal set; }

        protected Frame() { ; }

        public Frame(FrameSection section, Node nodei, Node nodej)
            : base(nodei, nodej)
        {
            Section = section;

            Loads = new List<FrameLoad>();
        }

        /// <summary>
        /// Adds a new frame uniform load.
        /// </summary>
        public void AddUniformLoad(AnalysisCase analysisCase, double fx, double fz)
        {
            Loads.Add(new FrameUniformLoad(analysisCase, fx, fz));
        }

        /// <summary>
        /// Adds a new frame trapezoidal load.
        /// </summary>
        public void AddTrapezoidalLoad(AnalysisCase analysisCase, double fxi, double fzi, double fxj, double fzj)
        {
            Loads.Add(new FrameTrapezoidalLoad(analysisCase, fxi, fzi, fxj, fzj));
        }

        /// <summary>
        /// Returns the element stiffness matrix in local coordinates.
        /// </summary>
        public override Matrix<double> LocalElementStiffness
        {
            get
            {
                Matrix<double> m = Matrix<double>.Build.Dense(6, 6, 0);

                double E = Section.Material.ElasticityModulus;
                double A = Section.Area;
                double I = Section.MomentOfInertia;
                double L = Length;

                m[0, 0] = E * A / L;
                m[1, 1] = 12.0 * E * I / (L * L * L);
                m[2, 1] = m[1, 2] = 6.0 * E * I / (L * L);
                m[2, 2] = 4.0 * E * I / L;
                m[3, 0] = m[0, 3] = -E * A / L;
                m[3, 3] = E * A / L;
                m[4, 1] = m[1, 4] = -12.0 * E * I / (L * L * L);
                m[4, 2] = m[2, 4] = -6.0 * E * I / (L * L);
                m[4, 4] = 12.0 * E * I / (L * L * L);
                m[5, 1] = m[1, 5] = 6.0 * E * I / (L * L);
                m[5, 2] = m[2, 5] = 2.0 * E * I / L;
                m[5, 4] = m[4, 5] = -6.0 * E * I / (L * L);
                m[5, 5] = 4.0 * E * I / L;

                return m;
            }
        }
    }
}
