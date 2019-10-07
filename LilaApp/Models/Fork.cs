namespace LilaApp.Models
{
    /// <summary>
    /// Структура для хранения координат Y-образного блока
    /// </summary>
    public struct Fork
    {
        /// <summary>
        /// Точка основания
        /// </summary>
        public Point Base { get; set; }

        /// <summary>
        /// Точка левого ответвления
        /// </summary>
        public Point Left { get; set; }

        /// <summary>
        /// Точка правого ответвления
        /// </summary>
        public Point Right { get; set; }

        /// <summary>
        /// Центральная точка блока
        /// </summary>
        public Point Center { get; set; }

        /// <summary>
        /// Индексатор для обращения через []
        /// </summary>
        /// <param name="direction">0 - Основание, 1 - Направо, -1 - Налево, 2 - Центр</param>
        public Point this[int direction] {
            get {
                switch (direction)
                {
                    case -1: return Left;
                    case 1: return Right;
                    case 0: return Base;
                    default: return Center;
                }
            }
            set {
                switch (direction)
                {
                    case -1: Left = value; break;
                    case 1: Right = value; break;
                    case 0: Base = value; break;
                    default: Center = value; break;
                }
            }
        }
    }
}
