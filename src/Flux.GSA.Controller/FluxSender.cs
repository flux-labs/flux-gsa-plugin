using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flux.SDK.DataTableAPI;
using Flux.SDK.Serialization;
using Flux.GSA.Interop;
using System.IO;
using Flux.SDK.Types;

namespace Flux.GSA.Controller
{
    class FluxSender //:FluxConnection
    {
        public FluxSender(Project project, CellInfo cell, IGSAList list)
        {
            this.Cell = cell;
            this.Project = project;
            this.List = list;
        }

        public CellInfo Cell { get; set; }
        public Project Project { get; set; }
        public IGSAList List { get; set; }

        public void Send(IGSACommand command)
        {
            IGSAElement[] elements = command.GetElementsForList(List);
            if(elements != null)
            {
                SerializationSettings settings = new SerializationSettings();
                settings.EnableConvertersSupport = true;

                string data = DataSerializer.Serialize(elements, settings);
                Stream output = GenerateStreamFromObject(data);
                
                CellInfo ci = Project.DataTable.SetCell(
                    Cell.CellId, output);
            }
        }

        private static Stream GenerateStreamFromObject(object o)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(o);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
