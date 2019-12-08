using System.Collections.Generic;
using System.Linq;

namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Цепочка блоков рельсов.
    /// Состоит из нескольких соединенных подряд блоков.
    /// Реализует интерфейс шаблона железной дороги.
    /// </summary>
    public class RailwayChain : IRailwayTemplate
    {
        #region Fields

        private readonly Railway[] _railways;

        private Point _start;

        #endregion

        #region .ctor

        /// <summary>
        /// Цепочка блоков рельсов
        /// </summary>
        /// <param name="railways">Блоки рельсов</param>
        public RailwayChain(params Railway[] railways)
        {
            _railways = railways;

            Dimensions = new TemplateDimensions(this, false);
        }

        #endregion

        #region Implementation of IRailwayTemplate

        /// <summary>
        /// Следующий шаблон железной дороги
        /// </summary>
        public IRailwayTemplate Next { get; set; }

        /// <summary>
        /// Предыдущий шаблон железной дороги
        /// </summary>
        public IRailwayTemplate Prev { get; set; }

        /// <summary>
        /// Точка начала текущего шаблона железной дороги
        /// </summary>
        public Point Start {
            get => _start;
            set {
                _start = value;

                for (var i = 0; i < _railways.Length; i++)
                {
                    if (i == 0)
                    {
                        // Задаём стартовую точку для первого блока в цепочке
                        _railways[i].Start = _start;
                    }
                    else
                    {
                        // Присоединяем остальные блоки друг за другом,
                        _railways[i - 1].Append(_railways[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Точка окончания текущего шаблона железной дороги
        /// </summary>
        public Point End => _railways.LastOrDefault()?.End ?? Point.Zero;

        /// <summary>
        /// Шаблон железной дороги, симметричный текущему.
        /// Если добавить после текущего шаблона блок L1,
        /// то для того, чтобы путь остался замкнутым,
        /// надо добавить после симметричного ещё один блок L1.
        /// (при этом направления блоков L1 должны быть противоположными)
        /// </summary>
        public IRailwayTemplate Symmetric { get; set; }

        /// <summary>
        /// Создать копию шаблона, не привязанную к другим шаблонам
        /// </summary>
        /// <returns></returns>
        public IRailwayTemplate Clone()
        {
            var copiedRailways = (Railway[])_railways.Clone();

            return new RailwayChain(copiedRailways);
        }

        /// <summary>
        /// Преобразование шаблона железной дороги к списку отдельных блоков рельсов
        /// </summary>
        /// <returns></returns>
        public List<Railway> GetRailways()
        {
            return _railways.ToList();
        }

        /// <summary>
        /// Размеры шаблона
        /// </summary>
        public TemplateDimensions Dimensions { get; }

        #endregion
    }
}
