using System;
using System.Diagnostics;

namespace LilaApp.Generator
{
    /// <summary>
    /// Диапазон значений
    /// </summary>
    /// <typeparam name="T">Тип данных, должен быть сравнимым</typeparam>
    [DebuggerDisplay("{Min} - {Max}")]
    [Serializable]
    public class Range<T> where T : IComparable
    {
        /// <summary>
        /// Минимальное значение
        /// </summary>
        public T Min { get; set; }

        /// <summary>
        /// Минимальное значение
        /// </summary>
        public T Max { get; set; }

        #region .ctor

        public Range() { }

        /// <summary>
        /// Диапазон значений
        /// </summary>
        /// <param name="min">Минимальное значение</param>
        /// <param name="max">Максимальное значение</param>
        /// <exception cref="ArgumentException"/>
        public Range(T min, T max)
        {
            if (min.CompareTo(max) > 0)
            {
                throw new ArgumentException($"Значение min: {min} больше чем max: {max}", nameof(min));
            }

            Min = min;
            Max = max;

        }

        #endregion
    }
}
