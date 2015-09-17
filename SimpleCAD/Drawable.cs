using System;
using System.Drawing;

namespace SimpleCAD
{
    public abstract class Drawable
    {
        public OutlineStyle OutlineStyle { get; set; }
        public FillStyle FillStyle { get; set; }

        public abstract void Draw(DrawParams param);
        public abstract Extents GetExtents();
        public abstract void TransformBy(Matrix2D transformation);

        protected Drawable()
        {
            OutlineStyle = new OutlineStyle(Color.Black, 0, System.Drawing.Drawing2D.DashStyle.Solid);
            FillStyle = new FillStyle();
        }
    }
}
