using System;

using LilaApp.Models;

namespace LilaApp.Algorithm
{
    public static class MathFunctions
    {
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

            var pointVector = new Point()
            {
                X = point.X - center.X,
                Y = point.Y - center.Y
            };
            var turnStartVector = new Point()
            {
                X = startPoint.X - center.X,
                Y = startPoint.Y - center.Y
            };
            var turnEndVvector = new Point()
            {
                X = endPoint.X - center.X,
                Y = endPoint.Y - center.Y
            };

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
            return new Point()
            {
                X = point.X * Math.Cos(angle) + point.Y * Math.Sin(angle),
                Y = point.Y * Math.Cos(angle) - point.X * Math.Sin(angle)
            };
        }
    }
}
