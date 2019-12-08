using System.Collections.Generic;

namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Интерфейс шаблона железной дороги.
    /// Шаблон может состоять из одного или нескольких соединённых блоков.
    /// Каждый шаблон является элементом двусвязного списка:
    /// содержит ссылку на предыдущий и следующий элемент
    /// </summary>
    public interface IRailwayTemplate
    {
        /// <summary>
        /// Следующий шаблон железной дороги
        /// </summary>
        IRailwayTemplate Next { get; set; }

        /// <summary>
        /// Предыдущий шаблон железной дороги
        /// </summary>
        IRailwayTemplate Prev { get; set; }

        /// <summary>
        /// Точка начала текущего шаблона железной дороги
        /// </summary>
        Point? Start { get; set; }

        /// <summary>
        /// Точка окончания текущего шаблона железной дороги
        /// </summary>
        Point? End { get; }

        /// <summary>
        /// Шаблон железной дороги, симметричный текущему.
        /// Если добавить после текущего шаблона блок L1,
        /// то для того, чтобы путь остался замкнутым,
        /// надо добавить после симметричного ещё один блок L1.
        /// (при этом направления блоков L1 должны быть противоположными)
        /// </summary>
        IRailwayTemplate Symmetric { get; set; }

        /// <summary>
        /// Создать копию шаблона, не привязанную к другим шаблонам
        /// </summary>
        /// <returns></returns>
        IRailwayTemplate Clone();

        /// <summary>
        /// Преобразование шаблона железной дороги к списку отдельных блоков рельсов
        /// </summary>
        /// <returns></returns>
        List<Railway> GetRailways();
    }
}
