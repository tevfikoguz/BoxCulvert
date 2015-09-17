using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SimpleCAD
{
    public struct OutlineStyle
    {
        private Color mColor;
        private float mLineWeight;
        private DashStyle mDashStyle;

        public Color Color { get { return mColor; } set { mColor = value; } }
        public float LineWeight { get { return mLineWeight; } set { mLineWeight = value; } }
        public DashStyle DashStyle { get { return mDashStyle; } set { mDashStyle = value; } }

        public OutlineStyle(Color color, float lineWeight, DashStyle dashStyle)
        {
            mColor = color;
            mLineWeight = lineWeight;
            mDashStyle = dashStyle;
        }

        public OutlineStyle(Color color, float lineWeight)
            : this(color, lineWeight, DashStyle.Solid)
        {
            ;
        }

        public OutlineStyle(Color color)
            : this(color, 0, DashStyle.Solid)
        {
            ;
        }

        public Pen CreatePen(DrawParams param)
        {
            Pen pen = new Pen(Color, param.GetScaledLineWeight(LineWeight));
            pen.DashStyle = DashStyle;
            return pen;
        }
    }
}
