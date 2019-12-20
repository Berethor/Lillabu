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
            _current1 = _chain;
            _current2 = _chain;

            Context = new DrawableContext();

            foreach (var item in _model.Topology)
            {
                if (item.SecondBlock == 0) continue;

                var block = _model.Order[item.SecondBlock - 1];
                if (item.Direction == -1 && block.StartsWith("T")) block = block.ToLower();

                _current1.Append(Railway.From(block));
                Next();
            }

            _current2 = _current1;

            Context.BotsRating = "Управление:\n" +
                                 "W = Add L3\n" +
                                 "A = Add 2xT4L\n" +
                                 "S = Remove\n" +
                                 "D = Add 2xT4R\n" +
                                 "B = Add Bridge\n" +
                                 "Q = Prev item\n" +
                                 "E = Next item\n" +
                                 "Shift+W = L1\n" +
                                 "Shift+A = T4L\n" +
                                 "Shift+D = T4R\n" +
                                 "C = Cursors";

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
        private IRailwayTemplate _current1;
        private IRailwayTemplate _current2;
        private int _cursorMode = 0;
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
                case JoystickKey.Bridge: Add("B1"); break;
                case JoystickKey.Next: Next(); break;
                case JoystickKey.Previous: Prev(); break;
                case JoystickKey.ChangeCursor: ChangeCursor(); break;
            }

            SendAnswer();
        }

        private void Add(string blueprint)
        {
            if (Cursor1Enabled)
            {
                if (RailwayFactory.Default.TryBuildTemplate(out var template, out var error, blueprint, _answer))
                {
                    _current1.Append(template);

                    if (_current1?.Next != null)
                    {
                        _current1 = _current1.Next;
                    }
                }
                else
                {
                    Context.ErrorMessage = error;
                }
            }

            if (Cursor2Enabled)
            {
                if (RailwayFactory.Default.TryBuildTemplate(out var template, out var error, blueprint, _answer))
                {
                    _current2.Append(template);

                    if (_current2?.Next != null)
                    {
                        _current2 = _current2.Next;
                    }
                }
                else
                {
                    Context.ErrorMessage = error;
                }
            }
        }

        private void Remove()
        {
            if (Cursor1Enabled)
            {
                if (!(_current1 is Railway railway && railway.Type == RailwayType.L0))
                {
                    // Возвращаем блоки в список доступных
                    _current1.ReturnBlocksToModel(_answer);

                    if (_current1.Prev != null) _current1.Prev.Next = _current1.Next;
                    if (_current1.Next != null)
                    {
                        _current1.Next.Prev = _current1.Prev;
                        _current1.Next.Start = _current1.Prev?.End ?? Point.Zero;
                    }

                    _current1 = _current1.Prev;
                }
            }
            if (Cursor2Enabled)
            {
                if (!(_current2 is Railway railway && railway.Type == RailwayType.L0))
                {
                    // Возвращаем блоки в список доступных
                    _current2.ReturnBlocksToModel(_answer);

                    if (_current2.Prev != null) _current2.Prev.Next = _current2.Next;
                    if (_current2.Next != null)
                    {
                        _current2.Next.Prev = _current2.Prev;
                        _current2.Next.Start = _current2.Prev?.End ?? Point.Zero;
                    }

                    _current2 = _current2.Prev;
                }
            }
        }

        private void Next()
        {
            if (Cursor1Enabled)
            {
                if (_current1?.Next != null)
                {
                    _current1 = _current1.Next;
                }
            }
            if (Cursor2Enabled)
            {
                if (_current2?.Next != null)
                {
                    _current2 = _current2.Next;
                }
            }
        }

        private void Prev()
        {
            if (Cursor1Enabled)
            {
                if (!(_current1 is Railway railway && railway.Type == RailwayType.L0))
                {
                    _current1 = _current1.Prev;
                }
            }
            if (Cursor2Enabled)
            {
                if (!(_current2 is Railway railway && railway.Type == RailwayType.L0))
                {
                    _current2 = _current2.Prev;
                }
            }
        }

        private void SendAnswer()
        {
            _answer = _chain.ConvertToModel(_answer);
            
            var p1 = _current1.End;
            Context.Cursor1Point = new Point(p1.X, p1.Y, p1.Angle, Cursor1Enabled ? 1 : 0);

            var p2 = _current2.End;
            Context.Cursor2Point = new Point(p2.X, p2.Y, p2.Angle, Cursor2Enabled ? 1 : 0);
            
            OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));
        }

        private void ChangeCursor()
        {
            _cursorMode = (_cursorMode + 1) % 3; 

            var p1 = Context.Cursor1Point;
            Context.Cursor1Point = new Point(p1.X, p1.Y, p1.Angle, Cursor1Enabled ? 1 : 0);

            var p2 = Context.Cursor2Point;
            Context.Cursor2Point = new Point(p2.X, p2.Y, p2.Angle, Cursor2Enabled ? 1 : 0);
        }

        private bool Cursor1Enabled => _cursorMode == 0 || _cursorMode == 2;
        private bool Cursor2Enabled => _cursorMode == 1 || _cursorMode == 2;

        #region Implementation of IDrawableContextProvider

        public DrawableContext Context { get; set; } = new DrawableContext();

        #endregion
    }
}
