using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flux.GSA.Interop.Impl;

namespace Flux.GSA.Interop
{
    public class GSA
    {
        public static IGSACommand createCommand(string filePath)
        {
            return new GSACommand(filePath);
        }

        public static IGSAElement createElementInstance()
        {
            return new GSAElement();
        }

        public static IGSAList createListInstance()
        {
            return new GSAList();
        }

        public static IGSANode createNodeInstance()
        {
            return new GSANode();
        }
    }
}
