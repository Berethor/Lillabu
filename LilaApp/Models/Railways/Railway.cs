using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LilaApp.Models.Railways
{
    using Algorithm;

    /// <summary>
    /// Блок рельсов. Атомарная единица железной дороги.
    /// Блок рельсов реализует интерфейс шаблона железной дороги
    /// </summary>
    [DebuggerDisplay("{Name} [{Start} → {End}]")]
    public class Railway : IRailwayTemplate
    {
        #region Fields

        private Point _start;

        #endregion

        #region Implementation of IRailwayTemplate

        /// <summary>
        /// Следующий шаблон железной дороги
        /// </summary>
        public IRailwayTemplate Next { get; set; }

        /// <summary>
        /// Предыдущий шаблон железной дороги
        /// </summary>
        public IRailwayTemplate Prev { get; set; }

        /// <summary>
        /// Точка начала блока рельсов
        /// </summary>
        public Point Start {
            get => _start;
            set {
                _start = value;
                End = CalculateEndPoint();
            }
        }

        /// <summary>
        /// Точка окончания блока рельсов
        /// </summary>
        public Point End { get; private set; }

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
            return new Railway(Type);
        }

        /// <summary>
        /// Преобразование шаблона железной дороги к списку отдельных блоков рельсов
        /// </summary>
        /// <returns></returns>
        public List<Railway> GetRailways()
        {
            return new List<Railway>(1) { this };
        }

        /// <summary>
        /// Размеры шаблона
        /// </summary>
        public TemplateDimensions Dimensions { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Тип блока рельсов
        /// </summary>
        public RailwayType Type { get; }

        /// <summary>
        /// Название блока, например "L1"
        /// </summary>
        public string Name => MapTypeToName(Type).name;

        /// <summary>
        /// Направление блока: -1 - налево, 1 - направо или прямо 
        /// </summary>
        public int Direction => Type == RailwayType.T4L || Type == RailwayType.T8L ? -1 : 1;

        /// <summary>
        /// Индексатор.
        /// Возвращает i-ый следующий шаблон железной дороги, начиная от текущего блока.
        /// Т.е. this[0] = this, this[1] = this.Next, this[2] = this.Next.Next и т.д.
        /// Допустимы и отрицательные значения: this[-1] = this.Prev и т.д.
        /// </summary>
        /// <param name="index">Индекс следующего шаблона железной дороги, начиная отсчёт от текущего блока</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <returns></returns>
        public IRailwayTemplate this[int index] {
            get {
                IRailwayTemplate item = this;

                for (var i = 0; i < Math.Abs(index); i++)
                {
                    item = (index > 0) ? item.Next : item.Prev;

                    //if (item.IsHead()) // Проскакиваем стартовый элемент
                    //{
                    //    item = (index > 0) ? item.Next : item.Prev;
                    //}
                }

                return item;
            }
        }
        #endregion

        #region .ctor

        /// <summary>
        /// Блок рельсов
        /// </summary>
        /// <param name="type">Тип блока</param>
        public Railway(RailwayType type)
        {
            Type = type;
            End = CalculateEndPoint();
            Dimensions = new TemplateDimensions(this, false);
        }

        public static Railway L1 => new Railway(RailwayType.L1);
        public static Railway L2 => new Railway(RailwayType.L2);
        public static Railway L3 => new Railway(RailwayType.L3);
        public static Railway L4 => new Railway(RailwayType.L4);
        public static Railway T4R => new Railway(RailwayType.T4R);
        public static Railway T4L => new Railway(RailwayType.T4L);
        public static Railway T8R => new Railway(RailwayType.T8R);
        public static Railway T8L => new Railway(RailwayType.T8L);

        #endregion

        #region Private methods

        /// <summary>
        /// Вычисление точки окончания блока рельсов
        /// </summary>
        /// <returns>Точка окончания бока рельсов</returns>
        private Point CalculateEndPoint()
        {
            var (name, direction) = MapTypeToName(Type);
            var endPoint = TraceBuilder.MakeStep(_start, name, direction);
            return endPoint;
        }

        /// <summary>
        /// Сопоставление типа блока с названием и направлением
        /// </summary>
        /// <param name="railType"></param>
        /// <returns></returns>
        private (string name, int direction) MapTypeToName(RailwayType railType)
        {
            switch (railType)
            {
                case RailwayType.L0: return ("L0", 1);
                case RailwayType.L1: return ("L1", 1);
                case RailwayType.L2: return ("L2", 1);
                case RailwayType.L3: return ("L3", 1);
                case RailwayType.L4: return ("L4", 1);
                case RailwayType.T4L: return ("T4", -1);
                case RailwayType.T4R: return ("T4", 1);
                case RailwayType.T8R: return ("T8", 1);
                case RailwayType.T8L: return ("T8", -1);
                case RailwayType.B1: return ("B1", 1);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Implementation of IScalableTemplate

        /// <summary>
        /// Есть ли возможность изменить размер шаблона
        /// в указанном или любом направлении
        /// </summary>
        /// <param name="angle">Направление, если null - то любое</param>
        public bool CanScale(double? angle = null)
        {
            // Если нет симметричного блока, расширить нельзя
            if (Symmetric == null) return false;

            // TODO: добавить проверку на наличие доступных прямых блоков (L1, L2, L3, L4) 

            // Если указано определённое направление масштабирования
            if (angle != null)
            {
                // То увеличить можно, только в случае совпадения направления блока или его симметричного элемента
                return (Math.Abs((double) angle - End.Angle) < Constants.Precision ||
                        Math.Abs((double) angle - Symmetric.End.Angle) < Constants.Precision);
            }

            // Если направление не задано, то гарантированно можно увеличить, присоединив прямой элемент
            return true;
        }

        /// <summary>
        /// Вызвать изменение размера
        /// в указанном или любом направлении
        /// </summary>
        /// <param name="angle">Направление, если null - то любое</param>
        public bool TryScale(double? angle = null)
        {
            if (!CanScale(angle)) return false;

            // TODO: выбрать блок среди доступных (L1, L2, L3, L4), т.к. L1 может не быть

            // Добавляем прямой блок 
            this.AppendSymmetric(new Railway(RailwayType.L1));

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
            // Если размеры предлагаемого шаблона не совпадают с размерами текущего блока, то применить мутацию невозможно
            if (dimensions != null && dimensions.Output != this.Dimensions.Output) return false;
            
            return (Type == RailwayType.T4L || Type == RailwayType.T4R);
        }

        /// <summary>
        /// Применить мутацию
        /// </summary>
        /// <param name="template">Шаблон для вставки. Если null - то применить любую мутацию на своё усмотрение</param>
        public bool TryMutate(IRailwayTemplate template = null)
        {
            if (!CanMutate(template?.Dimensions)) return false;

            if (template != null)
            {
                // Присоединяем новый шаблон
                if (Prev != null) Prev.Next = template;
                if (Next != null) Next.Prev = template;

                // Разрываем связь симметрии
                Symmetric.Symmetric = null;

                return true;
            }

            return false;

            // TODO: произвести вставку моста с кольцом
            // Some magic code will be here
        }

        #endregion
    }

}
