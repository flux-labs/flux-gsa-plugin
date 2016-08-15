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
    public class FluxElementTests
    {

        [TestMethod()]
        public void FluxObjectTest()
        {
            ConversionToGSATest();
            ConversionToFluxTest();
        }

        private void ConversionToFluxTest()
        {
            IGSAElement gsa = Flux.GSA.Interop.GSA.createElementInstance();
            gsa.Beta = 1.0;
            gsa.Dummy = false;
            gsa.eType = 1;
            gsa.Group = 1;
            gsa.ID = 1;
            gsa.Name = "test";
            gsa.OrientNode = 1;
            gsa.Property = 1;
            
            //compare element topography
            IGSANode node = Flux.GSA.Interop.GSA.createNodeInstance();
            node.Coor = new double[] { 1, 1, 1 };
            gsa.Topo.Add(node);

            //test conversion of GSAElement to FluxElement
            FluxElement flux = new FluxElement();
            flux.SetFluxObject(gsa);

            Assert.AreEqual(gsa.Beta, flux.Beta);
            Assert.AreEqual(gsa.Dummy, flux.Dummy);
            Assert.AreEqual(gsa.eType, flux.ElementType);
            Assert.AreEqual(gsa.Group, flux.Group);
            Assert.AreEqual(gsa.ID, flux.Id);
            Assert.AreEqual(gsa.Name, flux.Name);
            Assert.AreEqual(gsa.OrientNode, flux.OrientNode);
            Assert.AreEqual(gsa.Property, flux.Property);
            Assert.AreEqual(flux.Nodes.Count,1);
            CollectionAssert.AreEqual(node.Coor, flux.Nodes[0].Point.ToArray());
        }

        private void ConversionToGSATest()
        {
            FluxElement flux = new FluxElement();
            flux.Beta = 1.0;
            flux.Dummy = false;
            flux.ElementType = 1;
            flux.Group = 1;
            flux.Id = 1;
            flux.Name = "test";
            flux.OrientNode = 1;
            flux.Property = 1;

            //add a point to the element
            FluxPoint p = new FluxPoint();
            p.Point = new double[] { 1, 1, 1 }.ToList();
            flux.Nodes.Add(p);

            //test conversion of FluxElement to GSAElement
            IGSAElement gsa = flux.GetFluxObject();
            Assert.AreEqual(gsa.Beta, flux.Beta);
            Assert.AreEqual(gsa.Dummy, flux.Dummy);
            Assert.AreEqual(gsa.eType, flux.ElementType);
            Assert.AreEqual(gsa.Group, flux.Group);
            Assert.AreEqual(gsa.ID, flux.Id);
            Assert.AreEqual(gsa.Name, flux.Name);
            Assert.AreEqual(gsa.OrientNode, flux.OrientNode);
            Assert.AreEqual(gsa.Property, flux.Property);
            Assert.AreEqual(gsa.Topo.Count, 1);
            CollectionAssert.AreEqual(gsa.Topo[0].Coor, p.Point.ToArray());
        }
    }
}