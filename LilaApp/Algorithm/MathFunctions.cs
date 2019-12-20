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


        /// <summary>
        /// Векторное произведение
        /// </summary>
        public static double VectorMultiply(double ax, double ay, double bx, double by)
        {
            return ax * by - bx * ay;
        }
        /// <summary>
        /// Пересекаются ли два отрезка
        /// </summary>
        public static bool AreCrossing(Point p1, Point p2, Point p3, Point p4)//проверка пересечения
        {
            if (p1 == p3 || p1 == p4 || p2 == p4) return true;

            var v1 = VectorMultiply(p4.X - p3.X, p4.Y - p3.Y, p1.X - p3.X, p1.Y - p3.Y);
            var v2 = VectorMultiply(p4.X - p3.X, p4.Y - p3.Y, p2.X - p3.X, p2.Y - p3.Y);
            var v3 = VectorMultiply(p2.X - p1.X, p2.Y - p1.Y, p3.X - p1.X, p3.Y - p1.Y);
            var v4 = VectorMultiply(p2.X - p1.X, p2.Y - p1.Y, p4.X - p1.X, p4.Y - p1.Y);

            return ((v1 * v2) < 0 && (v3 * v4) < 0);
        }

        /// <summary>
        /// Точка пересечения двух отрезков
        /// </summary>
        /// <returns> null, если отрезки не пересекаются </returns>
        public static Point? CrossPoint(Point p1, Point p2, Point p3, Point p4)
        {
            if (AreCrossing(p1, p2, p3, p4))
            {
                var (a1, b1, c1) = LineEquation(p1, p2);
                var (a2, b2, c2) = LineEquation(p3, p4);
                var p = CrossingPoint(a1, b1, c1, a2, b2, c2);

                return p;
            }

            return null;
        }

        /// <summary>
        /// Построение уравнения прямой
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns> Коэффициенты уравнения прямой вида: Ax+By+C=0 </returns>
        public static (double A, double B, double C) LineEquation(Point p1, Point p2)
        {
            var a = p2.Y - p1.Y;
            var b = p1.X - p2.X;
            var c = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);

            return (a, b, c);
        }

        private static Point CrossingPoint(double a1, double b1, double c1, double a2, double b2, double c2)
        {
            var d = (a1 * b2 - b1 * a2);
            var dx = (-c1 * b2 + b1 * c2);
            var dy = (-a1 * c2 + c1 * a2);

            return new Point(x: dx / d, y: dy / d);
        }

    }
}
