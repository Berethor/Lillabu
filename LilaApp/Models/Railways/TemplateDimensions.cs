using System.Linq;

namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Размеры шаблона
    /// </summary>
    public class TemplateDimensions
    {
        #region Properties

        /// <summary>
        /// Точка выхода из шаблона.
        /// Вычисляется из расчёта, что шаблон присоединяется к точке (0,0,↑).
        /// Позволяет определить эквивалентные шаблоны
        /// </summary>
        public Point Output { get; }

        /// <summary>
        /// Минимальная пара координат X,Y
        /// </summary>
        public Point Min { get; }

        /// <summary>
        /// Максимальная пара координат X,Y
        /// </summary>
        public Point Max { get; }

        /// <summary>
        /// Ширина шаблона
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Высота шаблона
        /// </summary>
        public double Height { get; }

        #endregion

        /// <summary>
        /// Размеры шаблона
        /// </summary>
        /// <param name="railway">Шаблон</param>
        /// <param name="shouldClone">Стоит ли клонировать объект</param>
        public TemplateDimensions(IRailwayTemplate railway, bool shouldClone = true)
        {
            var template = shouldClone ? railway.Clone() : railway;

            var railways = template.GetRailways();
            var points = railways.Select(_ => _.Start).ToList();
            points.AddRange(railways.Select(_ => _.End));

            Min = new Point(points.Min(_ => _.X), points.Min(_ => _.Y));
            Max = new Point(points.Max(_ => _.X), points.Max(_ => _.Y));

            Output = new Point(
                template.End.X - template.Start.X,
                template.End.Y - template.Start.Y,
                template.End.Angle - template.Start.Angle);

            Width = Max.X - Min.X;
            Height = Max.Y - Min.Y;
        }
    }
}
