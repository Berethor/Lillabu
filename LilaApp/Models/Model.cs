using System.Collections.Generic;

namespace LilaApp.Models
{
    public class Model
    {
        /// <summary>
        /// Список доступных блоков
        /// </summary>
        public List<Block> Blocks;

        /// <summary>
        /// Точки маршрута
        /// </summary>
        public List<Point> Points;

        /// <summary>
        /// Порядок элементов
        /// </summary>
        public List<string> Order;

        /// <summary>
        /// Топология соединения блоков
        /// </summary>
        public List<TopologyItem> Topology;

        public Model()
        {
            Blocks = new List<Block>();
            Points = new List<Point>();
            Order = new List<string>();
            Topology = new List<TopologyItem>();
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
