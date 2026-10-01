// =============================================================================
// TCVN Material Importer - Macro Launcher
// Place this file in: Tekla macros folder (e.g. ..\Environments\Vietnam\General\macros\modeling\)
// This launcher calls the main TCVN_Material_Importer.cs from the material-add folder.
// =============================================================================

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            // Resolve the material-add folder path relative to the environment
            string xsDataDir = System.Environment.GetEnvironmentVariable("XSDATADIR");

            // Try Vietnam environment path first
            string macroPath = System.IO.Path.Combine(
                xsDataDir, "environments", "vietnam", "material-add", "TCVN_Material_Importer.cs");

            if (!System.IO.File.Exists(macroPath))
            {
                // Fallback: try common macros location
                macroPath = System.IO.Path.Combine(
                    xsDataDir, "environments", "common", "macros", "modeling",
                    "TCVN_Material_Importer.cs");
            }

            if (System.IO.File.Exists(macroPath))
            {
                // Use Tekla's built-in macro runner to compile and execute the main script
                akit.Callback("acmd_run_script", macroPath, "main_frame");
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(
                    "Không tìm thấy file TCVN_Material_Importer.cs!\n\n" +
                    "Đường dẫn đã tìm:\n" + macroPath + "\n\n" +
                    "Hãy đảm bảo thư mục material-add nằm trong Environment Vietnam.",
                    "TCVN Material Importer",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
