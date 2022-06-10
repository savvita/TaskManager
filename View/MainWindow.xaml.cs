using System.Windows;
using TaskManager.ViewModel;

namespace TaskManager.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProcessesViewModel _viewmodel;
        public MainWindow()
        {
            InitializeComponent();
            _viewmodel = new ProcessesViewModel();
            Processes.DataContext = _viewmodel;
            Total.DataContext = _viewmodel;
        }
    }
}
