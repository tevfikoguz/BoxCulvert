using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SimpleCAD
{
    public struct FillStyle
    {
        public enum FillType
        {
            None = 0,
            Solid = 1,
            Hatch = 2
        }

        private FillType mType;
        private Color mColor;
        private Color mFillColor;
        private HatchStyle mHatchStyle;

        public FillType Type { get { return mType; } set { mType = value; } }
        public Color Color { get { return mColor; } set { mColor = value; } }
        public Color FillColor { get { return mFillColor; } set { mFillColor = value; } }
        public HatchStyle HatchStyle { get { return mHatchStyle; } set { mHatchStyle = value; } }

        public FillStyle(Color color)
        {
            mType = FillType.Solid;
            mColor = color;
            mFillColor = Color.Transparent;
            mHatchStyle = HatchStyle.Percent50;
        }

        public FillStyle(Color color, Color fillColor, HatchStyle hatchStyle)
        {
            mType = FillType.Hatch;
            mColor = color;
            mFillColor = fillColor;
            mHatchStyle = hatchStyle;
        }

        public Brush CreateBrush(DrawParams param)
        {
            if (Type == FillType.Solid)
                return new SolidBrush(Color);
            else if (Type == FillType.Hatch)
                return new HatchBrush(HatchStyle, Color, FillColor);

            return new SolidBrush(Color.Transparent);
        }
    }
}
