using System;
using LilaApp;
using LilaApp.Generator;
using LilaApp.Models;

namespace Lilabu.ViewModels
{
    public class GeneratorViewModel : ABaseViewModel
    {
        public int MinBlockCount { get => Get<int>(); set => Set(value); }
        public int MaxBlockCount { get => Get<int>(); set => Set(value); }

        public int MinBlockPrice { get => Get<int>(); set => Set(value); }
        public int MaxBlockPrice { get => Get<int>(); set => Set(value); }

        public int MinRoutesCount { get => Get<int>(); set => Set(value); }
        public int MaxRoutesCount { get => Get<int>(); set => Set(value); }

        public int MinRoutePrice { get => Get<int>(); set => Set(value); }
        public int MaxRoutePrice { get => Get<int>(); set => Set(value); }

        public double MinRouteX { get => Get<double>(); set => Set(value); }
        public double MaxRouteX { get => Get<double>(); set => Set(value); }
        public double MinRouteY { get => Get<double>(); set => Set(value); }
        public double MaxRouteY { get => Get<double>(); set => Set(value); }

        /// <summary>
        /// Команда генерации исходных данных
        /// </summary>
        public BaseCommand GenerateCommand { get; }

        /// <summary>
        /// Событие генерации новой конфигурации
        /// </summary>
        public event EventHandler<GeneratorConfiguration> OnGeneration;

        public GeneratorViewModel()
        {
            GenerateCommand = new BaseCommand(() =>
            {
                var config = this.ToConfiguration();
                OnGeneration?.Invoke(this, config);
                config.Save();
            });
        }

        public static GeneratorViewModel FromConfiguration(GeneratorConfiguration config)
        {
            return new GeneratorViewModel
            {
                MinBlockCount = config.BlockCount.Min,
                MaxBlockCount = config.BlockCount.Max,

                MinBlockPrice = config.BlockPrice.Min,
                MaxBlockPrice = config.BlockPrice.Max,

                MinRoutesCount = config.RoutesCount.Min,
                MaxRoutesCount = config.RoutesCount.Max,

                MinRoutePrice = config.RoutePrice.Min,
                MaxRoutePrice = config.RoutePrice.Max,

                MinRouteX = config.MinRoutePoint.X,
                MaxRouteX = config.MaxRoutePoint.X,
                MinRouteY = config.MinRoutePoint.Y,
                MaxRouteY = config.MaxRoutePoint.Y,
            };
        }

        public GeneratorConfiguration ToConfiguration()
        {
            return new GeneratorConfiguration()
            {
                BlockCount = new Range<int>(MinBlockCount, MaxBlockCount),
                BlockPrice = new Range<int>(MinBlockPrice, MaxBlockPrice),
                RoutesCount = new Range<int>(MinRoutesCount, MaxRoutesCount),
                RoutePrice = new Range<int>(MinRoutePrice, MaxRoutePrice),
                MinRoutePoint = new Point(MinRouteX, MinRouteY),
                MaxRoutePoint = new Point(MaxRouteX, MaxRouteY),
            };
        }
    }
}
