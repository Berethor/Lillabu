using System;
using System.Text;
using System.Collections.Generic;

using LilaApp.Models;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    static class HelperActions
    {
        /// <summary>
        /// Добавляет начальную трасу, с поворотом туда,
        /// где самые прибыльные точки.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="topology"></param>
        /// <param name="turn"></param>
        public static void InitialRouteBuilder(List<string> order, List<TopologyItem> topology, int turn)
        {
            int i = 0;

            order.AddRange(new string[]
            {
                "T4","T4",
                "T4","T4",
                "T4","T4",
                "T4","T4"
            });

            while (i < order.Count)
            {
                topology.Add(new TopologyItem(i, i + 1, turn));
                i++;
            }

            topology.Add(new TopologyItem(i, 0, turn));
        }
        /// <summary>
        /// Возвращает направление, где самые прибыльные точки
        /// 1 - вправо
        /// -1 - влево
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static int GetRichestSide(List<Point> points)
        {
            double leftSideIncome  = 0;
            double rightSideIncome = 0;

            foreach(var point in points)
            {
                if (point.X == 0)
                    continue;
                if(point.X > 0)
                {
                    rightSideIncome += point.Price;
                }
                else
                {
                    leftSideIncome  += point.Price;
                }
            }
            if(leftSideIncome > rightSideIncome)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}
