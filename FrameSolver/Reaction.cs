using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class Reaction : SerializableItem, ICombinable<Reaction>
    {
        public Node Node { get; set; }
        public double FX { get; set; }
        public double FZ { get; set; }
        public double MY { get; set; }

        private Reaction() { ; }

        public Reaction(Node node, double fx, double fz, double my)
        {
            Node = node;
            FX = fx;
            FZ = fz;
            MY = my;
        }

        public override string ToString()
        {
            return Node.Index + ": " + FX.ToString("0.0") + ", " + FZ.ToString("0.0") + ", " + MY.ToString("0.0");
        }

        public bool IsCompatibleWith(Reaction other)
        {
            return Node.ID == other.Node.ID;
        }

        public Reaction Abs()
        {
            return new Reaction(Node, Math.Abs(FX), Math.Abs(FZ), Math.Abs(MY));
        }

        public Reaction Min(Reaction other)
        {
            return new Reaction(Node, Math.Min(FX, other.FX), Math.Min(FZ, other.FZ), Math.Min(MY, other.MY));
        }

        public Reaction Max(Reaction other)
        {
            return new Reaction(Node, Math.Max(FX, other.FX), Math.Max(FZ, other.FZ), Math.Max(MY, other.MY));
        }

        public Reaction Multiply(double factor)
        {
            return new Reaction(Node, factor * FX, factor * FZ, factor * MY);
        }

        public Reaction Add(Reaction other)
        {
            return new Reaction(Node, FX + other.FX, FZ + other.FZ, MY + other.MY);
        }

        public Reaction MultiplyAndAdd(double factor, Reaction other)
        {
            return new Reaction(Node, FX + factor * other.FX, FZ + factor * other.FZ, MY + factor * other.MY);
        }
    }
}
