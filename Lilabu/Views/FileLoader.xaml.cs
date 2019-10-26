using System.Linq;
using System.Windows.Controls;

namespace Lilabu.Views
{
    public partial class FileLoader : UserControl
    {
        public FileLoader()
        {
            InitializeComponent();

            textBox_text.SelectionChanged += TextBox_text_SelectionChanged;
        }

        private void TextBox_text_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if  (!(sender is TextBox textBox)) { return; }

            var position = textBox.SelectionStart;
            var lineNumber = textBox.Text.Substring(0, position).Count(c => c == '\n') + 1 ;

            textBlock_lineNumber.Text = "Line: " + lineNumber;
        }
    }
}
