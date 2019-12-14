using System;
using System.Linq;

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

            // Если у существующего шаблона было продолжение,
            // то присоединяем его к присоединяемому шаблону
            // TODO: Если у присоединяемого блока есть продолжение, то оно потеряется!
            if (next != null && next != second)
            {
                second.Next = next;
                second.Next.Prev = second;
            }

            second.Start = first.End;

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
                throw new InvalidOperationException("Отсутствует симметричная точка для назначенного шаблона");
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
            return item is Railway railway && railway.Type == RailwayType.L0;
        }

        /// <summary>
        /// Вызвать изменение размера
        /// в указанном или любом направлении
        /// </summary>
        /// <param name="item">Шаблон масштабирования</param>
        /// <param name="direction">Направление</param>
        /// <param name="template">Шаблон для вставки. Если null - то вставить любой блок</param>
        /// <returns>True - если получилось, иначе - false</returns>
        public static bool TryScale(this IRailwayTemplate item, Direction direction, IRailwayTemplate template = null)
        {
            return item.TryScale(direction.ToAngle(), template);
        }

        /// <summary>
        /// Преобразовать двусвязный список шаблонов железной дороги в модель по ТЗ
        /// </summary>
        /// <param name="head">Указатель на начало списка</param>
        /// <param name="initial">Исходные данные модели (блоки DATA, ROUTES)</param>
        /// <returns></returns>
        public static Model ConvertToModel(this IRailwayTemplate head, Model initial = null)
        {
            // Копируем исходные данные
            var model = Model.Copy(initial);
            model.Topology.Clear();
            model.Order.Clear();

            var k = 0;
            var template = head;
            do
            {
                // Разбиваем шаблон железной дороги на отдельные блоки
                var railways = template.GetRailways();
                for (var i = 0; i < railways.Count; i++)
                {
                    var railway = railways[i];

                    if (railway.Type == RailwayType.L0)
                    {
                        continue;
                    }

                    var source = k + i - 1;
                    var destination = source + 1;

                    // Добавляем каждый блок в модель
                    model.Order.Add(railway.Name);

                    var topology = new TopologyItem(source, destination, railway.Direction);
                    model.Topology.Add(topology);
                }

                template = template.Next;
                k += railways.Count;

                if (template == null)
                {
                    var topology = new TopologyItem(k - 1, k, railways.Last().Direction);
                    model.Topology.Add(topology);
                }

            }
            while (template != null);

            model.Topology.Last().SecondBlock = 0;

            return model;
        }
    }

}
