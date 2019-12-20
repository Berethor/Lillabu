using System;
using System.Windows;

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
        public BaseCommand SwapCursor { get; set; }
        public BaseCommand InsertTemplate { get; set; }

        public event EventHandler<JoystickEventArg> OnKeyPress;

        public void InvokeKeyPress(object sender, JoystickEventArg e)
        {
            OnKeyPress?.Invoke(sender, e);
        }

        public JoystickViewModel()
        {
            Up = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.Up)));
            SmallUp = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.SmallUp)));
            Down = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.Down)));
            Left = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.Left)));
            SmallLeft = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.SmallLeft)));
            Right = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.Right)));
            SmallRight = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.SmallRight)));
            Bridge = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.Bridge)));
            Previous = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.Previous)));
            LargePrev = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.LargePrev)));
            Next = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.Next)));
            LargeNext = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.LargeNext)));
            ChangeCursor = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.ChangeCursor)));
            SyncCursor = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.SyncCursor)));
            SwapCursor = new BaseCommand(() => OnKeyPress?.Invoke(this, new JoystickEventArg(JoystickKey.SwapCursor)));

        }
    }

    public struct JoystickEventArg
    {
        public JoystickEventArg(JoystickKey key)
        {
            Key = key;
            Template = null;
        }

        public JoystickKey Key { get; set; }

        public string Template { get; set; }
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
        SwapCursor,
        InsertTemplate,
    }

}
