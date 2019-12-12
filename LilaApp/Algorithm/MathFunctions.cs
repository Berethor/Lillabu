using System;
using System.Collections.Generic;
using LilaApp.Models;

namespace LilaApp.Algorithm
{
    public static class MathFunctions
    {
        /// <summary>
        /// Прибыль с точки маршрута
        /// </summary>
        /// <param name="double">Расстояние до точки</param>
        /// <param name="price">Прибыль с точки</param>
        /// <returns></returns>
        public static double GetWaypointIncome(double length, double price)
        {
            return price / (1 + length);
        }

        public static bool CheckSegment(Point point, Point startPoint, Point endPoint)
        {
            var t = ((point.X - startPoint.X) * (endPoint.X - startPoint.X) +
                     (point.Y - startPoint.Y) * (endPoint.Y - startPoint.Y)) /
                   (Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));

            var length = Math.Sqrt(
                Math.Pow(startPoint.X - point.X + (endPoint.X - startPoint.X) * t, 2) +
                Math.Pow(startPoint.Y - point.Y + (endPoint.Y - startPoint.Y) * t, 2));

            if(length < 0.1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CheckTurn(Point point, Point center, Point startPoint, Point endPoint,double angle, double r)
        {
            var distanceToCircle = Math.Abs(
                r - Math.Sqrt(
                    Math.Pow(center.X - point.X, 2) +
                    Math.Pow(center.Y - point.Y, 2)));

            var pointVector = new Point(x: point.X - center.X, y: point.Y - center.Y);
            var turnStartVector = new Point(x: startPoint.X - center.X, y: startPoint.Y - center.Y);
            var turnEndVvector = new Point(x: endPoint.X - center.X, y: endPoint.Y - center.Y);

            var isInSector = GetAngleBetweenVectors(pointVector, turnStartVector) < angle &&
                             GetAngleBetweenVectors(pointVector, turnEndVvector)  < angle;

            if (isInSector && distanceToCircle < 0.1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static double GetAngleBetweenVectors(Point firstVector, Point secondVector)
        {
            return Math.Acos((firstVector.X * secondVector.X + firstVector.Y * secondVector.Y) /
                                  (VectorLength(firstVector) * VectorLength(secondVector)));
        }
        public static double VectorLength(Point vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
        }
        public static Point RotateCoordinates(double angle, Point point)
        {
            return new Point(
                x: point.X * Math.Cos(angle) + point.Y * Math.Sin(angle),
                y: point.Y * Math.Cos(angle) - point.X * Math.Sin(angle),
                point.Angle - angle
            );
        }
        public static double GetDistanceToPoint(Point detailPoint, Point waypoint)
        {
            return Math.Sqrt(
                Math.Pow(waypoint.X - detailPoint.X, 2) +
                Math.Pow(waypoint.Y - detailPoint.Y, 2));
        }

        /// <summary>
        /// Рассчитать расстояние между всеми точками
        /// </summary>
        /// <param name="routes">Список точек</param>
        /// <returns>Матрица N*N расстояний между i и j точкой</returns>
        public static double[][] GetDistanсeBetweenAllPoints(List<Point> routes)
        {
            var points = routes?.ToArray();
            var n = points.Length;
            var distance = new double[n][];
            for (var i = 0; i < n; i++)
            {
                distance[i] = new double[n];
            }

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        distance[i][j] = 0;
                        continue;
                    }

                    distance[i][j] = distance[j][i] = MathFunctions.GetDistanceToPoint(points[i], points[j]);
                }
            }

            return distance;
        }
    }
}
