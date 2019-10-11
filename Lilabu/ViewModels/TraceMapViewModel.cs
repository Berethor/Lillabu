﻿using System.Collections.Generic;
using Point = LilaApp.Models.Point;

namespace Lilabu.ViewModels
{
    public class TraceMapViewModel : ABaseViewModel
    {
        #region Properties

        /// <summary>
        /// Ширина
        /// </summary>
        public double Width { get => Get<double>(); set => Set(value); }

        /// <summary>
        /// Высота
        /// </summary>
        public double Height { get => Get<double>(); set => Set(value); }

        /// <summary>
        /// Точки для отрисовки
        /// </summary>
        public Point[] Points { get => Get<Point[]>(); set => Set(value); }

        #endregion

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public TraceMapViewModel()
        {
            Width = 100;
            Height = 100;
            Points = new Point[0];
        }
    }
}
