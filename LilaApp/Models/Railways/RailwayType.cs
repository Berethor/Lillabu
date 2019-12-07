namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Тип блока рельсов
    /// </summary>
    public enum RailwayType
    {
        /// <summary>
        /// Прямой блок
        /// </summary>
        Line,

        /// <summary>
        /// Поворотный блок налево
        /// </summary>
        TurnLeft,

        /// <summary>
        /// Поворотный блок направо
        /// </summary>
        TurnRight,

        /// <summary>
        /// Мост
        /// </summary>
        Bridge
    }
}
