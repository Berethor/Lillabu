using System;
using System.Collections.Generic;
using System.Linq;

namespace LilaApp
{
    /// <summary>
    /// Расширения класса <seealso cref="List{T}"/>
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Клонирование списка
        /// </summary>
        /// <typeparam name="T">Тип объектов листа. Должен быть <seealso cref="ICloneable"/></typeparam>
        /// <param name="list">Исходный список</param>
        /// <returns>Новый список</returns>
        public static List<T> Clone<T>(this List<T> list) where T : ICloneable
        {
            return list.Select(item => item.Clone()).Cast<T>().ToList();
        }
    }
}
