using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flux.SDK.Serialization;
using Flux.GSA.Interop.Impl;
using Flux.GSA.Interop;
using Newtonsoft.Json;

namespace Flux.GSA.Converters
{
    //[FluxType("gsaElement", typeof(GSAElement))]
    [FluxConverter]
    public class FluxElement : IConverter<IGSAElement>
    {
        public static bool CanConvert(object data, Type objectType)
        {
            if (typeof(IGSAElement) == objectType)
                return true;
            return false;
        }

        public FluxElement()
        {
            this.Nodes = new List<FluxPoint>();
        }

        [JsonProperty("primitive")]
        public string Primitive
        {
            get { return "gsaElement"; }
        }

        [JsonProperty("property")]
        public int? Property { get; set; }

        [JsonProperty("group")]
        public int? Group { get; set; }

        [JsonProperty("beta")]
        public double? Beta { get; set; }

        [JsonProperty("elementType")]
        public int? ElementType { get; set; }

        [JsonProperty("orientNode")]
        public int? OrientNode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("dummy")]
        public bool? Dummy { get; set; }
        
        [JsonProperty("nodes")]
        public List<FluxPoint> Nodes { get; set; }


        public Dictionary<string, string> Units
        {
            get; set;
        }

        public void ApplyUnits()
        {
            //NYI
        }

        public void SetFluxObject(IGSAElement model)
        {
            if (model != null)
            {
                this.Property = model.Property;
                this.Group = model.Group;
                this.Id = model.ID;
                this.Beta = model.Beta;
                this.ElementType = model.eType;
                this.Name = model.Name;
                this.OrientNode = model.OrientNode;
                this.Dummy = model.Dummy;
                this.Nodes = new List<FluxPoint>();

                foreach (IGSANode n in model.Topo)
                {
                    this.Nodes.Add(new FluxPoint(n));
                }
            }
        }

        public IGSAElement GetFluxObject()
        {
            IGSAElement e = Flux.GSA.Interop.GSA.createElementInstance();
            e.ID = this.Id;
            e.Property = this.Property;
            e.Group = this.Group;
            e.Beta = this.Beta;
            e.eType = this.ElementType;
            e.Name = this.Name;
            e.OrientNode = this.OrientNode;
            e.Dummy = (this.Dummy.HasValue) ? this.Dummy : false;

            //add nodes to the element topology
            e.Topo = new List<IGSANode>();
            foreach (FluxPoint p in this.Nodes)
                e.Topo.Add(p.GetFluxObject());

            return e;
        }
    }
}
