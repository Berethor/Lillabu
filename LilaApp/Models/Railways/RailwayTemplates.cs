namespace LilaApp.Models.Railways
{
    public static class RailwayTemplates
    {
        /// <summary>
        ///  Создать стартовую трассу - кольцо
        /// </summary>
        /// <param name="turnDirection">Направление кольца (T4R или T4L)</param>
        /// <returns>Указатель на начало трассы - блок L0</returns>
        public static Railway CreateCircle(RailwayType turnDirection)
        {
            // Добавляем блок L0
            var head = new Railway(RailwayType.L0) { Start = new Point(0, 0), };

            IRailwayTemplate last = head;
            // Добавляем 8 блоков T4
            for (var i = 0; i < 8; i++)
            {
                last = last.Append(new Railway(turnDirection));
            }
            // Закольцовываем список
            last.Next = head;

            // Связываем симметричные блоки
            for (var i = 0; i < 4; i++)
            {
                head[i].Symmetric = head[i + 4];
                head[i + 4].Symmetric = head[i];
            }

            return head;
        }

        /// <summary>
        /// Указатели на элементы кольца по сторонам света:
        ///        north (север)
        /// west (запад)      east (восток)
        ///         south (юг)
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public static (
            Railway NW, Railway N, Railway NE,
            Railway W, /*       */ Railway E,
            Railway SW, Railway S, Railway SE)
            Compass(Railway head)
        {
            return (
                head[1] as Railway, head[2] as Railway, head[3] as Railway,
                head[0] as Railway, /*               */ head[4] as Railway,
                head[7] as Railway, head[6] as Railway, head[5] as Railway);
        }
    }
}
