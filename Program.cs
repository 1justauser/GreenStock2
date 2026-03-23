using System;
using System.Windows.Forms;
using GreenStock.Forms;

namespace GreenStock
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            while (true)
            {
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() != DialogResult.OK || loginForm.LoggedInUser == null)
                    break;

                var catalogForm = new CatalogForm(loginForm.LoggedInUser);
                catalogForm.ShowDialog();
                
            }
        }
    }
}
