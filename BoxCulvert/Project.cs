using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace BoxCulvert
{
    public delegate void ProjectModifiedEventHandler(object sender, EventArgs e);
    
    public class Project
    {
        public event ProjectModifiedEventHandler Modified;

        private bool mIsDirty;

        private string mFileName;
        private SectionCollection mSections;

        public string Name { get { return Path.GetFileName(mFileName); } }
        public string FileName { get { return mFileName; } set { mFileName = value; IsDirty = true; } }
        public SectionCollection Sections { get { return mSections; } private set { mSections = value; IsDirty = true; } }

        public bool IsDirty
        {
            get
            {
                return mIsDirty;
            }
            internal set
            {
                mIsDirty = value;
            }
        }

        public Project()
        {
            mFileName = "Project1";
            mSections = new SectionCollection(this);
            mSections.Add("Section1");
            mIsDirty = false;
        }

        protected virtual void OnModified(EventArgs e)
        {
            if (Modified != null)
                Modified(this, e);
        }

        public static Project ReadFile(string filename)
        {
            Project p = new Project();
            p.FileName = filename;

            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (BinaryReader r = new BinaryReader(stream))
                {
                    p.Sections = SectionCollection.FromStream(p, r);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Box Culvert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            p.IsDirty = false;
            return p;
        }

        public bool SaveFile(string filename)
        {
            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                using (BinaryWriter w = new BinaryWriter(stream))
                {
                    Sections.SaveFile(w);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Box Culvert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            FileName = filename;
            IsDirty = false;
            return true;
        }
    }
}
