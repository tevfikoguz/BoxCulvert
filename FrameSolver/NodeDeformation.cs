using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class NodeDeformation : SerializableItem, ICombinable<NodeDeformation>
    {
        public Node Node { get; set; }
        public double UX { get; set; }
        public double UZ { get; set; }
        public double RY { get; set; }

        private NodeDeformation() { ; }

        public NodeDeformation(Node node, double ux, double uz, double ry)
        {
            Node = node;
            UX = ux;
            UZ = uz;
            RY = ry;
        }

        public override string ToString()
        {
            return Node.Index + ": " + UX.ToString("0.0000") + ", " + UZ.ToString("0.0000") + ", " + RY.ToString("0.0000");
        }

        public bool IsCompatibleWith(NodeDeformation other)
        {
            return Node.ID == other.Node.ID;
        }

        public NodeDeformation Abs()
        {
            return new NodeDeformation(Node, Math.Abs(UX), Math.Abs(UZ), Math.Abs(RY));
        }

        public NodeDeformation Min(NodeDeformation other)
        {
            return new NodeDeformation(Node, Math.Min(UX, other.UX), Math.Min(UZ, other.UZ), Math.Min(RY, other.RY));
        }

        public NodeDeformation Max(NodeDeformation other)
        {
            return new NodeDeformation(Node, Math.Max(UX, other.UX), Math.Max(UZ, other.UZ), Math.Max(RY, other.RY));
        }

        public NodeDeformation Multiply(double factor)
        {
            return new NodeDeformation(Node, factor * UX, factor * UZ, factor * RY);
        }

        public NodeDeformation Add(NodeDeformation other)
        {
            return new NodeDeformation(Node, UX + other.UX, UZ + other.UZ, RY + other.RY);
        }

        public NodeDeformation MultiplyAndAdd(double factor, NodeDeformation other)
        {
            return new NodeDeformation(Node, UX + factor * other.UX, UZ + factor * other.UZ, RY + factor * other.RY);
        }
    }
}
