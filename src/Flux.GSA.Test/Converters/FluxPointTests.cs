using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flux.GSA.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flux.GSA.Interop;

namespace Flux.GSA.Converters.Tests
{
    [TestClass()]
    public class FluxPointTests
    {
        [TestMethod()]
        public void FluxPointTest()
        {
            ConvertToGSANodeTest();
            ConvertToFluxTest();
        }

        private void ConvertToGSANodeTest()
        {
            FluxPoint p = new FluxPoint();
            p.Point = new double[] { 1, 1, 1 }.ToList();
            p.Attributes["id"] = 1;
            p.Attributes["name"] = "test";
            p.Attributes["restraint"] = 1;
            p.Attributes["stiffness"] = new double[] { 1, 1 };

            IGSANode node = p.GetFluxObject();
            Assert.AreEqual(node.Name, p.Attributes["name"]);
            Assert.AreEqual(node.ID, p.Attributes["id"]);
            Assert.AreEqual(node.Restraint, p.Attributes["restraint"]);
            CollectionAssert.AreEqual(node.Stiffness, (double[])p.Attributes["stiffness"]);
            CollectionAssert.AreEqual(node.Coor, p.Point.ToArray());
        }

        private void ConvertToFluxTest()
        {
            IGSANode node = Flux.GSA.Interop.GSA.createNodeInstance();
            node.ID = 1;
            node.Coor = new double[] { 1, 1, 1 };
            node.Name = "test";
            node.Restraint = 1;
            node.Stiffness = new double[] { 1, 1 };

            FluxPoint p = new FluxPoint();
            p.SetFluxObject(node);

            Assert.AreEqual(node.Name, p.Attributes["name"]);
            Assert.AreEqual(node.ID, p.Attributes["id"]);
            Assert.AreEqual(node.Restraint, p.Attributes["restraint"]);
            CollectionAssert.AreEqual(node.Stiffness, (double[])p.Attributes["stiffness"]);
            CollectionAssert.AreEqual(node.Coor, p.Point.ToArray());
        }
    }
}