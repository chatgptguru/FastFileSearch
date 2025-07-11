// namespace FastFileSearch
// {
//     internal static class Program
//     {
//         /// <summary>
//         ///  The main entry point for the application.
//         /// </summary>
//         [STAThread]
//         static void Main()
//         {
//             // To customize application configuration such as set high DPI settings or default font,
//             // see https://aka.ms/applicationconfiguration.
//             ApplicationConfiguration.Initialize();
//             Application.Run(new Form1());
//         }
//     }
// }

using System;
using System.Windows.Forms;

namespace FastFileSearch
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}