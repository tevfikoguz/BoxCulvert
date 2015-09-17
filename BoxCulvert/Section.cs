using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace BoxCulvert
{
    public class Section
    {
        internal Project mParent;

        private string mName;

        public string Name { get { return mName; } set { mName = value; IsDirty = true; } }
        public SectionProperties SectionProperties { get; private set; }
        public SoilParameters SoilParameters { get; private set; }
        public CulvertAnalysisModel AnalysisModel { get; private set; }

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

        public Section(Project parent, string name)
        {
            mParent = parent;
            mName = name;
            SectionProperties = new SectionProperties(this);
            SoilParameters = new SoilParameters(this);
            AnalysisModel = new CulvertAnalysisModel(this);

            AnalysisModel.UpdateModel();
        }

        public Section(Project parent)
            : this(parent, "Section1")
        {
            ;
        }

        public static Section FromStream(Project parent, BinaryReader r)
        {
            Section s = new Section(parent);

            s.Name = r.ReadString();

            s.SectionProperties = SectionProperties.FromStream(s, r);
            s.SoilParameters = SoilParameters.FromStream(s, r);

            return s;
        }

        public void SaveFile(BinaryWriter w)
        {
            w.Write(Name);

            SectionProperties.SaveFile(w);
            SoilParameters.SaveFile(w);
        }

        public Section Clone()
        {
            Section s = new Section(mParent);

            s.Name = Name;

            s.SectionProperties = SectionProperties.Clone();
            s.SoilParameters = SoilParameters.Clone();

            return s;
        }

        public override string ToString()
        {
            return mName + " (" + SectionProperties.InnerWidth.ToString("0.00") + "x" + SectionProperties.InnerHeight.ToString("0.00") + ") H=" + SoilParameters.FillHeight.ToString("0.00") + " m";
        }
    }
}
