using System;
using System.Text;
using System.Collections.Generic;

using LilaApp.Models;
using System.Linq;

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
        public static void InitialRouteBuilder(Model _answer,
            int turn, List<Block> blocks,
            List<RouteSide> sides)
        {
            int i = 0;
            int startI = 0;
            TraceBuilder.Result trace;

            AddSide(_answer, turn, blocks, sides, ref i, out trace, ref startI);
            AddSide(_answer, turn, blocks, sides, ref i, out trace, ref startI);
            AddSide(_answer, turn, blocks, sides, ref i, out trace, ref startI);
            AddSide(_answer, turn, blocks, sides, ref i, out trace, ref startI);

            _answer.Topology.Add(new TopologyItem(i, 0, turn));

            trace.Points = trace.Points.Take(trace.Points.Length - 1).ToArray();

            sides.Add(new RouteSide(trace.Points[0], trace.Points[0], i, 0, 4, turn, 0));

            sides[0].actions.Add(("Up", sides[2], true));
            sides[0].actions.Add(("Down", sides[4], false));
            sides[0].actions.Add(("Right", sides[1], false));
            sides[0].actions.Add(("Left", sides[3], false));

            sides[1].actions.Add(("Up", sides[0], false));
            sides[1].actions.Add(("Down", sides[4], false));
            sides[1].actions.Add(("Right", sides[3], Convert.ToBoolean(turn + 1)));
            sides[1].actions.Add(("Left", sides[3], !Convert.ToBoolean(turn + 1)));

            sides[2].actions.Add(("Up", sides[0], true));
            sides[2].actions.Add(("Down", sides[4], true));
            sides[2].actions.Add(("Right", sides[1], false));
            sides[2].actions.Add(("Left", sides[3], false));

            sides[3].actions.Add(("Up", sides[0], false));
            sides[3].actions.Add(("Down", sides[4], false));
            sides[3].actions.Add(("Right", sides[1], Convert.ToBoolean(turn + 1)));
            sides[3].actions.Add(("Left", sides[1], !Convert.ToBoolean(turn + 1)));

            sides[4].actions.Add(("Up", sides[0], false));
            sides[4].actions.Add(("Down", sides[2], true));
            sides[4].actions.Add(("Right", sides[1], false));
            sides[4].actions.Add(("Left", sides[3], false));
            sides[4].details.Add(trace.Points[0]);
        }

        private static void AddSide(Model _answer, int turn,
            List<Block> blocks, List<RouteSide> sides,
            ref int i, out TraceBuilder.Result trace, ref int startI)
        {
            var firstTurn = GetTurnElements(blocks);
            _answer.Order.AddRange(firstTurn);

            for (int j = 0; j < firstTurn.Count; j++)
            {
                _answer.Topology.Add(new TopologyItem(i, i + 1, turn));
                i++;
            }

            trace = TraceBuilder.CalculateTrace(_answer);
            sides.Add(new RouteSide(trace.Points[startI], startI, 4, turn, firstTurn.Count));

            for (int j = 0; j < firstTurn.Count; j++)
            {
                sides[sides.Count - 1].details.Add(trace.Points[startI]);
                startI++;
            }
        }

        public static List<string> GetTurnElements(List<Block> sortedTurnBlocks)
        {
            var turnAngle = 90;
            double angle = 0;
            List<string> blocks = new List<string>();

            foreach (var block in sortedTurnBlocks)
            {
                var blockAngle = int.Parse(block.Name[1].ToString());
                int tries = 0;
                while (angle < 90)
                {
                    if (tries == 10)
                    {
                        break;
                    }

                    if (block.Count < 1)
                    {
                        break;
                    }

                    switch (blockAngle)
                    {
                        case (2):
                            if (angle + 90 <= turnAngle)
                            {
                                angle += 90;
                                blocks.Add(block.Name);
                                block.Count--;
                            }
                            break;
                        case (4):
                            if (angle + 45 <= turnAngle)
                            {
                                angle += 45;
                                blocks.Add(block.Name);
                                block.Count--;
                            }
                            break;
                        case (8):
                            if (block.Count > 1 && angle + 45 <= turnAngle)
                            {
                                angle += 45;
                                blocks.Add(block.Name);
                                blocks.Add(block.Name);
                                block.Count -= 2;
                            }
                            break;
                    }

                    tries++;
                }
            }

            if (angle != 90)
            {
                throw new Exception("Не хватает элементов");
            }
            return blocks;
        }
        public static List<string> GetStraightElements(ref List<Block> sortedStraightBlocks, int length)
        {
            int currentLength = 0;
            List<string> blocks = new List<string>();
            var startI = 0;

            var newBlocks = (sortedStraightBlocks
                .Select(item => new Block(item.Name, item.Count, item.Price))).ToList();

            while (currentLength != length)
            {
                blocks = new List<string>();

                for (int i = startI; i < newBlocks.Count; i++)
                {
                    var blockLength = int.Parse(newBlocks[i].Name[1].ToString());

                    while (currentLength != length)
                    {
                        if (newBlocks[i].Count < 1)
                        {
                            break;
                        }

                        if (currentLength + blockLength <= length)
                        {
                            currentLength += blockLength;
                            blocks.Add(newBlocks[i].Name);
                            newBlocks[i].Count--;
                            continue;
                        }

                        break;
                    }
                }

                startI++;

                if (currentLength == 0)
                {
                    return blocks;
                }
            }

            sortedStraightBlocks = newBlocks;
            return blocks;
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
            double leftSideIncome = 0;
            double rightSideIncome = 0;
            int rightCount = 0;
            int leftCount = 0;
            foreach (var point in points)
            {
                if (point.X == 0)
                    continue;

                if (point.X > 0)
                {
                    rightSideIncome += point.Price;
                    rightCount++;
                }
                else
                {
                    leftSideIncome += point.Price;
                    leftCount++;
                }
            }
            if (leftSideIncome / leftCount > rightSideIncome / rightCount)
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
