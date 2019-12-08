using System.Text;
using LilaApp.Models;

namespace LilaApp
{
    /// <summary>
    /// Класс для сохранения модели в текстовый формат
    /// </summary>
    public static class Saver
    {
        /// <summary>
        /// Записать данные модели в текстовый формат
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string Serialize(this Model model)
        {
            var text = new StringBuilder();

            if (model.Blocks?.Count > 0)
            {
                text.AppendLine("DATA");

                foreach (var block in model.Blocks)
                {
                    text.AppendLine($"{block.Name} {block.Count} {block.Price}");
                }

                text.AppendLine("/");
                text.AppendLine();
            }

            if (model.Points?.Count > 0)
            {
                text.AppendLine("ROUTE");

                foreach (var route in model.Points)
                {
                    text.AppendLine($"{route.X} {route.Y} {route.Price}");
                }

                text.AppendLine("/");
                text.AppendLine();
            }

            if (model.Order?.Count > 0)
            {
                text.AppendLine("ORDER");

                foreach (var order in model.Order)
                {
                    text.AppendLine($"{order}");
                }

                text.AppendLine("/");
                text.AppendLine();
            }

            if (model.Topology?.Count > 0)
            {
                text.AppendLine("TOP");

                foreach (var topology in model.Topology)
                {
                    text.AppendLine($"{topology.FirstBlock} {topology.SecondBlock} {topology.Direction}");
                }

                text.AppendLine("/");
            }

            return text.ToString();
        }
    }
}
