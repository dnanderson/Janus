using System.Windows;
using Janus.ViewModels;

namespace Janus
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Closing += (sender, args) => viewModel.Dispose();
        }
    }
}
