using Lilabu.ViewModels;
using System.Windows;

namespace Lilabu
{
    public partial class MainWindow : Window
    {
        public MainViewModel VM { get; }

        public MainWindow()
        {
            VM = new MainViewModel(this);

            InitializeComponent();

            DataContext = VM;
        }
    }
}
