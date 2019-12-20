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
                                 "W: Add L3\n" +
                                 "A: Add 2xT4L\n" +
                                 "S: Remove\n" +
                                 "D: Add 2xT4R\n" +
                                 "B: Add Bridge\n" +
                                 "Q: Prev item\n" +
                                 "E: Next item\n" +
                                 "Shift+W: L1\n" +
                                 "Shift+A: T4L\n" +
                                 "Shift+D: T4R\n" +
                                 "Z: Sync curs\n" +
                                 "X: Swap curs\n" +
                                 "C: Change mode\n" +
                                 "V: Insert tmpl\n" +
                                 "";

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
            if (_answer == null)
            {
                Context.ErrorMessage = "Алгоритм не запущен";
                
                return;
            }

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
                case JoystickKey.LargeNext: for (var i = 0; i < 10; i++) Next(); break;
                case JoystickKey.Previous: Prev(); break;
                case JoystickKey.LargePrev: for (var i = 0; i < 10; i++) Prev(); break;
                case JoystickKey.ChangeCursor: ChangeCursor(); break;
                case JoystickKey.SyncCursor: SyncCursor(); break;
                case JoystickKey.SwapCursor: SwapCursor(); break;
                case JoystickKey.InsertTemplate: InsertTemplate(e.Template); break;
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
                RemoveAt(_current1);
            }
            if (Cursor2Enabled)
            {
                RemoveAt(_current2);
            }
        }

        private void RemoveAt(IRailwayTemplate cursor)
        {
            if (!(cursor is Railway railway && railway.Type == RailwayType.L0))
            {
                // Возвращаем блоки в список доступных
                cursor.ReturnBlocksToModel(_answer);

                if (cursor.Prev != null) cursor.Prev.Next = cursor.Next;
                if (cursor.Next != null)
                {
                    cursor.Next.Prev = cursor.Prev;
                    cursor.Next.Start = cursor.Prev?.End ?? Point.Zero;
                }

                if (cursor == _current1) _current1 = cursor.Prev;
                else if (cursor == _current2) _current2 = cursor.Prev;
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
            if (_answer == null) return;

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

        private void SyncCursor()
        {
            if (Cursor1Enabled)
            {
                _current2 = _current1;
            }
            else
            {
                _current1 = _current2;
            }
        }

        private void SwapCursor()
        {
            var temp = _current1;
            _current1 = _current2;
            _current2 = temp;
            
            //if (_cursorMode == 0) _cursorMode = 1;
            //else if (_cursorMode == 1) _cursorMode = 0;
        }

        private void InsertTemplate(string blueprint)
        {
            if (blueprint == null)
            {
                Context.ErrorMessage = "Не удалось добавить шаблон";

                return;
            }

            var p = _current1;

            while (p != _current2)
            {
                p = p?.Next;

                if (p == null || (p is Railway railway && railway.Type == RailwayType.L0))
                {
                    Context.ErrorMessage = "Не удалось обнаружить второй курсор дальше по направлению трассы";
                    
                    return;
                }
            }

            if (RailwayFactory.Default.TryBuildTemplate(out var template, out var error, blueprint, _answer))
            {
                // Проверяем, что новая трасса подходит
                // TODO

                // Удаляем старую трассу между курсорами
                while (_current2 != _current1)
                {
                    RemoveAt(_current2);

                    if (_current2 is Railway railway && railway.Type == RailwayType.L0)
                    {
                        // Возвращаем блоки
                        template.ReturnBlocksToModel(_answer);

                        Context.ErrorMessage = "При удалении старого участка трассы возникла ошибка";
                        
                        return;
                    }
                }

                // Добавляем новую трасса 
                foreach (var railway in template.GetRailways())
                {
                    _current2.Append(railway);
                    _current2 = _current2.Next;
                }
            }
            else
            {
                Context.ErrorMessage = error;
            }
        }

        #region Implementation of IDrawableContextProvider

        public DrawableContext Context { get; set; } = new DrawableContext();

        #endregion
    }
}
