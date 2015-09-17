using System;

namespace SimpleCAD
{
    public struct Point2D
    {
        private float mX;
        private float mY;

        public float X { get { return mX; } set { mX = value; } }
        public float Y { get { return mY; } set { mY = value; } }

        public Point2D(float x, float y)
        {
            mX = x;
            mY = y;
        }

        public void TransformBy(Matrix2D transformation)
        {
            float x = transformation.M11 * X + transformation.M12 * Y + transformation.DX;
            float y = transformation.M21 * X + transformation.M22 * Y + transformation.DY;
            X = x;
            Y = y;
        }

        public static float Distance(Point2D p1, Point2D p2)
        {
            return (p1 - p2).Length;
        }

        public static Point2D operator +(Point2D p, Vector2D v)
        {
            return new Point2D(p.X + v.X, p.Y + v.Y);
        }

        public static Point2D operator -(Point2D p, Vector2D v)
        {
            return new Point2D(p.X - v.X, p.Y - v.Y);
        }

        public static Vector2D operator -(Point2D p1, Point2D p2)
        {
            return new Vector2D(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point2D operator *(Point2D p, float f)
        {
            return new Point2D(p.X * f, p.Y * f);
        }

        public static Point2D operator *(float f, Point2D p)
        {
            return new Point2D(p.X * f, p.Y * f);
        }

        public static Point2D operator /(Point2D p, float f)
        {
            return new Point2D(p.X / f, p.Y / f);
        }

        public static implicit operator System.Drawing.PointF(Point2D p)
        {
            return new System.Drawing.PointF(p.X, p.Y);
        }

        public static implicit operator Point2D(System.Drawing.PointF p)
        {
            return new Point2D(p.X, p.Y);
        }

        public override string ToString()
        {
            return "{" + X.ToString() + ", " + Y.ToString() + "}";
        }
    }

    public struct Vector2D
    {
        private float mX;
        private float mY;

        public float X { get { return mX; } set { mX = value; } }
        public float Y { get { return mY; } set { mY = value; } }
        public float Length { get { return (float)Math.Sqrt(X * X + Y * Y); } }
        public float Angle { get { return AngleTo(Vector2D.XAxis); } }

        public static Vector2D XAxis { get { return new Vector2D(1, 0); } }
        public static Vector2D YAxis { get { return new Vector2D(0, 1); } }

        public Vector2D(float x, float y)
        {
            mX = x;
            mY = y;
        }

        public void TransformBy(Matrix2D transformation)
        {
            float x = transformation.M11 * X + transformation.M12 * Y + transformation.DX;
            float y = transformation.M21 * X + transformation.M22 * Y + transformation.DY;
            X = x;
            Y = y;
        }

        public void Normalize()
        {
            float len = Length;
            X /= len;
            Y /= len;
        }

        public float DotProduct(Vector2D v)
        {
            return X * v.X + Y * v.Y;
        }

        public float AngleTo(Vector2D v)
        {
            return (float)Math.Acos(DotProduct(v) / Length / v.Length);
        }

        public Vector2D GetPerpendicularVector()
        {
            return new Vector2D(-Y, X);
        }

        public static Vector2D operator *(Vector2D p, float f)
        {
            return new Vector2D(p.X * f, p.Y * f);
        }

        public static Vector2D operator *(float f, Vector2D p)
        {
            return new Vector2D(p.X * f, p.Y * f);
        }

        public static Vector2D operator /(Vector2D p, float f)
        {
            return new Vector2D(p.X / f, p.Y / f);
        }

        public override string ToString()
        {
            return "{" + X.ToString() + ", " + Y.ToString() + "}";
        }
    }

    public struct Matrix2D
    {
        private float mM11;
        private float mM12;
        private float mM21;
        private float mM22;
        private float mDX;
        private float mDY;

        public float M11 { get { return mM11; } set { mM11 = value; } }
        public float M12 { get { return mM12; } set { mM12 = value; } }
        public float M21 { get { return mM21; } set { mM21 = value; } }
        public float M22 { get { return mM22; } set { mM22 = value; } }
        public float DX { get { return mDX; } set { mDX = value; } }
        public float DY { get { return mDY; } set { mDY = value; } }
        public float RotationAngle { get { return -(float)Math.Asin(M12); } }

        public Matrix2D(float m11, float m12, float m21, float m22, float dx, float dy)
        {
            mM11 = m11; mM12 = m12;
            mM21 = m21; mM22 = m22;
            mDX = dx; mDY = dy;
        }

        public static Matrix2D Transformation(float xScale, float yScale, float rotation, float dx, float dy)
        {
            float m11 = xScale * (float)Math.Cos(rotation);
            float m12 = -(float)Math.Sin(rotation);
            float m21 = (float)Math.Sin(rotation);
            float m22 = yScale * (float)Math.Cos(rotation);

            return new Matrix2D(m11, m12, m21, m22, dx, dy);
        }

        public static Matrix2D Scale(float xScale, float yScale)
        {
            float m11 = xScale;
            float m22 = yScale;

            return new Matrix2D(m11, 0, 0, m22, 0, 0);
        }

        public static Matrix2D Rotation(float rotation)
        {
            float m11 = (float)Math.Cos(rotation);
            float m12 = (float)Math.Sin(rotation);
            float m21 = -(float)Math.Sin(rotation);
            float m22 = (float)Math.Cos(rotation);

            return new Matrix2D(m11, m12, m21, m22, 0, 0);
        }

        public static Matrix2D Translation(float dx, float dy)
        {
            return new Matrix2D(0, 0, 0, 0, dx, dy);
        }

        public override string ToString()
        {
            string str = "|" + M11.ToString() + ", " + M12.ToString() + ", " + DX.ToString() + "|" + Environment.NewLine +
                         "|" + M21.ToString() + ", " + M22.ToString() + ", " + DY.ToString() + "|" + Environment.NewLine +
                         "|0, 0, 1|";
            return str;
        }
    }
}
