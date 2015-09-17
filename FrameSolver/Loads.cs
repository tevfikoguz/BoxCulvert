using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public abstract class NodeLoad : SerializableItem
    {
        public AnalysisCase AnalysisCase { get; private set; }

        protected NodeLoad() { ; }

        public NodeLoad(AnalysisCase analysisCase)
        {
            AnalysisCase = analysisCase;
        }
    }

    public class NodePointLoad : NodeLoad
    {
        public double FX { get; set; }
        public double FZ { get; set; }
        public double MY { get; set; }

        protected NodePointLoad() { ; }

        public NodePointLoad(AnalysisCase analysisCase, double fx, double fz, double my)
            : base(analysisCase)
        {
            FX = fx;
            FZ = fz;
            MY = my;
        }
    }

    public abstract class FrameLoad : SerializableItem
    {
        public AnalysisCase AnalysisCase { get; private set; }
        
        protected FrameLoad() { ; }

        public FrameLoad(AnalysisCase analysisCase)
        {
            AnalysisCase = analysisCase;
        }
    }

    public class FrameUniformLoad : FrameLoad
    {
        public double FX { get; set; }
        public double FZ { get; set; }

        protected FrameUniformLoad() { ; }

        public FrameUniformLoad(AnalysisCase analysisCase, double fx, double fz)
            : base(analysisCase)
        {
            FX = fx;
            FZ = fz;
        }
    }

    public class FrameTrapezoidalLoad : FrameLoad
    {
        public double FXI { get; set; }
        public double FZI { get; set; }
        public double FXJ { get; set; }
        public double FZJ { get; set; }

        protected FrameTrapezoidalLoad() { ; }

        public FrameTrapezoidalLoad(AnalysisCase analysisCase, double fxi, double fzi, double fxj, double fzj)
            : base(analysisCase)
        {
            FXI = fxi;
            FZI = fzi;

            FXJ = fxj;
            FZJ = fzj;
        }
    }
}
