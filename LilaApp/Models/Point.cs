using System;
using System.Diagnostics;

namespace LilaApp.Models
{
    [DebuggerDisplay("{StringValue}")]
    public struct Point
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Angle { get; set; }
        
        public Point(double x, double y, double angle = 0)
        {
            X = x;
            Y = y;
            Angle = angle;
        }

        public string StringValue => $"{Math.Round(X, 10)} {Math.Round(Y, 10)} ({Math.Round(Angle, 10)})";
    }
}
