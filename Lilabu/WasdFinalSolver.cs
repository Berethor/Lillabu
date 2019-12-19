using System;
using System.Threading;
using LilaApp.Algorithm;
using LilaApp.Models;
using LilaApp.Models.Railways;
using Lilabu.ViewModels;

namespace Lilabu
{
    public class WasdFinalSolver : IFinalTaskSolver, IDrawableContextProvider
    {
        #region Implementation of IFinalTaskSolver

        /// <summary>
        /// Решить обратную задачу
        /// </summary>
        /// <param name="model">Неполная модель исходных данных (только блоки DATA и ROUTE)</param>
        /// <param name="checker">Алгоритм решения прямой задачи - для вычисления стоимости</param>
        /// <returns>Полная модель (включая блоки ORDER и TOP)</returns>
        public FinalAnswer Solve(Model model, IDirectTaskSolver checker)
        {
            _model = model;
            _answer = Model.Copy(model);
            _checker = checker;

            _chain = new Railway(RailwayType.L0);
            _current = _chain;

            Context = new DrawableContext();

            foreach (var item in _model.Topology)
            {
                if (item.SecondBlock == 0) continue;

                var block = _model.Order[item.SecondBlock - 1];
                if (item.Direction == -1) block = block.ToLower();
                Add(block);
            }

            Context.BotsRating = "Управление:\n" +
                                 "W = Add L3\n" +
                                 "A = Add 2xT4L\n" +
                                 "S = Remove\n" +
                                 "D = Add 2xT4R\n" +
                                 "Q = Prev item\n" +
                                 "E = Next item\n" +
                                 "Shift+W = L1\n" +
                                 "Shift+A = T4L\n" +
                                 "Shift+D = T4R\n";

            SendAnswer();

            while (!Token.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }

            _answer.Blocks.Clear();
            _answer.Blocks.AddRange(_model.Blocks);
            
            Context = new DrawableContext();
            SendAnswer();

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

        private IRailwayTemplate _chain;
        private IRailwayTemplate _current;
        private Model _answer;
        private Model _model;

        private void Joystick_OnKeyPress(object sender, JoystickEventArg e)
        {
            Context.ErrorMessage = null;

            switch (e.Key)
            {
                case JoystickKey.Up: Add("L3"); break;
                case JoystickKey.SmallUp: Add("L1"); break;
                case JoystickKey.Down: Remove(); break;
                case JoystickKey.Left: Add("t2"); break;
                case JoystickKey.SmallLeft: Add("t4"); break;
                case JoystickKey.Right: Add("T2"); break;
                case JoystickKey.SmallRight: Add("T4"); break;
                case JoystickKey.Next: Next(); break;
                case JoystickKey.Previous: Prev(); break;
            }

            SendAnswer();
        }

        private void Add(string blueprint)
        {
            if (RailwayFactory.Default.TryBuildTemplate(out var template, out var error, blueprint, _answer))
            {
                _current.Append(template);

                Next();
            }
            else
            {
                Context.ErrorMessage = error;
            }
        }

        private void Remove()
        {
            if (_current is Railway railway && railway.Type == RailwayType.L0) return;

            // Возвращаем блоки в список доступных
            _current.ReturnBlocksToModel(_answer);

            if (_current.Prev != null) _current.Prev.Next = _current.Next;
            if (_current.Next != null)
            {
                _current.Next.Prev = _current.Prev;
                _current.Next.Start = _current.Prev?.End ?? Point.Zero;
            }

            _current = _current.Prev;
        }

        private void Next()
        {
            if (_current?.Next == null) return;

            _current = _current.Next;
        }

        private void Prev()
        {
            if (_current is Railway railway && railway.Type == RailwayType.L0) return;

            _current = _current.Prev;
        }

        private void SendAnswer()
        {
            _answer = _chain.ConvertToModel(_answer);

            Context.CursorPoint = _current.End;

            OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));
        }

        #region Implementation of IDrawableContextProvider

        public DrawableContext Context { get; set; } = new DrawableContext();

        #endregion
    }
}
