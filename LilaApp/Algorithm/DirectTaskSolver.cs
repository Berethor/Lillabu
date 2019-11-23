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
                var waypointLength = MathFunctions.GetDistanceToPoint(detailsPoints[0], waypoint);

                foreach (var detailPoint in detailsPoints)
                {
                    var length = MathFunctions.GetDistanceToPoint(detailPoint, waypoint);

                    if (length < waypointLength)
                    {
                        waypointLength = length;
                    }
                }

                var income = MathFunctions.GetWaypointIncome(waypointLength, waypoint.Price);
                routePrice += income;
            }

            return Math.Round(routePrice, 8);
        }
    }
}
