using System;
using System.Windows.Input;

namespace Lilabu.ViewModels
{
    public class JoystickViewModel: ABaseViewModel
    {
        public BaseCommand Up { get; set; }
        public BaseCommand SmallUp { get; set; }
        public BaseCommand Down { get; set; }
        public BaseCommand Left { get; set; }
        public BaseCommand Right { get; set; }
        public BaseCommand Previous { get; set; }
        public BaseCommand Next { get; set; }

        public event EventHandler<JoystickEventArg> OnKeyPress;

        public JoystickViewModel()
        {
            Up = new BaseCommand(() =>
            {
                OnKeyPress?.Invoke(this, new JoystickEventArg { Key = JoystickKey.Up });
            });

            SmallUp = new BaseCommand(() =>
            {
                OnKeyPress?.Invoke(this, new JoystickEventArg { Key =  JoystickKey.SmallUp } );
            });

            Down = new BaseCommand(() =>
            {
                OnKeyPress?.Invoke(this, new JoystickEventArg { Key = JoystickKey.Down });
            });

            Left = new BaseCommand(() =>
            {
                OnKeyPress?.Invoke(this, new JoystickEventArg { Key = JoystickKey.Left });
            });

            Right = new BaseCommand(() =>
            {
                OnKeyPress?.Invoke(this, new JoystickEventArg { Key = JoystickKey.Right });
            });

            Previous = new BaseCommand(() =>
            {
                OnKeyPress?.Invoke(this, new JoystickEventArg { Key = JoystickKey.Previous });
            });

            Next = new BaseCommand(() =>
            {
                OnKeyPress?.Invoke(this, new JoystickEventArg { Key = JoystickKey.Next });
            });
        }
    }

    public struct JoystickEventArg
    {
        public JoystickKey Key { get; set; }
    }

    public enum JoystickKey
    {
        Up,
        SmallUp,
        Down,
        Left,
        Right,
        Previous,
        Next
    }

}
