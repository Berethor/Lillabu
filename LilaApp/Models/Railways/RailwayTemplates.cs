using System;
using System.Collections.Generic;
using System.Linq;
using LilaApp.Algorithm;

namespace LilaApp.Models.Railways
{
    public static class RailwayTemplates
    {
        /// <summary>
        ///  Создать стартовую трассу - кольцо
        /// </summary>
        /// <param name="turnDirection">Направление кольца (T4R, T4L, T8R или T8L)</param>
        /// <param name="model">Модель</param>
        /// <returns>Указатель на начало трассы - блок L0</returns>
        public static RailwayChain CreateCircle(RailwayType turnDirection, Model model = null)
        {
            // Добавляем блок L0
            var head = new Railway(RailwayType.L0) { Start = new Point(0, 0), };

            var count = turnDirection == RailwayType.T4L || turnDirection == RailwayType.T4R ? 8 : 16;

            var list = new List<IRailwayTemplate>() { head };

            var blueprint =
                turnDirection == RailwayType.T4L ? "t4" :
                turnDirection == RailwayType.T4R ? "T4" :
                turnDirection == RailwayType.T8L ? "t8" :
                turnDirection == RailwayType.T8R ? "T8" : "";

            // Добавляем 8 блоков T4 или 16 T8
            for (var i = 0; i < count; i++)
            {
                if (!RailwayFactory.Default.TryBuildTemplate(out var turn, out var error, blueprint, model))
                {
                    turn = new RailwayChain(new Railway(turnDirection));
                }

                list.AddRange(turn.GetRailways());
            }
            var chain = new RailwayChain(list.ToArray());
            //head.Append(chain);

            // Закольцовываем список
            //chain.Next = chain[0];

            // Связываем симметричные блоки
            for (var i = 0; i < list.Count; i++)
            {
                var block = list[i];
                
                if (!(DirectionExtensions.FromAngle(block.End.Angle) is Direction direction)) continue;

                if (direction == Direction.S)
                {
                    //var last = list.Last();
                    //last.Symmetric = block;

                    // TODO: задаём в качестве симметричного элемента для ↓ саму цепочку,
                    // чтобы при расширении блоки добавлялись после неё.
                    // По-хорошему надо добавлять внутрь цепочки в конец, 
                    // но при этом ломается конвертация в модель,
                    // т.к. при добавлении а конец цепочки _tail не меняется

                    block.Symmetric = chain;
                    continue;
                }

                if (block.Symmetric != null) break;

                var sym = direction.GetSymmetric();

                for (var j = i + 1; j < list.Count; j++)
                {
                    var other = list[j];

                    if (DirectionExtensions.FromAngle(other.End.Angle) is Direction otherDirection
                        && otherDirection == sym)
                    {
                        other.Symmetric = block;
                        block.Symmetric = other;

                        break;
                    }

                }
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

        public static List<string> Library  = new List<string>()
        {
            "t2T1L6",
            "L6T1t2",
            "t2T2T2T2t2",
            "T2t2t2t2T2",
            "t2T1t2",
            "T2t1T2",
        };
    }
}
