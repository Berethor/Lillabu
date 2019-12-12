using LilaApp.Models;
using LilaApp.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LilaAppTests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void Test_1_MathFunctions()
        {
            var pointA = new Point(x: 0, y: 0);
            var pointB = new Point(x: 0, y: 4);
            var pointC = new Point(x: 0.2, y: 1);
            var pointD = new Point(x: 0.05, y: 2);
            var pointE = new Point(x: -3, y: 3);
            var pointE1 = new Point(x: -2, y: 3);
            var pointE2 = new Point(x: -2.95, y: 3);
            var pointE3 = new Point(x: -5.95, y: 3);
            var pointCenter = new Point(x: -3, y: 0);

            // Проверка отрезка (0 0) (0 4) и точек (0.2 1) и (0.05 2)
            Assert.IsFalse(MathFunctions.CheckSegment(pointC, pointA, pointB));
            Assert.IsTrue(MathFunctions.CheckSegment(pointD, pointA, pointB));

            // Проверка поворота (0 0) (-3 3) на 90 градусов и точек (-2 3) (-2.95 3) (-5.95 3)
            Assert.IsFalse(MathFunctions.CheckTurn(pointE1, pointCenter, pointA, pointE, 90, 3));
            Assert.IsTrue(MathFunctions.CheckTurn(pointE2, pointCenter, pointA, pointE, 90, 3));
            Assert.IsFalse(MathFunctions.CheckTurn(pointE3, pointCenter, pointA, pointE, 90, 3));

        }

        [TestMethod]
        public void Test_2_Segments()
        {
            var A = new Point(x: 0, y: 0);
            var B = new Point(x: 0, y: 4);
            var C = new Point(x: 4, y: 0);

            var d = 0.099;

            // Вертикальный отрезок (0, 0) - (0, 4) и точки немного правее
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(d, A.Y), A, B));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(d, (A.Y + B.Y) / 2), A, B));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(d, B.Y), A, B));

            // Вертикальный отрезок (0, 0) - (0, 4) и точки немного левее
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(-d, A.Y), A, B));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(-d, (A.Y + B.Y) / 2), A, B));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(-d, B.Y), A, B));

            // Горизонтальный отрезок (0, 0) - (4, 0) и точки немного выше
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(A.X, d), A, C));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point((A.X + C.X) / 2, d), A, C));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(C.X, d), A, C));

            // Горизонтальный отрезок (0, 0) - (4, 0) и точки немного ниже
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(A.X, -d), A, C));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point((A.X + C.X) / 2, -d), A, C));
            Assert.IsTrue(MathFunctions.CheckSegment(new Point(C.X, -d), A, C));
        }


        [TestMethod]
        public void Test_2_Turns()
        {
            var A = new Point(x: 0, y: 4);
            var B = new Point(x: 4, y: 0);
            var C = new Point(x: 0, y: 0);
            var angle = 90;
            var R = 3;

            var d = 0.099;

            // Поворот (0, 4) - (4, 0) с центром в (0, 0)

            // Тестируем окрестности точки А:
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(0, R), C, A, B, angle, R));

            // Чуть выше, чуть ниже точки А
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(0, R + d), C, A, B, angle, R));
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(0, R - d), C, A, B, angle, R));
            // Значительно выше / ниже точки А
            Assert.IsFalse(MathFunctions.CheckTurn(new Point(0, R + 2 * d), C, A, B, angle, R));
            Assert.IsFalse(MathFunctions.CheckTurn(new Point(0, R - 2 * d), C, A, B, angle, R));

            // Чуть правее / левее точки А
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(d, R), C, A, B, angle, R));
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(-d, R), C, A, B, angle, R));

            // Значительно левее точки А
            Assert.IsFalse(MathFunctions.CheckTurn(new Point(-2 * d, R), C, A, B, angle, R));


            // Тестируем окрестности точки В:
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(R, 0), C, A, B, angle, R));

            // Чуть правее / левее точки В
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(R + d, 0), C, A, B, angle, R));
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(R - d, 0), C, A, B, angle, R));
            // Значительно правее / левее точки В
            Assert.IsFalse(MathFunctions.CheckTurn(new Point(R + 2 * d, 0), C, A, B, angle, R));
            Assert.IsFalse(MathFunctions.CheckTurn(new Point(R - 2 * d, 0), C, A, B, angle, R));

            // Чуть выше / ниже точки В
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(R, d), C, A, B, angle, R));
            Assert.IsTrue(MathFunctions.CheckTurn(new Point(R, -d), C, A, B, angle, R));

            // Значительно ниже точки В
            Assert.IsFalse(MathFunctions.CheckTurn(new Point(R, - 2 * d), C, A, B, angle, R));
        }

    }
}
