using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interop.gsa_8_7;

namespace Flux.GSA.Interop.Impl
{
    class GSASection : IGSASection
    {
        public GSASection()
        {

        }

        public GSASection(GsaSection section) {
            this.Color = section.Color;
            this.Material = section.Material;
            this.Name = section.Name;
            this.ID = section.Ref;
            this.SectDesc = section.SectDesc;
        }

        public int Color { get; set; }

        public int Material { get; set; }
        
        public string Name { get; set; }
        
        public int? ID { get; set; }

        public string SectDesc { get; set; }
    }
}
