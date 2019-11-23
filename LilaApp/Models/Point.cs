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

        public double Price { get; set; }

        public Point(double x, double y, double angle = 0, double price = 0)
        {
            X = x;
            Y = y;
            Angle = angle;
            Price = price;
        }

        public string StringValue => $"{Math.Round(X, 10)} {Math.Round(Y, 10)} ({Math.Round(Angle, 10)})";

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            var other = (Point)obj;

            return
                Math.Round(other.X, 12) == Math.Round(this.X, 12) &&
                Math.Round(other.Y, 12) == Math.Round(this.Y, 12) &&
                Math.Round(other.Angle, 12) == Math.Round(this.Angle, 12);
        }

        public override string ToString()
        {
            return $"{StringValue}";
        }
    }
}
