using System;
using System.Linq;
using System.Threading;
using LilaApp.Generator;

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
        public FinalAnswer Solve(Model model, IDirectTaskSolver checker)
        {
            _model = model; // ?? throw new ArgumentNullException(nameof(model));

            var factory = new RailwayFactory();

            // Разброс точек маршрута
            var routesSize = GetRoutesSize(model);

            // Создать стартовую трассу - кольцо
            var cycle = RailwayTemplates.CreateCircle(RailwayType.T4R);

            // Функция отображения модели
            void DisplayStep(IRailwayTemplate t)
            {
                _answer = t.ConvertToModel(_model);
                OnStepEvent?.Invoke(this, new FinalAnswer(_answer, checker.Solve(_answer)));
            }

            DisplayStep(cycle);

            var compass = RailwayTemplates.Compass(cycle);


            var l = 2;
            IRailwayTemplate head = cycle;
            for (var k = 0; k < 2; k++)
            {
                // Left
                for (var i = 0; i < l; i++)
                {
                    if (!head.TryScale(Direction.E, Railway.L3)) break;
                    DisplayStep(cycle);
                }

                // Up
                var chain1 = factory.BuildTemplate("t2T1L6", model);
                head.TryMutate(chain1);
                DisplayStep(cycle);

                // Up
                for (var i = 0; i < l; i++)
                {
                    if (!chain1.TryScale(Direction.E, Railway.L3)) break;
                    DisplayStep(cycle);
                }

                // Right
                var chain2 = factory.BuildTemplate("t2T1L6", model);
                chain1.TryMutate(chain2);
                DisplayStep(cycle);

                l += 3;

                // Right
                for (var i = 0; i < l; i++)
                {
                    if (!chain2.TryScale(Direction.W, Railway.L3)) break;
                    DisplayStep(cycle);
                }

                // Down
                var chain3 = factory.BuildTemplate("t2T1L6", model);
                chain2.TryMutate(chain3);
                DisplayStep(cycle);

                // Down
                for (var i = 0; i < l; i++)
                {
                    if (!chain3.TryScale(Direction.S, Railway.L3)) break;
                    DisplayStep(cycle);
                }

                // Right
                var chain4 = factory.BuildTemplate("t2T1L6", model);
                chain3.TryMutate(chain4);
                DisplayStep(cycle);

                head = chain4;
                l += 3;
            }


            // Первый этап - расширяем кольцо вверх, вниз и в сторону
            /*
            const int count = 16;
            for (var i = 0; i < count; i++)
            {
                // Блок, к которому добавляем L1
                var dest = compass.W;

                // Расширение по сторонам
                if (i < count / 4) dest = compass.W;
                //else if (i < 2 * count / 4) dest = compass.NW;
                else if (i < 3 * count / 4) dest = compass.N;
                //else if (i < 4 * count / 4) dest = compass.NE;

                // Чередование расширений по сторонам
                //if (i % 4 == 0) dest = compass.W;
                //else if (i % 4 == 1) dest = compass.NW;
                //else if (i % 4 == 2) dest = compass.N;
                //else if (i % 4 == 3) dest = compass.NE;

                var chain = new RailwayChain(new[]
                {
                    new Railway(RailwayType.T8L),
                    new Railway(RailwayType.T8R),
                });
                //dest.AppendSymmetric(chain);

                dest.AppendSymmetric(Railway.L1);
                
                DisplayStep(cycle);
            }
            */

            return new FinalAnswer
            {
                Model = _answer,
                Price = checker.Solve(_answer),
            };
        }

        /// <inheritdoc />
        public CancellationToken Token { get; set; }

        /// <inheritdoc />
        public event EventHandler<FinalAnswer> OnStepEvent;

        #endregion

        #region Fields 

        private Model _model;
        private Model _answer;

        #endregion

        private (Range<double> X, Range<double> Y) GetRoutesSize(Model model)
        {
            if (model.Points.Count == 0)
            {
                var infinity = new Range<double>(double.NegativeInfinity, double.PositiveInfinity);
                return (infinity, infinity);
            }

            var xPoints = model.Points.Select(p => p.X).ToArray();
            var yPoints = model.Points.Select(p => p.Y).ToArray();
            var xRange = new Range<double>(xPoints.Min(), xPoints.Max());
            var yRange = new Range<double>(yPoints.Min(), yPoints.Max());

            return (xRange, yRange);
        }

    }
}
