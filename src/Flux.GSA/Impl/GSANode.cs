using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interop.gsa_8_7;

namespace Flux.GSA.Interop.Impl
{
    class GSANode : IGSANode
    {

        public GSANode()
        {

        }

        public GSANode(GsaNode node)
        {
            this.Color = node.Color;
            this.Coor = node.Coor;
            this.Name = node.Name;
            this.ID = node.Ref;
            this.Restraint = node.Restraint;
            this.Stiffness = node.Stiffness;
        }

        public int? Color { get; set; }

        public double[] Coor  { get; set; }

        public string Name { get; set; }

        public int? ID { get; set; }

        public int? Restraint { get; set; }

        public double[] Stiffness { get; set; }

        public double? X
        {
            get {
                if(Coor != null)
                    return Math.Round(Coor[0], 2);
                return null;
            }
        }

        public double? Y
        {
            get {
                if (Coor != null)
                    return Math.Round(Coor[1], 2);
                return null;
            }
        }

        public double? Z
        {
            get {
                if(Coor != null)
                    return Math.Round(Coor[2], 2);
                return null;
            }
        }

        public override string ToString()
        {
            return String.Format("GSANode:({0},{1},{2})", this.X, this.Y, this.Z);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            GSANode comp = (GSANode)obj;

            if (this.ID.HasValue && comp.ID == this.ID)
                return true;

            if (this.Coor.Count() != comp.Coor.Count())
                return false;

            return (this.X == comp.X) 
                && (this.Y == comp.Y) 
                && (this.Z == comp.Z);
        }
    }
}
