using LilaApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LilaAppTests
{
    [TestClass]
    public class LoaderTests
    {
        static readonly Loader Loader = new Loader();

        [TestMethod]
        public void Test_1_LoadData_1()
        {
            var text = @"
DATA
L1 10 100 -- 10 блоков типа L1, стоимостью 100
T4 1 24 -- 1 блок типа Т4, стоимостью 24
";

            var model = Loader.Parse(text);

            Assert.AreEqual(2, model.Blocks.Count);

            Assert.AreEqual("L1", model.Blocks[0].Name);
            Assert.AreEqual(10, model.Blocks[0].Count);
            Assert.AreEqual(100, model.Blocks[0].Price);

            Assert.AreEqual("T4", model.Blocks[1].Name);
            Assert.AreEqual(1, model.Blocks[1].Count);
            Assert.AreEqual(24, model.Blocks[1].Price);

        }

        [TestMethod]
        public void Test_2_LoadPoints_1()
        {
            var text = @"
ROUTE
0-0
0-10
";

            var model = Loader.Parse(text);

            Assert.AreEqual(2, model.Points.Count);

            Assert.AreEqual(0, model.Points[0].X);
            Assert.AreEqual(0, model.Points[0].Y);

            Assert.AreEqual(0, model.Points[1].X);
            Assert.AreEqual(10, model.Points[1].Y);

        }

        [TestMethod]
        public void Test_3_LoadDataAndPoints_1()
        {
            var text = @"
DATA
L1 10 100 -- 10 блоков типа L1, стоимостью 100
T4 1 24 -- 1 блок типа Т4, стоимостью 24
ROUTE
0-0
0-10
";

            var model = Loader.Parse(text);

            // Блоки
            Assert.AreEqual(2, model.Blocks.Count);

            Assert.AreEqual("L1", model.Blocks[0].Name);
            Assert.AreEqual(10, model.Blocks[0].Count);
            Assert.AreEqual(100, model.Blocks[0].Price);

            Assert.AreEqual("T4", model.Blocks[1].Name);
            Assert.AreEqual(1, model.Blocks[1].Count);
            Assert.AreEqual(24, model.Blocks[1].Price);

            // Точки
            Assert.AreEqual(2, model.Points.Count);

            Assert.AreEqual(0, model.Points[0].X);
            Assert.AreEqual(0, model.Points[0].Y);

            Assert.AreEqual(0, model.Points[1].X);
            Assert.AreEqual(10, model.Points[1].Y);

        }


        [TestMethod]
        public void Test_4_LoadAll()
        {
            var text = @"
DATA
L1 10 100
T4 1 24
ROUTE
0-0
0-10
ORDER
L1
T4
TOP
0 1 1
1 2 1
2 0 1
";

            var model = Loader.Parse(text);

            // Блоки
            Assert.AreEqual(2, model.Blocks.Count);

            Assert.AreEqual("L1", model.Blocks[0].Name);
            Assert.AreEqual(10, model.Blocks[0].Count);
            Assert.AreEqual(100, model.Blocks[0].Price);

            Assert.AreEqual("T4", model.Blocks[1].Name);
            Assert.AreEqual(1, model.Blocks[1].Count);
            Assert.AreEqual(24, model.Blocks[1].Price);

            // Точки
            Assert.AreEqual(2, model.Points.Count);

            Assert.AreEqual(0, model.Points[0].X);
            Assert.AreEqual(0, model.Points[0].Y);

            Assert.AreEqual(0, model.Points[1].X);
            Assert.AreEqual(10, model.Points[1].Y);

        }
        [TestMethod]
        public void Test_MathFunctions()
        {
            var pointA = new LilaApp.Models.Point()
            {
                X = 0,
                Y = 0
            };
            var pointB = new LilaApp.Models.Point()
            {
                X = 0,
                Y = 4
            };
            var pointC = new LilaApp.Models.Point()
            {
                X = 0.2,
                Y = 1
            };
            var pointD = new LilaApp.Models.Point()
            {
                X = 0.05,
                Y = 2
            };
            var pointE = new LilaApp.Models.Point()
            {
                X = -3,
                Y = 3
            };
            var pointE1 = new LilaApp.Models.Point()
            {
                X = -2,
                Y = 3
            };
            var pointE2 = new LilaApp.Models.Point()
            {
                X = -2.95,
                Y = 3
            };
            var pointE3 = new LilaApp.Models.Point()
            {
                X = -5.95,
                Y = 3
            };
            var pointCenter = new LilaApp.Models.Point()
            {
                X = -3,
                Y = 0
            };

            // Проверка отрезка (0 0) (0 4) и точек (0.2 1) и (0.05 2)
            Assert.IsFalse(LilaApp.Algorithm.MathFunctions.CheckSegment(pointC, pointA, pointB));
            Assert.IsTrue(LilaApp.Algorithm.MathFunctions.CheckSegment(pointD, pointA, pointB));

            // Проверка пворота (0 0) (-3 3) на 90 градусов и точек (-2.95 3) (-5.95 3) (-2 3)
            Assert.IsFalse(LilaApp.Algorithm.MathFunctions.CheckTurn(pointE1, pointCenter, pointA, pointE, 90, 3));
            Assert.IsTrue(LilaApp.Algorithm.MathFunctions.CheckTurn(pointE2, pointCenter, pointA, pointE, 90, 3));

            Assert.IsFalse(LilaApp.Algorithm.MathFunctions.CheckTurn(pointE3, pointCenter, pointA, pointE, 90, 3));

        }
    }
}
