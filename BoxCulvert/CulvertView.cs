using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using FrameSolver;
using System.Collections.Generic;

namespace BoxCulvert
{
    public enum ViewMode
    {
        Section,
        AnalysisModel,
        Loads,
        Rebars,
        Moments,
        AxialForces,
        ShearForces,
        Deflections
    }

    public enum OutputMode
    {
        SelfWeight,
        EarthFill,
        SoilPressure,
        Surcharge,
        Racking,
        ServiceCombination,
        FactoredCombination1,
        FactoredCombination2,
        SeismicCombination,
        Envelope
    }

    public enum LoadMode
    {
        EarthFill,
        SoilPressure,
        Surcharge,
        Racking
    }

    [Flags]
    public enum LabelMode
    {
        None = 0,
        Nodes = 1,
        Frames = 2,
        All = Nodes | Frames
    }

    public enum EnvelopeMode
    {
        Minimum,
        Maximum,
        Envelope
    }

    public partial class CulvertView : SimpleCAD.CADWindow
    {
        private Section mCurrentSection;

        private ViewMode mViewMode;
        private OutputMode mOutputMode;
        private LoadMode mLoadMode;
        private LabelMode mLabelMode;
        private EnvelopeMode mEnvelopeMode;

        private float mDrawingPadding;
        private float mDimensionOffset;
        private float mTextSize;

        private Color mFormWorkColor;
        private Color mShadingColor;
        private Color mDimensionColor;
        private Color mLoadColor;

        private Color mNegativeForceColor;
        private Color mPositiveForceColor;
        private Color mEnvelopeForceColor;
        private float mFillTransparency;

        private float mLineThickness;
        private float mDimensionLineThickness;

        [Category("Drawing"), DefaultValue(typeof(ViewMode), "Section")]
        public ViewMode ViewMode { get { return mViewMode; } set { mViewMode = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(OutputMode), "Service")]
        public OutputMode OutputMode { get { return mOutputMode; } set { mOutputMode = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(LoadMode), "EarthFill")]
        public LoadMode LoadMode { get { return mLoadMode; } set { mLoadMode = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(LabelMode), "None")]
        public LabelMode LabelMode { get { return mLabelMode; } set { mLabelMode = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(EnvelopeMode), "Range")]
        public EnvelopeMode EnvelopeMode { get { return mEnvelopeMode; } set { mEnvelopeMode = value; Refresh(); } }

