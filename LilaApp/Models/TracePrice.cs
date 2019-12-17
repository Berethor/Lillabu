using System;
using System.Diagnostics;

namespace LilaApp.Models
{
    /// <summary>
    /// Стоимость маршрута
    /// </summary>
    [DebuggerDisplay("{StringValue}")]
    public struct TracePrice : IComparable
    {
        /// <summary>
        /// Прибыль с точек маршрута
        /// </summary>
        public double Income { get; set; }

        /// <summary>
        /// Цена блоков маршрута
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Результат = прибыль - цена блоков
        /// </summary>
        public double Result => Income - Price;

        /// <summary>
        /// Стоимость маршрута
        /// </summary>
        /// <param name="income">Прибыль с точек маршрута</param>
        /// <param name="price">Цена блоков маршрута</param>
        public TracePrice(double income, double price)
        {
            Income = income;
            Price = price;
        }

        public string StringValue => $"{Result:F2} = {Income:F2} - {Price:F2}";

        #region Implementation of IComparable

        public int CompareTo(object obj)
        {
            if (obj is TracePrice other)
            {
                return Result.CompareTo(other.Result);
            }

            if (obj is double d)
            {
                return Result.CompareTo(d);
            }

            throw new ArgumentException($"Невозможно сравнить {typeof(FinalAnswer)} с {obj.GetType()}");
        }

        public static bool operator >(TracePrice a, TracePrice b)
        {
            return a.CompareTo(b) == 1;
        }

        public static bool operator <(TracePrice a, TracePrice b)
        {
            return a.CompareTo(b) == -1;
        }

        #endregion
    }
}
