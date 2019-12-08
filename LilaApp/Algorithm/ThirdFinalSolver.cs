using System;

namespace LilaApp.Algorithm
{
    using Models;
    using Models.Railways;

    /// <summary>
    /// Третий вариант решения обратной задачи.
    /// Основан на вставке элементов в двусвязный список, а также
    /// замене нескольких элементов, объединённых в шаблон, на другой шаблон и т.д.
    /// </summary>
    public class ThirdFinalSolver : IFinalTaskSolver
    {
        #region Implementation of IFinalTaskSolver

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException" />
        public FinalAnswer Solve(Model model, IDirectTaskSolver directTaskSolver)
        {
            _model = model; // ?? throw new ArgumentNullException(nameof(model));

            // Создать стартовую трассу - кольцо
            var head = CreateCircle(RailwayType.TurnRight);

            _answer = ConvertToModel(head);
            OnStepEvent?.Invoke(this, _answer);

            var compass = Compass(head);

            // Первый этап - расширяем кольцо вверх, вниз и в сторону
            for (var i = 0; i < 40; i++)
            {
                // Блок, к которому добавляем L1
                var dest = compass.W;

                // Расширение по сторонам
                if (i < 10) dest = compass.W;
                else if (i < 20) dest = compass.NW;
                else if (i < 30) dest = compass.N;
                else if (i < 40) dest = compass.NE;

                // Чередование расширений по сторонам
                //if (i % 4 == 0) dest = compass.W;
                //else if (i % 4 == 1) dest = compass.NW;
                //else if (i % 4 == 2) dest = compass.N;
                //else if (i % 4 == 3) dest = compass.NE;

                dest.AppendSymmetric(new Railway(RailwayType.Line));

                _answer = ConvertToModel(head);
                OnStepEvent?.Invoke(this, _answer);
            }

            return new FinalAnswer
            {
                Model = _answer,
                Price = directTaskSolver.Solve(_answer),
            };
        }

        /// <inheritdoc />
        public event EventHandler<Model> OnStepEvent;

        #endregion

        #region Fields 

        private Model _model;
        private Model _answer;

        #endregion

        /// <summary>
        ///  Создать стартовую трассу - кольцо
        /// </summary>
        /// <param name="turnDirection">Направление кольца (TurnRight или TurnLeft)</param>
        /// <returns>Указатель на начало трассы - блок L0</returns>
        public static Railway CreateCircle(RailwayType turnDirection = RailwayType.TurnRight)
        {
            // Добавляем блок L0
            var head = new Railway(RailwayType.Line, length: 0) { Start = new Point(0, 0), };

            IRailwayTemplate last = head;
            // Добавляем 8 блоков T4
            for (var i = 0; i < 8; i++)
            {
                last = last.Append(new Railway(turnDirection));
            }
            // Закольцовываем список
            last.Next = head;

            // Связываем симметричные блоки
            for (var i = 0; i < 4; i++)
            {
                head[i].Symmetric = head[i + 4];
                head[i + 4].Symmetric = head[i];
            }

            return head;
        }

        /// <summary>
        /// Указатели на элементы кольца по сторонам света:
        ///        north (север)
        /// west (запад)      east (восток)
        ///         south (юг)
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public static (
            Railway NW, Railway N, Railway NE,
            Railway W, /*       */ Railway E,
            Railway SW, Railway S, Railway SE)
            Compass(Railway head)
        {
            return (
                head[1] as Railway, head[2] as Railway, head[3] as Railway,
                head[0] as Railway, /*               */ head[4] as Railway,
                head[7] as Railway, head[6] as Railway, head[5] as Railway);
        }

        /// <summary>
        /// Преобразовать двусвязный список в модель
        /// </summary>
        /// <param name="head">Указатель на начало списка</param>
        /// <returns></returns>
        public Model ConvertToModel(Railway head)
        {
            // Копируем исходные данные
            var model = new Model(_model);

            var k = 1;
            var template = head.Next;
            do
            {
                // Разбиваем шаблон железной дороги на отдельные блоки
                var railways = template.GetRailways();
                for (var i = 0; i < railways.Count; i++)
                {
                    var railway = railways[i];

                    var source = k + i - 1;
                    var destination = railway.Next.IsHead() ? 0 : k + i;

                    // Добавляем каждый блок в модель
                    model.Order.Add(railway.Name);
                    var topology = new TopologyItem(source, destination, railway.Direction);
                    model.Topology.Add(topology);
                }

                template = template.Next;
                k+= railways.Count;
            }
            while (!template.IsHead());

            return model;
        }
    }
}
