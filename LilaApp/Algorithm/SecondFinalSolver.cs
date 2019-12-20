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
        private List<Block> _newStraightBlocks;
        private List<Block> _newTurnBlocks;
        private List<TopologyItem> _newTopology;
        private int _turn;

        //Коэффециент для подсчёта оптимальной стоимости деталей
        private double _priceC = 1;
        private static double traceLengthForTurn = Constants.RADIUS * 4;

        private (string side, int length) _previousExpand;
        private Point _previousPoint;
        private Point _previousDetail;

        private void Run()
        {
            var blocks = _answer.Blocks;
            var points = _answer.Points;
            TracePrice traceIncome;
            Point pointShift;

            _priceC = 1;

            foreach (var block in blocks)
            {
                if (block.Name.StartsWith("L"))
                {
                    _priceC *= int.Parse(block.Name[1].ToString());
                }
            }

            var sortedStraightBlocks = blocks.Where(a => a.Name[0].ToString() == "L")
                .OrderBy(a => a.Price * (_priceC / int.Parse(a.Name[1].ToString())))
                .Select(a => a).ToList();

            var sortedTurnBlocks = blocks.Where(a => a.Name[0].ToString() == "T")
                .OrderBy(a => a.Price * (int.Parse(a.Name[1].ToString()) / 2))
                .Select(a => a).ToList();

            var order = new List<string>();
            var topology = new List<TopologyItem>();

            var sides = new List<RouteSide>();

            _turn = HelperActions.GetRichestSide(points);

            HelperActions.InitialRouteBuilder(_answer,
                _turn, sortedTurnBlocks, sides);

            var trace = TraceBuilder.CalculateTrace(_answer);
            traceIncome = _checker.Solve(_answer);
            trace.Points = trace.Points.Take(trace.Points.Length - 1).ToArray();

            _newOrder = new List<string>(_answer.Order);
            _newStraightBlocks = (sortedStraightBlocks
                .Select(item => new Block(item.Name, item.Count, item.Price))).ToList();
            _newTurnBlocks = (sortedTurnBlocks
                .Select(item => new Block(item.Name, item.Count, item.Price))).ToList();
            _newTopology = (_answer.Topology.Select(item => new TopologyItem(item))).ToList();
            topology = _newTopology;
            order = _newOrder;
            _newOrder = new List<string>(_answer.Order);
            _newTopology = (_answer.Topology.Select(item => new TopologyItem(item))).ToList();
            OnStepEvent.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));

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

                    if (minLength < 1)
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

                if (expand.side == _previousExpand.side &&
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
                        AddElement(detailName, action.Item2, out pointShift, order, topology, sortedStraightBlocks);
                        UpdateSides(pointShift, sides, sides.IndexOf(action.Item2), action.Item2.SideEndIndex);
                        return true;
                    }

                    return false;
                }

                for (int i = 0; i < sides.Count; i++)
                {
                    if (IsDetailBelongsToSide(sides, detail, i))
                    {
                        string detailName = GetDetail(expand, sortedStraightBlocks);

                        if (detailName == string.Empty)
                        {
                            break;
                        }

                        var currentSide = sides[i];
                        int tries = 0;
                        while (true)
                        {
                            if (tries > 100)
                            {
                                if (i == sides.Count - 1)
                                {
                                    trace = addReverse(blocks,ref sortedStraightBlocks, sortedTurnBlocks, order, topology, sides, sides[i].Turn, trace, sides[i - 1].EndIndex, ref pointsCopy);
                                }
                                else
                                {
                                    trace = addReverse(blocks,ref sortedStraightBlocks, sortedTurnBlocks, order, topology, sides, sides[i].Turn, trace, sides[i].EndIndex, ref pointsCopy);
                                }
                                break;
                            }

                            var nextSide = currentSide.actions
                                                    .Where(a => a.command == expand.side)
                                                    .Select(a => a)?.FirstOrDefault().Item2;

                            if (nextSide == null)
                            {
                                break;
                            }

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

                var newTraceIncome = _checker.Solve(_answer);

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
            List<Block> blocks, ref List<Block> sortedStraightElements,
            List<Block> sortedTurnElements, List<string> order,
            List<TopologyItem> topology, List<RouteSide> sides,
            int turn, TraceBuilder.Result trace, int index,
            ref List<Point> pointsCopy)
        {
            if (blocks.Find(_ => _.Name.StartsWith("B"))?.Count > 0 
                && sortedStraightElements.Find(_ =>_.Name == "L1")?.Count > 2)
            {
                var startIndex = index;
                double angle = 0;
                try
                {
                    angle = trace.Points[index].Angle;
                }
                catch(Exception ex)
                {

                }
                //Номер стороны от которой создается круг
                var sideIndex = sides.FindIndex(a =>
                           a.StartIndex - index == 0 || a.EndIndex - index == 0);

                if(sides[sideIndex].TurnsNum == 0)
                {
                    return trace;
                }

                var copyOrder = new List<string>(order);
                var copyTopology = (topology.Select(item => new TopologyItem(item))).ToList();
                var copySides = (sides.Select(item => new RouteSide(item))).ToList();

                for (int k = 0; k < sides[sideIndex].TurnsNum; k++)
                {
                    order.RemoveAt(index);
                    topology.RemoveAt(index);
                }


                for (int i = index; i < topology.Count - 1; i++)
                {
                    topology[i].FirstBlock -= sides[sideIndex].TurnsNum;
                    topology[i].SecondBlock -= sides[sideIndex].TurnsNum;
                }

                topology[topology.Count - 1].FirstBlock -= sides[sideIndex].TurnsNum;

                _answer.Order = order;
                _answer.Topology = topology;

                OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));
                ///
                int insertIndex = sideIndex + 1;
                try
                {
                    AddReverseSide(ref sortedStraightElements, sortedTurnElements,
                        order, topology, sides, turn * -1, out trace, ref index,
                        insertIndex, 6);
                    ///
                    insertIndex++;
                    AddReverseSide(ref sortedStraightElements, sortedTurnElements,
                        order, topology, sides, turn * -1, out trace, ref index,
                        insertIndex, 0);

                    insertIndex++;

                    AddReverseSide(ref sortedStraightElements, sortedTurnElements,
                        order, topology, sides, turn * -1, out trace, ref index,
                        insertIndex, 0);

                    insertIndex++;

                    AddReverseSide(ref sortedStraightElements, sortedTurnElements,
                        order, topology, sides, turn * -1, out trace, ref index,
                        insertIndex, -1);

                    blocks.Find(a => a.Name == "B1").Count -= 1;
                }
                catch (Exception ex)
                {
                    _answer.Order = copyOrder;
                    order = copyOrder;
                    _answer.Topology = copyTopology;
                    topology = copyTopology;
                    sides = copySides;
                    trace = TraceBuilder.CalculateTrace(_answer);
                    OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));
                    return trace;
                }

                _answer.Order = order;
                _answer.Topology = topology;

                OnStepEvent?.Invoke(this, new FinalAnswer(_answer, _checker.Solve(_answer)));

                for (int i = 0; i < sides.Count; i++)
                {
                    sides[i].SideEndIndex += 4;
                }

                insertIndex = sideIndex + 1;
                if (turn == 1)
                {
                    switch (angle)
                    {
                        // вверх влево
                        case (0):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 1], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex + 3], true));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex + 1], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 1], true));

                            break;
                        // влево вниз
                        case (Math.PI / 2):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 2], true));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex], true));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 2], false));

                            break;
                        // вправо вверх
                        case (3 * Math.PI / 2):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 2], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex + 2], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex + 2], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 2], false));

                            break;
                        // вправо вниз
                        case (Math.PI):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex], false));

                            break;
                    }
                }
                else
                {
                    switch (angle)
                    {
                        // вверх вправо
                        case (0):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex + 2], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex], false));

                            //ChangeSidePointer(sides, insertIndex, "Right");
                            //ChangeSidePointer(sides, insertIndex, "Up");

                            break;
                        // влево вверх +
                        case (Math.PI / 2):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 2], true));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 2], true));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex + 2], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex], true));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 2], false));

                            //ChangeSidePointer(sides, insertIndex, "Left");
                            //ChangeSidePointer(sides, insertIndex, "Left");
                            break;
                        // вправо вниз+
                        case (3 * Math.PI / 2):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 2], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex + 3], true));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex], false));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex + 1], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex + 2], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex + 1], true));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex + 2], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex], false));

                            //ChangeSidePointer(sides, insertIndex, "Left");
                            //ChangeSidePointer(sides, insertIndex, "Up");
                            break;
                        // влево вниз
                        case (Math.PI):
                            sides[insertIndex].actions.Add(("Up", sides[insertIndex + 3], false));
                            sides[insertIndex].actions.Add(("Down", sides[insertIndex + 2], true));
                            sides[insertIndex].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex].actions.Add(("Left", sides[insertIndex + 1], false));

                            sides[insertIndex + 1].actions.Add(("Up", sides[insertIndex + 3], false));
                            sides[insertIndex + 1].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 1].actions.Add(("Left", sides[insertIndex + 3], true));

                            sides[insertIndex + 2].actions.Add(("Up", sides[insertIndex + 3], false));
                            sides[insertIndex + 2].actions.Add(("Down", sides[insertIndex], true));
                            sides[insertIndex + 2].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 2].actions.Add(("Left", sides[insertIndex + 1], false));

                            sides[insertIndex + 3].actions.Add(("Up", sides[insertIndex + 3], false));
                            sides[insertIndex + 3].actions.Add(("Down", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Right", sides[insertIndex], false));
                            sides[insertIndex + 3].actions.Add(("Left", sides[insertIndex + 1], true));

                            //ChangeSidePointer(sides, insertIndex, "Right");
                            //ChangeSidePointer(sides, insertIndex, "Down");
                            break;
                    }
                }

                var Xcentre = sides[insertIndex].EndPoint.X - (sides[insertIndex].EndPoint.X - sides[insertIndex + 2].EndPoint.X) / 2;
                var Ycentre = sides[insertIndex].EndPoint.Y - (sides[insertIndex].EndPoint.Y - sides[insertIndex + 2].EndPoint.Y) / 2;

                var circleCentre = new Point(Xcentre, Ycentre);

                pointsCopy = pointsCopy.Where(a =>
                {
                    if (MathFunctions.GetDistanceToPoint(circleCentre, a) < Constants.RADIUS + 1)
                    {
                        return false;
                    }

                    return true;
                }).ToList();

                for (int i = insertIndex + 4; i < sides.Count; i++)
                {
                    sides[i].StartIndex += index - startIndex - sides[sideIndex].TurnsNum;

                    if (sides[i].EndIndex != 0)
                        sides[i].EndIndex += index - startIndex - sides[sideIndex].TurnsNum;
                }
                sides[sideIndex].TurnsNum = 0;
            }

            return trace;
        }

        private void AddReverseSide(ref List<Block> sortedStraightElements,
            List<Block> sortedTurnElements, List<string> order,
            List<TopologyItem> topology, List<RouteSide> sides,
            int turn, out TraceBuilder.Result trace, ref int index,
            int insertIndex, int staightLength)
        {
            List<string> firstSide = new List<string>();
            List<string> firstTurn = new List<string>();

            if (staightLength == -1)
            {

                if (sortedStraightElements.Find(a => a.Name == "L1").Count >= 2)
                {
                    firstSide.AddRange(new string[] { "L1", "B1", "L1" });
                    firstTurn.AddRange(new string[] { "L1", "B1", "L1" });
                    sortedStraightElements.Find(a => a.Name == "L1").Count -= 2;
                }
                else
                {
                    throw new Exception("Не хватает эл-ов");
                }
            }
            else
            {
                firstSide = HelperActions.GetStraightElements(ref sortedStraightElements, staightLength);
                firstTurn = firstSide.Concat(HelperActions.GetTurnElements(sortedTurnElements)).ToList();
            }

            order.InsertRange(index, firstTurn);
            for (int j = firstTurn.Count() - 1; j >= 0; j--)
            {
                topology.Insert(index, new TopologyItem(index + j, index + j + 1, turn));
            }

            for (int i = index + firstTurn.Count(); i < topology.Count - 1; i++)
            {
                topology[i].FirstBlock += firstTurn.Count();
                topology[i].SecondBlock += firstTurn.Count();
            }

            topology[topology.Count - 1].FirstBlock += firstTurn.Count();

            trace = TraceBuilder.CalculateTrace(_answer);
            trace.Points = trace.Points.Take(trace.Points.Length - 1).ToArray();

            if (staightLength == 0)
            {
                sides.Insert(insertIndex, new RouteSide(
                    trace.Points[index],
                    index, sides[0].SideEndIndex,
                    turn, firstTurn.Count - firstSide.Count));
            }
            else
            {
                sides.Insert(insertIndex, new RouteSide(
                    trace.Points[index + 1],
                    trace.Points[index + firstSide.Count],
                    index + 1, index + firstSide.Count,
                    sides[0].SideEndIndex, turn, firstTurn.Count - firstSide.Count));
            }
            for (int i = 0; i < firstTurn.Count(); i++)
            {
                sides[insertIndex].details.Add(trace.Points[index + i]);
            }

            index += firstTurn.Count();
        }

        private static void ChangeSidePointer(List<RouteSide> sides, int insertIndex, string command)
        {
            var oldSide = sides[insertIndex - 1].actions;
            var oldIndex = oldSide.FindIndex(a => a.command == command);
            var b = oldSide[oldIndex];
            b.Item2 = sides[insertIndex];
            oldSide[oldIndex] = b;
            sides[insertIndex - 1].actions = oldSide;
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
            string detailName = string.Empty;

            foreach (var block in directBlocks)
            {
                if (block.Count < 2)
                {
                    continue;
                }

                var blockLength = int.Parse(block.Name[1].ToString());

                if (blockLength <= expand.length)
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
            var block = blocks.Find(a => a.Name == name);

            if (block == null || block.Count < 1)
            {
                pointShift = new Point(0, 0);
                return;
            }

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

            block.Count -= 1;

            if (block.Count <= 0)
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
                && Math.Abs(yShift) >= 1)
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
            else if (Math.Abs(xShift) >= 1)
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
