using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Bitbird.Core.Tests.Forms
{
    public partial class StartupDialog : Form
    {
        public Type SelectedTestForm { get; private set; }

        public StartupDialog()
        {
            InitializeComponent();

            srcTestFormTypes.DataSource = typeof(StartupDialog)
                .Assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<TestFormAttribute>() != null &&
                            typeof(Form).IsAssignableFrom(t))
                .OrderBy(x => x.Name)
                .ToArray();

            DialogResult = DialogResult.Cancel;

            lbTestFormTypes_SelectedIndexChanged(lbTestFormTypes, new EventArgs());
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!(srcTestFormTypes.Current is Type currentType))
            {
                MessageBox.Show("No valid item selected.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SelectedTestForm = currentType;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void lbTestFormTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOpen.Enabled = srcTestFormTypes.Current is Type;
        }

        private void StartupDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
