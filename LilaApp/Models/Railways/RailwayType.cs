namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Тип блока рельсов
    /// </summary>
    public enum RailwayType
    {
        /// <summary>
        /// Прямой псевдо-блок, длина 0
        /// </summary>
        L0,

        /// <summary>
        /// Прямой блок, длина 1
        /// </summary>
        L1,

        /// <summary>
        /// Прямой блок, длина 2
        /// </summary>
        L2,

        /// <summary>
        /// Прямой блок, длина 3
        /// </summary>
        L3,

        /// <summary>
        /// Прямой блок, длина 4
        /// </summary>
        L4,

        /// <summary>
        /// Поворотный блок T4 налево
        /// </summary>
        T4L,

        /// <summary>
        /// Поворотный блок T4 направо
        /// </summary>
        T4R,

        /// <summary>
        /// Поворотный блок T8 налево
        /// </summary>
        T8L,

        /// <summary>
        /// Поворотный блок T8 направо
        /// </summary>
        T8R,

        /// <summary>
        /// Мост, длина 4,
        /// пересекать перпендикулярно,
        /// ровно по центру
        /// </summary>
        B1
    }
}
