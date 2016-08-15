using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flux.GSA.Interop.Impl
{
    class GSAList : IGSAList
    {

        public GSAList()
        {

        }

        public GSAList(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public int? ID { get; set; }

        public string Name { get; set; }

        public string Definition { get; set; }

        public string Type { get; set; }

        public int[] EntityIDs
        {
            get
            {
                if (Definition == null)
                    return null;

                string[] refs = Definition.Split(' ');
                int[] ids = new int[refs.Length];
                for(int i = 0; i < refs.Length; i++)
                {
                    int.TryParse(refs[i], out ids[i]);
                }
                return ids;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
