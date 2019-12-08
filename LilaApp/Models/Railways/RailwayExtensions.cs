using System;

namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Расширения класса <see cref="Railway"/>
    /// </summary>
    public static class RailwayExtensions
    {
        /// <summary>
        /// Присоединить шаблон железной дороги к существующему шаблону железной дороги
        /// </summary>
        /// <param name="first">Существующий шаблон железной дороги</param>
        /// <param name="second">Присоединяемый шаблон железной дороги</param>
        /// <returns></returns>
        public static IRailwayTemplate Append(this IRailwayTemplate first, IRailwayTemplate second)
        {
            var next = first.Next;

            first.Next = second;
            second.Prev = first;
            second.Start = first.End;

            // Если у существующего шаблона было продолжение,
            // то присоединяем его к присоединяемому шаблону
            // TODO: Если у присоединяемого блока есть продолжение, то оно потеряется!
            if (next != null)
            {
                second.Next = next;
            }

            return second;
        }

        /// <summary>
        /// Симметричное добавление шаблона к текущему шаблону
        /// </summary>
        /// <param name="destination">Текущий шаблон (шаблон назначения)</param>
        /// <param name="template">Образец присоединяемого шаблона</param>
        /// <exception cref="InvalidOperationException" />
        public static void AppendSymmetric(this IRailwayTemplate destination, IRailwayTemplate template)
        {
            if (destination.Symmetric == null)
            {
                throw  new InvalidOperationException("Отсутствует симметричная точка для назначенного шаблона");
            }
            
            // Создаём копии шаблона
            var forward = template.Clone();
            var backward = template.Clone();
            // Добавляем шаблоны в две точки разрыва
            destination.Append(forward);
            destination.Symmetric.Append(backward);
            // Связываем добавленные блоки связью симметрии
            forward.Symmetric = backward;
            backward.Symmetric = forward;
        }

        /// <summary>
        /// Является ли данный элемент начальным (L0)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsHead(this IRailwayTemplate item)
        {
            return item is Railway railway && railway.Length == 0;
        }

    }
}
