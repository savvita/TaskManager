using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Linq;
using TaskManager.Model;
using System;
using GalaSoft.MvvmLight.Command;

namespace TaskManager.ViewModel
{
    public class ProcessesViewModel : INotifyPropertyChanged
    {
        #region Private fields
        private ObservableCollection<ProcessModel> _processes;
        private List<ProcessModel> _processesList;

        private int _totalProcesses;
        private double _totalCPUUsage;
        private double _totalMemoryUsage;

        private PerformanceCounter _memoryPerformanceCounter;
        private PerformanceCounter _cpuPerformanceCounter;
        private PerformanceCounter _totalMemoryCounter;
        private PerformanceCounter _totalCPUUsageCounter;

        private readonly int _cores;

        private ProcessModel _selectedProcess;
        #endregion

        #region Public properties
        public ProcessModel SelectedProcess
        {
            get => _selectedProcess;

            set
            {
                _selectedProcess = value;
                OnPropertyChanged(nameof(SelectedProcess));
            }
        }

        public ObservableCollection<ProcessModel> Processes
        {
            get => _processes;

            private set
            {
                _processes = value;
                OnPropertyChanged(nameof(Processes));
            }
        }

        public int TotalProcesses
        {
            get => _totalProcesses;

            private set
            {
                _totalProcesses = value;
                OnPropertyChanged(nameof(TotalProcesses));
            }
        }

        public double TotalCPUUsage
        {
            get => _totalCPUUsage;

            private set
            {
                _totalCPUUsage = value;
                OnPropertyChanged(nameof(TotalCPUUsage));
            }
        }

        public double TotalMemoryUsage
        {
            get => _totalMemoryUsage;

            private set
            {
                _totalMemoryUsage = value;
                OnPropertyChanged(nameof(TotalMemoryUsage));
            }
        } 
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ProcessesViewModel()
        {
            Processes = new ObservableCollection<ProcessModel>();
            _processesList = new List<ProcessModel>();

            InitializeCounters();

            _cores = GetNumberOfCores();

            Thread thread = new Thread(new ThreadStart(UpdateProcessesList));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Initialize counters with category name and name of counter. The name of an instance does not set!
        /// </summary>
        private void InitializeCounters()
        {
            _memoryPerformanceCounter = new PerformanceCounter("Process", "Working Set - Private");
            _cpuPerformanceCounter = new PerformanceCounter("Process", "% Processor Time");
            _totalMemoryCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", null);
            _totalCPUUsageCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        /// <summary>
        /// Get a number of cores of the processor
        /// </summary>
        /// <returns></returns>
        private int GetNumberOfCores()
        {
            PerformanceCounterCategory category = new PerformanceCounterCategory("Processor");
            return category.GetInstanceNames().Count();
        }

        /// <summary>
        /// Update list of current processes and set actual values of counters
        /// </summary>
        private void UpdateProcessesList()
        {
            while (true)
            {
                try
                {
                    var processes = Process.GetProcesses();

                    //Set total values
                    TotalProcesses = processes.Length;
                    TotalMemoryUsage = _totalMemoryCounter.NextValue();
                    TotalCPUUsage = _totalCPUUsageCounter.NextValue();

                    //Group all the process by name
                    var groups = processes.GroupBy(x => x.ProcessName);

                    //Fill the list of unique processes and update its values of counters
                    _processesList.Clear();

                    foreach (var group in groups)
                    {
                        ProcessModel process = new ProcessModel(group.Key, group.Count());
                        process.UpdateValues(_memoryPerformanceCounter, _cpuPerformanceCounter, _cores);
                        _processesList.Add(process);
                    }

                    //Sort the list
                    _processesList = _processesList.OrderBy(x => x.ProcessName).ToList();

                    //Clean garbage
                    var i = Processes.Count > 0 ? GC.GetGeneration(Processes[0]) : 0;
                    Processes = null;
                    Processes = new ObservableCollection<ProcessModel>(_processesList);
                    GC.Collect(i);
                }
                catch { } 
            }

        }


        #region Commands
       
        private RelayCommand? _killCommand;

        /// <summary>
        /// Command to kill selected process
        /// </summary>
        public RelayCommand KillCommand
        {
            get => _killCommand ?? (_killCommand = new RelayCommand(KillProcess));
        }
        #endregion

        /// <summary>
        /// Kill selected process
        /// </summary>
        private void KillProcess()
        {
            try
            {
                if (SelectedProcess != null)
                {
                    SelectedProcess.Kill();
                }
            }
            catch { }
        }
    }
}
