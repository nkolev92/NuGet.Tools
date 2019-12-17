
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using NuGet.VisualStudio;


namespace IVsTestingExtension.ToolWindows
{
    public class PackageInstallerModel : INotifyPropertyChanged
    {
        private string _resultText;
        private string _packageId;
        private string _packageVersion;
        private HashSet<string> _projects;
        private string _projectName;
        private ThreadAffinity _threadAffinity;

        private readonly DTE dte;
        private readonly IVsAsyncPackageInstaller vsAsyncPackageInstaller;
        private SolutionEvents solutionEvents; // We need a reference to SolutionEvents to avoid getting GC'ed

        // We don't really handle the no solution case so well :) 
        public PackageInstallerModel(DTE _dte, IVsAsyncPackageInstaller _vsAsyncPackageInstaller)
        {
            vsAsyncPackageInstaller = _vsAsyncPackageInstaller ?? throw new ArgumentNullException(nameof(_vsAsyncPackageInstaller));
            dte = _dte ?? throw new ArgumentNullException(nameof(_dte));
            ResultText = string.Empty;
            ProjectName = "Project Name";
            PackageVersion = "6.0.4";
            PackageId = "Newtonsoft.Json";
        }

        internal async System.Threading.Tasks.Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            solutionEvents = dte.Events.SolutionEvents;
            solutionEvents.Opened += OnSolutionLoaded;
            solutionEvents.BeforeClosing += OnSolutionClosing;
            solutionEvents.ProjectAdded += OnEnvDTEProjectAdded;
            solutionEvents.ProjectRemoved += OnEnvDTEProjectRemoved;
            solutionEvents.ProjectRenamed += OnEnvDTEProjectRenamed;
        }

        public void Clicked()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Project projectSelected = GetSelectedProject();
            ResultText = $"Project {projectSelected.Name}! PackageId: {PackageId}, PackageVersion: {PackageVersion}";

            switch (ThreadAffinity)
            {
                case ThreadAffinity.SYNC_JTFRUN_BLOCKING: // deadlock
                    {
                        ResultText += $"{Environment.NewLine}Running blocking call on the UI thread. ThreadHelper.JoinableTaskFactory.Run(async ()";
                        ThreadHelper.JoinableTaskFactory.Run(async () =>
                        {
                            await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                                  projectSelected,
                                  PackageId,
                                  PackageVersion,
                                  ignoreDependencies: false);
                        });
                        break;
                    }
                case ThreadAffinity.SYNC_THREADPOOL_TASKRUN: // no deadlock usually.
                    {
                        ResultText += $"{Environment.NewLine}Running on a background thread. Kicking off asynchronously. Task.Run(async ()";
                        _ = System.Threading.Tasks.Task.Run(async () =>
                        {
                            await vsAsyncPackageInstaller.InstallPackageAsync(source: null,
                                projectSelected,
                                PackageId,
                                PackageVersion,
                                ignoreDependencies: false);
                        });
                        break;
                    }
                case ThreadAffinity.SYNC_BLOCKING_TASKRUN: // deadlock
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
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits - Done on purpose. This would usually deadlock :) 
                            task.Wait(CancellationToken.None);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                        }
                        catch (AggregateException)
                        {
                        }
                        break;
                    }
                case ThreadAffinity.SYNC_JTFRUNASYNC_FIRE_FORGET:
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
                        break;
                    }
                default:
                    ResultText += "Unexpected ThreadAffinity";
                    break;
            }

            ResultText += $"The invocation returned.";
        }

        public async System.Threading.Tasks.Task ClickedAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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

        public IEnumerable<string> Projects
        {
            get => _projects;
            set
            {
                _projects = new HashSet<string>(value);
                OnPropertyChanged("Projects");
            }
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

        private void OnSolutionClosing()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projects?.Clear();
            Projects = _projects;
            UpdateProjectName();
        }

        private void OnSolutionLoaded()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projects = new HashSet<string>();
            foreach (Project project in dte.Solution.Projects)
            {
                projects.Add(project.Name);
            }
            _projects = projects;
            Projects = _projects;
            UpdateProjectName();
        }

        private void OnEnvDTEProjectRenamed(Project Project, string OldName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projects.Remove(OldName);
            _projects.Add(Project.Name);
            Projects = _projects;
            UpdateProjectName();
        }

        private void OnEnvDTEProjectRemoved(Project Project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projects.Remove(Project.Name);
            Projects = _projects;
            UpdateProjectName();
        }

        private void OnEnvDTEProjectAdded(Project Project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projects.Add(Project.Name);
            Projects = _projects;
            UpdateProjectName();
        }

        private void UpdateProjectName()
        {
            if (string.IsNullOrEmpty(_projectName))
            {
                if (_projects?.Count > 0)
                {
                    ProjectName = _projects.First();
                }
            }
            else
            {
                if (!_projects.Contains(_projectName))
                {
                    ProjectName = _projects.First();
                }
            }
        }
    }
}
