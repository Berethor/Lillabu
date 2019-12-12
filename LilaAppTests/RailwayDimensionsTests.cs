using System.Collections.Generic;
using LilaApp;
using LilaApp.Models;
using LilaApp.Models.Railways;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LilaAppTests
{
    /// <summary>
    /// Тесты для проверки правильности вычисления размеров шаблона
    /// </summary>
    [TestClass]
    public class RailwayDimensionsTests
    {
        /// <summary>
        /// Тест, проверяющий размеры шаблона на примере кольцевой трассы
        /// </summary>
        [TestMethod]
        public void Test_1_Circle()
        {
            var head = RailwayTemplates.CreateCircle(RailwayType.T4R);

            var template = head as RailwayChain;

            var dimensions = template.Dimensions;

            Assert.AreEqual(2 * Constants.RADIUS, dimensions.Width, 1E-12, "Ширина отличается от ожидаемой");
            Assert.AreEqual(2 * Constants.RADIUS, dimensions.Height, 1E-12, "Высота отличается от ожидаемой");

            Assert.AreEqual(new Point(0, -Constants.RADIUS), dimensions.Min, "Минимальная пара координат (X,Y) отличается от ожидаемой");
            Assert.AreEqual(new Point(2*Constants.RADIUS, Constants.RADIUS), dimensions.Max, "Максимальная пара координат (X,Y) отличается от ожидаемой");

            Assert.AreEqual(Point.Zero, dimensions.Output, "Точка выхода отличается от ожидаемой");
        }
    }
}
