using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TaskManager.Model
{
    public class ProcessModel : INotifyPropertyChanged
    {
        #region Private fields

        private string _processName;
        private string _processNameTotalCount;
        private double _memoryUsage;
        private double _CPUUsage;
        private double _diskUsage;

        #endregion

        #region Public properties
        public string ProcessName
        {
            get => _processName;

            private set
            {
                _processName = value;
                OnPropertyChanged(nameof(ProcessName));
            }
        }

        public string ProcessNameTotalCount
        {
            get => _processNameTotalCount;

            private set
            {
                _processNameTotalCount = value;
                OnPropertyChanged(nameof(ProcessNameTotalCount));
            }
        }

        public double MemoryUsage
        {
            get => _memoryUsage;

            private set
            {
                _memoryUsage = value;
                OnPropertyChanged(nameof(MemoryUsage));
            }
        }

        public double CPUUsage
        {
            get => _CPUUsage;

            private set
            {
                _CPUUsage = value;
                OnPropertyChanged(nameof(CPUUsage));
            }
        }

        public double DiskUsage
        {
            get => _diskUsage;

            private set
            {
                _diskUsage = value;
                OnPropertyChanged(nameof(DiskUsage));
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProcessModel(string processName, int count)
        {
            ProcessName = processName;
            ProcessNameTotalCount = $"{processName} ({count})";
        }

        public void UpdateValues(PerformanceCounter memoryCounter, PerformanceCounter cpuCounter, int cores)
        {
            SetMemoryUsage(memoryCounter);
            SetCPUUsage(cpuCounter, cores);
        }

        private void SetMemoryUsage(PerformanceCounter memoryCounter)
        {
            memoryCounter.InstanceName = ProcessName;

            try
            {
                MemoryUsage = memoryCounter.NextValue() / 1024 / 1024;
            }
            catch { }
        }

        private void SetCPUUsage(PerformanceCounter cpuCounter, int cores)
        {
            cpuCounter.InstanceName = ProcessName;

            try
            {
                cpuCounter.NextValue();
                CPUUsage = cpuCounter.NextValue() / cores;
            }
            catch { }
        }


        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Kill()
        {
            try
            {
                foreach(Process process in Process.GetProcessesByName(ProcessName))
                {
                    process.Kill();
                }
            }
            catch { }
        }
    }
}
