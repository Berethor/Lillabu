using LilaApp.Models;

namespace LilaApp.Algorithm
{
    /// <summary>
    /// Структкра результата решения обратной задачи
    /// </summary>
    public class FinalAnswer
    {
        /// <summary>
        /// Полная модель (включая блоки ORDER и TOP)
        /// </summary>
        public Model Model { get; set; }

        /// <summary>
        /// Цена полученного маршрута
        /// </summary>
        public double Price { get; set; }
    }
}
