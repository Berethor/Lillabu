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
    }
}
