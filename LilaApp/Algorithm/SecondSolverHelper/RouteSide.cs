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
        }
        public RouteSide(Point routeStartPoint, Point routeEndPoint,int startI, int endI)
        {
            StartPoint = routeStartPoint;
            StartIndex = startI;
            EndPoint = routeEndPoint;
            EndIndex = endI;
            actions = new List<(string command, RouteSide, bool action)> ();
        }
    }
}
