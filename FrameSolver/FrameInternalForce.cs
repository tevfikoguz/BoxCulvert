using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class FrameInternalForce : SerializableItem, ICombinable<FrameInternalForce>
    {
        public Frame Frame { get; set; }

        public double NI { get; set; }
        public double NJ { get; set; }

        public double VI { get; set; }
        public double VJ { get; set; }

        public double MI { get; set; }
        public double MJ { get; set; }

        private FrameInternalForce() { ; }

        public FrameInternalForce(Frame frame, double ni, double vi, double mi, double nj, double vj, double mj)
        {
            Frame = frame;

            NI = ni;
            VI = vi;
            MI = mi;

            NJ = nj;
            VJ = vj;
            MJ = mj;
        }

        public override string ToString()
        {
            return Frame.Index + ": I->(" + NI.ToString("0.0") + ", " + VI.ToString("0.0") + ", " + MI.ToString("0.0") + "), J->(" +
                NJ.ToString("0.0") + ", " + VJ.ToString("0.0") + ", " + MJ.ToString("0.0") + ")";
        }

        public bool IsCompatibleWith(FrameInternalForce other)
        {
            return Frame.ID == other.Frame.ID;
        }

        public FrameInternalForce Abs()
        {
            return new FrameInternalForce(Frame, Math.Abs(NI), Math.Abs(VI), Math.Abs(MI),
                Math.Abs(NJ), Math.Abs(VJ), Math.Abs(MJ));
        }

        public FrameInternalForce Min(FrameInternalForce other)
        {
            return new FrameInternalForce(Frame, Math.Min(NI, other.NI), Math.Min(VI, other.VI), Math.Min(MI, other.MI),
                Math.Min(NJ, other.NJ), Math.Min(VJ, other.VJ), Math.Min(MJ, other.MJ));
        }

        public FrameInternalForce Max(FrameInternalForce other)
        {
            return new FrameInternalForce(Frame, Math.Max(NI, other.NI), Math.Max(VI, other.VI), Math.Max(MI, other.MI),
                Math.Max(NJ, other.NJ), Math.Max(VJ, other.VJ), Math.Max(MJ, other.MJ));
        }

        public FrameInternalForce Multiply(double factor)
        {
            return new FrameInternalForce(Frame, factor * NI, factor * VI, factor * MI,
                factor * NJ, factor * VJ, factor * MJ);
        }

        public FrameInternalForce Add(FrameInternalForce other)
        {
            return new FrameInternalForce(Frame, NI + other.NI, VI + other.VI, MI + other.MI,
                NJ + other.NJ, VJ + other.VJ, MJ + other.MJ);
        }

        public FrameInternalForce MultiplyAndAdd(double factor, FrameInternalForce other)
        {
            return new FrameInternalForce(Frame, NI + factor * other.NI, VI + factor * other.VI, MI + factor * other.MI,
                NJ + factor * other.NJ, VJ + factor * other.VJ, MJ + factor * other.MJ);
        }
    }
}
