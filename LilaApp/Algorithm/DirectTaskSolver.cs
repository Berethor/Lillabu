using System;
using System.Linq;

namespace LilaApp.Algorithm
{
    using Models;

    public class DirectTaskSolver : IDirectTaskSolver
    {
        public static double GetRouteIncome(Model taskModel, Point[] detailsPoints)
        {
            var routeIncome = 0.0;

            if (taskModel == null) return routeIncome;

            foreach (var wayPoint in taskModel.Points)
            {
                var length = detailsPoints.Min(detailPoint => MathFunctions.GetDistanceToPoint(detailPoint, wayPoint));

                var income = MathFunctions.GetWaypointIncome(length, wayPoint.Price);

                routeIncome += income;
            }

            return Math.Round(routeIncome, 10);
        }

        #region Implementation of IDirectTaskSolver

        /// <summary>
        /// Решить прямую задачу
        /// </summary>
        /// <param name="model">Полная модель</param>
        /// <returns>Прибыль с точек, цена блоков маршрута, результат = прибыль - цена блоков</returns>
        public TracePrice Solve(Model model)
        {
            var trace = TraceBuilder.CalculateTrace(model);

            var income = GetRouteIncome(model, trace.Points);

            return new TracePrice(income, trace.Price);
        }

        #endregion
    }
}
