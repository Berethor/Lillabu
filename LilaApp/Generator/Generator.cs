using LilaApp.Models;
using System;

namespace LilaApp.Generator
{
    /// <summary>
    /// Класс для генерации случайных входных данных
    /// </summary>
    public class Generator
    {
        /// <summary>
        /// Параметры генератора
        /// </summary>
        public GeneratorConfiguration Configuration { get; set; } = GeneratorConfiguration.Default;

        /// <summary>
        /// Типы блоков
        /// </summary>
        public string[] BlockTypes = new[]
        {
            "L1", "L2", "L3", "L4",
            "T4", "T8",
            "B1",
        };

        #region .ctor

        public Generator() { }

        public Generator(GeneratorConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        private readonly Random _random = new Random();

        private int GetRandom(Range<int> range) => _random.Next(range.Min, range.Max);

        /// <summary>
        /// Сгенерировать исходные данные
        /// </summary>
        /// <returns>Модель с блоками DAT и ROUTE</returns>
        public Model CreateInputData()
        {
            var model = new Model();

            for (var i = 0; i < BlockTypes.Length; i++)
            {
                var count = GetRandom(Configuration.BlockCount);
                var price = GetRandom(Configuration.BlockPrice);

                var block = new Block(BlockTypes[i], count, price);
                model.Blocks.Add(block);
            }

            var min = Configuration.MinRoutePoint;
            var max = Configuration.MaxRoutePoint;

            model.Points.Add(new Point(0, 0, 0, GetRandom(Configuration.RoutePrice)));

            for (var i = 0; i < GetRandom(Configuration.RoutesCount); i++)
            {
                var x = Math.Round(min.X + _random.NextDouble() * (max.X - min.X), 2);
                var y = Math.Round(min.Y + _random.NextDouble() * (max.Y - min.Y), 2);
                var price = GetRandom(Configuration.RoutePrice);

                var route = new Point(x, y, price: price);
                model.Points.Add(route);
            }

            return model;
        }
    }

}
