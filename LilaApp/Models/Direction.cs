using System;

namespace LilaApp.Models
{
    /// <summary>
    /// Направление (по сторонам света)
    /// </summary>
    public enum Direction
    {
        /// <summary>Север</summary>
        N,

        /// <summary>Юг</summary>
        S,

        /// <summary>Восток</summary>
        E,

        /// <summary>Запад</summary>
        W,

        // По-диагоналям

        /// <summary>Северо-запад</summary>
        NW,

        /// <summary>Северо-восток</summary>
        NE,

        /// <summary>Юго-запад</summary>
        SW,

        /// <summary>Юго-восток</summary>
        SE,
    }

    public static class DirectionExtensions
    {
        /// <summary>
        /// Преобразовать к углу наклона
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static double ToAngle(this Direction direction)
        {
            switch (direction)
            {
                case Direction.N: return 0;
                case Direction.S: return Math.PI;
                case Direction.E: return 3 * Math.PI / 2;
                case Direction.W: return Math.PI / 2;

                case Direction.NE: return 3 * Math.PI / 4;
                case Direction.NW: return Math.PI / 4;

                case Direction.SE: return 5 * Math.PI / 4;
                case Direction.SW: return 7 * Math.PI / 4;

                default:
                    throw new ArgumentOutOfRangeException($"Для направления {direction} не определен угол");
            }
        }

        public static Direction GetSymmetric(this Direction direction)
        {
            switch (direction)
            {
                case Direction.N: return Direction.S;
                case Direction.S: return Direction.N;
                case Direction.E: return Direction.W;
                case Direction.W: return Direction.E;

                case Direction.NE: return Direction.SW;
                case Direction.NW: return Direction.SE;

                case Direction.SE: return Direction.NW;
                case Direction.SW: return Direction.NE;

                default:
                    throw new ArgumentOutOfRangeException($"Для направления {direction} не определен угол");
            }
        }

        /// <summary>
        /// Преобразовать угол наклона к направлению сторон света
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Direction? FromAngle(double angle)
        {
            if (Math.Abs(angle - 0) < Constants.Precision) return Direction.N;
            if (Math.Abs(angle - Math.PI) < Constants.Precision) return Direction.S;
            if (Math.Abs(angle - 3 * Math.PI / 2) < Constants.Precision) return Direction.E;
            if (Math.Abs(angle - Math.PI / 2) < Constants.Precision) return Direction.W;

            if (Math.Abs(angle - 3 * Math.PI / 4) < Constants.Precision) return Direction.NE;
            if (Math.Abs(angle - Math.PI / 4) < Constants.Precision) return Direction.NW;

            if (Math.Abs(angle - 5 * Math.PI / 4) < Constants.Precision) return Direction.SE;
            if (Math.Abs(angle - 7 * Math.PI / 4) < Constants.Precision) return Direction.SW;

            // throw new ArgumentOutOfRangeException($"Для угла {angle} не определено направление");
            return null;
        }
    }
}
