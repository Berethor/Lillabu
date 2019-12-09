using System.Collections.Generic;

namespace LilaApp.Models
{
    public static class ModelExtensions
    {
        /// <summary>
        /// Исправить топологию, основываясь на блок ORDER.
        /// T4 - поворот направо, t4 - налево
        /// </summary>
        /// <param name="model"></param>
        public static void FixTopology(this Model model)
        {
            model.Topology = new List<TopologyItem>(model.Order.Count + 1);

            for (var i = 0; i < model.Order.Count; i++)
            {
                var dir = 1;

                if (model.Order[i][0] == 't')
                {
                    dir = -1;
                    model.Order[i] = model.Order[i].Replace('t', 'T');
                }

                model.Topology.Add(new TopologyItem(i, i + 1, dir));
            }

            model.Topology.Add(new TopologyItem(model.Order.Count, 0, 1));
        }
    }
}