        [Category("Drawing"), DefaultValue(50)]
        public float DrawingPadding { get { return mDrawingPadding; } set { mDrawingPadding = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(0.2f)]
        public float DimensionOffset { get { return mDimensionOffset; } set { mDimensionOffset = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(0.1f)]
        public float TextSize { get { return mTextSize; } set { mTextSize = value; Refresh(); } }

        [Category("Drawing"), DefaultValue(typeof(Color), "DarkBlue")]
        public Color FormWorkColor { get { return mFormWorkColor; } set { mFormWorkColor = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(Color), "LightGray")]
        public Color ShadingColor { get { return mShadingColor; } set { mShadingColor = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(Color), "Black")]
        public Color DimensionColor { get { return mDimensionColor; } set { mDimensionColor = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(Color), "Black")]
        public Color LoadColor { get { return mLoadColor; } set { mLoadColor = value; Refresh(); } }

        [Category("Drawing"), DefaultValue(typeof(Color), "Red")]
        public Color NegativeForceColor { get { return mNegativeForceColor; } set { mNegativeForceColor = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(Color), "Blue")]
        public Color PositiveForceColor { get { return mPositiveForceColor; } set { mPositiveForceColor = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(typeof(Color), "Purple")]
        public Color EnvelopeForceColor { get { return mEnvelopeForceColor; } set { mEnvelopeForceColor = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(0.65f)]
        public float FillTransparency { get { return mFillTransparency; } set { mFillTransparency = value; Refresh(); } }

        [Category("Drawing"), DefaultValue(3.0f)]
        public float LineThickness { get { return mLineThickness; } set { mLineThickness = value; Refresh(); } }
        [Category("Drawing"), DefaultValue(1.0f)]
        public float DimensionLineThickness { get { return mDimensionLineThickness; } set { mDimensionLineThickness = value; Refresh(); } }

        public CulvertView()
        {
            mCurrentSection = null;

            mViewMode = ViewMode.Section;
            mOutputMode = OutputMode.ServiceCombination;
            mLoadMode = LoadMode.EarthFill;
            mLabelMode = LabelMode.None;
            mEnvelopeMode = EnvelopeMode.Envelope;

            mDrawingPadding = 50;
            mDimensionOffset = 0.2f;
            mTextSize = 0.1f;

            mFormWorkColor = Color.DarkBlue;
            mShadingColor = Color.LightGray;
            mDimensionColor = Color.Black;
            mLoadColor = Color.Black;

            mNegativeForceColor = Color.Red;
            mPositiveForceColor = Color.Blue;
            mEnvelopeForceColor = Color.Purple;
            mFillTransparency = 0.65f;

            mLineThickness = 3.0f;
            mDimensionLineThickness = 1.0f;

            ResizeRedraw = true;
            InitializeComponent();
        }

        public void SetSection(Section section)
        {
            mCurrentSection = section;

            if (mCurrentSection == null) return;

            Model.Clear();
            switch (ViewMode)
            {
                case ViewMode.Section:
                    DrawFormwork();
                    DrawDimensions();
                    break;

                case ViewMode.AnalysisModel:
                    DrawAnalysisModel();
                    if ((LabelMode & LabelMode.Nodes) != LabelMode.None)
                        DrawNodeNumbers();
                    if ((LabelMode & LabelMode.Frames) != LabelMode.None)
                        DrawFrameNumbers();
                    break;

                case ViewMode.Loads:
                    DrawCulvertFrame();
                    if (LoadMode == LoadMode.EarthFill)
                        DrawEarthFillLoad();
                    else if (LoadMode == LoadMode.SoilPressure)
                        DrawSoilPressureLoad();
                    else if (LoadMode == LoadMode.Surcharge)
                        DrawSurchargeLoad();
                    else if (LoadMode == LoadMode.Racking)
                        DrawRackingLoad();
                    break;

                case ViewMode.Rebars:
                    DrawFormwork(false);

                    break;

                case ViewMode.AxialForces:
                    DrawAnalysisModel(Color.Gray, false, false);
                    if (OutputMode == OutputMode.SelfWeight)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.AnalysisCases[0]);
                    else if (OutputMode == OutputMode.EarthFill)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.AnalysisCases[1]);
                    else if (OutputMode == OutputMode.SoilPressure)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.AnalysisCases[2]);
                    else if (OutputMode == OutputMode.Surcharge)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.AnalysisCases[3]);
                    else if (OutputMode == OutputMode.Racking)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.AnalysisCases[4]);
                    else if (OutputMode == OutputMode.ServiceCombination)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.Combinations[0]);
                    else if (OutputMode == OutputMode.FactoredCombination1)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.Combinations[1]);
                    else if (OutputMode == OutputMode.FactoredCombination2)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.Combinations[2]);
                    else if (OutputMode == OutputMode.SeismicCombination)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.Combinations[4]);
                    else if (OutputMode == OutputMode.Envelope)
                        DrawFrameAxialForces(mCurrentSection.AnalysisModel.Combinations[5]);

                    break;

                case ViewMode.ShearForces:
                    DrawAnalysisModel(Color.Gray, false, false);
                    if (OutputMode == OutputMode.SelfWeight)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.AnalysisCases[0]);
                    else if (OutputMode == OutputMode.EarthFill)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.AnalysisCases[1]);
                    else if (OutputMode == OutputMode.SoilPressure)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.AnalysisCases[2]);
                    else if (OutputMode == OutputMode.Surcharge)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.AnalysisCases[3]);
                    else if (OutputMode == OutputMode.Racking)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.AnalysisCases[4]);
                    else if (OutputMode == OutputMode.ServiceCombination)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.Combinations[0]);
                    else if (OutputMode == OutputMode.FactoredCombination1)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.Combinations[1]);
                    else if (OutputMode == OutputMode.FactoredCombination2)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.Combinations[2]);
                    else if (OutputMode == OutputMode.SeismicCombination)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.Combinations[4]);
                    else if (OutputMode == OutputMode.Envelope)
                        DrawFrameShearForces(mCurrentSection.AnalysisModel.Combinations[5]);

                    break;

                case ViewMode.Moments:
                    DrawAnalysisModel(Color.Gray, false, false);
                    if (OutputMode == OutputMode.SelfWeight)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.AnalysisCases[0]);
                    else if (OutputMode == OutputMode.EarthFill)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.AnalysisCases[1]);
                    else if (OutputMode == OutputMode.SoilPressure)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.AnalysisCases[2]);
                    else if (OutputMode == OutputMode.Surcharge)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.AnalysisCases[3]);
                    else if (OutputMode == OutputMode.Racking)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.AnalysisCases[4]);
                    else if (OutputMode == OutputMode.ServiceCombination)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.Combinations[0]);
                    else if (OutputMode == OutputMode.FactoredCombination1)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.Combinations[1]);
                    else if (OutputMode == OutputMode.FactoredCombination2)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.Combinations[2]);
                    else if (OutputMode == OutputMode.SeismicCombination)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.Combinations[4]);
                    else if (OutputMode == OutputMode.Envelope)
                        DrawFrameMoments(mCurrentSection.AnalysisModel.Combinations[5]);

                    break;

                case ViewMode.Deflections:
                    DrawAnalysisModel(Color.Gray, false, false);
                    if (OutputMode == OutputMode.SelfWeight)
                        DrawDeformations(mCurrentSection.AnalysisModel.AnalysisCases[0]);
                    else if (OutputMode == OutputMode.EarthFill)
                        DrawDeformations(mCurrentSection.AnalysisModel.AnalysisCases[1]);
                    else if (OutputMode == OutputMode.SoilPressure)
                        DrawDeformations(mCurrentSection.AnalysisModel.AnalysisCases[2]);
                    else if (OutputMode == OutputMode.Surcharge)
                        DrawDeformations(mCurrentSection.AnalysisModel.AnalysisCases[3]);
                    else if (OutputMode == OutputMode.Racking)
                        DrawDeformations(mCurrentSection.AnalysisModel.AnalysisCases[4]);
                    else if (OutputMode == OutputMode.ServiceCombination)
                        DrawDeformations(mCurrentSection.AnalysisModel.Combinations[0]);
                    else if (OutputMode == OutputMode.FactoredCombination1)
                        DrawDeformations(mCurrentSection.AnalysisModel.Combinations[1]);
                    else if (OutputMode == OutputMode.FactoredCombination2)
                        DrawDeformations(mCurrentSection.AnalysisModel.Combinations[2]);
                    else if (OutputMode == OutputMode.SeismicCombination)
                        DrawDeformations(mCurrentSection.AnalysisModel.Combinations[4]);
                    else if (OutputMode == OutputMode.Envelope)
                        DrawDeformations(mCurrentSection.AnalysisModel.Combinations[5]);

                    break;
            }

            ZoomToExtents();
            Refresh();
        }

        private void DrawFormwork(bool shading)
        {
            // Dimensions
            float wi = mCurrentSection.SectionProperties.InnerWidth;
            float hi = mCurrentSection.SectionProperties.InnerHeight;
            float wo = mCurrentSection.SectionProperties.OuterWidth;
            float ho = mCurrentSection.SectionProperties.OuterHeight;
            float tw = mCurrentSection.SectionProperties.OuterWallThickness;
            float tf = mCurrentSection.SectionProperties.FoundationThickness;
            float ts = mCurrentSection.SectionProperties.SlabThickness;
            int iw = mCurrentSection.SectionProperties.InnerWalls;
            float twi = mCurrentSection.SectionProperties.InnerWallThickness;
            bool gusset = mCurrentSection.SectionProperties.HasSlabGussets;
            float gw = mCurrentSection.SectionProperties.SlabGussetWidth;
            float gh = mCurrentSection.SectionProperties.SlabGussetHeight;

            // Outer formwork
            PointF[] outerPoints = new PointF[4];
            outerPoints[0] = new PointF(-wo / 2, 0);
            outerPoints[1] = new PointF(wo / 2, 0);
            outerPoints[2] = new PointF(wo / 2, ho);
            outerPoints[3] = new PointF(-wo / 2, ho);

            SimpleCAD.Polygon outer = new SimpleCAD.Polygon(outerPoints);
            outer.OutlineStyle = new SimpleCAD.OutlineStyle(FormWorkColor, LineThickness);
            if (shading) outer.FillStyle = new SimpleCAD.FillStyle(ShadingColor);
            Model.Add(outer);

            // Inner formwork
            PointF[] innerTyp = mCurrentSection.SectionProperties.HasSlabGussets ? new PointF[6] : new PointF[4];
            innerTyp[0] = new PointF(-wi / 2, tf);
            innerTyp[1] = new PointF(wi / 2, tf);
            if (gusset)
            {
                innerTyp[2] = new PointF(wi / 2, tf + hi - gh);
                innerTyp[3] = new PointF(wi / 2 - gw, tf + hi);
                innerTyp[4] = new PointF(-wi / 2 + gw, tf + hi);
                innerTyp[5] = new PointF(-wi / 2, tf + hi - gh);
            }
            else
            {
                innerTyp[2] = new PointF(wi / 2, tf + hi);
                innerTyp[3] = new PointF(-wi / 2, tf + hi);
            }

            float dx = -iw * (wi + twi) / 2;
            for (int i = 0; i < iw + 1; i++)
            {
                PointF[] innersPoints = TranslatePointArray(innerTyp, dx, 0);

                SimpleCAD.Polygon inner = new SimpleCAD.Polygon(innersPoints);
                inner.OutlineStyle = new SimpleCAD.OutlineStyle(FormWorkColor, LineThickness);
                if (shading) inner.FillStyle = new SimpleCAD.FillStyle(BackColor);
                Model.Add(inner);

                dx += wi + twi;
            }
        }

        private void DrawFormwork()
        {
            DrawFormwork(true);
        }

        private PointF[] TranslatePointArray(PointF[] points, float dx, float dy)
        {
            PointF[] tr = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                tr[i] = new PointF(points[i].X + dx, points[i].Y + dy);
            }
            return tr;
        }

        private void DrawCulvertFrame()
        {
            float wo = mCurrentSection.SectionProperties.OuterWidth - mCurrentSection.SectionProperties.OuterWallThickness;
            float ho = mCurrentSection.SectionProperties.OuterHeight - mCurrentSection.SectionProperties.SlabThickness / 2 - mCurrentSection.SectionProperties.FoundationThickness / 2;
            float x = -wo / 2;
            float iw = mCurrentSection.SectionProperties.InnerWalls;
            float dx = wo / (iw + 1);

            SimpleCAD.Rectangle outer = new SimpleCAD.Rectangle(-wo / 2, 0, wo / 2, ho);
            outer.OutlineStyle = new SimpleCAD.OutlineStyle(FormWorkColor);
            Model.Add(outer);

            for (int i = 0; i < iw; i++)
            {
                x += dx;
                SimpleCAD.Line inner = new SimpleCAD.Line(x, 0, x, ho);
                inner.OutlineStyle = new SimpleCAD.OutlineStyle(FormWorkColor);
                Model.Add(inner);
            }
        }

        private void DrawAnalysisModel(Color color, bool showNodes, bool showSprings)
        {
            // Draw frames
            foreach (Frame f in mCurrentSection.AnalysisModel.Frames)
            {
                SimpleCAD.Line line = new SimpleCAD.Line((float)f.NodeI.X, (float)f.NodeI.Z, (float)f.NodeJ.X, (float)f.NodeJ.Z);
                line.OutlineStyle = new SimpleCAD.OutlineStyle(color, 0, DashStyle.Dash);
                Model.Add(line);
            }

            // Draw nodes
            if (showNodes)
            {
                float nodeSize = 0.5f * TextSize;
                foreach (Node n in mCurrentSection.AnalysisModel.Nodes)
                {
                    SimpleCAD.Ellipse ellipse = new SimpleCAD.Ellipse((float)n.X - nodeSize / 2, (float)n.Z - nodeSize / 2, (float)n.X + nodeSize / 2, (float)n.Z + nodeSize / 2);
                    ellipse.OutlineStyle = new SimpleCAD.OutlineStyle(color, 0, DashStyle.Solid);
                    if (n.Restraints != DOF.Free)
                    {
                        ellipse.FillStyle = new SimpleCAD.FillStyle(color);
                    }
                    Model.Add(ellipse);
                }
            }

            // Draw springs
            if (showSprings)
            {
                foreach (Spring s in mCurrentSection.AnalysisModel.Springs)
                {
                    SimpleCAD.Line line = new SimpleCAD.Line((float)s.NodeI.X, (float)s.NodeI.Z, (float)s.NodeJ.X, (float)s.NodeJ.Z);
                    line.OutlineStyle = new SimpleCAD.OutlineStyle(color, 0);
                    Model.Add(line);
                }
            }
        }

        private void DrawNodeLoads(AnalysisCase analysisCase)
        {
            float textOffset = 0.5f * TextSize;
            foreach (Node n in mCurrentSection.AnalysisModel.Nodes)
            {
                foreach (NodeLoad load in n.Loads.FindAll((e) => e.AnalysisCase == analysisCase))
                {
                    if (load is NodePointLoad)
                    {
                        NodePointLoad pointLoad = load as NodePointLoad;
                        string loadText = pointLoad.FX.ToString("0") + ", " + pointLoad.FZ.ToString("0") + ", " + pointLoad.MY.ToString("0");
                        SimpleCAD.Text text = new SimpleCAD.Text((float)n.X + textOffset / 2, (float)n.Z + textOffset / 2, loadText, TextSize);
                        text.FontFamily = Font.Name;
                        text.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                        Model.Add(text);
                    }
                }
            }
        }

        private void DrawFrameLoads(AnalysisCase analysisCase)
        {
            float maxLoadSize = 10 * DimensionOffset;
            float textOffset = 0.5f * TextSize;
            float maxLoad = 0;
            foreach (Frame f in mCurrentSection.AnalysisModel.Frames)
            {
                foreach (FrameLoad load in f.Loads.FindAll((e) => e.AnalysisCase == analysisCase))
                {
                    if (load is FrameUniformLoad)
                    {
                        FrameUniformLoad uniformLoad = load as FrameUniformLoad;
                        maxLoad = (float)Math.Max(maxLoad, Math.Max(Math.Abs(uniformLoad.FX), Math.Abs(uniformLoad.FZ)));
                    }
                    if (load is FrameTrapezoidalLoad)
                    {
                        FrameTrapezoidalLoad trapezoidalLoad = load as FrameTrapezoidalLoad;
                        maxLoad = (float)Math.Max(maxLoad, Math.Max(Math.Abs(trapezoidalLoad.FXI), Math.Abs(trapezoidalLoad.FZI)));
                        maxLoad = (float)Math.Max(maxLoad, Math.Max(Math.Abs(trapezoidalLoad.FXJ), Math.Abs(trapezoidalLoad.FZJ)));
                    }
                }
            }
            foreach (Frame f in mCurrentSection.AnalysisModel.Frames)
            {
                float x1 = (float)f.NodeI.X;
                float y1 = (float)f.NodeI.Z;
                float x2 = (float)f.NodeJ.X;
                float y2 = (float)f.NodeJ.Z;
                float angle = (float)f.Angle * 180 / (float)Math.PI + 90;
                foreach (FrameLoad load in f.Loads.FindAll((e) => e.AnalysisCase == analysisCase))
                {
                    if (load is FrameUniformLoad)
                    {
                        FrameUniformLoad uniformLoad = load as FrameUniformLoad;
                        float loadx1 = x1 - (float)uniformLoad.FX * maxLoadSize / maxLoad;
                        float loady1 = y1 - (float)uniformLoad.FZ * maxLoadSize / maxLoad;
                        float loadx2 = x2 - (float)uniformLoad.FX * maxLoadSize / maxLoad;
                        float loady2 = y2 - (float)uniformLoad.FZ * maxLoadSize / maxLoad;
                        SimpleCAD.Polygon poly = new SimpleCAD.Polygon(new PointF[] { new PointF(x1, y1), new PointF(x2, y2), new PointF(loadx2, loady2), new PointF(loadx1, loady1) });
                        poly.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                        poly.FillStyle = new SimpleCAD.FillStyle(ShadingColor);
                        Model.Add(poly);

                        SimpleCAD.Text text = new SimpleCAD.Text(loadx1, loady1, uniformLoad.FX.ToString("0.0") + ", " + uniformLoad.FZ.ToString("0.0"), TextSize);
                        text.HorizontalAlignment = StringAlignment.Near;
                        text.VerticalAlignment = StringAlignment.Center;
                        text.Rotation = angle;
                        text.FontFamily = Font.Name;
                        text.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                        Model.Add(text);
                    }
                    if (load is FrameTrapezoidalLoad)
                    {
                        FrameTrapezoidalLoad trapezoidalLoad = load as FrameTrapezoidalLoad;
                        float loadx1 = x1 - (float)trapezoidalLoad.FXI * maxLoadSize / maxLoad;
                        float loady1 = y1 - (float)trapezoidalLoad.FZI * maxLoadSize / maxLoad;
                        float loadx2 = x2 - (float)trapezoidalLoad.FXJ * maxLoadSize / maxLoad;
                        float loady2 = y2 - (float)trapezoidalLoad.FZJ * maxLoadSize / maxLoad;
                        SimpleCAD.Polygon poly = new SimpleCAD.Polygon(new PointF[] { new PointF(x1, y1), new PointF(x2, y2), new PointF(loadx2, loady2), new PointF(loadx1, loady1) });
                        poly.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                        poly.FillStyle = new SimpleCAD.FillStyle(ShadingColor);
                        Model.Add(poly);

                        SimpleCAD.Text textI = new SimpleCAD.Text(loadx1, loady1, trapezoidalLoad.FXI.ToString("0.0") + ", " + trapezoidalLoad.FZI.ToString("0.0"), TextSize);
                        textI.HorizontalAlignment = StringAlignment.Near;
                        textI.VerticalAlignment = StringAlignment.Center;
                        textI.Rotation = angle;
                        textI.FontFamily = Font.Name;
                        textI.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                        Model.Add(textI);

                        SimpleCAD.Text textJ = new SimpleCAD.Text(loadx2, loady2, trapezoidalLoad.FXJ.ToString("0.0") + ", " + trapezoidalLoad.FZJ.ToString("0.0"), TextSize);
                        textJ.HorizontalAlignment = StringAlignment.Near;
                        textJ.VerticalAlignment = StringAlignment.Center;
                        textJ.Rotation = angle;
                        textJ.FontFamily = Font.Name;
                        textJ.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                        Model.Add(textJ);
                    }
                }
            }
        }

        private void DrawReactions(CombinableList<Reaction> reactions)
        {
            float textOffset = 0.5f * TextSize;
            foreach (Reaction r in reactions)
            {
                string reactionText = r.FX.ToString("0") + ", " + r.FZ.ToString("0") + ", " + r.MY.ToString("0");
                SimpleCAD.Text text = new SimpleCAD.Text((float)r.Node.X + textOffset / 2, (float)r.Node.Z + textOffset / 2, reactionText, TextSize);
                text.FontFamily = Font.Name;
                text.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                Model.Add(text);
            }
        }

        private void DrawReactions(CombinableList<Reaction> minReactions, CombinableList<Reaction> maxReactions)
        {
            float textOffset = 0.5f * TextSize;
            for (int i = 0; i < minReactions.Count; i++)
            {
                Reaction rmin = minReactions[i];
                Reaction rmax = maxReactions[i];
                string[] reactionText = new string[]{ rmin.FX.ToString("0") + ", " + rmin.FZ.ToString("0") + ", " + rmin.MY.ToString("0"),
                     rmax.FX.ToString("0") + ", " + rmax.FZ.ToString("0") + ", " + rmax.MY.ToString("0")};
                SimpleCAD.MultiLineText text = new SimpleCAD.MultiLineText((float)rmin.Node.X + textOffset / 2, (float)rmin.Node.Z + textOffset / 2, reactionText, TextSize);
                text.FontFamily = Font.Name;
                text.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                Model.Add(text);
            }
        }

        private void DrawReactions(AnalysisCase analysisCase)
        {
            DrawReactions(analysisCase.Reactions);
        }

        private void DrawReactions(Combination combination)
        {
            if (!combination.HasEnvelopeValues)
            {
                DrawReactions(combination.MinReactions);
            }
            else if (EnvelopeMode == EnvelopeMode.Minimum)
            {
                DrawReactions(combination.MinReactions);
            }
            else if (EnvelopeMode == EnvelopeMode.Maximum)
            {
                DrawReactions(combination.MaxReactions);
            }
            else
            {
                DrawReactions(combination.MinReactions, combination.MaxReactions);
            }
        }

        private void DrawFrameAxialForces(CombinableList<FrameInternalForce> internalForces)
        {
            float maxForceSize = 4 * DimensionOffset;
            float maxForce = 0;
            foreach (FrameInternalForce force in internalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.NI), Math.Abs(force.NJ)));
            }
            foreach (FrameInternalForce force in internalForces)
            {
                DrawInternalForce(force.Frame, (float)force.NI, (float)force.NJ, maxForceSize / maxForce);
            }
        }

        private void DrawFrameAxialForces(CombinableList<FrameInternalForce> minInternalForces, CombinableList<FrameInternalForce> maxInternalForces)
        {
            float maxForceSize = 4 * DimensionOffset;
            float maxForce = 0;
            foreach (FrameInternalForce force in minInternalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.NI), Math.Abs(force.NJ)));
            }
            foreach (FrameInternalForce force in maxInternalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.NI), Math.Abs(force.NJ)));
            }
            for (int i = 0; i < minInternalForces.Count; i++)
            {
                FrameInternalForce minF = minInternalForces[i];
                FrameInternalForce maxF = maxInternalForces[i];
                DrawInternalForce(minF.Frame, (float)minF.NI, (float)minF.NJ, (float)maxF.NI, (float)maxF.NJ, maxForceSize / maxForce);
            }
        }

        private void DrawFrameAxialForces(AnalysisCase analysisCase)
        {
            DrawFrameAxialForces(analysisCase.FrameInternalForces);
        }

        private void DrawFrameAxialForces(Combination combination)
        {
            if (!combination.HasEnvelopeValues)
            {
                DrawFrameAxialForces(combination.MinFrameInternalForces);
            }
            else if (EnvelopeMode == EnvelopeMode.Minimum)
            {
                DrawFrameAxialForces(combination.MinFrameInternalForces);
            }
            else if (EnvelopeMode == EnvelopeMode.Maximum)
            {
                DrawFrameAxialForces(combination.MaxFrameInternalForces);
            }
            else
            {
                DrawFrameAxialForces(combination.MinFrameInternalForces, combination.MaxFrameInternalForces);
            }
        }

        private void DrawFrameShearForces(CombinableList<FrameInternalForce> internalForces)
        {
            float maxForceSize = 4 * DimensionOffset;
            float maxForce = 0;
            foreach (FrameInternalForce force in internalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.VI), Math.Abs(force.VJ)));
            }
            foreach (FrameInternalForce force in internalForces)
            {
                DrawInternalForce(force.Frame, (float)force.VI, (float)force.VJ, maxForceSize / maxForce);
            }
        }

        private void DrawFrameShearForces(CombinableList<FrameInternalForce> minInternalForces, CombinableList<FrameInternalForce> maxInternalForces)
        {
            float maxForceSize = 4 * DimensionOffset;
            float maxForce = 0;
            foreach (FrameInternalForce force in minInternalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.VI), Math.Abs(force.VJ)));
            }
            foreach (FrameInternalForce force in maxInternalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.VI), Math.Abs(force.VJ)));
            }
            for (int i = 0; i < minInternalForces.Count; i++)
            {
                FrameInternalForce minF = minInternalForces[i];
                FrameInternalForce maxF = maxInternalForces[i];
                DrawInternalForce(minF.Frame, (float)minF.VI, (float)minF.VJ, (float)maxF.VI, (float)maxF.VJ, maxForceSize / maxForce);
            }
        }

        private void DrawFrameShearForces(AnalysisCase analysisCase)
        {
            DrawFrameShearForces(analysisCase.FrameInternalForces);
        }

        private void DrawFrameShearForces(Combination combination)
        {
            if (!combination.HasEnvelopeValues)
            {
                DrawFrameShearForces(combination.MinFrameInternalForces);
            }
            else if (EnvelopeMode == EnvelopeMode.Minimum)
            {
                DrawFrameShearForces(combination.MinFrameInternalForces);
            }
            else if (EnvelopeMode == EnvelopeMode.Maximum)
            {
                DrawFrameShearForces(combination.MaxFrameInternalForces);
            }
            else
            {
                DrawFrameShearForces(combination.MinFrameInternalForces, combination.MaxFrameInternalForces);
            }
        }

        private void DrawFrameMoments(CombinableList<FrameInternalForce> internalForces)
        {
            float maxForceSize = 4 * DimensionOffset;
            float maxForce = 0;
            foreach (FrameInternalForce force in internalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.MI), Math.Abs(force.MJ)));
            }
            foreach (FrameInternalForce force in internalForces)
            {
                DrawInternalForce(force.Frame, (float)force.MI, (float)force.MJ, -maxForceSize / maxForce);
            }
        }

        private void DrawFrameMoments(CombinableList<FrameInternalForce> minInternalForces, CombinableList<FrameInternalForce> maxInternalForces)
        {
            float maxForceSize = 4 * DimensionOffset;
            float maxForce = 0;
            foreach (FrameInternalForce force in minInternalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.MI), Math.Abs(force.MJ)));
            }
            foreach (FrameInternalForce force in maxInternalForces)
            {
                maxForce = (float)Math.Max(maxForce, Math.Max(Math.Abs(force.MI), Math.Abs(force.MJ)));
            }
            for (int i = 0; i < minInternalForces.Count; i++)
            {
                FrameInternalForce minF = minInternalForces[i];
                FrameInternalForce maxF = maxInternalForces[i];
                DrawInternalForce(minF.Frame, (float)minF.MI, (float)minF.MJ, (float)maxF.MI, (float)maxF.MJ, -maxForceSize / maxForce);
            }
        }

        private void DrawFrameMoments(AnalysisCase analysisCase)
        {
            DrawFrameMoments(analysisCase.FrameInternalForces);
        }

        private void DrawFrameMoments(Combination combination)
        {
            if (!combination.HasEnvelopeValues)
            {
                DrawFrameMoments(combination.MinFrameInternalForces);
            }
            else if (EnvelopeMode == EnvelopeMode.Minimum)
            {
                DrawFrameMoments(combination.MinFrameInternalForces);
            }
            else if (EnvelopeMode == EnvelopeMode.Maximum)
            {
                DrawFrameMoments(combination.MaxFrameInternalForces);
            }
            else
            {
                DrawFrameMoments(combination.MinFrameInternalForces, combination.MaxFrameInternalForces);
            }
        }

        private void DrawInternalForce(Frame f, float forceI, float forceJ, float scale)
        {
            float x1 = (float)f.NodeI.X;
            float y1 = (float)f.NodeI.Z;
            float x2 = (float)f.NodeJ.X;
            float y2 = (float)f.NodeJ.Z;
            float orientation = (float)f.Angle;
            float loadOrientation = orientation + (float)Math.PI / 2;
            float textAngle = orientation * 180 / (float)Math.PI + 90;

            int fillAlpha = (int)((1 - mFillTransparency) * 255);
            Color negativeColor = Color.FromArgb(fillAlpha, NegativeForceColor);
            Color positiveColor = Color.FromArgb(fillAlpha, PositiveForceColor);

            float loadx1 = x1 + (float)(forceI * scale * Math.Cos(loadOrientation));
            float loady1 = y1 + (float)(forceI * scale * Math.Sin(loadOrientation));
            float loadx2 = x2 + (float)(forceJ * scale * Math.Cos(loadOrientation));
            float loady2 = y2 + (float)(forceJ * scale * Math.Sin(loadOrientation));

            if ((Math.Abs(forceI - forceJ) < float.Epsilon) || (Math.Sign(forceI) * Math.Sign(forceJ) > 0))
            {
                SimpleCAD.Polygon poly = new SimpleCAD.Polygon(new PointF[] { new PointF(x1, y1), new PointF(x2, y2), new PointF(loadx2, loady2), new PointF(loadx1, loady1) });
                poly.OutlineStyle = new SimpleCAD.OutlineStyle(Color.Transparent);
                poly.FillStyle = new SimpleCAD.FillStyle(forceI < 0 ? negativeColor : positiveColor);
                Model.Add(poly);
            }
            else
            {
                float xmid = Math.Abs(forceI) / Math.Abs(forceI - forceJ) * (x2 - x1) + x1;
                float ymid = Math.Abs(forceI) / Math.Abs(forceI - forceJ) * (y2 - y1) + y1;

                SimpleCAD.Polygon polyI = new SimpleCAD.Polygon(new PointF[] { new PointF(x1, y1), new PointF(xmid, ymid), new PointF(loadx1, loady1) });
                polyI.OutlineStyle = new SimpleCAD.OutlineStyle(Color.Transparent);
                polyI.FillStyle = new SimpleCAD.FillStyle(forceI < 0 ? negativeColor : positiveColor);
                Model.Add(polyI);

                SimpleCAD.Polygon polyJ = new SimpleCAD.Polygon(new PointF[] { new PointF(xmid, ymid), new PointF(x2, y2), new PointF(loadx2, loady2) });
                polyJ.OutlineStyle = new SimpleCAD.OutlineStyle(Color.Transparent);
                polyJ.FillStyle = new SimpleCAD.FillStyle(forceJ < 0 ? negativeColor : positiveColor);
                Model.Add(polyJ);
            }

            SimpleCAD.Line line1 = new SimpleCAD.Line(x1, y1, loadx1, loady1);
            line1.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line1);
            SimpleCAD.Line line2 = new SimpleCAD.Line(x2, y2, loadx2, loady2);
            line2.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line2);
            SimpleCAD.Line line3 = new SimpleCAD.Line(loadx1, loady1, loadx2, loady2);
            line3.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line3);
            SimpleCAD.Line line4 = new SimpleCAD.Line(x1, y1, x2, y2);
            line4.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line4);

            SimpleCAD.Text textI = new SimpleCAD.Text(loadx1, loady1, forceI.ToString("0.0"), TextSize);
            textI.HorizontalAlignment = StringAlignment.Near;
            textI.VerticalAlignment = StringAlignment.Center;
            textI.Rotation = textAngle;
            textI.FontFamily = Font.Name;
            textI.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(textI);

            SimpleCAD.Text textJ = new SimpleCAD.Text(loadx2, loady2, forceJ.ToString("0.0"), TextSize);
            textJ.HorizontalAlignment = StringAlignment.Near;
            textJ.VerticalAlignment = StringAlignment.Center;
            textJ.Rotation = textAngle;
            textJ.FontFamily = Font.Name;
            textJ.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(textJ);
        }

        private void DrawInternalForce(Frame f, float minForceI, float minForceJ, float maxForceI, float maxForceJ, float scale)
        {
            float x1 = (float)f.NodeI.X;
            float y1 = (float)f.NodeI.Z;
            float x2 = (float)f.NodeJ.X;
            float y2 = (float)f.NodeJ.Z;
            float orientation = (float)f.Angle;
            float loadOrientation = orientation + (float)Math.PI / 2;
            float textAngle = orientation * 180 / (float)Math.PI + 90;
            int fillAlpha = (int)((1 - mFillTransparency) * 255);
            Color negativeColor = Color.FromArgb(fillAlpha, NegativeForceColor);
            Color positiveColor = Color.FromArgb(fillAlpha, PositiveForceColor);
            Color envelopeColor = Color.FromArgb(fillAlpha, EnvelopeForceColor);

            float minLoadx1 = x1 + (float)(minForceI * scale * Math.Cos(loadOrientation));
            float minLoady1 = y1 + (float)(minForceI * scale * Math.Sin(loadOrientation));
            float minLoadx2 = x2 + (float)(minForceJ * scale * Math.Cos(loadOrientation));
            float minLoady2 = y2 + (float)(minForceJ * scale * Math.Sin(loadOrientation));
            float maxLoadx1 = x1 + (float)(maxForceI * scale * Math.Cos(loadOrientation));
            float maxLoady1 = y1 + (float)(maxForceI * scale * Math.Sin(loadOrientation));
            float maxLoadx2 = x2 + (float)(maxForceJ * scale * Math.Cos(loadOrientation));
            float maxLoady2 = y2 + (float)(maxForceJ * scale * Math.Sin(loadOrientation));

            SimpleCAD.Polygon range = new SimpleCAD.Polygon(new PointF[] { new PointF(minLoadx1, minLoady1), new PointF(minLoadx2, minLoady2), new PointF(maxLoadx2, maxLoady2), new PointF(maxLoadx1, maxLoady1) });
            range.OutlineStyle = new SimpleCAD.OutlineStyle(Color.Transparent);
            range.FillStyle = new SimpleCAD.FillStyle(envelopeColor);
            Model.Add(range);

            if (minForceI < 0 && maxForceI < 0 && minForceI < 0 && minForceJ < 0)
            {
                SimpleCAD.Polygon poly = new SimpleCAD.Polygon(new PointF[] { new PointF(maxLoadx1, maxLoady1), new PointF(maxLoadx2, maxLoady2), new PointF(x2, y2), new PointF(x1, y1) });
                poly.OutlineStyle = new SimpleCAD.OutlineStyle(Color.Transparent);
                poly.FillStyle = new SimpleCAD.FillStyle(negativeColor);
                Model.Add(poly);
            }
            else if (minForceI > 0 && maxForceI > 0 && minForceI > 0 && minForceJ > 0)
            {
                SimpleCAD.Polygon poly = new SimpleCAD.Polygon(new PointF[] { new PointF(minLoadx1, minLoady1), new PointF(minLoadx2, minLoady2), new PointF(x2, y2), new PointF(x1, y1) });
                poly.OutlineStyle = new SimpleCAD.OutlineStyle(Color.Transparent);
                poly.FillStyle = new SimpleCAD.FillStyle(positiveColor);
                Model.Add(poly);
            }

            SimpleCAD.Line line1 = new SimpleCAD.Line(minLoadx1, minLoady1, minLoadx2, minLoady2);
            line1.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line1);
            SimpleCAD.Line line2 = new SimpleCAD.Line(maxLoadx1, maxLoady1, maxLoadx2, maxLoady2);
            line2.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line2);
            SimpleCAD.Line line3 = new SimpleCAD.Line(x1, y1, x2, y2);
            line3.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line3);
            SimpleCAD.Line line4 = new SimpleCAD.Line(minLoadx1, minLoady1, x1, y1);
            line4.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line4);
            SimpleCAD.Line line5 = new SimpleCAD.Line(minLoadx2, minLoady2, x2, y2);
            line5.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line5);
            SimpleCAD.Line line6 = new SimpleCAD.Line(maxLoadx1, maxLoady1, x1, y1);
            line6.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line6);
            SimpleCAD.Line line7 = new SimpleCAD.Line(maxLoadx2, maxLoady2, x2, y2);
            line7.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(line7);
        }

        private void DrawDeformations(CombinableList<NodeDeformation> defs)
        {
            double maxDef = 0;
            foreach (NodeDeformation def in defs)
            {
                maxDef = Math.Max(maxDef, Math.Abs(def.UX));
                maxDef = Math.Max(maxDef, Math.Abs(def.UZ));
            }
            double deformationScale = mCurrentSection.SectionProperties.OuterHeight / 10 / maxDef;

            // Draw frames
            foreach (Frame f in mCurrentSection.AnalysisModel.Frames)
            {
                float x1 = (float)(f.NodeI.X + defs[f.NodeI.Index].UX * deformationScale);
                float y1 = (float)(f.NodeI.Z + defs[f.NodeI.Index].UZ * deformationScale);
                float x2 = (float)(f.NodeJ.X + defs[f.NodeJ.Index].UX * deformationScale);
                float y2 = (float)(f.NodeJ.Z + defs[f.NodeJ.Index].UZ * deformationScale);

                SimpleCAD.Line line = new SimpleCAD.Line(x1, y1, x2, y2);
                line.OutlineStyle = new SimpleCAD.OutlineStyle(FormWorkColor);
                Model.Add(line);
            }
        }

        private void DrawDeformations(CombinableList<NodeDeformation> minDefs, CombinableList<NodeDeformation> maxDefs)
        {
            DrawDeformations(minDefs.AbsMax(maxDefs));
        }

        private void DrawDeformations(AnalysisCase analysisCase)
        {
            DrawDeformations(analysisCase.NodeDeformations);
        }

        private void DrawDeformations(Combination combination)
        {
            if (!combination.HasEnvelopeValues)
            {
                DrawDeformations(combination.MinNodeDeformations);
            }
            else if (EnvelopeMode == EnvelopeMode.Minimum)
            {
                DrawDeformations(combination.MinNodeDeformations);
            }
            else if (EnvelopeMode == EnvelopeMode.Maximum)
            {
                DrawDeformations(combination.MaxNodeDeformations);
            }
            else
            {
                DrawDeformations(combination.MinNodeDeformations, combination.MaxNodeDeformations);
            }
        }

        private void DrawAnalysisModel()
        {
            DrawAnalysisModel(FormWorkColor, true, true);
        }

        private void DrawNodeNumbers()
        {
            float textOffset = 0.5f * TextSize;
            foreach (Node n in mCurrentSection.AnalysisModel.Nodes)
            {
                SimpleCAD.Text text = new SimpleCAD.Text((float)n.X + textOffset / 2, (float)n.Z + textOffset / 2, (n.Index + 1).ToString(), TextSize);
                text.FontFamily = Font.Name;
                text.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                Model.Add(text);
            }
        }

        private void DrawFrameNumbers()
        {
            float textOffset = 0.5f * TextSize;
            foreach (Frame f in mCurrentSection.AnalysisModel.Frames)
            {
                float angle = (float)f.Angle * 180 / (float)Math.PI;
                float x = (float)f.Centroid.X - textOffset * 1.5f * (float)Math.Sin(angle * Math.PI / 180.0);
                float y = (float)f.Centroid.Z + textOffset * 1.5f * (float)Math.Cos(angle * Math.PI / 180.0);

                SimpleCAD.Text text = new SimpleCAD.Text(x, y, (f.Index + 1).ToString(), TextSize);
                text.HorizontalAlignment = StringAlignment.Center;
                text.VerticalAlignment = StringAlignment.Center;
                text.Rotation = angle;
                text.FontFamily = Font.Name;
                text.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
                Model.Add(text);
            }
        }

        private void DrawDimensions()
        {
            // Dimensions
            float wi = mCurrentSection.SectionProperties.InnerWidth;
            float hi = mCurrentSection.SectionProperties.InnerHeight;
            float wo = mCurrentSection.SectionProperties.OuterWidth;
            float ho = mCurrentSection.SectionProperties.OuterHeight;
            float tw = mCurrentSection.SectionProperties.OuterWallThickness;
            float tf = mCurrentSection.SectionProperties.FoundationThickness;
            float ts = mCurrentSection.SectionProperties.SlabThickness;
            int iw = mCurrentSection.SectionProperties.InnerWalls;
            float twi = mCurrentSection.SectionProperties.InnerWallThickness;
            bool gusset = mCurrentSection.SectionProperties.HasSlabGussets;
            float gw = mCurrentSection.SectionProperties.SlabGussetWidth;
            float gh = mCurrentSection.SectionProperties.SlabGussetHeight;

            // Width
            DrawDimension(-wo / 2, ho, wo / 2, ho, DimensionOffset);
            DrawDimension(-wo / 2, 0, -wo / 2 + tw, 0, -DimensionOffset);
            float dx = -wo / 2 + tw;
            for (int i = 0; i < iw + 1; i++)
            {
                DrawDimension(dx, 0, dx + wi, 0, -DimensionOffset);
                if (i < iw) DrawDimension(dx + wi, 0, dx + wi + twi, 0, -DimensionOffset);
                dx += wi + twi;
            }
            DrawDimension(wo / 2 - tw, 0, wo / 2, 0, -DimensionOffset);
            // Height
            DrawDimension(-wo / 2, 0, -wo / 2, ho, DimensionOffset);
            DrawDimension(wo / 2, 0, wo / 2, tf, -DimensionOffset);
            DrawDimension(wo / 2, tf, wo / 2, tf + hi, -DimensionOffset);
            DrawDimension(wo / 2, tf + hi, wo / 2, ho, -DimensionOffset);
            if (gusset)
            {
                float dgx = -wo / 2 + tw;
                for (int i = 0; i < iw + 1; i++)
                {
                    // Width
                    DrawDimension(dgx, tf + hi - gh, dgx + gw, tf + hi - gh, -DimensionOffset);
                    DrawDimension(dgx + wi - gw, tf + hi - gh, dgx + wi, tf + hi - gh, -DimensionOffset);
                    // Height
                    DrawDimension(dgx + gw, tf + hi - gh, dgx + gw, tf + hi, -DimensionOffset);
                    DrawDimension(dgx + wi - gw, tf + hi - gh, dgx + wi - gw, tf + hi, DimensionOffset);

                    dgx += wi + twi;
                }
            }
        }

        private void DrawDimension(float x1, float y1, float x2, float y2, float offset)
        {
            float len = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            SimpleCAD.Dimension dim = new SimpleCAD.Dimension(x1, y1, x2, y2, len.ToString("0.00"), TextSize);
            dim.Offset = offset;
            dim.FontFamily = Font.Name;
            dim.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor, DimensionLineThickness);
            dim.FillStyle = new SimpleCAD.FillStyle(BackColor);
            Model.Add(dim);
        }

        private void DrawEarthFillLoad()
        {
            float loadOffset = DimensionOffset;

            // Load scale
            float maxLoadSize = 2 * loadOffset;
            float maxLoad = Math.Max(mCurrentSection.SoilParameters.FillLoad, mCurrentSection.SoilParameters.SoilPressureBottom);

            // Dimensions
            float wi = mCurrentSection.SectionProperties.InnerWidth;
            float hi = mCurrentSection.SectionProperties.InnerHeight;
            float tw = mCurrentSection.SectionProperties.OuterWallThickness;
            float tf = mCurrentSection.SectionProperties.FoundationThickness;
            float ts = mCurrentSection.SectionProperties.SlabThickness;
            float w = mCurrentSection.SectionProperties.OuterWidth - tw;
            float h = hi + tf / 2 + ts / 2;

            // Earth fill
            float earthFillScale = mCurrentSection.SoilParameters.FillLoad / maxLoad * maxLoadSize;
            PointF[] earthFill = new PointF[4];
            earthFill[0] = new PointF(-w / 2, h + loadOffset);
            earthFill[1] = new PointF(w / 2, h + loadOffset);
            earthFill[2] = new PointF(w / 2, h + loadOffset + earthFillScale);
            earthFill[3] = new PointF(-w / 2, h + loadOffset + earthFillScale);

            SimpleCAD.Polygon load = new SimpleCAD.Polygon(earthFill);
            load.OutlineStyle = new SimpleCAD.OutlineStyle(LoadColor);
            load.FillStyle = new SimpleCAD.FillStyle(LoadColor, ShadingColor, HatchStyle.LightVertical);
            Model.Add(load);

            float x = -w / 2;
            float y = h + loadOffset + earthFillScale + loadOffset / 2;
            string text = mCurrentSection.SoilParameters.FillLoad.ToString("EV = 0.00 kPa");
            SimpleCAD.Text loadText = new SimpleCAD.Text(x, y, text, TextSize);
            loadText.HorizontalAlignment = StringAlignment.Near;
            loadText.VerticalAlignment = StringAlignment.Center;
            loadText.FontFamily = Font.Name;
            loadText.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(loadText);
        }

        private void DrawSoilPressureLoad()
        {
            float loadOffset = DimensionOffset;

            // Load scale
            float maxLoadSize = 2 * loadOffset;
            float maxLoad = Math.Max(mCurrentSection.SoilParameters.FillLoad, mCurrentSection.SoilParameters.SoilPressureBottom);

            // Dimensions
            float wi = mCurrentSection.SectionProperties.InnerWidth;
            float hi = mCurrentSection.SectionProperties.InnerHeight;
            float tw = mCurrentSection.SectionProperties.OuterWallThickness;
            float tf = mCurrentSection.SectionProperties.FoundationThickness;
            float ts = mCurrentSection.SectionProperties.SlabThickness;
            float w = mCurrentSection.SectionProperties.OuterWidth - tw;
            float h = hi + tf / 2 + ts / 2;

            // Earth pressure - left
            float earthPressureScale1 = mCurrentSection.SoilParameters.SoilPressureTop / maxLoad * maxLoadSize;
            float earthPressureScale2 = mCurrentSection.SoilParameters.SoilPressureBottom / maxLoad * maxLoadSize;
            PointF[] earthPressureLeft = new PointF[4];
            earthPressureLeft[0] = new PointF(-w / 2 - loadOffset - earthPressureScale2, 0);
            earthPressureLeft[1] = new PointF(-w / 2 - loadOffset, 0);
            earthPressureLeft[2] = new PointF(-w / 2 - loadOffset, h);
            earthPressureLeft[3] = new PointF(-w / 2 - loadOffset - earthPressureScale1, h);

            SimpleCAD.Polygon loadLeft = new SimpleCAD.Polygon(earthPressureLeft);
            loadLeft.OutlineStyle = new SimpleCAD.OutlineStyle(LoadColor);
            loadLeft.FillStyle = new SimpleCAD.FillStyle(LoadColor, ShadingColor, HatchStyle.LightHorizontal);
            Model.Add(loadLeft);

            SimpleCAD.Text loadTextLeftTop = new SimpleCAD.Text(-w / 2 - loadOffset, h + loadOffset / 4, mCurrentSection.SoilParameters.SoilPressureTop.ToString("EH = 0.00 kPa"), TextSize);
            loadTextLeftTop.HorizontalAlignment = StringAlignment.Far;
            loadTextLeftTop.VerticalAlignment = StringAlignment.Near;
            loadTextLeftTop.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            loadTextLeftTop.FontFamily = Font.Name;
            Model.Add(loadTextLeftTop);

            SimpleCAD.Text loadTextLeftBottom = new SimpleCAD.Text(-w / 2 - loadOffset, -loadOffset / 4, mCurrentSection.SoilParameters.SoilPressureBottom.ToString("EH = 0.00 kPa"), TextSize);
            loadTextLeftBottom.HorizontalAlignment = StringAlignment.Far;
            loadTextLeftBottom.VerticalAlignment = StringAlignment.Far;
            loadTextLeftBottom.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            loadTextLeftBottom.FontFamily = Font.Name;
            Model.Add(loadTextLeftBottom);

            // Earth pressure - right
            PointF[] earthPressureRight = new PointF[4];
            earthPressureRight[0] = new PointF(w / 2 + loadOffset + earthPressureScale2, 0);
            earthPressureRight[1] = new PointF(w / 2 + loadOffset, 0);
            earthPressureRight[2] = new PointF(w / 2 + loadOffset, h);
            earthPressureRight[3] = new PointF(w / 2 + loadOffset + earthPressureScale1, h);

            SimpleCAD.Polygon loadRight = new SimpleCAD.Polygon(earthPressureRight);
            loadRight.OutlineStyle = new SimpleCAD.OutlineStyle(LoadColor);
            loadRight.FillStyle = new SimpleCAD.FillStyle(LoadColor, ShadingColor, HatchStyle.LightHorizontal);
            Model.Add(loadRight);

            SimpleCAD.Text loadTextRightTop = new SimpleCAD.Text(w / 2 + loadOffset, h + loadOffset / 4, mCurrentSection.SoilParameters.SoilPressureTop.ToString("EH = 0.00 kPa"), TextSize);
            loadTextRightTop.HorizontalAlignment = StringAlignment.Near;
            loadTextRightTop.VerticalAlignment = StringAlignment.Near;
            loadTextRightTop.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            loadTextRightTop.FontFamily = Font.Name;
            Model.Add(loadTextRightTop);

            SimpleCAD.Text loadTextRightBottom = new SimpleCAD.Text(w / 2 + loadOffset, -loadOffset / 4, mCurrentSection.SoilParameters.SoilPressureBottom.ToString("EH = 0.00 kPa"), TextSize);
            loadTextRightBottom.HorizontalAlignment = StringAlignment.Near;
            loadTextRightBottom.VerticalAlignment = StringAlignment.Far;
            loadTextRightBottom.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            loadTextRightBottom.FontFamily = Font.Name;
            Model.Add(loadTextRightBottom);
        }

        private void DrawSurchargeLoad()
        {
            float loadOffset = DimensionOffset;

            // Load scale
            float maxLoadSize = 2 * loadOffset;

            // Dimensions
            float wi = mCurrentSection.SectionProperties.InnerWidth;
            float hi = mCurrentSection.SectionProperties.InnerHeight;
            float tw = mCurrentSection.SectionProperties.OuterWallThickness;
            float tf = mCurrentSection.SectionProperties.FoundationThickness;
            float ts = mCurrentSection.SectionProperties.SlabThickness;
            float w = mCurrentSection.SectionProperties.OuterWidth - tw;
            float h = hi + tf / 2 + ts / 2;

            // Surcharge left
            PointF[] surchargeLeft = new PointF[4];
            surchargeLeft[0] = new PointF(-w / 2 - loadOffset - maxLoadSize, 0);
            surchargeLeft[1] = new PointF(-w / 2 - loadOffset, 0);
            surchargeLeft[2] = new PointF(-w / 2 - loadOffset, h);
            surchargeLeft[3] = new PointF(-w / 2 - loadOffset - maxLoadSize, h);

            SimpleCAD.Polygon loadLeft = new SimpleCAD.Polygon(surchargeLeft);
            loadLeft.OutlineStyle = new SimpleCAD.OutlineStyle(LoadColor);
            loadLeft.FillStyle = new SimpleCAD.FillStyle(LoadColor, ShadingColor, HatchStyle.LightHorizontal);
            Model.Add(loadLeft);

            SimpleCAD.Text loadTextLeft = new SimpleCAD.Text(-w / 2 - loadOffset, h + loadOffset / 4, mCurrentSection.SoilParameters.SurchargeLoad.ToString("ES = 0.00 kPa"), TextSize);
            loadTextLeft.HorizontalAlignment = StringAlignment.Far;
            loadTextLeft.VerticalAlignment = StringAlignment.Near;
            loadTextLeft.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            loadTextLeft.FontFamily = Font.Name;
            Model.Add(loadTextLeft);

            // Surcharge right
            PointF[] surchargeRight = new PointF[4];
            surchargeRight[0] = new PointF(w / 2 + loadOffset + maxLoadSize, 0);
            surchargeRight[1] = new PointF(w / 2 + loadOffset, 0);
            surchargeRight[2] = new PointF(w / 2 + loadOffset, h);
            surchargeRight[3] = new PointF(w / 2 + loadOffset + maxLoadSize, h);

            SimpleCAD.Polygon loadRight = new SimpleCAD.Polygon(surchargeRight);
            loadRight.OutlineStyle = new SimpleCAD.OutlineStyle(LoadColor);
            loadRight.FillStyle = new SimpleCAD.FillStyle(LoadColor, ShadingColor, HatchStyle.LightHorizontal);
            Model.Add(loadRight);

            SimpleCAD.Text loadTextRight = new SimpleCAD.Text(w / 2 + loadOffset, h + loadOffset / 4, mCurrentSection.SoilParameters.SurchargeLoad.ToString("ES = 0.00 kPa"), TextSize);
            loadTextRight.HorizontalAlignment = StringAlignment.Near;
            loadTextRight.VerticalAlignment = StringAlignment.Near;
            loadTextRight.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            loadTextRight.FontFamily = Font.Name;
            Model.Add(loadTextRight);
        }

        private void DrawRackingLoad()
        {
            float wo = mCurrentSection.SectionProperties.OuterWidth - mCurrentSection.SectionProperties.OuterWallThickness;
            float ho = mCurrentSection.SectionProperties.OuterHeight - mCurrentSection.SectionProperties.SlabThickness / 2 - mCurrentSection.SectionProperties.FoundationThickness / 2;
            float dx = ho / 10;

            float ix = -wo / 2;
            float iw = mCurrentSection.SectionProperties.InnerWalls;
            float dix = wo / (iw + 1);

            float supportSize = DimensionOffset;
            // Left
            SimpleCAD.Triangle leftSupport = new SimpleCAD.Triangle(-wo / 2, 0,
                -wo / 2 - supportSize / 2, -supportSize,
                -wo / 2 + supportSize / 2, -supportSize);
            leftSupport.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(leftSupport);
            // Right
            SimpleCAD.Triangle rightSupport = new SimpleCAD.Triangle(wo / 2, 0,
                wo / 2 - supportSize / 2, -supportSize,
                wo / 2 + supportSize / 2, -supportSize);
            rightSupport.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            Model.Add(rightSupport);

            // Deformed shape
            PointF[] deformed = new PointF[4];
            deformed[0] = new PointF(-wo / 2, 0);
            deformed[1] = new PointF(wo / 2, 0);
            deformed[2] = new PointF(wo / 2 + dx, ho);
            deformed[3] = new PointF(-wo / 2 + dx, ho);
            SimpleCAD.Polygon deformedObj = new SimpleCAD.Polygon(deformed);
            deformedObj.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor, 3);
            Model.Add(deformedObj);
            for (int i = 0; i < iw; i++)
            {
                ix += dix;
                SimpleCAD.Line deformedWall = new SimpleCAD.Line(ix, 0, ix + dx, ho);
                deformedWall.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor, 3);
                Model.Add(deformedWall);
            }

            // Text
            float textOffset = DimensionOffset;
            float x = wo / 2 + dx + textOffset;
            float y = ho;

            string[] text = new string[]{ "Racking Parameters:",
                mCurrentSection.SoilParameters.PGA.ToString("PGA = 0.00 g"),
                mCurrentSection.SoilParameters.DepthToInvertLevel.ToString("z = 0.00 m"),
                mCurrentSection.SoilParameters.StressAtInvertLevel.ToString("σz = 0.00 kPa"),
                mCurrentSection.SoilParameters.StressReductionFactor.ToString("Rd = 0.00"),
                mCurrentSection.SoilParameters.MaximumShearStress.ToString("τ = 0.00 kPa"),
                mCurrentSection.SoilParameters.MaximumShearStrain.ToString("γ = 0.0000"),
                mCurrentSection.SoilParameters.FreeFieldDeformation.ToString("Δ = 0.0000 m")};

            SimpleCAD.MultiLineText rackingParamsText = new SimpleCAD.MultiLineText(x, y, text, TextSize);
            rackingParamsText.OutlineStyle = new SimpleCAD.OutlineStyle(DimensionColor);
            rackingParamsText.FontFamily = Font.Name;
            Model.Add(rackingParamsText);
        }
    }
}
