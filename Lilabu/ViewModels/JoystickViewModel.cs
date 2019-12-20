using System;

namespace Lilabu.ViewModels
{
    public class JoystickViewModel : ABaseViewModel
    {
        public BaseCommand Up { get; set; }
        public BaseCommand SmallUp { get; set; }
        public BaseCommand Down { get; set; }
        public BaseCommand Left { get; set; }
        public BaseCommand SmallLeft { get; set; }
        public BaseCommand Right { get; set; }
        public BaseCommand SmallRight { get; set; }
        public BaseCommand Bridge { get; set; }
        public BaseCommand Previous { get; set; }
        public BaseCommand LargePrev { get; set; }
        public BaseCommand Next { get; set; }
        public BaseCommand LargeNext { get; set; }
        public BaseCommand ChangeCursor { get; set; }
        public BaseCommand SyncCursor { get; set; }

        public event EventHandler<JoystickKey> OnKeyPress;

        public JoystickViewModel()
        {
            Up = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.Up));
            SmallUp = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.SmallUp));
            Down = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.Down));
            Left = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.Left));
            SmallLeft = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.SmallLeft));
            Right = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.Right));
            SmallRight = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.SmallRight));
            Bridge = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.Bridge));
            Previous = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.Previous));
            LargePrev = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.LargePrev));
            Next = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.Next));
            LargeNext = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.LargeNext));
            ChangeCursor = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.ChangeCursor));
            SyncCursor = new BaseCommand(() => OnKeyPress?.Invoke(this, JoystickKey.SyncCursor));
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
        SmallLeft,
        Right,
        SmallRight,
        Bridge,
        Previous,
        LargePrev,
        Next,
        LargeNext,
        ChangeCursor,
        SyncCursor,
    }

}
