using LilaApp;
using LilaApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LilaAppTests
{
    [TestClass]
    public class LoaderTests
    {
        static readonly Loader Loader = new Loader();

        [TestMethod]
        public void TestTrue()
        {
            string[] pathsToFile =
            {
                "Test files\\0_true_train.txt",
                "Test files\\5_interest_test.txt"
            };

            foreach (var pathToFile in pathsToFile)
            {
                try
                {
                    var loadModel = Loader.LoadAndParse(pathToFile);
                }
                catch (FormatException e)
                {
                    Assert.Fail(pathToFile + ": " + e.Message);
                }
            }
        }

        [TestMethod]
        public void TestFail()
        {
            var result = string.Empty;

            string[] pathsToFile =
            {
                "Test files\\1_without_0_0_end.txt",
                "Test files\\2_without_block.txt",
                "Test files\\3_without_end_of_block.txt",
                "Test files\\4_wrong_name_of_block.txt",
                "Test files\\6_connection_check.txt",
                "Test files\\7_mistake_format.txt",
                "Test files\\8_not_enough_parameters.txt",
                "Test files\\9_more_than_100k_element.txt",
                "Test files\\10_format.txt",
                "Test files\\11_two_connetions_with_0.txt"
            };

            foreach (var pathToFile in pathsToFile)
            {
                try
                {
                    var loadModel = Loader.LoadAndParse(pathToFile);
                    Assert.Fail($"Test should failed on {pathToFile} file");
                }
                catch (FormatException e)
                {
                    result += $"{pathToFile}-{e.Message};";
                }
            }
        }

        // TestVasekinFile# должны падать
        [TestMethod]
        public void TestVasekinFile1()
        {
            var pathToFile = "Test files\\test_1.txt";
            try
            {
                var loadModel = Loader.LoadAndParse(pathToFile);
            }
            catch (FormatException e)
            {
                Assert.Fail(pathToFile + ": " + e.Message);
            }
        }

        [TestMethod]
        public void TestVasekinFile2()
        {
            var pathToFile = "Test files\\test_2.txt";
            try
            {
                var loadModel = Loader.LoadAndParse(pathToFile);
            }
            catch (FormatException e)
            {
                Assert.Fail(pathToFile + ": " + e.Message);
            }
        }

        [TestMethod]
        public void TestVasekinFile3()
        {
            var pathToFile = "Test files\\test_3.txt";
            try
            {
                var loadModel = Loader.LoadAndParse(pathToFile);
            }
            catch (FormatException e)
            {
                Assert.Fail(pathToFile + ": " + e.Message);
            }
        }

        [TestMethod]
        public void TestVasekinFile4()
        {
            var pathToFile = "Test files\\test_4.txt";
            try
            {
                var loadModel = Loader.LoadAndParse(pathToFile);
            }
            catch (FormatException e)
            {
                Assert.Fail(pathToFile + ": " + e.Message);
            }
        }

        [TestMethod]
        public void TestVasekinFile5()
        {
            var pathToFile = "Test files\\test_5.txt";
            try
            {
                var loadModel = Loader.LoadAndParse(pathToFile);
            }
            catch (FormatException e)
            {
                Assert.Fail(pathToFile + ": " + e.Message);
            }
        }

        [TestMethod]
        public void TestVasekinFile6()
        {
            var pathToFile = "Test files\\test_6.txt";
            try
            {
                var loadModel = Loader.LoadAndParse(pathToFile);
            }
            catch (FormatException e)
            {
                Assert.Fail(pathToFile + ": " + e.Message);
            }
        }

        [TestMethod]
        public void TestVasekinFile7()
        {
            var pathToFile = "Test files\\test_7.txt";
            try
            {
                var loadModel = Loader.LoadAndParse(pathToFile);
            }
            catch (FormatException e)
            {
                Assert.Fail(pathToFile + ": " + e.Message);
            }
        }

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
0 0
0 10
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
0 0
0 10
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
0 0
0 10
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
