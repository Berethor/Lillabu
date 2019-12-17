using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using LilaApp.Models;
using LilaApp.Algorithm.SecondSolverHelper;

namespace LilaApp.Algorithm
{
    public class SecondFinalSolver : IFinalTaskSolver
    {
        #region Implementation of IFinalTaskSolver

        /// <inheritdoc />
        public FinalAnswer Solve(Model model, IDirectTaskSolver checker)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _checker = checker;

            _answer = Model.Copy(_model);

            Run();

            return new FinalAnswer()
            {
                Model = _answer,
                Price = _checker.Solve(_model),
            };
        }

        #endregion

        public event EventHandler<FinalAnswer> OnStepEvent;
        public CancellationToken Token { get; set; }

        private Model _model;
        private Model _answer;
        private IDirectTaskSolver _checker;

        private List<string> _newOrder;
        private List<TopologyItem> _newTopology;

        private static double traceLengthForTurn = Constants.RADIUS * 4;

        private void Run()
        {
            var blocks = _answer.Blocks;
            var points = _answer.Points;
            double traceIncome;
            Point pointShift;

            var order = new List<string>();
            var oldOrder = new List<string>();
            var topology = new List<TopologyItem>();
            var oldTopology = new List<TopologyItem>();

            var sides = new List<RouteSide>();

            var turn = HelperActions.GetRichestSide(points);
            turn = 1;

            HelperActions.InitialRouteBuilder(order,
                topology, turn, blocks);

            _answer.Order = order;
            _answer.Topology = topology;

            _newOrder = new List<string>(order);
            _newTopology = (topology.Select(item => new TopologyItem(item))).ToList();

            var trace = TraceBuilder.CalculateTrace(_answer);
            traceIncome = DirectTaskSolver.GetRouteIncome(_answer, trace.Points);

            oldOrder = order.GetRange(0, order.Count);
            oldTopology = topology.GetRange(0, topology.Count);

            sides.Add(new RouteSide(trace.Points[0], 0, 4));
            sides.Add(new RouteSide(trace.Points[2], 2, 4));
            sides.Add(new RouteSide(trace.Points[4], 4, 4));
            sides.Add(new RouteSide(trace.Points[6], 6, 4));
            sides.Add(new RouteSide(trace.Points[0], trace.Points[0], 8, 0, 4));

            sides[0].actions.Add(("Up", sides[2], true));
            sides[0].actions.Add(("Down", sides[4], false));
            sides[0].actions.Add(("Right", sides[1], false));
            sides[0].actions.Add(("Left", sides[3], false));
            sides[0].details.Add(trace.Points[0]);
            sides[0].details.Add(trace.Points[1]);

            sides[1].actions.Add(("Up", sides[0], false));
            sides[1].actions.Add(("Down", sides[4], false));
            sides[1].actions.Add(("Right", sides[3], Convert.ToBoolean(turn + 1)));
            sides[1].actions.Add(("Left", sides[3], !Convert.ToBoolean(turn + 1)));
            sides[1].details.Add(trace.Points[2]);
            sides[1].details.Add(trace.Points[3]);

            sides[2].actions.Add(("Up", sides[0], true));
            sides[2].actions.Add(("Down", sides[4], true));
            sides[2].actions.Add(("Right", sides[1], false));
            sides[2].actions.Add(("Left", sides[3], false));
            sides[2].details.Add(trace.Points[4]);
            sides[2].details.Add(trace.Points[5]);

            sides[3].actions.Add(("Up", sides[0], false));
            sides[3].actions.Add(("Down", sides[4], false));
            sides[3].actions.Add(("Right", sides[1], true));
            sides[3].actions.Add(("Left", sides[1], true));
            sides[3].details.Add(trace.Points[6]);
            sides[3].details.Add(trace.Points[7]);

            sides[4].actions.Add(("Up", sides[0], false));
            sides[4].actions.Add(("Down", sides[2], true));
            sides[4].actions.Add(("Right", sides[1], false));
            sides[4].actions.Add(("Left", sides[3], false));
            sides[4].details.Add(trace.Points[0]);

            int index = 0;
            if (blocks.Count(_ => _.Name.StartsWith("B")) > 0)
            {
                var angle = trace.Points[index].Angle;

                var sideIndex = sides.FindIndex(a =>
                           Math.Abs(a.EndIndex - index) == Math.Abs(a.StartIndex - index));

                for(int i = sideIndex; i >= 0; i--)
                {
                    sides[sideIndex].SideEndIndex += 4;
                }

                order.Remove(order[index]);
                order.Remove(order[index]);
                topology.Remove(topology[index]);
                topology.Remove(topology[index]);

                for (int i = index; i < topology.Count - 1; i++)
                {
                    topology[i].FirstBlock -= 2;
                    topology[i].SecondBlock -= 2;
                }

                topology[topology.Count - 1].FirstBlock -= 2;

                order.InsertRange(index, new string[]
                {
                    "L3","L3",
                    "T4","T4",
                    "T4","T4",
                    "T4","T4",
                    "L1","B1",
                    "L1"
                });

                blocks.Find(a => a.Name == "T4").Count -= 6;
                blocks.Find(a => a.Name == "B1").Count -= 1;
                blocks.Find(a => a.Name == "L3").Count -= 2;
                blocks.Find(a => a.Name == "L1").Count -= 2;

                topology.InsertRange(
                    index, new TopologyItem[]
                    {
                    new TopologyItem(index, index + 1, 1),
                    new TopologyItem(index + 1, index + 2, 1),
                    new TopologyItem(index + 2, index + 3, turn*-1),
                    new TopologyItem(index + 3, index + 4, turn*-1),
                    new TopologyItem(index + 4, index + 5, turn*-1),
                    new TopologyItem(index + 5, index + 6, turn*-1),
                    new TopologyItem(index + 6, index + 7, turn*-1),
                    new TopologyItem(index + 7, index + 8, turn*-1),
                    new TopologyItem(index + 8, index + 9, 1),
                    new TopologyItem(index + 9, index + 10, 1),
                    new TopologyItem(index + 10, index + 11, 1),
                    });

                for (int i = index + 11; i < topology.Count - 1; i++)
                {
                    topology[i].FirstBlock += 11;
                    topology[i].SecondBlock += 11;
                }

                topology[topology.Count - 1].FirstBlock += 11;

                _answer.Order = order;
                _answer.Topology = topology;

                OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));
                trace = TraceBuilder.CalculateTrace(_answer);

