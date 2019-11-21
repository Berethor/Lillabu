using System;

using LilaApp.Models;
using static LilaApp.Algorithm.TraceBuilder;

namespace LilaApp.Algorithm
{
    public static class DirectTaskSolver
    {
        public static double GetRoutePrice(Model taskModel, Point[] detailsPoints)
        {
            double routePrice = 0;

            foreach (var waypoint in taskModel.Points)
            {
                double waypointPrice = MathFunctions.GetWaypointPrice(detailsPoints[0], waypoint);

                foreach(var detailPoint in detailsPoints)
                {
                    var price = MathFunctions.GetWaypointPrice(detailPoint, waypoint);

                    if (waypointPrice > price)
                    {
                        waypointPrice = price;
                    }
                }

                routePrice += waypointPrice;
            }

            return routePrice;
        }
    }
}
