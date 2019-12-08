using System;
using System.Collections.Generic;
using System.Linq;
using LilaApp.Models;

namespace LilaApp.Algorithm
{
    /// <summary>
    /// Первый способ решения обратной задачи
    /// </summary>
    public class FirstFinalSolver : IFinalTaskSolver
    {
        #region Implementation of IFinalTaskSolver

        /// <inheritdoc />
        public FinalAnswer Solve(Model model, IDirectTaskSolver checker)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _checker = checker;

            _answer = Model.Copy(_model);

            Run();

            return new FinalAnswer()
            {
                Model = _answer,
                Price = 0,
            };
        }

        /// <inheritdoc />
        public event EventHandler<Model> OnStepEvent;

        #endregion

        private Model _model;
        private Model _answer;
        private IDirectTaskSolver _checker;

        private void Run()
        {
            var points = _answer.Points;

            const double epsilon = Constants.RADIUS * 2 / 3;

            var k = 0; // Индекс текущей точки
            var point = points[k];

            // Список посещённых точек
            var visited = new Dictionary<int, Point>();
            visited[k] = point;

            // Планируемая трасса
            var trace = new List<(string name, Point point, int direction)>();
            trace.Add(("L0", new Point(0, 0, 0), 1));

            int FindNearest()
            {
                var nearest = -1; // Индекс ближайшей соседней точки
                for (var i = 0; i < _answer.Distances[k].Length; i++)
                {
                    if (visited.ContainsKey(i) || point.Equals(points[i])) continue;
                    if (nearest == -1 || _answer.Distances[k][i] < _answer.Distances[k][nearest]) nearest = i;
                }

                return nearest;
            }

            var mi = FindNearest();
            var aim = points[mi];

            while (true)
            {
                var (_, last, _) = trace[trace.Count - 1];

                // Список возможных вариантов
                var stepVariants = new List<(string name, int direction)>
                {
                    ("T8", -1),
                    ("T4", -1),
                    ("L1", 1),
                    ("L2", 1),
                    ("L3", 1),
                    ("T4", +1),
                    ("T8", +1),
                };

                var variants = stepVariants.Select(variant =>
                {
                    var nextPoint = TraceBuilder.MakeStep(last, variant.name, variant.direction);
                    return (variant, point: nextPoint, distance: MathFunctions.GetDistanceToPoint(aim, nextPoint));
                }).ToList();

                var lastDistance = MathFunctions.GetDistanceToPoint(aim, last);

                // Выбираем наилучший вариант
                var best = variants[0];
                for (var i = 1; i < variants.Count; i++)
                    if (variants[i].distance < best.distance && variants[i].distance < lastDistance)
                        best = variants[i];

                // Добавляем его к маршруту
                trace.Add((best.variant.name, best.point, best.variant.direction));

                _answer.Order.Add(best.variant.name);
                _answer.Topology.Add(new TopologyItem(trace.Count - 2, trace.Count - 1, best.variant.direction));
                OnStepEvent?.Invoke(this, _answer);

                // Если приблизились достаточно близко к целевой точке - идём к следующей
                if (MathFunctions.GetDistanceToPoint(aim, best.point) < epsilon)
                {
                    k = mi;
                    point = points[k];
                    visited[k] = point;
                    mi = FindNearest();
                    if (mi == -1)
                    {
                        if (k == 0) break;
                        mi = 0;
                    }
                    aim = points[mi];
                }
            }

        }
    }
}
