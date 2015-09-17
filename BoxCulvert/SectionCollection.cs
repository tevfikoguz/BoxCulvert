using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace BoxCulvert
{
    public class SectionCollection : IList<Section>
    {
        Project mParent;
        List<Section> mSections;

        public SectionCollection(Project parent)
        {
            mParent = parent;
            mSections = new List<Section>();
        }

        public void RemoveAt(int index)
        {
            mSections.RemoveAt(index);
            mParent.IsDirty = true;
        }

        public Section this[int index]
        {
            get
            {
                return mSections[index];
            }
            set
            {
                value.mParent = mParent;
                mSections[index] = value;
                mParent.IsDirty = true;
            }
        }

        public void Add(Section item)
        {
            item.mParent = mParent;
            mSections.Add(item);
            mParent.IsDirty = true;
        }

        public void Add(string name)
        {
            Section item = new Section(mParent, name);
            mSections.Add(item);
            mParent.IsDirty = true;
        }

        public void Clear()
        {
            mSections.Clear();
            mParent.IsDirty = true;
        }

        public bool Contains(Section item)
        {
            return mSections.Contains(item);
        }

        public void CopyTo(Section[] array, int arrayIndex)
        {
            if (!(array is Section[]))
                throw new ArgumentException("An array of Section is required.", "array");
            mSections.CopyTo((Section[])array, arrayIndex);
        }

        public int Count
        {
            get { return mSections.Count; }
        }

        public int IndexOf(Section item)
        {
            return mSections.IndexOf(item);
        }

        public void Insert(int index, Section item)
        {
            item.mParent = mParent;
            mSections.Insert(index, item);
            mParent.IsDirty = true;
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Section item)
        {
            bool exists = mSections.Remove(item);
            if (exists) mParent.IsDirty = true;
            return exists;
        }

        public IEnumerator<Section> GetEnumerator()
        {
            return mSections.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static SectionCollection FromStream(Project parent, BinaryReader r)
        {
            SectionCollection coll = new SectionCollection(parent);

            int count = r.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                Section s = Section.FromStream(parent, r);
                coll.Add(s);
            }

            return coll;
        }

        public void SaveFile(BinaryWriter w)
        {
            w.Write(Count);

            foreach (Section s in mSections)
            {
                s.SaveFile(w);
            }
        }
    }
}