                var pSidesCount = sides.Count;

                sides.Add(new RouteSide(
                    trace.Points[index + 1],
                    trace.Points[index + 2],
                    index + 1, index + 2, pSidesCount + 4));

                sides[sides.Count - 1].details.Add(trace.Points[index + 1]);
                sides[sides.Count - 1].details.Add(trace.Points[index + 2]);
                sides[sides.Count - 1].details.Add(trace.Points[index + 3]);

                sides.Add(new RouteSide(
                    trace.Points[index + 4],
                    index + 4, pSidesCount + 4));

                sides[sides.Count - 1].details.Add(trace.Points[index + 4]);
                sides[sides.Count - 1].details.Add(trace.Points[index + 5]);

                sides.Add(new RouteSide(
                    trace.Points[index + 6],
                    index + 6, pSidesCount + 4));

                sides[sides.Count - 1].details.Add(trace.Points[index + 6]);
                sides[sides.Count - 1].details.Add(trace.Points[index + 7]);

                sides.Add(new RouteSide(
                    trace.Points[index + 8],
                    trace.Points[index + 10],
                    index + 8, index + 10, pSidesCount + 4));

                sides[sides.Count - 1].details.Add(trace.Points[index + 8]);
                sides[sides.Count - 1].details.Add(trace.Points[index + 9]);
                sides[sides.Count - 1].details.Add(trace.Points[index + 10]);

