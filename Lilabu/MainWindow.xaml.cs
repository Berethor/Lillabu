using Lilabu.ViewModels;
using System.Windows;

namespace Lilabu
{
    public partial class MainWindow : Window
    {
        public MainViewModel VM = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = VM;
        }
    }
}
