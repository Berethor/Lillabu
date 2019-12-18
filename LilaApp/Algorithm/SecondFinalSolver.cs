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
        private int _turn;
        private static double traceLengthForTurn = Constants.RADIUS * 4;

        private (string side, int length) _previousExpand;
        private Point _previousPoint;
        private Point _previousDetail;

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

            _turn = HelperActions.GetRichestSide(points);

            HelperActions.InitialRouteBuilder(order,
                topology, _turn, blocks);

            _answer.Order = order;
            _answer.Topology = topology;

            _newOrder = new List<string>(order);
            _newTopology = (topology.Select(item => new TopologyItem(item))).ToList();

            var trace = TraceBuilder.CalculateTrace(_answer);
            traceIncome = DirectTaskSolver.GetRouteIncome(_answer, trace.Points);
            trace.Points = trace.Points.Take(trace.Points.Length - 2).ToArray();

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
            sides[1].actions.Add(("Right", sides[3], Convert.ToBoolean(_turn + 1)));
            sides[1].actions.Add(("Left", sides[3], !Convert.ToBoolean(_turn + 1)));
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
            sides[3].actions.Add(("Right", sides[1], Convert.ToBoolean(_turn + 1)));
            sides[3].actions.Add(("Left", sides[1], !Convert.ToBoolean(_turn + 1)));
            sides[3].details.Add(trace.Points[6]);
            sides[3].details.Add(trace.Points[7]);

            sides[4].actions.Add(("Up", sides[0], false));
            sides[4].actions.Add(("Down", sides[2], true));
            sides[4].actions.Add(("Right", sides[1], false));
            sides[4].actions.Add(("Left", sides[3], false));
            sides[4].details.Add(trace.Points[0]);

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

            var circleCentre = new Point(_turn * Constants.RADIUS, 0);
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

                    if(expand.side == _previousExpand.side &&
                       expand.length == _previousExpand.length &&
                       detail.Equals(_previousDetail) && nextPoint.Equals(_previousPoint)) 
                    {
                        pointsCopy = pointsCopy.Where(a => !a.Equals(_previousPoint)).ToList();
                        _answer.Order = _newOrder;
                        _answer.Topology = _newTopology;
                        OnStepEvent.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));
                        continue;
                    }

                    bool TryAddToSide(RouteSide secondSide, string detailName)
                    {
                        foreach (var action in secondSide.actions
                            .Where(action => action.command == expand.side && action.action)
                            .Select(action => action))
                        {
                            AddElement(detailName, action.Item2, out pointShift, order, topology, blocks);
                            UpdateSides(pointShift, sides, sides.IndexOf(action.Item2), action.Item2.SideEndIndex);
                            return true;
                        }

                        return false;
                    }
                    
                    for (int i = 0; i < sides.Count; i++)
                    {
                        if (IsDetailBelongsToSide(sides, detail, i))
                        {
                            var directBlocks = blocks.FindAll(a => a.Name.StartsWith("L"));

                            string detailName = GetDetail(expand, directBlocks);
                            var currentSide = sides[i];
                            int tries = 0;
                            while (true)
                            {
                                if(tries > 100)
                                {
                                    trace = addReverse(blocks, order, topology, sides, _turn, trace, sides[i].EndIndex, ref pointsCopy);
                                    break;
                                }

                                var nextSide = currentSide.actions
                                                        .Where(a => a.command == expand.side)
                                                        .Select(a => a).First().Item2;

                                if (!TryAddToSide(currentSide, detailName))
                                {
                                    currentSide = nextSide;
                                    tries++;
                                    continue;
                                }
                                else
                                {
                                    TryAddToSide(nextSide, detailName);
                                    break;
                                }
                            }

                            break;
                        }
                    }

                    _answer.Order = order;
                    _answer.Topology = topology;
                    OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));

                    trace = TraceBuilder.CalculateTrace(_answer);
                    trace.Points = trace.Points.Take(trace.Points.Length - 2).ToArray();

                    var newTraceIncome = DirectTaskSolver.GetRouteIncome(_answer, trace.Points);

                    _previousExpand = expand;
                    _previousPoint = nextPoint;
                    _previousDetail = detail;

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

        /// <summary>
        /// Функция для добавления разворотного шаблона вместо поворотного
        /// </summary>
        /// <param name="blocks">Список доступных блоков</param>
        /// <param name="order">Порядок блоков</param>
        /// <param name="topology">Топология</param>
        /// <param name="sides">Стороны Дороги</param>
        /// <param name="turn">Направление шаблона 1 - вправо -1 влево</param>
        /// <param name="trace">Текущая траса</param>
        /// <param name="index">Номер блока в ордере для вставки шаблона</param>
        /// <returns></returns>
        private TraceBuilder.Result addReverse(
            List<Block> blocks, List<string> order,
            List<TopologyItem> topology, List<RouteSide> sides,
            int turn, TraceBuilder.Result trace, int index,
            ref List<Point> pointsCopy)
        {
            if (blocks.Find(_ => _.Name.StartsWith("B")).Count > 0)
            {
                var angle = trace.Points[index].Angle;

                var sideIndex = sides.FindIndex(a =>
                           a.StartIndex - index == 0 || a.EndIndex - index == 0);

                for (int i = 0; i < sides.Count; i++)
                {
                    sides[i].SideEndIndex += 4;
                }

                order.RemoveAt(index);
                order.RemoveAt(index);
                topology.RemoveAt(index);
                topology.RemoveAt(index);


                for (int i = index; i < topology.Count - 1; i++)
                {
                    topology[i].FirstBlock -= 2;
                    topology[i].SecondBlock -= 2;
                }

                topology[topology.Count - 1].FirstBlock -= 2;

                _answer.Order = order;
                _answer.Topology = topology;

                OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));

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
                trace.Points = trace.Points.Take(trace.Points.Length - 2).ToArray();

                var pSidesCount = sides.Count;

                var insertIndex = sideIndex + 1;
                sides.Insert(insertIndex, new RouteSide(
                    trace.Points[index + 1],
                    trace.Points[index + 2],
                    index + 1, index + 2, pSidesCount + 3));

                sides[insertIndex].details.Add(trace.Points[index + 1]);
                sides[insertIndex].details.Add(trace.Points[index + 2]);
                sides[insertIndex].details.Add(trace.Points[index + 3]);

                insertIndex++;

                sides.Insert(insertIndex, new RouteSide(
                    trace.Points[index + 4],
                    index + 4, pSidesCount + 3));

                sides[insertIndex].details.Add(trace.Points[index + 4]);
                sides[insertIndex].details.Add(trace.Points[index + 5]);

                insertIndex++;

                sides.Insert(insertIndex, new RouteSide(
                    trace.Points[index + 6],
                    index + 6, pSidesCount + 3));

                sides[insertIndex].details.Add(trace.Points[index + 6]);
                sides[insertIndex].details.Add(trace.Points[index + 7]);

                insertIndex++;

                sides.Insert(insertIndex, new RouteSide(
                    trace.Points[index + 8],
                    trace.Points[index + 10],
                    index + 8, index + 10, pSidesCount + 3));

                sides[insertIndex].details.Add(trace.Points[index + 8]);
                sides[insertIndex].details.Add(trace.Points[index + 9]);
                sides[insertIndex].details.Add(trace.Points[index + 10]);

                if (_turn == 1)
                {
                    switch (angle)
                    {
                        // вверх влево
                        case (0):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 1], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex + 3], true));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex + 1], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 1], true));

                            break;
                        // влево вниз
                        case (Math.PI / 2):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 2], true));

                            sides[insertIndex + 1].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex], true));

                            sides[insertIndex + 3].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 2], true));

                            break;
                        // вправо вверх
                        case (3 * Math.PI / 2):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[sideIndex], true));

                            break;
                        // вправо вниз
                        case (Math.PI):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Left", sides[sideIndex], false));

                            break;
                    }
                }
                else
                {
                    switch (angle)
                    {
                        // вверх влево
                        case (0):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Left", sides[sideIndex], false));

                            break;
                        // влево вниз
                        case (Math.PI / 2):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 2], true));

                            sides[insertIndex + 1].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex], true));

                            sides[insertIndex + 3].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Right", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 2], true));

                            break;
                        // вправо вверх
                        case (3 * Math.PI / 2):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Down", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[sideIndex], true));

                            break;
                        // вправо вниз
                        case (Math.PI):
                            insertIndex = sideIndex + 1;
                            sides[insertIndex].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[sideIndex], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[sideIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Left", sides[sideIndex], false));

                            break;
                    }
                }
                var Xcentre = sides[insertIndex].EndPoint.X - (sides[insertIndex].EndPoint.X - sides[insertIndex + 2].EndPoint.X) / 2;
                var Ycentre = sides[insertIndex].EndPoint.Y - (sides[insertIndex].EndPoint.Y - sides[insertIndex + 2].EndPoint.Y) / 2;

                var circleCentre = new Point(Xcentre, Ycentre);

                pointsCopy = pointsCopy.Where(a =>
                {
                    if (MathFunctions.GetDistanceToPoint(circleCentre, a) < Constants.RADIUS)
                    {
                        return false;
                    }

                    return true;
                }).ToList();
                for (int i = insertIndex + 4; i < sides.Count; i++)
                {
                    sides[i].StartIndex += 9;

                    if (sides[i].EndIndex != 0)
                        sides[i].EndIndex += 9;
                }
            }

            return trace;
        }

        private static bool IsDetailBelongsToSide(List<RouteSide> sides, Point detail, int i)
        {
            foreach (var d in sides[i].details)
            {
                if (detail.Equals(d))
                {
                    if (i == 0 && detail.X < 0 && detail.Y < 0)
                        return false;
                    return true;
                }
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

            if (block.Count == 0)
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

            if (maxValue > sides.Count)
            {
                maxValue = sides.Count;
            }

            for (int j = i + 1; j < maxValue; j++)
            {
                if (j != 0)
                    sides[j].StartIndex++;
                if (j != maxValue)
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
