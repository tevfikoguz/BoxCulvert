using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BoxCulvert
{
    public partial class MainForm : Form
    {
        private Project mProject;

        public Project CurrentProject 
        { 
            get 
            { 
                return mProject; 
            } 
            set 
            {
                if (mProject != null) mProject.Modified -= new ProjectModifiedEventHandler(mProject_Modified);
                mProject = value;
                if (mProject != null) mProject.Modified += new ProjectModifiedEventHandler(mProject_Modified);
                UpdateDisplayedProject(); 
            } 
        }

        void mProject_Modified(object sender, EventArgs e)
        {
            culvertView.SetSection(CurrentSection);
        }

        public Section CurrentSection
        {
            get
            {
                if (cbSections.SelectedIndex == -1)
                    return null;
                else
                    return (Section)cbSections.SelectedItem;
            }
            set
            {
                if (cbSections.SelectedItem != value)
                {
                    cbSections.SelectedItem = value;
                }
                RefreshDisplay();
            }
        }

        public MainForm()
        {
            InitializeComponent();

            SetSystemFont(this);

            NewProject();
            Application.Idle += new EventHandler(Application_Idle);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            mnuViewSection.Checked = (culvertView.ViewMode == ViewMode.Section);
            mnuViewModel.Checked = (culvertView.ViewMode == ViewMode.AnalysisModel);
            mnuViewLoads.Checked = (culvertView.ViewMode == ViewMode.Loads);
            mnuViewRebars.Checked = (culvertView.ViewMode == ViewMode.Rebars);
            mnuViewMoments.Checked = (culvertView.ViewMode == ViewMode.Moments);
            mnuViewAxialForces.Checked = (culvertView.ViewMode == ViewMode.AxialForces);
            mnuViewShearForces.Checked = (culvertView.ViewMode == ViewMode.ShearForces);
            mnuViewDeflections.Checked = (culvertView.ViewMode == ViewMode.Deflections);

            btnViewSection.Checked = (culvertView.ViewMode == ViewMode.Section);
            btnViewModel.Checked = (culvertView.ViewMode == ViewMode.AnalysisModel);
            btnViewLoads.Checked = (culvertView.ViewMode == ViewMode.Loads);
            btnViewRebars.Checked = (culvertView.ViewMode == ViewMode.Rebars);
            btnViewMoments.Checked = (culvertView.ViewMode == ViewMode.Moments);
            btnViewAxialForces.Checked = (culvertView.ViewMode == ViewMode.AxialForces);
            btnViewShearForces.Checked = (culvertView.ViewMode == ViewMode.ShearForces);
            btnViewDeflections.Checked = (culvertView.ViewMode == ViewMode.Deflections);

            if (!cbViewLoad.DroppedDown)
            {
                switch (culvertView.LoadMode)
                {
                    case LoadMode.EarthFill:
                        if (cbViewLoad.SelectedIndex != 0) cbViewLoad.SelectedIndex = 0;
                        break;
                    case LoadMode.SoilPressure:
                        if (cbViewLoad.SelectedIndex != 1) cbViewLoad.SelectedIndex = 1;
                        break;
                    case LoadMode.Surcharge:
                        if (cbViewLoad.SelectedIndex != 2) cbViewLoad.SelectedIndex = 2;
                        break;
                    case LoadMode.Racking:
                        if (cbViewLoad.SelectedIndex != 3) cbViewLoad.SelectedIndex = 3;
                        break;
                }
            }
            bool loadMode = (culvertView.ViewMode == ViewMode.Loads);
            cbViewLoad.Enabled = loadMode;

            if (!cbViewOutput.DroppedDown)
            {
                switch (culvertView.OutputMode)
                {
                    case OutputMode.SelfWeight:
                        if (cbViewOutput.SelectedIndex != 0) cbViewOutput.SelectedIndex = 0;
                        break;
                    case OutputMode.EarthFill:
                        if (cbViewOutput.SelectedIndex != 1) cbViewOutput.SelectedIndex = 1;
                        break;
                    case OutputMode.SoilPressure:
                        if (cbViewOutput.SelectedIndex != 2) cbViewOutput.SelectedIndex = 2;
                        break;
                    case OutputMode.Surcharge:
                        if (cbViewOutput.SelectedIndex != 3) cbViewOutput.SelectedIndex = 3;
                        break;
                    case OutputMode.Racking:
                        if (cbViewOutput.SelectedIndex != 4) cbViewOutput.SelectedIndex = 4;
                        break;
                    case OutputMode.ServiceCombination:
                        if (cbViewOutput.SelectedIndex != 6) cbViewOutput.SelectedIndex = 6;
                        break;
                    case OutputMode.FactoredCombination1:
                        if (cbViewOutput.SelectedIndex != 7) cbViewOutput.SelectedIndex = 7;
                        break;
                    case OutputMode.FactoredCombination2:
                        if (cbViewOutput.SelectedIndex != 8) cbViewOutput.SelectedIndex = 8;
                        break;
                    case OutputMode.SeismicCombination:
                        if (cbViewOutput.SelectedIndex != 9) cbViewOutput.SelectedIndex = 9;
                        break;
                    case OutputMode.Envelope:
                        if (cbViewOutput.SelectedIndex != 10) cbViewOutput.SelectedIndex = 10;
                        break;
                }
            }

            bool outputMode = (culvertView.ViewMode == ViewMode.Moments) || (culvertView.ViewMode == ViewMode.AxialForces) || (culvertView.ViewMode == ViewMode.ShearForces) || (culvertView.ViewMode == ViewMode.Deflections);
            cbViewOutput.Enabled = outputMode;

            if (!cbViewEnvelope.DroppedDown)
            {
                switch (culvertView.EnvelopeMode)
                {
                    case EnvelopeMode.Minimum:
                        if (cbViewEnvelope.SelectedIndex != 0) cbViewEnvelope.SelectedIndex = 0;
                        break;
                    case EnvelopeMode.Maximum:
                        if (cbViewEnvelope.SelectedIndex != 1) cbViewEnvelope.SelectedIndex = 1;
                        break;
                    case EnvelopeMode.Envelope:
                        if (cbViewEnvelope.SelectedIndex != 2) cbViewEnvelope.SelectedIndex = 2;
                        break;
                }
            }
            cbViewEnvelope.Enabled = outputMode && ((culvertView.OutputMode == OutputMode.SeismicCombination) || (culvertView.OutputMode == OutputMode.Envelope));

            if (!cbViewLabels.DroppedDown)
            {
                bool hasNodes = (culvertView.LabelMode & LabelMode.Nodes) != LabelMode.None;
                bool hasFrames = (culvertView.LabelMode & LabelMode.Frames) != LabelMode.None;
                if (!hasNodes && !hasFrames)
                {
                    if (cbViewLabels.SelectedIndex != 0) cbViewLabels.SelectedIndex = 0;
                }
                else if (hasNodes & !hasFrames)
                {
                    if (cbViewLabels.SelectedIndex != 1) cbViewLabels.SelectedIndex = 1;
                }
                else if (!hasNodes & hasFrames)
                {
                    if (cbViewLabels.SelectedIndex != 2) cbViewLabels.SelectedIndex = 2;
                }
                else
                {
                    if (cbViewLabels.SelectedIndex != 3) cbViewLabels.SelectedIndex = 3;
                }
            }
            bool labelMode = (culvertView.ViewMode == ViewMode.AnalysisModel);
            cbViewLabels.Enabled = labelMode;
        }

        private void UpdateDisplayedProject()
        {
            Section currentSection = CurrentSection;
            cbSections.Items.Clear();
            cbSections.Items.AddRange(CurrentProject.Sections.ToArray());
            bool found = false;
            for (int i = 0; i < cbSections.Items.Count; i++)
            {
                if (cbSections.Items[i] == currentSection)
                {
                    cbSections.SelectedIndex = i;
                    found = true;
                }
            }
            if (!found)
            {
                cbSections.SelectedIndex = 0;
            }

            Text = CurrentProject.Name + " - Box Culvert";
        }

        private void RefreshDisplay()
        {
            switch (culvertView.ViewMode)
            {
                case ViewMode.Section:
                    pgSection.SelectedObject = CurrentSection.SectionProperties;
                    break;
                case ViewMode.Loads:
                    pgSection.SelectedObject = CurrentSection.SoilParameters;
                    break;

            }
            culvertView.SetSection(CurrentSection);
        }

        private void SetSystemFont(Control control)
        {
            control.Font = SystemFonts.MessageBoxFont;
            foreach (Control c in control.Controls)
            {
                SetSystemFont(c);
            }
        }

        private void cbSections_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentSection = (Section)cbSections.SelectedItem;
        }

        private void mnuNew_Click(object sender, EventArgs e)
        {
            NewProject();
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            SaveProject(true);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckDirtyClear())
            {
                e.Cancel = true;
            }
        }

        private bool CheckDirtyClear()
        {
            if (mProject != null && mProject.IsDirty)
            {
                DialogResult res = MessageBox.Show("Do you want to save the changes to " + mProject.Name + "?", "Box Culvert", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (res == DialogResult.Cancel)
                {
                    return false;
                }
                else if (res == DialogResult.Yes)
                {
                    if (!SaveProject()) return false;
                }
            }
            return true;
        }

        private bool NewProject()
        {
            if (!CheckDirtyClear())
            {
                return false;
            }

            CurrentProject = new Project();
            return true;
        }

        private bool OpenProject()
        {
            if (!CheckDirtyClear())
            {
                return false;
            }

            DialogResult res = ofdProject.ShowDialog();
            if (res == DialogResult.Cancel) return false;

            Project p = Project.ReadFile(ofdProject.FileName);
            if (p == null) return false;
            CurrentProject = p;
            return true;
        }

        private bool SaveProject()
        {
            return SaveProject(false);
        }

        private bool SaveProject(bool forceSaveAs)
        {
            if (string.IsNullOrEmpty(mProject.FileName) || forceSaveAs || !File.Exists(mProject.FileName))
            {
                sfdProject.FileName = mProject.FileName;
                DialogResult res = sfdProject.ShowDialog();
                if (res == DialogResult.Cancel) return false;
                return mProject.SaveFile(sfdProject.FileName);
            }
            else
            {
                return mProject.SaveFile(mProject.FileName);
            }
        }

        private void mnuAddSection_Click(object sender, EventArgs e)
        {
            AddSection();
        }

        private void mnuRenameSection_Click(object sender, EventArgs e)
        {
            RenameSection();
        }

        private void mnuDuplicateSection_Click(object sender, EventArgs e)
        {
            DuplicateSection();
        }

        private void mnuDeleteSection_Click(object sender, EventArgs e)
        {
            DeleteSection();
        }

        private void btnAddSection_Click(object sender, EventArgs e)
        {
            AddSection();
        }

        private void btnRenameSection_Click(object sender, EventArgs e)
        {
            RenameSection();
        }

        private void btnDuplicateSection_Click(object sender, EventArgs e)
        {
            DuplicateSection();
        }

        private void btnDeleteSection_Click(object sender, EventArgs e)
        {
            DeleteSection();
        }

        private void AddSection()
        {
            string name = SectionName.GetSectionName();
            if (name != string.Empty)
            {
                Section s = new Section(mProject, name);
                mProject.Sections.Add(s);
                UpdateDisplayedProject();
                CurrentSection = s;
            }
        }

        private void RenameSection()
        {
            string name = SectionName.GetSectionName(CurrentSection.Name);
            if (name != string.Empty)
            {
                CurrentSection.Name = name;
                UpdateDisplayedProject();
            }
        }

        private void DuplicateSection()
        {
            string name = SectionName.GetSectionName();
            if (name != string.Empty)
            {
                Section s = CurrentSection.Clone();
                s.Name = name;
                mProject.Sections.Add(s);

                UpdateDisplayedProject();
                CurrentSection = s;
            }
        }

        private void DeleteSection()
        {
            if (mProject.Sections.Count <= 1)
            {
                MessageBox.Show("The last section can not be deleted.", "Box Culvert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete section " + CurrentSection.Name + "?", "Box Culvert", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
            {
                return;
            }

            mProject.Sections.Remove(CurrentSection);
            UpdateDisplayedProject();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            NewProject();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void pgSection_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            UpdateDisplayedProject();
        }

        private void mnuViewSection_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Section;
            RefreshDisplay();
        }

        private void mnuViewModel_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.AnalysisModel;
            RefreshDisplay();
        }

        private void mnuViewLoads_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Loads;
            RefreshDisplay();
        }

        private void mnuViewRebars_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Rebars;
            RefreshDisplay();
        }

        private void mnuViewMoments_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Moments;
            RefreshDisplay();
        }

        private void mnuViewAxialForces_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.AxialForces;
            RefreshDisplay();
        }

        private void mnuViewShearForces_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.ShearForces;
            RefreshDisplay();
        }

        private void mnuViewDeflections_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Deflections;
            RefreshDisplay();
        }

        private void btnViewSection_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Section;
            RefreshDisplay();
        }

        private void btnViewModel_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.AnalysisModel;
            RefreshDisplay();
        }

        private void btnViewLoads_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Loads;
            RefreshDisplay();
        }

        private void btnViewRebars_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Rebars;
            RefreshDisplay();
        }

        private void btnViewMoments_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Moments;
            RefreshDisplay();
        }

        private void btnViewAxialForces_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.AxialForces;
            RefreshDisplay();
        }

        private void btnViewShearForces_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.ShearForces;
            RefreshDisplay();
        }

        private void btnViewDeflections_Click(object sender, EventArgs e)
        {
            culvertView.ViewMode = ViewMode.Deflections;
            RefreshDisplay();
        }

        private void cbViewOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbViewOutput.SelectedIndex)
            {
                case 0:
                    culvertView.OutputMode = OutputMode.SelfWeight;
                    break;
                case 1:
                    culvertView.OutputMode = OutputMode.EarthFill;
                    break;
                case 2:
                    culvertView.OutputMode = OutputMode.SoilPressure;
                    break;
                case 3:
                    culvertView.OutputMode = OutputMode.Surcharge;
                    break;
                case 4:
                    culvertView.OutputMode = OutputMode.Racking;
                    break;
                case 6:
                    culvertView.OutputMode = OutputMode.ServiceCombination;
                    break;
                case 7:
                    culvertView.OutputMode = OutputMode.FactoredCombination1;
                    break;
                case 8:
                    culvertView.OutputMode = OutputMode.FactoredCombination2;
                    break;
                case 9:
                    culvertView.OutputMode = OutputMode.SeismicCombination;
                    break;
                case 10:
                    culvertView.OutputMode = OutputMode.Envelope;
                    break;
            }

            RefreshDisplay();
        }

        private void cbViewEnvelope_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbViewEnvelope.SelectedIndex)
            {
                case 0:
                    culvertView.EnvelopeMode = EnvelopeMode.Minimum;
                    break;
                case 1:
                    culvertView.EnvelopeMode = EnvelopeMode.Maximum;
                    break;
                case 2:
                    culvertView.EnvelopeMode = EnvelopeMode.Envelope;
                    break;
            }

            RefreshDisplay();
        }

        private void cbViewLoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbViewLoad.SelectedIndex)
            {
                case 0:
                    culvertView.LoadMode = LoadMode.EarthFill;
                    break;
                case 1:
                    culvertView.LoadMode = LoadMode.SoilPressure;
                    break;
                case 2:
                    culvertView.LoadMode = LoadMode.Surcharge;
                    break;
                case 3:
                    culvertView.LoadMode = LoadMode.Racking;
                    break;
            }
            RefreshDisplay();
        }

        private void cbViewLabels_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbViewLabels.SelectedIndex)
            {
                case 0:
                    culvertView.LabelMode = LabelMode.None;
                    break;
                case 1:
                    culvertView.LabelMode = LabelMode.Nodes;
                    break;
                case 2:
                    culvertView.LabelMode = LabelMode.Frames;
                    break;
                case 3:
                    culvertView.LabelMode = LabelMode.All;
                    break;
            }
            RefreshDisplay();
        }

        private void mnuZoomIn_Click(object sender, EventArgs e)
        {
            culvertView.ZoomIn();
        }

        private void mnuZoomOut_Click(object sender, EventArgs e)
        {
            culvertView.ZoomOut();
        }

        private void mnuZoomExtents_Click(object sender, EventArgs e)
        {
            culvertView.ZoomToExtents();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            culvertView.ZoomIn();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            culvertView.ZoomOut();
        }

        private void btnZoomExtents_Click(object sender, EventArgs e)
        {
            culvertView.ZoomToExtents();
        }
    }
}
