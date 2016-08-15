using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flux.GSA.Converters;
using System.Collections.Generic;
using System.Linq;
using Flux.GSA.Interop;

namespace Flux.GSA.Test.Converters
{
    [TestClass]
    public class FluxLineTests
    {
        [TestMethod]
        public void FluxObjectTest()
        {
            ConvertToGSAElement();
        }

        private void ConvertToGSAElement()
        {
            FluxLine line = new FluxLine();
            line.Start = new double[] { 1,1,1}.ToList();
            line.End = new double[] { 1, 0, 1 }.ToList();
            line.Attributes["name"] = "test";
            line.Attributes["id"] = 1;
            line.Attributes["property"] = 1;
            line.Attributes["group"] = 1;
            line.Attributes["orientNode"] = 1;
            line.Attributes["beta"] = 1.0;
            line.Attributes["dummy"] = true;
            line.Attributes["numTopo"] = 2;

            IGSAElement e = line.GetFluxObject();
            Assert.AreEqual(e.Name, "test");
            Assert.AreEqual(e.ID, 1);
            Assert.AreEqual(e.Property, 1);
            Assert.AreEqual(e.Group, 1);
            Assert.AreEqual(e.OrientNode, 1);
            Assert.AreEqual(e.Beta, 1.0);
            Assert.AreEqual(e.Dummy, true);
            Assert.AreEqual(e.NumTopo, 2);
        }
    }
}
