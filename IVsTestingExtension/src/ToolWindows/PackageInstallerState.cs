
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using NuGet.VisualStudio;


namespace IVsTestingExtension.ToolWindows
{
    public class PackageInstallerState : INotifyPropertyChanged
    {
        private string _resultText;
        private string _packageId;
        private string _packageVersion;
        private List<string> _projects;
        private string _projectName;
        private ThreadAffinity _threadAffinity;

        internal readonly DTE dte;
        private readonly IVsAsyncPackageInstaller vsAsyncPackageInstaller;

        public PackageInstallerState(DTE _dte, IVsAsyncPackageInstaller _vsAsyncPackageInstaller)
        {
            vsAsyncPackageInstaller = _vsAsyncPackageInstaller;
            dte = _dte;
            ResultText = string.Empty;
            ProjectName = "Project Name";
            PackageVersion = "Package Version";
            PackageId = "Package Id";
        }

        public List<string> Projects
        {
            get
            {
                if(_projects == null)
                {
                    var projects = new List<string>
                    {
                        "ConsoleApp4"
                    };
                    //foreach(Project project in (Solution)dte.Solution.Projects)
                    //{
                    //    projects.Add(project.Name);
                    //}
                    _projects = projects;

                }
                return _projects;
            }
            set
            {
                _projects = value;
                OnPropertyChanged("ResultText");
            }
        }

        public void Clicked()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Project projectSelected = GetSelectedProject();

            if (vsAsyncPackageInstaller == null)
            {
                ResultText = "No installer service";
                return;
            }

            if (projectSelected == null)
            {
                ResultText = $"Could not find project {ProjectName}";
            }
            else
            {
                ResultText = $"Found the project! PackageId: {PackageId}, PackageVersion: {PackageVersion}";

                if (ThreadAffinity == ThreadAffinity.SYNC_JTFRUN_BLOCKING)
                {
                    // This deadlocks - yikes.
                    ResultText += $"{Environment.NewLine}Running blocking call on the UI thread. ThreadHelper.JoinableTaskFactory.Run(async ()";
                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                              projectSelected,
                              PackageId,
                              PackageVersion,
                              ignoreDependencies: false);
                    });
                }
                else if (ThreadAffinity == ThreadAffinity.SYNC_THREADPOOL_TASKRUN) // no deadlock
                {
                    ResultText += $"{Environment.NewLine}Running on a background thread. Kicking off asynchronously. Task.Run(async ()";
                    System.Threading.Tasks.Task.Run(async () =>
                      {
                          await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                              projectSelected,
                              PackageId,
                              PackageVersion,
                              ignoreDependencies: false);
                      });
                }
                else if (ThreadAffinity == ThreadAffinity.SYNC_BLOCKING_TASKRUN) //deadlock
                {
                    ResultText += $"{Environment.NewLine}Running on the UI thread, blocking BLOCKING_TASKRUN. Kicking off asynchronously. Task.Run(async ()";
                    var task = System.Threading.Tasks.Task.Run(async () =>
                    {
                        await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                            projectSelected,
                            PackageId,
                            PackageVersion,
                            ignoreDependencies: false);
                    });
                    try
                    {
                        task.Wait(CancellationToken.None);
                    }
                    catch (AggregateException ex)
                    {
                    }
                }
                else if (ThreadAffinity == ThreadAffinity.SYNC_JTFRUNASYNC_FIRE_FORGET) // No deadlock
                {
                    ResultText += $"{Environment.NewLine}Running blocking call on the UI thread. ThreadHelper.JoinableTaskFactory.RunAsync(async ()";
                    ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        // This is a background thread isn't it?
                        // Actually it's the UI thread.
                        await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                              projectSelected,
                              PackageId,
                              PackageVersion,
                              ignoreDependencies: false);
                    });
                }
                else
                {
                    ResultText += $"{Environment.NewLine} Not suitable {ThreadAffinity}";
                }

                ResultText += $"{Environment.NewLine}Kicked off install package.";
            }
        }

        public async System.Threading.Tasks.Task ClickedAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (vsAsyncPackageInstaller == null)
            {
                ResultText = "No installer service";
                return;
            }

            Project projectSelected = GetSelectedProject();

            if (projectSelected == null)
            {
                ResultText = $"Could not find project {ProjectName}";
            }
            else
            {
                ResultText = $"Found the project! PackageId: {PackageId}, PackageVersion: {PackageVersion}";
                ResultText += $"{Environment.NewLine}Kicking off install package.";

                if (ThreadAffinity == ThreadAffinity.ASYNC_FROM_UI)
                {
                    ResultText += $"{Environment.NewLine} Async on the UI thread";
                    try
                    {
                        await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                              projectSelected,
                              PackageId,
                              PackageVersion,
                              ignoreDependencies: false);
                    }
                    catch (Exception e)
                    {
                        ResultText += e.Message;
                    }
                }
                else if (ThreadAffinity == ThreadAffinity.ASYNC_FROM_BACKGROUND)
                {
                    ResultText += $"{Environment.NewLine}Switching to a threadpool thread.";
                    await TaskScheduler.Default;
                    try
                    {
                        await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                            projectSelected,
                            PackageId,
                            PackageVersion,
                            ignoreDependencies: false);
                    }
                    catch (Exception e)
                    {
                        ResultText += e.Message;
                    }
                }
                else
                {
                    ResultText += $"{Environment.NewLine} Not suitable {ThreadAffinity}";
                }
            }
        }

        private Project GetSelectedProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = (SolutionClass)dte.Solution;
            Project projectSelected = null;

            foreach (Project project in solution.Projects)
            {
                if (project.Name.Equals(ProjectName))
                {
                    projectSelected = project;
                    break;
                }
            }

            return projectSelected;
        }

        public string ResultText
        {
            get => _resultText;
            set
            {
                _resultText = value;
                OnPropertyChanged("ResultText");
            }
        }

        public ThreadAffinity ThreadAffinity
        {
            get => _threadAffinity;
            set
            {
                _threadAffinity = value;
                OnPropertyChanged("ThreadAffinity");
            }
        }

        public string PackageId
        {
            get => _packageId;
            set
            {
                _packageId = value;
                OnPropertyChanged("PackageId");
            }
        }

        public string PackageVersion
        {
            get => _packageVersion;
            set
            {
                _packageVersion = value;
                OnPropertyChanged("PackageVersion");
            }
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged("ProjectName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
