using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BoxCulvert
{
    public partial class SectionName : Form
    {
        public string SelectedName { get { return txtSectionName.Text; } set { txtSectionName.Text = value; } }

        public SectionName()
        {
            InitializeComponent();
        }

        private void txtSectionName_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSectionName.Text)) e.Cancel = true;
        }

        public static string GetSectionName(string name)
        {
            SectionName form = new SectionName();
            form.SelectedName = name;
            if (form.ShowDialog() == DialogResult.Cancel)
                return string.Empty;
            else
                return form.SelectedName;
        }

        public static string GetSectionName()
        {
            return GetSectionName(string.Empty);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SectionName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
        }
    }
}
