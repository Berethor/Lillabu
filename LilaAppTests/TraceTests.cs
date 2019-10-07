using LilaApp;
using LilaApp.Algorithm;
using LilaApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LilaAppTests
{
    [TestClass]
    public class TraceTests
    {
        Model model;

        public void Prepare()
        {
            var text = @"
DATA
L1 1000 1
L2 1000 1
L3 1000 1
L4 1000 1
T2 1000 1
T4 1000 1
T8 1000 1
T16 1000 1
Y1 1000 1
ROUTE
0 0
ORDER
L1
L1
TOP
0 1 1
1 2 1
";
            var loader = new Loader();

            model = loader.Parse(text);

        }

        [TestMethod]
        public void Test_1()
        {
            Prepare();

            var trace = TraceBuilder.CalculateTrace(model);

            Assert.AreEqual(0, trace.Points[1].X);
            Assert.AreEqual(1, trace.Points[1].Y);

            Assert.AreEqual(0, trace.Points[2].X);
            Assert.AreEqual(2, trace.Points[2].Y);
        }
    }
}
