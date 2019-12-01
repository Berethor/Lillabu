using System;
using System.Linq;
using System.Text;
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

            _answer = new Model(model);
            Run();

            return new FinalAnswer()
            {
                Model = _answer,
                Price = 0,
            };
        }

        #endregion

        public event EventHandler<Model> OnStepEvent;

        private Model _model;
        private Model _answer;
        private IDirectTaskSolver _checker;

        private static double traceLengthForTurn = Constants.RADIUS * 4;

        private void Run()
        {
            var blocks = _model.Blocks;
            var points = _model.Points;
            double traceIncome;
            Point pointShift;

            var order = new List<string>();
            var oldOrder = new List<string>();
            var topology = new List<TopologyItem>();
            var oldTopology = new List<TopologyItem>();

            var sides = new List<RouteSide>();

            var turn = HelperActions.GetRichestSide(points);

            HelperActions.InitialRouteBuilder(order,
                topology, turn);

            _answer.Order = order;
            _answer.Topology = topology;

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

            sides[1].actions.Add(("Up", sides[0], false));
            sides[1].actions.Add(("Down", sides[4], false));
            sides[1].actions.Add(("Right", sides[3], true));
            sides[1].actions.Add(("Left", sides[3], false));

            sides[2].actions.Add(("Up", sides[0], true));
            sides[2].actions.Add(("Down", sides[4], true));
            sides[2].actions.Add(("Right", sides[1], false));
            sides[2].actions.Add(("Left", sides[3], false));

            sides[3].actions.Add(("Up", sides[0], false));
            sides[3].actions.Add(("Down", sides[4], false));
            sides[3].actions.Add(("Right", sides[1], false));

            sides[4].actions.Add(("Up", sides[0], false));
            sides[4].actions.Add(("Down", sides[2], true));
            sides[4].actions.Add(("Right", sides[1], false));
            sides[4].actions.Add(("Left", sides[3], false));

            double minLength = double.MaxValue;
            double minRouteLength = double.MaxValue;
            var detail = new Point(0, 0);
            var nextPoint = new Point(0, 0);
            var plannedPoint = new Point(0, 0);


            while (true)
            {
                foreach (var waypoint in points)
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

                    if (minLength < 1)
                    {
                        continue;
                    }

                    if (minLength < minRouteLength)
                    {
                        minRouteLength = minLength;
                        detail = plannedPoint;
                        nextPoint = waypoint;
                    }
                }

                string expandSide = GetExpandSide(ref detail, nextPoint);

                void SearchSide(RouteSide secondSide)
                {
                    foreach (var action in secondSide.actions
                        .Where(action => action.command == expandSide)
                        .Select(action => action))
                    {
                        AddElement("L1", action.Item2, out pointShift, order, topology);
                        UpdateSides(pointShift, sides, sides.IndexOf(action.Item2));
                    }
                }

                for (int i = 0; i < sides.Count; i++)
                {
                    if (detail.Angle == sides[i].StartPoint.Angle &&
                        (detail.X - sides[i].StartPoint.X < 1||
                         detail.Y - sides[i].StartPoint.Y < 1))
                    {
                        foreach (var action in sides[i].actions
                            .Where(action => action.command == expandSide)
                            .Select(action => action))
                        {
                            AddElement("L1", action.Item2, out pointShift, order, topology);
                            UpdateSides(pointShift, sides, sides.IndexOf(action.Item2));
                            SearchSide(action.Item2);
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
                    continue;
                }
                break;
            }

            _answer.Order = order;
            _answer.Topology = topology;

            OnStepEvent.Invoke(this, _answer);
        }

        private static void AddElement(string name, RouteSide side, out Point pointShift, List<string> order, List<TopologyItem> topology)
        {
            var index = side.StartIndex;
            order.Insert(index, name);
            topology.Insert(index, new TopologyItem(index - 1, index, 1));

            for (int i = index; i < topology.Count - 1; i++)
            {
                topology[i].FirstBlock++;
                topology[i].SecondBlock++;
            }

            side.EndPoint = TraceBuilder.MakeStep(side.EndPoint, name, 1);

            var step = TraceBuilder.MakeStep(side.StartPoint, name, 1);

            pointShift = new Point(step.X - side.StartPoint.X,
                step.Y - side.StartPoint.Y);

            topology[topology.Count - 1].FirstBlock++;
        }

        private static void UpdateSides(Point pointShift, List<RouteSide> sides, int i)
        {
            for (int j = i + 1; j < sides.Count; j++)
            {
                sides[j].StartIndex++;
                if(sides[j].EndIndex != 0)
                    sides[j].EndIndex++;
                sides[j].StartPoint.X += pointShift.X;
                sides[j].StartPoint.Y += pointShift.Y;
                sides[j].EndPoint.X += pointShift.X;
                sides[j].EndPoint.Y += pointShift.Y;
            }
        }

        private static string GetExpandSide(ref Point detail, Point nextPoint)
        {
            string expandSide = string.Empty;
            var xShift = nextPoint.X - detail.X;
            var yShift = nextPoint.Y - detail.Y;

            if (Math.Abs(xShift) < Math.Abs(yShift)
                && Math.Abs(yShift) >= 1)
            {
                if (yShift > 0)
                {
                    expandSide = "Up";
                }
                else
                {
                    expandSide = "Down";
                }
            }
            else if (Math.Abs(xShift) >= 1)
            {
                if (xShift > 0)
                {
                    expandSide = "Right";
                }
                else
                {
                    expandSide = "Left";
                }
            }

            return expandSide;
        }
    }
}
