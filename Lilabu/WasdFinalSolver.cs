using System;
using System.Threading;
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
        /// <param name="checker">Решатель прямой задачи - для вычисления стоимости</param>
        /// <returns>Полная модель (включая блоки ORDER и TOP)</returns>
        public FinalAnswer Solve(Model model, IDirectTaskSolver checker)
        {
            _answer = Model.Copy(model);
            _checker = checker;
            _cursor = 0;

            return new FinalAnswer() { Model = _answer, Price = checker.Solve(_answer) };
        }

        /// <summary>
        /// Токен отмены задачи
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// Событие для отрисовки каждого шага в процессе решения
        /// </summary>
        public event EventHandler<FinalAnswer> OnStepEvent;

        #endregion

        private IDirectTaskSolver _checker;

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
            OnStepEvent?.Invoke(this, new FinalAnswer(copy, _checker.Solve(copy)));
        }

        private void AddElement(string type)
        {
            _answer.Order.Insert(_cursor, type);
            _cursor = Math.Min(_cursor + 1, _answer.Order.Count);
            var copy = Model.Copy(_answer);
            copy.FixTopology();
            OnStepEvent?.Invoke(this, new FinalAnswer(copy, _checker.Solve(copy)));
        }
    }
}
