using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    internal static class Program
    {
        #region Application startup

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Database.InitializeSchema();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The application database could not be initialized.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Login is the application's main form.  Starting the message loop
            // with it keeps the application alive while the user navigates to
            // the dashboard or registration form.
            Application.Run(new Login());
        }

        #endregion
    }
}
