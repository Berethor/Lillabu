using System;

using LilaApp.Models;

namespace LilaApp.Algorithm
{
    public static class MathFunctions
    {
        public static double GetDistanceToPoint(Point point, Point waypoint)
        {
            return Math.Sqrt(
                Math.Pow(waypoint.X - point.X, 2) +
                Math.Pow(waypoint.Y - point.Y, 2));
        }

        public static double GetWaypointPrice(double pointImportance, Point point, Point waypoint)
        {
            return pointImportance /
                (1 + GetDistanceToPoint(point, waypoint));
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
                y: point.Y * Math.Cos(angle) - point.X * Math.Sin(angle)
            );
        }
    }
}
