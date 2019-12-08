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
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _model = model;
            _checker = checker;

            _answer = Model.Copy(_model);

            Run();

            return new FinalAnswer()
            {
                Model = _answer,
                Price = 0,
            };
        }

        #endregion

        public event EventHandler<Model> OnStepEvent;
        public CancellationToken cancellationToken;

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

            HelperActions.InitialRouteBuilder(order,
                topology, turn, blocks);

            _answer.Order = order;
            _answer.Topology = topology;

            _newOrder = new List<string>(order);
            _newTopology = (topology.Select(item => new TopologyItem(item))).ToList();

            var trace = TraceBuilder.CalculateTrace(_answer);
            traceIncome = DirectTaskSolver.GetRoutePrice(_answer, trace.Points);

            oldOrder = order.GetRange(0, order.Count);
            oldTopology = topology.GetRange(0, topology.Count);

            sides.Add(new RouteSide(trace.Points[0], 0));
            sides.Add(new RouteSide(trace.Points[2], 2));
            sides.Add(new RouteSide(trace.Points[4], 4));
            sides.Add(new RouteSide(trace.Points[6], 6));
            sides.Add(new RouteSide(trace.Points[0], trace.Points[0], 8, 0));

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
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var waypoint in pointsCopy)
                    {
                        double length = 0;
                        minRouteLength = double.MaxValue;
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
                            UpdateSides(pointShift, sides, sides.IndexOf(action.Item2));
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
                                    UpdateSides(pointShift, sides, sides.IndexOf(action.Item2));
                                    SearchSide(action.Item2, detailName);
                                }
                            }

                            break;
                        }
                    }
                    _answer.Order = order;
                    _answer.Topology = topology;
                    OnStepEvent.Invoke(this, _answer);

                    trace = TraceBuilder.CalculateTrace(_answer);
                    var newTraceIncome = DirectTaskSolver.GetRoutePrice(_answer, trace.Points);

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

            OnStepEvent.Invoke(this, _answer);
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
            string detailName = directBlocks[directBlocks.Count() - 1].Name;

            foreach (var block in directBlocks)
            {
                var blockLength = int.Parse(block.Name[1].ToString());

                if ((expand.length - blockLength) < 0.5)
                {
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
            topology.Insert(index, new TopologyItem(index - 1, index, 1));

            for (int i = index; i < topology.Count - 1; i++)
            {
                topology[i].FirstBlock++;
                topology[i].SecondBlock++;
            }

            blocks.Find(a => a.Name == name).Count -= 1;

            side.EndPoint = TraceBuilder.MakeStep(side.EndPoint, name, 1);

            var step = TraceBuilder.MakeStep(side.StartPoint, name, 1);

            pointShift = new Point(step.X - side.StartPoint.X,
                step.Y - side.StartPoint.Y);

            side.AddShiftToDetails(pointShift, 1);

            side.details.Add(step);

            topology[topology.Count - 1].FirstBlock++;
        }

        private static void UpdateSides(Point pointShift, List<RouteSide> sides, int i)
        {
            for (int j = i + 1; j < sides.Count; j++)
            {
                sides[j].StartIndex++;
                if (sides[j].EndIndex != 0)
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
                    expandData = ("Up", (int)Math.Abs(yShift));
                }
                else
                {
                    expandData = ("Down", (int)Math.Abs(yShift));
                }
            }
            else if (Math.Abs(xShift) >= 0.5)
            {
                if (xShift > 0)
                {
                    expandData = ("Right", (int)Math.Abs(xShift));
                }
                else
                {
                    expandData = ("Left", (int)Math.Abs(xShift));
                }
            }

            return expandData;
        }
    }
}
