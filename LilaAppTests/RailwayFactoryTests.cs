using LilaApp.Algorithm;
using LilaApp.Models;
using LilaApp.Models.Railways;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LilaAppTests
{
    [TestClass]
    public class RailwayFactoryTests
    {
        static readonly RailwayFactory factory = new RailwayFactory();

        public void BuildPrimitive(string name)
        {
            var model = new Model()
            {
                Blocks = { new Block(name, 1, 10) },
            };

            var chain = factory.BuildTemplate(name, model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual(name, answer.Order[0]);
        }

        [TestMethod]
        public void Test_1_BuildL1()
        {
            BuildPrimitive("L1");
        }

        [TestMethod]
        public void Test_2_BuildL2()
        {
            BuildPrimitive("L2");
        }

        [TestMethod]
        public void Test_3_BuildL3()
        {
            BuildPrimitive("L3");
        }

        [TestMethod]
        public void Test_4_BuildL4()
        {
            BuildPrimitive("L4");
        }

        [TestMethod]
        public void Test_5_BuildL4_2xL2()
        {
            var model = new Model()
            {
                Blocks = { new Block("L2", 2, 10) },
            };

            var chain = factory.BuildTemplate("L4", model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual(2, answer.Order.Count);
            Assert.AreEqual("L2", answer.Order[0]);
            Assert.AreEqual("L2", answer.Order[1]);
        }

        [TestMethod]
        public void Test_6_BuildL3_3xL1()
        {
            var model = new Model()
            {
                Blocks = { new Block("L1", 3, 10) },
            };

            var chain = factory.BuildTemplate("L3", model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual(3, answer.Order.Count);
            Assert.AreEqual("L1", answer.Order[0]);
            Assert.AreEqual("L1", answer.Order[1]);
            Assert.AreEqual("L1", answer.Order[2]);
        }

        [TestMethod]
        public void Test_7_BuildL4_4xL1()
        {
            var model = new Model()
            {
                Blocks = { new Block("L1", 4, 10) },
            };

            var chain = factory.BuildTemplate("L4", model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual(4, answer.Order.Count);
            Assert.AreEqual("L1", answer.Order[0]);
            Assert.AreEqual("L1", answer.Order[1]);
            Assert.AreEqual("L1", answer.Order[2]);
            Assert.AreEqual("L1", answer.Order[3]);
        }

        [TestMethod]
        public void Test_8_BuildL5_1xL3_2xL1()
        {
            var model = new Model()
            {
                Blocks =
                {
                    new Block("L1", 5, 10),
                    new Block("L3", 5, 10),
                },
            };

            var chain = factory.BuildTemplate("L5", model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual(3, answer.Order.Count);
            Assert.AreEqual("L3", answer.Order[0]);
            Assert.AreEqual("L1", answer.Order[1]);
            Assert.AreEqual("L1", answer.Order[2]);
        }

        [TestMethod]
        public void Test_9_BuildL10()
        {
            var model = new Model()
            {
                Blocks =
                {
                    new Block("L1", 1, 10),
                    new Block("L2", 1, 10),
                    new Block("L3", 1, 10),
                    new Block("L4", 1, 10),
                },
            };

            var chain = factory.BuildTemplate("L10", model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual("L4L3L2L1", string.Join("", answer.Order));
        }

        [TestMethod]
        public void Test_10_BuildT4()
        {
            BuildPrimitive("T4");
        }

        [TestMethod]
        public void Test_11_BuildT8()
        {
            BuildPrimitive("T8");
        }

        [TestMethod]
        public void Test_12_BuildT4_2xT8()
        {
            var model = new Model()
            {
                Blocks = { new Block("T8", 2, 10) },
            };

            var chain = factory.BuildTemplate("T4", model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual("T8T8", string.Join("", answer.Order));
        }

        [TestMethod]
        public void Test_13_BuildT2_1xT4_2xT8()
        {
            var model = new Model()
            {
                Blocks =
                {
                    new Block("T4", 1, 10),
                    new Block("T8", 2, 10),
                },
            };

            var chain = factory.BuildTemplate("T2", model);

            var answer = chain.ConvertToModel();

            Assert.AreEqual("T4T8T8", string.Join("", answer.Order));
        }

        [TestMethod]
        public void Test_14_BuildB1()
        {
            BuildPrimitive("B1");
        }
    }
}
