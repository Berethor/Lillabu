using System.Collections.Generic;
using System.Linq;

namespace LilaApp.Models.Railways
{
    public static class RailwayTemplates
    {
        /// <summary>
        ///  Создать стартовую трассу - кольцо
        /// </summary>
        /// <param name="turnDirection">Направление кольца (T4R, T4L, T8R или T8L)</param>
        /// <returns>Указатель на начало трассы - блок L0</returns>
        public static RailwayChain CreateCircle(RailwayType turnDirection)
        {
            // Добавляем блок L0
            var head = new Railway(RailwayType.L0) { Start = new Point(0, 0), };

            var count = turnDirection == RailwayType.T4L || turnDirection == RailwayType.T4R ? 8 : 16;

            var list = new List<IRailwayTemplate>() { head };

            // Добавляем 8 блоков T4
            for (var i = 0; i < count; i++)
            {
                list.Add(new Railway(turnDirection));
            }
            var chain = new RailwayChain(list.ToArray());
            //head.Append(chain);

            // Закольцовываем список
            //chain.Next = chain[0];

            // Связываем симметричные блоки
            for (var i = 0; i < count / 2; i++)
            {
                chain[i].Symmetric = chain[i + count / 2];
                chain[i + count / 2].Symmetric = chain[i];
            }

            //head.Symmetric = chain[count / 2 - 1];
            //chain[count / 2 - 1].Symmetric = head;

            return chain;
        }

        /// <summary>
        /// Указатели на элементы кольца по сторонам света:
        ///        north (север)
        /// west (запад)      east (восток)
        ///         south (юг)
        /// </summary>
        /// <returns></returns>
        public static (
            Railway NW, Railway N, Railway NE,
            Railway W, /*       */ Railway E,
            Railway SW, Railway S, Railway SE)
            Compass(RailwayChain chain)
        {
            var m = new[] {RailwayType.T8L, RailwayType.T8R}.Contains(((Railway) chain[0]).Type) ? 2 : 1;

            return (
                chain[1 * m - 1] as Railway, chain[2 * m - 1] as Railway, chain[3 * m - 1] as Railway,
                chain[0] as Railway, /*                                */ chain[4 * m - 1] as Railway,
                chain[7 * m - 1] as Railway, chain[6 * m - 1] as Railway, chain[5 * m - 1] as Railway);
        }
    }
}
