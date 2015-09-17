using System;
using System.Collections.Generic;

namespace FrameSolver
{
    public class Node : SerializableItem
    {
        public double X { get; set; }
        public double Z { get; set; }
        public DOF Restraints { get; set; }

        public int Index { get; internal set; }

        public List<NodeLoad> Loads { get; internal set; }

        private Node() { ; }

        public Node(double x, double z)
            : this(x, z, DOF.Free)
        {
            ;
        }

        public Node(double x, double z, DOF restraints)
        {
            X = x;
            Z = z;
            Restraints = restraints;
            Loads = new List<NodeLoad>();
        }

        public double DistanceTo(Node n)
        {
            return (float)Math.Sqrt((X - n.X) * (X - n.X) + (Z - n.Z) * (Z - n.Z));
        }

        public static double Distance(Node n1, Node n2)
        {
            return n1.DistanceTo(n2);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", X, Z);
        }

        /// <summary>
        /// Adds a new node point load.
        /// </summary>
        public void AddPointLoad(AnalysisCase analysisCase, double fx, double fz, double my)
        {
            Loads.Add(new NodePointLoad(analysisCase, fx, fz, my));
        }
    }
}
