using System;
using LilaApp.Algorithm;
using LilaApp.Models;
using Lilabu.ViewModels;

namespace Lilabu
{
    public class WasdFinalSolver : IFinalTaskSolver
    {
        #region Implementation of IFinalTaskSolver

        /// <summary>
        /// Решить обратную задачу
        /// </summary>
        /// <param name="model">Неполная модель исходных данных (только блоки DATA и ROUTE)</param>
        /// <param name="directTaskSolver">Решатель прямой задачи - для вычисления стоимости</param>
        /// <returns>Полная модель (включая блоки ORDER и TOP)</returns>
        public FinalAnswer Solve(Model model, IDirectTaskSolver directTaskSolver)
        {
            _answer = Model.Copy(model);
            _cursor = 0;

            return new FinalAnswer() { Model = _answer, Price = 0 };
        }

        /// <summary>
        /// Событие для отрисовки каждого шага в процессе решения
        /// </summary>
        public event EventHandler<Model> OnStepEvent;

        #endregion

        public WasdFinalSolver(JoystickViewModel joystick)
        {
            joystick.OnKeyPress += Joystick_OnKeyPress;
        }

        private Model _answer = new Model();
        private int _cursor = 0;

        private void Joystick_OnKeyPress(object sender, JoystickEventArg e)
        {
            switch (e.Key)
            {
                case JoystickKey.Up: AddElement("L3"); break;
                case JoystickKey.SmallUp: AddElement("L1"); break;
                case JoystickKey.Down: RemoveElement(); break;
                case JoystickKey.Left: AddElement("t4"); break;
                case JoystickKey.Right: AddElement("T4"); break;
                case JoystickKey.Next: _cursor = Math.Min(_cursor + 1, _answer.Order.Count); break;
                case JoystickKey.Previous: _cursor = Math.Max(_cursor - 1, 0); break;
            }
        }

        private void RemoveElement()
        {
            if (_answer.Order.Count == 0) return;
            _answer.Order.RemoveAt(Math.Max(_cursor - 1, 0));
            _cursor = Math.Max(_cursor - 1, 0);
            var copy = Model.Copy(_answer);
            copy.FixTopology();
            OnStepEvent?.Invoke(this, copy);
        }

        private void AddElement(string type)
        {
            _answer.Order.Insert(_cursor, type);
            _cursor = Math.Min(_cursor + 1, _answer.Order.Count);
            var copy = Model.Copy(_answer);
            copy.FixTopology();
            OnStepEvent?.Invoke(this, copy);
        }

    }
}
