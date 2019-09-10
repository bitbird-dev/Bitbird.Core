using System;
using System.Windows.Forms;

namespace Bitbird.Core.Tests.Forms
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Type formType;
            using (var dialog = new StartupDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                formType = dialog.SelectedTestForm;
            }

            Application.Run((Form) Activator.CreateInstance(formType));
        }
    }
}
