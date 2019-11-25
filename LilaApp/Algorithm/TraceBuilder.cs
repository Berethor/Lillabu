using System;
using System.Collections.Generic;

namespace LilaApp.Algorithm
{
    using Models;
    using System.Linq;

    /// <summary>
    /// Класс для вычисления координат блоков маршрута
    /// </summary>
    public class TraceBuilder
    {
        /// <summary>
        /// Добавить к точке деталь
        /// </summary>
        /// <param name="startPoint">Исходная точка</param>
        /// <param name="item">Название детали</param>
        /// <param name="direction">Направление (-1 - влево, +1 - вправо)</param>
        /// <returns>Новая точка маршрута</returns>
        public static Point MakeStep(Point startPoint, string item, int direction)
        {
            var angle = startPoint.Angle;

            //Вычисление конечной точки
            var rotatedPoint = Rotate(startPoint, angle);
            var endPoint = startPoint;
            switch (item[0])
            {
                case 'L':
                    rotatedPoint.Y += int.Parse(item.Substring(1));
                    endPoint = Rotate(rotatedPoint, -angle);
                    endPoint.Angle = startPoint.Angle;
                    break;
                case 'B':
                    rotatedPoint.Y += 4; // Длина моста - 4
                    endPoint = Rotate(rotatedPoint, -angle);
                    endPoint.Angle = startPoint.Angle;
                    break;
                case 'T':
                    var alpha = Constants.T_ANGLES[item];
                    rotatedPoint.X += direction * Constants.RADIUS * (1 - Math.Cos(alpha));
                    rotatedPoint.Y += Constants.RADIUS * Math.Sin(alpha);
                    endPoint = Rotate(rotatedPoint, -angle);
                    endPoint.Angle = startPoint.Angle - direction * alpha;
                    break;
                case 'Y':
                    // forks[current] = MakeFork(startPoint, angle, direction);
                    // endPoint = forks[current].Base;
                    break;
            }

            if (endPoint.Angle < 0)
            {
                endPoint.Angle += 2 * Math.PI;
            }
            else if (endPoint.Angle >= 2 * Math.PI)
            {
                endPoint.Angle -= 2 * Math.PI;
            }

            return endPoint;
        }

        /// <summary>
        /// Просчитать трассу модели
        /// </summary>
        /// <param name="model">Полная модель</param>
        public static Result CalculateTrace(Model model)
        {
            var exceptions = new List<Exception>();

            var n = model.Topology.Count;

            var points = new Point?[n+1];
            points[0] = new Point(0, 0);

            // var forks = new Dictionary<int, Fork>();

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

                //var startPoint = t1[0] == 'Y')
                //    ? forks[previos][direction]
                //    : points[previos];

                if (points[previos] == null)
                {
                    exceptions.Add(new Exception($"Ошибка в {i + 1} строке раздела TOP: \"{item}\". Нет информации о точке начала блока №{previos}({t2})"));
                    break;
                }
                var startPoint = (Point)points[previos];
                
                var endPoint = MakeStep(startPoint, t2, direction);

                if (points[current] != null && !((Point)points[current]).Equals(endPoint))
                {
                    exceptions.Add(new Exception($"Ошибка в {i + 1} строке раздела TOP: \"{item}\". Попытка перезаписи точки начала блока №{current}. Старая: {points[current]}, новая: {endPoint}"));
                    break;
                }

                // Сохранение конечной точки
                points[current] = endPoint;
            }

            // Вычисляем стоимость всех деталей
            double price = 0;
            foreach (var detail in model.Order)
            {
                price += model.Blocks.FirstOrDefault(_ => _.Name == detail)?.Price ?? 0;
            }

            var nonNullablePoints = points.Select(point => point ?? new Point(0, 0, -99)).ToArray();

            return new Result
            {
                Points = nonNullablePoints,
                // Forks = forks,
                Price = price,
                Exceptions = exceptions,
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

        [Obsolete("Устарело в связи с удалением Y блоков")]
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

            //[Obsolete("Устарело в связи с удалением Y блоков")]
            //public Dictionary<int, Fork> Forks { get; set; }

            /// <summary>
            /// Стоимость всех деталей
            /// </summary>
            public double Price { get; set; }

            /// <summary>
            /// Список ошибок
            /// </summary>
            public List<Exception> Exceptions { get; set; }
        }
    }

}
