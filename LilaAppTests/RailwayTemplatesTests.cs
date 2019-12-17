using System;
using System.Collections.Generic;
using System.Linq;
using LilaApp;
using LilaApp.Models;
using LilaApp.Models.Railways;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LilaAppTests
{
    [TestClass]
    public class RailwayTemplatesTests
    {
        [TestMethod]
        public void Test_01_CircleToModel()
        {
            var circle = RailwayTemplates.CreateCircle(RailwayType.T4R);
            var model = circle.ConvertToModel();
            var text = model.Serialize();

            var k = 0;
            var expected = new Model();
            expected.Order = new List<string>(){ "T4", "T4", "T4", "T4", "T4", "T4", "T4", "T4" };
            expected.Topology = expected.Order.Select(_ => new TopologyItem(k, ++k, 1)).ToList();
            expected.Topology.Add(new TopologyItem(k, 0, 1));
            var expectedText = expected.Serialize();

            Assert.AreEqual(expectedText, text);
        }

        [TestMethod]
        public void Test_02_CircleScaleS()
        {
            var circle = RailwayTemplates.CreateCircle(RailwayType.T4R);
            circle.TryScale(Direction.S, Railway.L3);
            var model = circle.ConvertToModel();

            Assert.AreEqual("T4T4T4T4L3T4T4T4T4L3", string.Join("", model.Order));
        }

        [TestMethod]
        public void Test_1_SubTemplate_L6T2R()
        {
            var chain = new RailwayChain(Railway.L3, Railway.L3, Railway.T4R, Railway.T4R);

            var head = new Railway(RailwayType.L0);
            head.Append(chain);

            var destination = new Point(3, 9, -Math.PI / 2);

            var indexes = chain.FindSubTemplate(destination);

            Assert.IsNotNull(indexes);

            Assert.AreEqual(chain[0], indexes?.start);
            Assert.AreEqual(chain[3], indexes?.end);
        }

        [TestMethod]
        public void Test_2_SubTemplate_L6T2R_Rotated()
        {
            var chain = new RailwayChain(Railway.L3, Railway.L3, Railway.T4R, Railway.T4R);

            var head = new Railway(RailwayType.L0);
            var last = head.Append(Railway.T4R);
            last = last.Append(Railway.T4R);
            last.Append(chain);

            var destination = new Point(3, 9, -Math.PI / 2);

            var indexes = chain.FindSubTemplate(destination);

            Assert.IsNotNull(indexes);

            Assert.AreEqual(chain[0], indexes?.start);
            Assert.AreEqual(chain[3], indexes?.end);
        }

        [TestMethod]
        public void Test_3_SubTemplate_L6T2L_Rotated()
        {
            var chain = new RailwayChain(Railway.L3, Railway.L3, Railway.T4L, Railway.T4L);

            var head = new Railway(RailwayType.L0);
            var last = head.Append(Railway.T4R);
            last = last.Append(Railway.T4R);
            last.Append(chain);

            var destination = new Point(-3, 9, Math.PI / 2);

            var indexes = chain.FindSubTemplate(destination);

            Assert.IsNotNull(indexes);

            Assert.AreEqual(chain[0], indexes?.start);
            Assert.AreEqual(chain[3], indexes?.end);
        }

        [TestMethod]
        public void Test_4_CircleResize()
        {
            var circle = RailwayTemplates.CreateCircle(RailwayType.T4R);

            var scalable = circle.CanScale();
            Assert.IsTrue(scalable, "Кольцевая трасса должна быть масштабируема");

            var attempt = circle.TryScale(Math.PI / 2);
            Assert.IsTrue(attempt, "Кольцевая трасса должна быть масштабируема");

            attempt = circle.TryScale(0);
            Assert.IsTrue(attempt, "Кольцевая трасса должна быть масштабируема");
        }
    }
}
