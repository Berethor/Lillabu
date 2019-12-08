using System;
using System.Collections.Generic;
using System.Linq;

namespace LilaApp.Models
{
    public class Model : ICloneable
    {
        /// <summary>
        /// Список доступных блоков
        /// </summary>
        public List<Block> Blocks { get; private set; }

        /// <summary>
        /// Точки маршрута
        /// </summary>
        public List<Point> Points { get; private set; }

        /// <summary>
        /// Порядок элементов
        /// </summary>
        public List<string> Order { get; set; }

        /// <summary>
        /// Топология соединения блоков
        /// </summary>
        public List<TopologyItem> Topology { get; set; }

        /// <summary>
        /// Матрица расстояний между точками маршрута
        /// </summary>
        public double[][] Distances { get; set; }

        public Model()
        {
            Blocks = new List<Block>();
            Points = new List<Point>();
            Order = new List<string>();
            Topology = new List<TopologyItem>();
        }

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="other"></param>
        public static Model Copy(Model other)
        {
            var cloned = other?.Clone() as Model;

            return cloned ?? new Model();
        }

        /// <summary>
        /// Конструктор с установкой Capacity
        /// </summary>
        /// <param name="elementRestriction">Ограничение на максимальное количество элементов маршрута</param>
        /// <param name="pointRestriction">Ограничение на максимальное количество точек маршрута</param>
        public Model(int elementRestriction, int pointRestriction)
        {
            // В худшем случае Blocks.Count достигает Blocks.Capacity
            Blocks = new List<Block>(elementRestriction);
            Points = new List<Point>(pointRestriction);
            Order = new List<string>(elementRestriction);
            Topology = new List<TopologyItem>(elementRestriction);
        }

        #region Implementation of ICloneable

        public object Clone()
        {
            var model = new Model
            {
                Blocks = Blocks.Clone(),
                Points = Points.Clone(),
                Order = Order.Clone(),
                Topology = Topology.Clone(),
            };

            return model;
        }

        #endregion
    }
}
