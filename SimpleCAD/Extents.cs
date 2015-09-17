using System;
using System.Drawing;

namespace SimpleCAD
{
    public class Extents
    {
        public bool Empty { get; private set; }
        public float XMin { get; private set; }
        public float YMin { get; private set; }
        public float XMax { get; private set; }
        public float YMax { get; private set; }
        public float Width { get { return Math.Abs(XMax - XMin); } }
        public float Height { get { return Math.Abs(YMax - YMin); } }

        public Extents()
        {
            Empty = true;
        }

        public void Reset()
        {
            Empty = true;
        }

        public void Add(float x, float y)
        {
            if (Empty || x < XMin) XMin = x;
            if (Empty || y < YMin) YMin = y;
            if (Empty || x > XMax) XMax = x;
            if (Empty || y > YMax) YMax = y;

            Empty = false;
        }

        public void Add(Point2D pt)
        {
            Add(pt.X, pt.Y);
        }

        public void Add(Point2D[] points)
        {
            foreach (Point2D pt in points)
            {
                Add(pt.X, pt.Y);
            }
        }

        public void Add(RectangleF rectangle)
        {
            Add(rectangle.X, rectangle.Y);
            Add(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
        }

        public void Add(Extents extents)
        {
            Add(extents.XMin, extents.YMin);
            Add(extents.XMax, extents.YMax);
        }

        public static implicit operator RectangleF(Extents extents)
        {
            if (extents.Empty)
                return RectangleF.Empty;
            else
                return new RectangleF(extents.XMin, extents.YMin, extents.XMax - extents.XMin, extents.YMax - extents.YMin);
        }
    }
}
