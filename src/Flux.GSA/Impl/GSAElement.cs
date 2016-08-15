using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interop.gsa_8_7;

namespace Flux.GSA.Interop.Impl
{
    public class GSAElement : IGSAElement
    {
        
        public GSAElement()
        {
            this.Topo = new List<IGSANode>();
        }
        
        public double? Beta {get;set;}
        public int? Color { get; set; }
        public int? eType { get; set; }
        public int? Group { get; set; }
        public string Name { get; set; }
        public int? NumTopo { get; set; }
        public int? OrientNode { get; set; }
        public int? Property { get; set; }
        public int? ID { get; set; }
        public List<IGSANode> Topo { get; set; }
        public bool? Dummy { get; set; }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (IGSANode n in Topo)
                hashCode = (hashCode + n.GetHashCode()) % Int32.MaxValue;
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            GSAElement comp = (GSAElement)obj;

            //first check for equivalence of ids
            if (this.ID.HasValue && comp.ID == this.ID)
                return true;

            //if unequal in length, we know they are different
            if (comp.Topo.Count() != this.Topo.Count())
                return false;

            //compare sets of nodes, and fail as quickly as possible
            HashSet<IGSANode> set = new HashSet<IGSANode>(this.Topo);
            foreach (IGSANode n in comp.Topo)
            {
                if (!set.Contains(n))
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("GSAElement:[{0},{1}]", Topo[0], Topo[1]);
        }
    }
}
