using System;
using System.Collections.Generic;

namespace LilaApp.Algorithm
{
    using Models;

    /// <summary>
    /// Класс для вычисления координат блоков маршрута
    /// </summary>
    public class TraceBuilder
    {
        /// <summary>
        /// Просчитать трассу модели
        /// </summary>
        /// <param name="model">Полная модель</param>
        public static Result CalculateTrace(Model model)
        {
            var n = model.Topology.Count;

            var points = new Point[n + 1];
            points[0] = new Point(0, 0);

            var forks = new Dictionary<int, Fork>();

            for (var i = 0; i < n; i++)
            {
                var item = model.Topology[i];
                var previos = item.FirstBlock;
                var current = item.SecondBlock;
                var direction = item.Direction;

                // Типы блоков
                var t1 = previos > 0 ? model.Order[previos - 1] : "L0";
                var t2 = current > 0 ? model.Order[current - 1] : "L0";

                // Извлечение начальной точки
                var startPoint = (t1[0] == 'Y')
                    ? forks[previos][direction]
                    : points[previos];

                var angle = startPoint.Angle;

                //Вычисление конечной точки
                var rotatedPoint = Rotate(startPoint, angle);
                var endPoint = startPoint;
                switch (t2[0])
                {
                    case 'L':
                        rotatedPoint.Y += int.Parse(t2.Substring(1));
                        endPoint = Rotate(rotatedPoint, -angle);
                        endPoint.Angle = startPoint.Angle;
                        break;
                    case 'T':
                        var alpha = Constants.T_ANGLES[t2];
                        rotatedPoint.X += direction * Constants.RADIUS * (1 - Math.Cos(alpha));
                        rotatedPoint.Y += Constants.RADIUS * Math.Sin(alpha);
                        endPoint = Rotate(rotatedPoint, -angle);
                        endPoint.Angle = startPoint.Angle - direction * alpha;
                        break;
                    case 'Y':
                        forks[current] = MakeFork(startPoint, angle, direction);
                        endPoint = forks[current].Base;
                        break;
                }

                // Сохранение конечной точки
                points[current] = endPoint;
            }

            // Вычисляем стоимость всех деталей
            double price = 0;
            foreach(var detail in model.Order)
            {
                price += model.Blocks.Find(_ => _.Name == detail).Price;
            }

            return new Result
            {
                Points = points,
                Forks = forks,
                Price = price,
            };
        }

        private static Point Rotate(Point point, double angle)
        {
            return MathFunctions.RotateCoordinates(angle, point);
        }

        private static Point Rotate(double x, double y, double angle)
        {
            return MathFunctions.RotateCoordinates(angle, new Point(x, y));
        }

        private static Fork MakeFork(Point point, double angle, int direction)
        {
            const double Y_LEN = Constants.Y_LENGTH;
            const double Y_ANG = Constants.Y_LENGTH;

            var rotatedPoint = Rotate(point, angle);

            // Координаты центра
            var center = Rotate(new Point(rotatedPoint.X, rotatedPoint.Y + Y_LEN), -angle);
            center.Angle = direction == 0 ? angle : angle + direction * Math.PI - Y_ANG;

            // Вычисляем координаты и углы
            var y_ang = Math.PI / 2 - Y_ANG;
            var rotatedCenter = Rotate(center, center.Angle);
            var forkBase = Rotate(rotatedCenter.X, rotatedCenter.Y - Y_LEN, -center.Angle);
            forkBase.Angle = center.Angle;
            var forkRight = Rotate(rotatedCenter.X + Y_LEN * Math.Cos(y_ang), rotatedCenter.Y + Y_LEN * Math.Sin(y_ang), -center.Angle);
            forkRight.Angle = center.Angle - Y_ANG;
            var forkLeft = Rotate(rotatedCenter.X - Y_LEN * Math.Cos(y_ang), rotatedCenter.Y + Y_LEN * Math.Sin(y_ang), -center.Angle);
            forkLeft.Angle = center.Angle + Y_ANG;

            // fork[direction] = new Point(point.X, point.Y, Math.PI - angle);
            return new Fork
            {
                Base = forkBase,
                Right = forkRight,
                Left = forkLeft,
                Center = center,
            };
        }

        public struct Result
        {
            /// <summary>
            /// Точки и направления блоков
            /// </summary>
            public Point[] Points { get; set; }

            [Obsolete("Устарело в связи с удалением Y блоков")]
            public Dictionary<int, Fork> Forks { get; set; }

            /// <summary>
            /// Стоимость всех деталей
            /// </summary>
            public double Price { get; set; }
            
        }
    }

}
