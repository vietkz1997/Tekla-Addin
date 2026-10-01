using System;
using System.Windows;
using System.Windows.Interop;
using RevitQtoTool.ViewModels;

namespace RevitQtoTool.Views
{
    public partial class QtoWindow : Window
    {
        public QtoWindow(object revitContext = null)
        {
            InitializeComponent();

            if (revitContext != null)
            {
                AttachRevitOwner(revitContext);
            }

            DataContext = new MainQtoViewModel(revitContext);
        }

        /// <summary>
        /// Attaches the Revit main window as owner for proper Z-order behavior.
        /// Uses NoInlining to prevent CLR from eager-loading RevitAPIUI.dll in standalone mode.
        /// All Revit types are referenced via fully-qualified names to avoid top-level 'using'.
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void AttachRevitOwner(object revitContext)
        {
            try
            {
                if (revitContext is Autodesk.Revit.UI.UIApplication uiapp)
                {
                    IntPtr revitHandle = uiapp.MainWindowHandle;
                    if (revitHandle != IntPtr.Zero)
                    {
                        new WindowInteropHelper(this).Owner = revitHandle;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AttachRevitOwner] Failed: {ex.Message}");
            }
        }
    }
}
