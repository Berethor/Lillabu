using System.Collections.Generic;

namespace LilaApp.Models
{
    public class Model
    {
        /// <summary>
        /// Список доступных блоков
        /// </summary>
        public List<Block> Blocks { get; }

        /// <summary>
        /// Точки маршрута
        /// </summary>
        public List<Point> Points { get; }

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
        public Model(Model other)
        {
            if (other == null)
            {
                Blocks = new List<Block>();
                Points = new List<Point>();
                Order = new List<string>();
                Topology = new List<TopologyItem>();

                return;
            }

            Blocks = new List<Block>(other?.Blocks);
            Points = new List<Point>(other?.Points);
            Order = new List<string>(other?.Order);
            Topology = new List<TopologyItem>(other?.Topology);
            Distances = new double[other.Distances?.Length ?? 0][];
            for(int i = 0; i < other.Distances?.Length; i++)
                Distances[i] = new List<double>(other.Distances[i]).ToArray();
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

    }
}
