using System;
using LilaApp.Models;

namespace LilaApp.Generator
{
    /// <summary>
    /// Параметры генератора
    /// </summary>
    [Serializable]
    public class GeneratorConfiguration : IConfiguration
    {
        /// <summary>
        /// Количество блоков одного типа
        /// </summary>
        public Range<int> BlockCount { get; set; }

        /// <summary>
        /// Цена одного блока
        /// </summary>
        public Range<int> BlockPrice { get; set; }

        /// <summary>
        /// Количество точек маршрута
        /// </summary>
        public Range<int> RoutesCount { get; set; }

        /// <summary>
        /// Прибыль с точки маршрута
        /// </summary>
        public Range<int> RoutePrice { get; set; }

        /// <summary>
        /// Минимальные координаты точек маршрута
        /// </summary>
        public Point MinRoutePoint { get; set; }

        /// <summary>
        /// Максимальные координат точек маршрута
        /// </summary>
        public Point MaxRoutePoint { get; set; }

        /// <summary>
        /// Параметры по-умолчанию
        /// </summary>
        /// <returns></returns>
        public static GeneratorConfiguration Default =>

            new GeneratorConfiguration
            {
                BlockCount = new Range<int>(0, 100),
                BlockPrice = new Range<int>(0, 500),
                RoutesCount = new Range<int>(5, 20),
                RoutePrice = new Range<int>(0, 1000),
                MinRoutePoint = new Point(-20, -20),
                MaxRoutePoint = new Point(+20, +20),
            };

        #region Implementation of IConfiguration

        /// <summary>
        /// Название файла конфигурации
        /// </summary>
        public string FileName => "generator_configuration.xml";

        #endregion
    }
}
