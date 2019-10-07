using LilaApp.Models;
using System;
using System.IO;
using System.Linq;

namespace LilaApp
{
    public class Loader
    {
        public Model Load(string fileName)
        {
            var text = File.ReadAllText(fileName);

            return Parse(text);
        }

        public Model Parse(string text)
        {
            var lines = text.Split('\n')
                .Select(line => line.Replace("\r", ""))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries)[0])
                .ToList();

            var dataIndex = lines.IndexOf("DATA");
            var routeIndex = lines.IndexOf("ROUTE");
            var orderIndex = lines.IndexOf("ORDER");
            var topIndex = lines.IndexOf("TOP");

            if (dataIndex == -1) routeIndex = 0;
            if (routeIndex == -1) routeIndex = lines.Count;
            if (orderIndex == -1) orderIndex = lines.Count;
            if (topIndex == -1) topIndex = lines.Count;

            // Блоки
            var blocks = lines
                .Where((line, i) => dataIndex < i && i < routeIndex)
                .Select(line =>
                {
                    var values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var block = new Block()
                    {
                        Name = values[0],
                        Count = int.Parse(values[1]),
                        Price = int.Parse(values[2]),
                    };

                    return block;
                })
                .ToList();

            // Точки маршрута
            var points = lines
                .Where((line, i) => routeIndex < i && i < orderIndex)
                .Select(line =>
                {
                    var values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var point = new Point(x: double.Parse(values[0]), y: double.Parse(values[1]));

                    return point;
                })
                .ToList();

            // Порядок используемых блоков
            var order = lines
                .Where((line, i) => orderIndex < i && i < topIndex)
                .ToList();

            // Топология соединения блоков
            var top = lines
                .Where((line, i) => topIndex < i && i < lines.Count)
                .Select(line =>
                {
                    var values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var item = new TopologyItem()
                    {
                        FirstBlock = int.Parse(values[0]),
                        SecondBlock = int.Parse(values[1]),
                        Direction = int.Parse(values[2]),
                    };

                    return item;
                })
                .ToList();

            // Итоговая модель
            var model = new Model()
            {
                Blocks = blocks,
                Points = points,
                Order = order,
                Topology = top,
            };

            return model;
        }
    }

}
