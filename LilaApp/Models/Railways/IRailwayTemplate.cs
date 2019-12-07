namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Интерфейс шаблона железной дороги
    /// </summary>
    interface IRailwayTemplate
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
    }
}
