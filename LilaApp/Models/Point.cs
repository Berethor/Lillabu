using System;
using System.Diagnostics;

namespace LilaApp.Models
{
    [DebuggerDisplay("{StringValue}")]
    [Serializable]
    public struct Point : ICloneable
    {
        #region Fields

        private double _angle;

        #endregion

        #region Properties

        /// <summary>
        /// Координата Х
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Координата Y
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Угол наклона относительно оси Х. 
        /// Измеряется в радианах, от 0 до 2 * Math.Pi
        /// </summary>
        public double Angle
        {
            get => _angle;
            set
            {
                _angle = value;

                while (_angle < 0)
                {
                    _angle += 2 * Math.PI;
                }
                while (_angle >= 2 * Math.PI)
                {
                    _angle -= 2 * Math.PI;
                }
            }
        }

        /// <summary>
        /// Доход с точки (бизнес-логика).
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Текстовое представление точки
        /// </summary>
        public string StringValue => $"{Math.Round(X, 10)} {Math.Round(Y, 10)} ({Math.Round(Angle, 10)})";

        #endregion

        #region .ctor

        public Point(double x, double y, double angle = 0, double price = 0)
        {
            _angle = 0;

            X = x;
            Y = y;
            _angle = angle;
            Price = price;
        }

        /// <summary>
        /// Нулевая точка
        /// </summary>
        public static Point Zero = new Point(0, 0, 0);

        #endregion

        #region Override of Object

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            var other = (Point)obj;

            const double tolerance = 1E-12;

            return Math.Abs(other.X - X) < tolerance &&
                   Math.Abs(other.Y - Y) < tolerance &&
                   Math.Abs(other.Angle - Angle) < tolerance;
        }

        public override string ToString()
        {
            return $"{StringValue}";
        }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        #endregion
    }
}
