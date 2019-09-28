using System.Collections.Generic;

namespace LilaApp.Models
{
    public class Model
    {
        /// <summary>
        /// Список доступных блоков
        /// </summary>
        public List<Block> Blocks = new List<Block>();

        /// <summary>
        /// Точки маршрута
        /// </summary>
        public List<Point> Points = new List<Point>();

        /// <summary>
        /// Порядок элементов
        /// </summary>
        public List<string> Order = new List<string>();

        /// <summary>
        /// Топология соединения блоков
        /// </summary>
        public List<TopologyItem> Topology = new List<TopologyItem>();

     }
}
