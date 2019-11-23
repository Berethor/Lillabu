using System;

namespace LilaApp.Algorithm
{
    using Models;

    public static class DirectTaskSolver
    {
        public static double GetRoutePrice(Model taskModel, Point[] detailsPoints)
        {
            var routePrice = 0.0;

            foreach (var waypoint in taskModel.Points)
            {
                var waypointIncome = MathFunctions.GetWaypointIncome(detailsPoints[0], waypoint);

                foreach (var detailPoint in detailsPoints)
                {
                    var income = MathFunctions.GetWaypointIncome(detailPoint, waypoint);

                    if (income > waypointIncome)
                    {
                        waypointIncome = income;
                    }
                }

                routePrice += waypointIncome;
            }

            return Math.Round(routePrice, 8);
        }
    }
}
