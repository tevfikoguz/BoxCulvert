using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace BoxCulvert
{
    public class SectionProperties
    {
        private Section mParent;

        private float mInnerWidth;
        private float mInnerHeight;
        private float mSlabThickness;
        private float mFoundationThickness;
        private float mOuterWallThickness;
        private int mInnerWalls;
        private float mInnerWallThickness;
        private bool mHasSlabGussets;
        private float mSlabGussetWidth;
        private float mSlabGussetHeight;

        [Category("Culvert Dimensions")]
        public float InnerWidth { get { return mInnerWidth; } set { mInnerWidth = value; Update(); } }
        [Category("Culvert Dimensions")]
        public float InnerHeight { get { return mInnerHeight; } set { mInnerHeight = value; Update(); } }
        [Category("Culvert Dimensions")]
        public float SlabThickness { get { return mSlabThickness; } set { mSlabThickness = value; Update(); } }
        [Category("Culvert Dimensions")]
        public float FoundationThickness { get { return mFoundationThickness; } set { mFoundationThickness = value; Update(); } }
        [Category("Culvert Dimensions")]
        public float OuterWallThickness { get { return mOuterWallThickness; } set { mOuterWallThickness = value; Update(); } }
        [Category("Culvert Dimensions")]
        public int InnerWalls { get { return mInnerWalls; } set { mInnerWalls = value; Update(); } }
        [Category("Culvert Dimensions")]
        public float InnerWallThickness { get { return mInnerWallThickness; } set { mInnerWallThickness = value; Update(); } }
        [Category("Slab Gussets")]
        public bool HasSlabGussets { get { return mHasSlabGussets; } set { mHasSlabGussets = value; Update(); } }
        [Category("Slab Gussets")]
        public float SlabGussetWidth { get { return mSlabGussetWidth; } set { mSlabGussetWidth = value; Update(); } }
        [Category("Slab Gussets")]
        public float SlabGussetHeight { get { return mSlabGussetHeight; } set { mSlabGussetHeight = value; Update(); } }

        [Category("Culvert Dimensions")]
        public float OuterWidth { get { return (InnerWalls + 1) * InnerWidth + 2.0f * OuterWallThickness + InnerWalls * InnerWallThickness; } }
        [Category("Culvert Dimensions")]
        public float OuterHeight { get { return InnerHeight + FoundationThickness + SlabThickness; } }

        [Browsable(false)]
        public bool IsDirty
        {
            get
            {
                return mParent.IsDirty;
            }
            internal set
            {
                mParent.IsDirty = value;
            }
        }

        public SectionProperties(Section parent)
        {
            mParent = parent;

            mInnerWidth = 4.0f;
            mInnerHeight = 3.0f;
            mSlabThickness = 0.6f;
            mFoundationThickness = 0.6f;
            mOuterWallThickness = 0.5f;
            mInnerWalls = 0;
            mInnerWallThickness = 0.5f;
            mHasSlabGussets = true;
            mSlabGussetWidth = 0.5f;
            mSlabGussetHeight = 0.25f;
        }

        private void Update()
        {
            mParent.AnalysisModel.UpdateModel();
            IsDirty = true;
        }

        public static SectionProperties FromStream(Section parent, BinaryReader r)
        {
            SectionProperties s = new SectionProperties(parent);

            s.InnerWidth = r.ReadSingle();
            s.InnerHeight = r.ReadSingle();
            s.SlabThickness = r.ReadSingle();
            s.FoundationThickness = r.ReadSingle();
            s.OuterWallThickness = r.ReadSingle();
            s.InnerWalls = r.ReadInt32();
            s.InnerWallThickness = r.ReadSingle();
            s.HasSlabGussets = r.ReadBoolean();
            s.SlabGussetWidth = r.ReadSingle();
            s.SlabGussetHeight = r.ReadSingle();

            s.mParent.AnalysisModel.UpdateModel();

            return s;
        }

        public void SaveFile(BinaryWriter w)
        {
            w.Write(InnerWidth);
            w.Write(InnerHeight);
            w.Write(SlabThickness);
            w.Write(FoundationThickness);
            w.Write(OuterWallThickness);
            w.Write(InnerWalls);
            w.Write(InnerWallThickness);
            w.Write(HasSlabGussets);
            w.Write(SlabGussetWidth);
            w.Write(SlabGussetHeight);
        }

        public SectionProperties Clone()
        {
            SectionProperties s = new SectionProperties(mParent);

            s.InnerWidth = InnerWidth;
            s.InnerHeight = InnerHeight;
            s.SlabThickness = SlabThickness;
            s.FoundationThickness = FoundationThickness;
            s.OuterWallThickness = OuterWallThickness;
            s.InnerWalls = InnerWalls;
            s.InnerWallThickness = InnerWallThickness;
            s.HasSlabGussets = HasSlabGussets;
            s.SlabGussetWidth = SlabGussetWidth;
            s.SlabGussetHeight = SlabGussetHeight;

            return s;
        }
    }
}
