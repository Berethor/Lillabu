using System;
using System.Collections.Generic;
using System.Text;
using LilaApp.Models;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    public class RouteSide
    {
        public Point StartPoint;
        public Point EndPoint;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public List<Point> details;
        /// <summary>
        /// Список доступных расширений и их связи для куска маршрута
        /// </summary>
        public List<(string command, RouteSide, bool action)> actions;

        public RouteSide(Point routeStartPoint, int index)
        {
            StartPoint = routeStartPoint;
            StartIndex = index;
            EndPoint = routeStartPoint;
            EndIndex = index;
            actions = new List<(string command, RouteSide, bool action)>();
            details = new List<Point>();
        }
        public RouteSide(Point routeStartPoint, Point routeEndPoint,int startI, int endI)
        {
            StartPoint = routeStartPoint;
            StartIndex = startI;
            EndPoint = routeEndPoint;
            EndIndex = endI;
            actions = new List<(string command, RouteSide, bool action)> ();
            details = new List<Point>();
        }
        public void AddShiftToDetails(Point shift, int startIndex)
        {
            for (int i = startIndex; i < details.Count; i++)
            {
                var a = details[i];
                a.X += shift.X;
                a.Y += shift.Y;
                details[i] = a;
            }
        }
    }
}
