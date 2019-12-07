using System;
using System.Diagnostics;

namespace LilaApp.Models.Railways
{
    using Algorithm;

    /// <summary>
    /// Блок рельсов.
    /// Является двусвязным списком - содержит ссылку на предыдущий и следующий элемент
    /// </summary>
    [DebuggerDisplay("{Name} [{Start} → {End}]")]
    public class Railway
    {
        #region Fields

        private Point? _start;
        private Point? _end;

        #endregion

        #region Properties

        /// <summary>
        /// Тип блока рельсов
        /// </summary>
        public RailwayType Type { get; }

        /// <summary>
        /// Длина блока рельс (только для прямых)
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Название блока, например "L1"
        /// </summary>
        public string Name => MapTypeToName(Type).name;

        /// <summary>
        /// Направление блока: -1 - налево, 1 - направо или прямо 
        /// </summary>
        public int Direction => Type == RailwayType.TurnLeft ? -1 : 1;

        /// <summary>
        /// Точка начала блока рельсов
        /// </summary>
        public Point? Start {
            get => _start;
            set {
                _start = value;
                _end = CalculateEndPoint();
            }
        }

        /// <summary>
        /// Точка окончания блока рельсов
        /// </summary>
        public Point? End => _end;

        /// <summary>
        /// Следующий блок рельсов
        /// </summary>
        public Railway Next { get; set; }

        /// <summary>
        /// Предыдущий блок рельсов
        /// </summary>
        public Railway Prev { get; set; }

        /// <summary>
        /// Блок рельсов, симметричный текущему.
        /// Если добавить после текущего блока блок L1,
        /// то для того, чтобы путь остался замкнутым,
        /// надо добавить после симметричного ещё один блок L1.
        /// (при этом направления блоков L1 должны быть противоположными)
        /// </summary>
        public Railway Symmetric { get; set; }

        /// <summary>
        /// Индексатор.
        /// Возвращает i-ый следующий элемент, начиная от текущего блока.
        /// Т.е. this[0] = this, this[1] = this.Next, this[2] = this.Next.Next и т.д.
        /// Допустимы и отрицательные значения: this[-1] = this.Prev и т.д.
        /// </summary>
        /// <param name="index">Индекс следующего элемента, начиная отсчёт от текущего блока</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <returns></returns>
        public Railway this[int index] {
            get {
                var item = this;

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
        /// <param name="length">Длина блока рельс (только для прямых)</param>
        public Railway(RailwayType type, int length = 1)
        {
            Type = type;
            Length = length;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Вычисление точки окончания блока рельсов
        /// </summary>
        /// <returns>Точка окончания бока рельсов</returns>
        private Point? CalculateEndPoint()
        {
            if (_start == null)
            {
                return null;
            }

            var (name, direction) = MapTypeToName(Type);
            var endPoint = TraceBuilder.MakeStep((Point)_start, name, direction);
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
                case RailwayType.Line: return ($"L{Length}", 1);
                case RailwayType.TurnLeft: return ("T4", -1);
                case RailwayType.TurnRight: return ("T4", 1);
                case RailwayType.Bridge: return ("B1", 1);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }

}
