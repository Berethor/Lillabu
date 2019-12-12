using System;
using System.Collections.Generic;

namespace LilaApp
{
    /// <summary>
    /// Константы
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Радиус поворотного блока
        /// </summary>
        public const double RADIUS = 3;

        /// <summary>
        /// Углы поворотных блоков
        /// </summary>
        public static readonly Dictionary<string, double> T_ANGLES = new Dictionary<string, double>()
        {
            { "T2", Math.PI / 2 },
            { "T4", Math.PI / 4 },
            { "T8", Math.PI / 8 },
            { "T16", Math.PI / 16 },
        };

        /// <summary>
        /// Угол отклонения ответвления Y-блока относительно вертикали
        /// </summary>
        public const double Y_ANGLE = Math.PI / 4;

        /// <summary>
        /// Длина ответвлений и основания Y-блока
        /// </summary>
        public const double Y_LENGTH = 1;

        /// <summary>
        /// Допустимая погрешность
        /// </summary>
        public const double EPSILON = 0.01;

        /// <summary>
        /// Точность измерения дробных чисел
        /// </summary>
        public static double Precision { get; } = 10E-10;
    }
}
