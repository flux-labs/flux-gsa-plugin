using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flux.SDK.Serialization;
using Flux.GSA.Interop;

namespace Flux.GSA.Converters
{
    //[FluxType("point", typeof(IGSANode))]
    [FluxConverter]
    public class FluxPoint : IConverter<IGSANode>
    {
        public static bool CanConvert(object data, Type objectType)
        {
            if (typeof(IGSANode) == objectType)
                return true;
            return false;
        }

        public FluxPoint()
        {
            this.Attributes = new Dictionary<string, object>();
        }

        public FluxPoint(IGSANode n) : this()
        {
            SetFluxObject(n);
        }

        [JsonProperty("point")]
        public List<double> Point{ get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("primitive")]
        public string Primitive
        {
            get
            {
                return "point";
            }
        }

        public Dictionary<string, string> Units
        {
            get; set;
        }

        public void ApplyUnits()
        {
            
        }

        public void SetFluxObject(IGSANode model)
        {
            if (model != null)
            {
                //assign node properties
                this.Point = new List<double>(model.Coor);

                this.Attributes = new Dictionary<string, object>();
                this.Attributes["id"] = model.ID;

                if (model.Name != null)
                    this.Attributes["name"] = model.Name;

                if (model.Restraint.HasValue)
                    this.Attributes["restraint"] = model.Restraint;

                if (model.Stiffness != null)
                    this.Attributes["stiffness"] = model.Stiffness;
            }
        }

        public IGSANode GetFluxObject()
        {
            IGSANode e = Flux.GSA.Interop.GSA.createNodeInstance();
            if (Attributes != null)
            {
                if (Attributes.ContainsKey("id"))
                    e.ID = Convert.ToInt32(Attributes["id"]);

                if (Attributes.ContainsKey("name"))
                    e.Name = (string)Attributes["name"];

                if (Attributes.ContainsKey("restraint"))
                    e.Restraint = Convert.ToInt32(Attributes["restraint"]);

                if (Attributes.ContainsKey("stiffness"))
                    e.Stiffness = (double[])Attributes["stiffness"];
            }
            if (this.Point != null)
                e.Coor = this.Point.ToArray();
            return e;
        }
    }
}
