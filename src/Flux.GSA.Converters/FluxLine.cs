using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flux.SDK.Serialization;
using Flux.GSA.Interop;
using Flux.GSA.Interop.Impl;

namespace Flux.GSA.Converters
{
    //[FluxType("line", typeof(GSAElement))]
    [FluxConverter]
    public class FluxLine : IConverter<IGSAElement>
    {
        public static bool CanConvert(object data, Type objectType)
        {
            if (typeof(IGSAElement) == objectType)
                return true;
            return false;
        }
        public FluxLine()
        {
            this.Attributes = new Dictionary<string, object>();
        }

        [JsonProperty("start")]
        public List<double> Start { get; set; }

        [JsonProperty("end")]
        public List<double> End { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("primitive")]
        public string Primitive
        {
            get { return "line"; }
        }

        public Dictionary<string, string> Units
        {
            get; set;
        }

        public void ApplyUnits()
        {
            
        }

        public void SetFluxObject(IGSAElement data)
        {
            throw new NotImplementedException();
        }

        public IGSAElement GetFluxObject()
        {
            IGSAElement e = Flux.GSA.Interop.GSA.createElementInstance();

            if (Attributes != null)
            {
                if (Attributes.ContainsKey("id"))
                    e.ID = (int)Attributes["id"];

                if (Attributes.ContainsKey("property"))
                    e.Property = (int)Attributes["property"];

                if (Attributes.ContainsKey("group"))
                    e.Group = (int)Attributes["group"];

                if (Attributes.ContainsKey("orientNode"))
                    e.OrientNode = (int)Attributes["orientNode"];

                if (Attributes.ContainsKey("beta"))
                    e.Beta = (double)Attributes["beta"];

                if (Attributes.ContainsKey("name"))
                    e.Name = (string)Attributes["name"];

                if (Attributes.ContainsKey("numTopo"))
                    e.NumTopo = (int)Attributes["numTopo"];

                if (Attributes.ContainsKey("dummy"))
                    e.Dummy = (bool)Attributes["dummy"];
                else
                    e.Dummy = false;
            }

            IGSANode n1 = Flux.GSA.Interop.GSA.createNodeInstance();
            n1.Coor = Start.ToArray();

            IGSANode n2 = Flux.GSA.Interop.GSA.createNodeInstance();
            n2.Coor = End.ToArray();

            //add nodes to the element topology
            e.Topo = new List<IGSANode>();
            e.Topo.Add(n1);
            e.Topo.Add(n2);

            return e;
        }
    }
}
