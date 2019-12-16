using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IVsTestingExtension.ToolWindows
{
    public partial class ToolWindowControl : UserControl
    {

        private PackageInstallerModel _state;

        public ToolWindowControl(PackageInstallerModel state)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _state = state;
            InitializeComponent();
            PackageId.DataContext = _state;
            PackageVersion.DataContext = _state;
            ProjectName.DataContext = _state;
            Result.DataContext = _state;
            Affinity.DataContext = _state;
            Affinity.ItemsSource = Enum.GetValues(typeof(ThreadAffinity)).Cast<ThreadAffinity>();
            Affinity.SelectedItem = ThreadAffinity.SYNC_JTFRUN_BLOCKING;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _state.Clicked();
        }

#pragma warning disable VSTHRD100 // Avoid async void methods - UI events need to be void.
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            try
            {
                await _state.ClickedAsync();
            }
            catch
            {
                // do nothing
            }
        }
    }
}
