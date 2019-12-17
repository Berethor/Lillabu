using System;

namespace LilaApp.Models
{
    /// <summary>
    /// Структура результата решения обратной задачи
    /// </summary>
    public struct FinalAnswer : IComparable
    {
        /// <summary>
        /// Полная модель (включая блоки ORDER и TOP)
        /// </summary>
        public Model Model { get; set; }

        /// <summary>
        /// Цена полученного маршрута
        /// </summary>
        public TracePrice Price { get; set; }

        /// <summary>
        /// Результат решения задачи
        /// </summary>
        /// <param name="answer"> Полная модель ответа (включая блоки ORDER и TOP)</param>
        /// <param name="price">Цена полученного маршрута</param>
        public FinalAnswer(Model answer, TracePrice price)
        {
            Model = answer;
            Price = price;
        }

        // public FinalAnswer() { }

        #region Implementation of IComparable

        public int CompareTo(object obj)
        {
            if (obj is FinalAnswer other)
            {
                return Price.CompareTo(other.Price);
            }

            if (obj is double d)
            {
                return Price.Result.CompareTo(d);
            }

            throw new ArgumentException($"Невозможно сравнить {typeof(FinalAnswer)} с {obj.GetType()}");
        }

        public static bool operator >(FinalAnswer a, FinalAnswer b)
        {
            return a.CompareTo(b) == 1;
        }

        public static bool operator <(FinalAnswer a, FinalAnswer b)
        {
            return a.CompareTo(b) == -1;
        }

        #endregion
    }
}
