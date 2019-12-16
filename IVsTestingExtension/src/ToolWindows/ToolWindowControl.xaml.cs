using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IVsTestingExtension.ToolWindows
{
    public partial class ToolWindowControl : UserControl
    {

        private PackageInstallerState _state;

        public ToolWindowControl(PackageInstallerState state)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _state = state;
            InitializeComponent();
            PackageId.DataContext = _state;
            PackageVersion.DataContext = _state;
            ProjectName.DataContext = _state;
            Result.DataContext = _state;
            Affinity.ItemsSource = Enum.GetValues(typeof(ThreadAffinity)).Cast<ThreadAffinity>();
            Affinity.DataContext = _state;
            Affinity.SelectedItem = ThreadAffinity.SYNC_JTFRUN_BLOCKING;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _state.Clicked();
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            await _state.ClickedAsync();
        }
    }

    public enum ThreadAffinity
    {
        ASYNC_FROM_UI,
        ASYNC_FROM_BACKGROUND,
        SYNC_JTFRUN_BLOCKING, 
        SYNC_THREADPOOL_TASKRUN,
        SYNC_JTFRUNASYNC_FIRE_FORGET,
        SYNC_BLOCKING_TASKRUN,
    }
}
