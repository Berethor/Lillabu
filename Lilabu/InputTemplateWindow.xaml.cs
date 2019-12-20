using System;
using Lilabu.ViewModels;
using System.Windows;

namespace Lilabu
{
    /// <summary>
    /// Логика взаимодействия для InputTemplateWindow.xaml
    /// </summary>
    public partial class InputTemplateWindow : Window
    {
        public InputTemplateViewModel VM { get; set; }

        public event EventHandler<string> OnResult;

        public InputTemplateWindow()
        {
            InitializeComponent();

            VM = new InputTemplateViewModel();
            VM.Ok = new BaseCommand(() => CloseWithResult(true));
            VM.Canсel = new BaseCommand(() => CloseWithResult(false));

            void AddText(string text)
            {
                try
                {
                    var pos = TextBoxTemplate.CaretIndex;
                    VM.InputText = VM.InputText.Insert(pos, text);
                    TextBoxTemplate.CaretIndex = pos + text.Length;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            void Remove()
            {
                try
                {
                    var pos = TextBoxTemplate.CaretIndex;
                    if (pos < 2) return;
                    VM.InputText = VM.InputText.Substring(0, pos - 2) + VM.InputText.Substring(pos);
                    TextBoxTemplate.CaretIndex = pos;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            VM.Joystick = new JoystickViewModel()
            {
                Up = new BaseCommand(() => AddText("L3")),
                Left = new BaseCommand(() => AddText("t2")),
                Right = new BaseCommand(() => AddText("T2")),
                SmallUp = new BaseCommand(() => AddText("L1")),
                SmallLeft = new BaseCommand(() => AddText("t4")),
                SmallRight = new BaseCommand(() => AddText("T4")),
                Bridge = new BaseCommand(() => AddText("B1")),
                Down = new BaseCommand(Remove),
                Next = new BaseCommand(() => TextBoxTemplate.CaretIndex += 2),
                Previous = new BaseCommand(() => TextBoxTemplate.CaretIndex -= 2),
            };

            DataContext = VM;
        }

        private void CloseWithResult(bool dialogResult)
        {
            if (dialogResult)
            {
                OnResult?.Invoke(this, VM.InputText);
            }

            //DialogResult = dialogResult;
            Close();
        }
    }
}
