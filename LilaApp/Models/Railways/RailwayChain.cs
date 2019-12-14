using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LilaApp.Algorithm;

[assembly: InternalsVisibleTo("LilaAppTests")]
namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Цепочка блоков рельсов.
    /// Состоит из нескольких соединенных подряд блоков.
    /// Реализует интерфейс шаблона железной дороги.
    /// </summary>
    public class RailwayChain : IRailwayTemplate
    {
        #region Fields

        private IRailwayTemplate _head;
        private IRailwayTemplate _tail;

        private Point _start;

        private IRailwayTemplate _next;
        private IRailwayTemplate _prev;

        /// <summary>
        /// Индексатор
        /// </summary>
        public IRailwayTemplate this[int index] {
            get {
                var item = _head;

                for (var i = 0; i < Math.Abs(index); i++)
                {
                    item = (index > 0) ? item.Next : item.Prev;
                }

                return item;
            }
        }
        #endregion

        #region .ctor

        /// <summary>
        /// Цепочка блоков рельсов
        /// </summary>
        /// <param name="railways">Блоки рельсов</param>
        public RailwayChain(params IRailwayTemplate[] railways)
        {
            for (var i = 0; i < railways.Length; i++)
            {
                if (i == 0)
                {
                    _head = railways[i];
                    _tail = _head;
                    continue;
                }

                _tail = _tail.Append(railways[i]);
            }

            Dimensions = new TemplateDimensions(this, false);
        }

        #endregion

        #region Implementation of IRailwayTemplate

        /// <summary>
        /// Следующий шаблон железной дороги
        /// </summary>
        public IRailwayTemplate Next {
            get => _next;
            set {
                _next = value;
                _tail.Next = value;
            }
        }

        /// <summary>
        /// Предыдущий шаблон железной дороги
        /// </summary>
        public IRailwayTemplate Prev {
            get => _prev;
            set {
                _prev = value;
                _head.Prev = value;
            }
        }

        /// <summary>
        /// Точка начала текущего шаблона железной дороги
        /// </summary>
        public Point Start {
            get => _start;
            set {
                _start = value;

                for (var element = _head; element != _tail.Next; element = element.Next)
                {
                    if (element == _head)
                    {
                        // Задаём стартовую точку для первого блока в цепочке
                        element.Start = _start;
                    }
                    else
                    {
                        // Присоединяем остальные блоки друг за другом,
                        element.Prev.Append(element);
                    }
                }
            }
        }

        /// <summary>
        /// Точка окончания текущего шаблона железной дороги
        /// </summary>
        public Point End => _tail.End;

        /// <summary>
        /// Шаблон железной дороги, симметричный текущему.
        /// Если добавить после текущего шаблона блок L1,
        /// то для того, чтобы путь остался замкнутым,
        /// надо добавить после симметричного ещё один блок L1.
        /// (при этом направления блоков L1 должны быть противоположными)
        /// </summary>
        public IRailwayTemplate Symmetric { get; set; }

        /// <summary>
        /// Создать копию шаблона, не привязанную к другим шаблонам
        /// </summary>
        /// <returns></returns>
        public IRailwayTemplate Clone()
        {
            var list = new List<IRailwayTemplate>();

            for (var element = _head; element != _tail.Next; element = element.Next)
            {
                list.Add(element.Clone());
            }

            return new RailwayChain(list.ToArray());
        }

        /// <summary>
        /// Преобразование шаблона железной дороги к списку отдельных блоков рельсов
        /// </summary>
        /// <returns></returns>
        public List<Railway> GetRailways()
        {
            var list = new List<Railway>();

            for (var element = _head; element != _tail.Next; element = element.Next)
            {
                switch (element)
                {
                    case Railway railway:
                        list.Add(railway);
                        break;

                    default:
                        list.AddRange(element.GetRailways());
                        break;
                }
            }

            return list;
        }

        /// <summary>
        /// Размеры шаблона
        /// </summary>
        public TemplateDimensions Dimensions { get; }

        #endregion

        #region Implementation of IScalableTemplate

        /// <summary>
        /// Есть ли возможность изменить размер шаблона
        /// в указанном или любом направлении
        /// </summary>
        /// <param name="angle">Направление, если null - то любое</param>
        public bool CanScale(double? angle = null)
        {
            var applicants = new List<IRailwayTemplate>();

            for (var element = _head; element != _tail.Next; element = element.Next)
            {
                if (element.Symmetric != null)
                {
                    applicants.Add(element);
                }
            }

            // Если указано конкретное направление
            if (angle != null)
            {
                applicants = applicants.Where(_ => Math.Abs((double)angle - _.End.Angle) < Constants.Precision).ToList();

                return applicants.Any();
            }

            // Если направление не задано
            return applicants.Any(_ => _.Symmetric != null);
        }

        /// <summary>
        /// Вызвать изменение размера
        /// в указанном или любом направлении
        /// </summary>
        /// <param name="angle">Направление, если null - то любое</param>
        /// <param name="template">Шаблон для вставки. Если null - то вставить любой блок</param>
        /// <returns>True - если получилось, иначе - false</returns>
        public bool TryScale(double? angle = null, IRailwayTemplate template = null)
        {
            if (!CanScale(angle)) return false;

            var applicants = new List<IRailwayTemplate>();

            for (var element = _head; element != _tail.Next; element = element.Next)
            {
                if (element.Symmetric != null)
                {
                    applicants.Add(element);
                }
            }

            if (angle != null)
            {
                applicants = applicants.Where(_ => Math.Abs((double)angle - _.End.Angle) < Constants.Precision).ToList();
            }

            // TODO: выбрать блок среди доступных (L1, L2, L3, L4), т.к. L1 может не быть

            var applicant = applicants.FirstOrDefault();
            if (applicant == null) return false;

            applicant.AppendSymmetric(template ?? Railway.L1);

            return true;
        }

        #endregion

        #region Implementation of IMutableTemplate

        /// <summary>
        /// Есть ли возможность применить мутацию к шаблону
        /// </summary>
        /// <param name="dimensions">Размеры шаблона, который хотим вставить. Если null - то любой</param>
        /// <returns></returns>
        public bool CanMutate(TemplateDimensions dimensions = null)
        {
            if (dimensions != null)
            {
                // Если размеры предлагаемого шаблона совпадают с размерами текущего, то можно просто заменить шаблон
                if (dimensions.Output != this.Dimensions.Output) return true;

                // Ищем среди дочерних элементов цепочку, эквивалентную предлагаемому шаблону
                if (FindSubTemplate(Dimensions.Output) != null) return true;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Применить мутацию
        /// </summary>
        /// <param name="template">Шаблон для вставки. Если null - то применить любую мутацию на своё усмотрение</param>
        public virtual bool TryMutate(IRailwayTemplate template = null)
        {
            if (!CanMutate(template?.Dimensions)) return false;

            if (template != null)
            {
                var indexes = FindSubTemplate(template.Dimensions.Output);
                if (indexes == null) return false;
                var (start, end) = indexes.Value;

                // Присоединяем новый шаблон
                start.Prev.Next = template;
                template.Prev = start.Prev;

                end.Next.Prev = template;
                template.Next = end.Next;

                template.Start = start.Start;

                // Записываем в словарь направления всех точек стыка нового шаблона
                var directions = (template is RailwayChain chain) ? chain.GetDirections() : null;

                // Заменяем связи симметрии
                for (var i = start; i != end.Next; i = i.Next)
                {
                    var symmetric = i.Symmetric;
                    
                    if (symmetric == null) continue;

                    if (DirectionExtensions.FromAngle(i.End.Angle) is Direction dir)
                    {
                        if (directions?.ContainsKey(dir) == true && directions[dir].Count > 0)
                        {
                            var fromTemplate = directions[dir].First();

                            symmetric.Symmetric = fromTemplate;
                            fromTemplate.Symmetric = symmetric;

                            continue;
                        }
                    }

                    symmetric.Symmetric = null;
                    i.Symmetric = null;
                }

                return true;
            }

            return false;

            // TODO: сделать случайную мутацию
        }

        #endregion

        /// <summary>
        /// Поиск под-шаблона среди дочерних элементов, подходящего под указанную точку выхода
        /// </summary>
        /// <param name="templateOutput">Точка выхода из шаблона, из расчёта, что шаблон присоединяется к точке (0,0,↑)</param>
        /// <returns>начало и окончание цепочки или null, если не не удалось найти</returns>
        internal (IRailwayTemplate start, IRailwayTemplate end)? FindSubTemplate(Point templateOutput)
        {
            var applicants = new List<(int length, IRailwayTemplate start, IRailwayTemplate end)>();

            for (var i = _head; i != _tail.Next; i = i.Next)
            {
                var length = 0;
                var start = i.Start;
                var angle = start.Angle;
                start = MathFunctions.RotateCoordinates(angle, start);

                for (var j = i; j != _tail.Next; j = j.Next)
                {
                    length++;
                    var end = j.End;
                    end = MathFunctions.RotateCoordinates(angle, end);

                    var output = new Point(end.X - start.X, end.Y - start.Y, end.Angle - start.Angle);

                    if (output == templateOutput)
                    {
                        applicants.Add((length, i, j));
                    }
                }
            }

            if (applicants.Count == 0) return null;

            var min = 0;

            for (var i = 0; i < applicants.Count; i++)
            {
                if (applicants[i].length < applicants[min].length)
                {
                    min = i;
                }
            }

            return (applicants[min].start, applicants[min].end);

        }

        /// <summary>
        /// Получить список направлений всех точек стыка
        /// </summary>
        /// <returns></returns>
        public Dictionary<Direction, List<IRailwayTemplate>> GetDirections()
        {
            var directions = new Dictionary<Direction, List<IRailwayTemplate>>();

            for (var i = _head; i != _tail.Next; i = i.Next)
            {
                if (!(DirectionExtensions.FromAngle(i.End.Angle) is Direction dir)) continue;

                if (!directions.ContainsKey(dir)) directions[dir] = new List<IRailwayTemplate>();

                directions[dir].Add(i);
            }

            return directions;
        }
    }
}