                if (angle == 0)
                {
                    sides[pSidesCount].actions.Add(("Up", sides[pSidesCount + 2], true));
                    sides[pSidesCount].actions.Add(("Down", sides[2], false));
                    sides[pSidesCount].actions.Add(("Right", sides[1], false));
                    sides[pSidesCount].actions.Add(("Left", sides[pSidesCount + 1], false));

                    sides[pSidesCount + 1].actions.Add(("Up", sides[pSidesCount + 2], false));
                    sides[pSidesCount + 1].actions.Add(("Down", sides[2], false));
                    sides[pSidesCount + 1].actions.Add(("Right", sides[1], false));
                    sides[pSidesCount + 1].actions.Add(("Left", sides[pSidesCount + 3], true));

                    sides[pSidesCount + 2].actions.Add(("Up", sides[pSidesCount], true));
                    sides[pSidesCount + 2].actions.Add(("Down", sides[2], false));
                    sides[pSidesCount + 2].actions.Add(("Right", sides[1], false));
                    sides[pSidesCount + 2].actions.Add(("Left", sides[pSidesCount + 1], false));

                    sides[pSidesCount + 3].actions.Add(("Up", sides[pSidesCount], false));
                    sides[pSidesCount + 3].actions.Add(("Down", sides[2], false));
                    sides[pSidesCount + 3].actions.Add(("Right", sides[1], false));
                    sides[pSidesCount + 3].actions.Add(("Left", sides[pSidesCount + 1], true));
                }

