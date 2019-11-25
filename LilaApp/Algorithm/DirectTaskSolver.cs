using System;
using System.Linq;

namespace LilaApp.Algorithm
{
    using Models;

    public class DirectTaskSolver : IDirectTaskSolver
    {
        public static double GetRoutePrice(Model taskModel, Point[] detailsPoints)
        {
            var routePrice = 0.0;

            foreach (var waypoint in taskModel?.Points)
            {
                var length = detailsPoints.Min(detailPoint => MathFunctions.GetDistanceToPoint(detailPoint, waypoint));

                var income = MathFunctions.GetWaypointIncome(length, waypoint.Price);

                routePrice += income;
            }

            return Math.Round(routePrice, 10);
        }

        #region Implementation of IDirectTaskSolver

        /// <inheritdoc />
        public double Solve(Model model)
        {
            var trace = TraceBuilder.CalculateTrace(model);

            var prcie = GetRoutePrice(model, trace.Points);

            return prcie;
        }

        #endregion
    }
}
