using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace SimpleCAD
{
    public partial class CADWindow : UserControl
    {
        private bool mPanning;
        private System.Drawing.Point mLastMouse;

        public CADView View { get; private set; }
        public CADModel Model { get; private set; }
        public float DrawingScale { get { return View.ZoomFactor; } }

        public bool AllowZoomAndPan { get; set; }

        public CADWindow()
        {
            InitializeComponent();

            Model = new CADModel();
            View = new CADView(Model, ClientRectangle.Width, ClientRectangle.Height);

            AllowZoomAndPan = true;
            mPanning = false;

            BackColor = Color.White;
            Cursor = Cursors.Cross;

            Resize += new EventHandler(CadView_Resize);
            MouseDown += new MouseEventHandler(CadView_MouseDown);
            MouseUp += new MouseEventHandler(CadView_MouseUp);
            MouseMove += new MouseEventHandler(CadView_MouseMove);
            MouseDoubleClick += new MouseEventHandler(CadView_MouseDoubleClick);
            MouseWheel += new MouseEventHandler(CadView_MouseWheel);
            Paint += new PaintEventHandler(CadView_Paint);
        }

        public void ZoomIn()
        {
            View.ZoomIn();
            Invalidate();
        }

        public void ZoomOut()
        {
            View.ZoomOut();
            Invalidate();
        }

        public void ZoomToExtents()
        {
            View.ZoomToExtents();
            Invalidate();
        }

        void CadView_Resize(object sender, EventArgs e)
        {
            View.Resize(ClientRectangle.Width, ClientRectangle.Height);
            Invalidate();
        }

        void CadView_MouseDown(object sender, MouseEventArgs e)
        {
            if (((e.Button & MouseButtons.Middle) != MouseButtons.None) && AllowZoomAndPan)
            {
                mPanning = true;
                mLastMouse = e.Location;
                Cursor = Cursors.NoMove2D;
            }
        }

        void CadView_MouseUp(object sender, MouseEventArgs e)
        {
            if (((e.Button & MouseButtons.Middle) != MouseButtons.None) && mPanning)
            {
                mPanning = false;
                Invalidate();
            }

            Cursor = Cursors.Cross;
        }

        void CadView_MouseMove(object sender, MouseEventArgs e)
        {
            if (((e.Button & MouseButtons.Middle) != MouseButtons.None) && mPanning)
            {
                // Relative mouse movement
                PointF cloc = View.ScreenToWorld(e.Location);
                PointF ploc = View.ScreenToWorld(mLastMouse);
                SizeF delta = new SizeF(cloc.X - ploc.X, cloc.Y - ploc.Y);
                View.Pan(delta);
                mLastMouse = e.Location;
                Invalidate();
            }
        }

        void CadView_MouseWheel(object sender, MouseEventArgs e)
        {
            if (AllowZoomAndPan)
            {
                System.Drawing.Point pt = e.Location;
                PointF ptw = View.ScreenToWorld(pt);

                if (e.Delta > 0)
                {
                    View.ZoomIn();
                }
                else
                {
                    View.ZoomOut();
                }
                Invalidate();
            }
        }

        void CadView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (((e.Button & MouseButtons.Middle) != MouseButtons.None) && AllowZoomAndPan)
            {
                View.ZoomToExtents();
            }
        }

        void CadView_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            e.Graphics.Clear(BackColor);

            View.Render(e.Graphics);
        }
    }
}