                for (int i = 1; i < pSidesCount; i++)
                {
                    sides[i].StartIndex += 9;

                    if (sides[i].EndIndex != 0)
                        sides[i].EndIndex += 9;
                }
            }

            _answer.Order = order;
            _answer.Topology = topology;

            _newOrder = new List<string>(order);
            _newTopology = (topology.Select(item => new TopologyItem(item))).ToList();

            double minLength = double.MaxValue;
            double minRouteLength = double.MaxValue;
            var detail = new Point(0, 0);
            var nextPoint = new Point(0, 0);
            var plannedPoint = new Point(0, 0);

            var pointsCopy = points;

            var circleCentre = new Point(turn * Constants.RADIUS, 0);
            pointsCopy = pointsCopy.Where(a =>
            {
                if (MathFunctions.GetDistanceToPoint(circleCentre, a) < Constants.RADIUS)
                {
                    return false;
                }

                return true;
            }).ToList();

            try
            {
                while (!Token.IsCancellationRequested)
                {
                    minRouteLength = double.MaxValue;

                    foreach (var waypoint in pointsCopy)
                    {
                        double length = 0;
                        minLength = double.MaxValue;

                        foreach (var detailPoint in trace.Points)
                        {
                            length = MathFunctions.GetDistanceToPoint(detailPoint, waypoint);

                            if (length < minLength)
                            {
                                minLength = length;
                                plannedPoint = detailPoint;
                            }
                        }

                        if (minLength < 0.5)
                        {
                            pointsCopy = pointsCopy.Where(a => !a.Equals(waypoint)).ToList();
                            continue;
                        }

                        if (minLength < minRouteLength)
                        {
                            minRouteLength = minLength;
                            detail = plannedPoint;
                            nextPoint = waypoint;
                        }
                    }

                    if (pointsCopy.Count == 0)
                    {
                        break;
                    }

                    (string side, int length) expand = GetExpandData(ref detail, nextPoint);

                    void SearchSide(RouteSide secondSide, string detailName)
                    {
                        foreach (var action in secondSide.actions
                            .Where(action => action.command == expand.side)
                            .Select(action => action))
                        {
                            AddElement(detailName, action.Item2, out pointShift, order, topology, blocks);
                            UpdateSides(pointShift, sides, sides.IndexOf(action.Item2), action.Item2.SideEndIndex);
                        }
                    }

                    for (int i = 0; i < sides.Count; i++)
                    {
                        if (IsDetailBelongsToSide(sides, detail, i))
                        {
                            foreach (var action in sides[i].actions
                                .Where(action => action.command == expand.side)
                                .Select(action => action))
                            {
                                if (action.Item2.actions
                                    .Where(a => a.command == expand.side && a.action)
                                    .Count() != 0)
                                {
                                    var directBlocks = blocks.FindAll(a => a.Name.StartsWith("L"));

                                    string detailName = GetDetail(expand, directBlocks);

                                    AddElement(detailName, action.Item2, out pointShift, order, topology, blocks);
                                    UpdateSides(pointShift, sides, sides.IndexOf(action.Item2), action.Item2.SideEndIndex);
                                    SearchSide(action.Item2, detailName);
                                }
                            }

                            break;
                        }
                    }

                    _answer.Order = order;
                    _answer.Topology = topology;
                    OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));

                    trace = TraceBuilder.CalculateTrace(_answer);
                    var newTraceIncome = DirectTaskSolver.GetRouteIncome(_answer, trace.Points);

                    if (traceIncome < newTraceIncome)
                    {
                        traceIncome = newTraceIncome;
                        _newOrder = new List<string>(order);
                        _newTopology = (topology.Select(item => new TopologyItem(item))).ToList();
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            _answer = Model.Copy(_model);
            _answer.Order = _newOrder;
            _answer.Topology = _newTopology;

            OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));
        }

        private static bool IsDetailBelongsToSide(List<RouteSide> sides, Point detail, int i)
        {
            foreach (var d in sides[i].details)
            {
                if (detail.Equals(d))
                    return true;
            }
            return false;
        }

        private static string GetDetail((string side, int length) expand, List<Block> directBlocks)
        {
            var lastBlock = directBlocks.Count() - 1;
            
            string detailName = directBlocks[lastBlock].Name;

            int minLength = int.Parse(directBlocks[lastBlock].Name[1].ToString());

            foreach (var block in directBlocks)
            {
                var blockLength = int.Parse(block.Name[1].ToString());
                var length = Math.Abs(expand.length - blockLength);

                if (length < minLength)
                {
                    minLength = length;
                    detailName = "L" + blockLength;
                    break;
                }
            }

            return detailName;
        }

        private static void AddElement(string name, RouteSide side, out Point pointShift, List<string> order, List<TopologyItem> topology, List<Block> blocks)
        {
            var index = side.StartIndex;

            if (blocks.Find(a => a.Name == name).Count < 1)
                throw new Exception("Кончились блоки");

            order.Insert(index, name);
            if (index != 0)
            {
                topology.Insert(index, new TopologyItem(index - 1, index, 1));

                for (int i = index; i < topology.Count - 1; i++)
                {
                    topology[i].FirstBlock++;
                    topology[i].SecondBlock++;
                }
            }
            else
            {
                topology.Insert(index, new TopologyItem(index, index + 1, 1));

                for (int i = index + 1; i < topology.Count - 1; i++)
                {
                    topology[i].FirstBlock++;
                    topology[i].SecondBlock++;
                }
            }
            var block = blocks.Find(a => a.Name == name);
            block.Count -= 1;

            if(block.Count == 0)
            {
                blocks.Remove(block);
            }

            side.EndPoint = TraceBuilder.MakeStep(side.EndPoint, name, 1);

            var step = TraceBuilder.MakeStep(side.StartPoint, name, 1);

            pointShift = new Point(step.X - side.StartPoint.X,
                step.Y - side.StartPoint.Y);

            side.AddShiftToDetails(pointShift, 1);

            side.details.Add(step);

            side.EndIndex++;

            topology[topology.Count - 1].FirstBlock++;
        }

        private static void UpdateSides(Point pointShift, List<RouteSide> sides, int i, int endIndex)
        {
            var maxValue = endIndex + 1;

            if(maxValue > sides.Count)
            {
                maxValue = sides.Count;
            }

            for (int j = i + 1; j < maxValue; j++)
            {
                if (j != 0)
                    sides[j].StartIndex++;
                if (j != 4)
                    sides[j].EndIndex++;
                sides[j].StartPoint.X += pointShift.X;
                sides[j].StartPoint.Y += pointShift.Y;
                sides[j].EndPoint.X += pointShift.X;
                sides[j].EndPoint.Y += pointShift.Y;

                sides[j].AddShiftToDetails(pointShift, 0);
            }
        }

        private static (string, int) GetExpandData(ref Point detail, Point nextPoint)
        {
            (string, int) expandData = (string.Empty, 0);
            var xShift = nextPoint.X - detail.X;
            var yShift = nextPoint.Y - detail.Y;

            if (Math.Abs(xShift) < Math.Abs(yShift)
                && Math.Abs(yShift) >= 0.5)
            {
                if (yShift > 0)
                {
                    expandData = ("Up", (int)Math.Round(Math.Abs(yShift)));
                }
                else
                {
                    expandData = ("Down", (int)Math.Round(Math.Abs(yShift)));
                }
            }
            else if (Math.Abs(xShift) >= 0.5)
            {
                if (xShift > 0)
                {
                    expandData = ("Right", (int)Math.Round(Math.Abs(xShift)));
                }
                else
                {
                    expandData = ("Left", (int)Math.Round(Math.Abs(xShift)));
                }
            }

            return expandData;
        }
    }
}
