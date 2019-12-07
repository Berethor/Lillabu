namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Расширения класса <see cref="Railway"/>
    /// </summary>
    public static class RailwayExtensions
    {
        /// <summary>
        /// Присоединить блок рельс к существующему блоку рельс
        /// </summary>
        /// <param name="first">Существующий блок рельсов</param>
        /// <param name="second">Присоединяемый блок рельсов</param>
        /// <returns></returns>
        public static Railway Append(this Railway first, Railway second)
        {
            var next = first.Next;

            first.Next = second;
            second.Prev = first;
            second.Start = first.End;

            // Если у существующего блока было продолжение,
            // то присоединяем его к присоединяемому блоку
            // TODO: Если у присоединяемого блока есть продолжение, то оно потеряется!
            if (next != null)
            {
                second.Next = next;
            }

            return second;
        }

        /// <summary>
        /// Является ли данный элемент начальным (L0)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsHead(this Railway item)
        {
            return item.Length == 0;
        }

    }
}
