using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flux.SDK.DataTableAPI;
using Flux.SDK.DataTableAPI.DatatableTypes;
using Flux.SDK.Serialization;
using Flux.GSA.Interop;
using System.IO;
using Flux.SDK.Types;
using Flux.SDK.Logger;

namespace Flux.GSA.Controller
{
    
    class FluxReceiver //:FluxConnection
    {
        private IFluxLogger _log;

        public FluxReceiver (Project project, CellInfo cell, bool analyze)
        {
            _log = LogHelper.GetLogger("Flux.GSA.Controller");

            this.Cell = cell;
            this.Project = project;
            this.Analyze = analyze;
        }
        
        public CellInfo Cell { get; set; }
        public Project Project { get; set; }
        public bool Analyze { get; set; }

        public string ListName
        {
            get
            {
                return string.Format("{0:s} ({1:s})", Cell.ClientMetadata.Label, Cell.CellId);
            }
        }

        public void Receive(IGSACommand command)
        {
            SerializationSettings settings = new SerializationSettings();
            settings.EnableConvertersSupport = true;

            Cell result = Project.DataTable.GetCell(Cell.CellId, true, true);

            var model = DataSerializer.Deserialize<IGSAElement[]>(
                result.Value.Stream, settings);

            if (model != null)
            {
                ReceiveElements(command, model);
            }
        }

        public void ReceiveElements(IGSACommand command, object[] received)
        {
            ISet<IGSAEntity> elements = GetElements(received);
            Dictionary<IGSAEntity, IGSAEntity> existing = GetExistingElements(command);
            if (elements.Count > 0 || existing.Count > 0)
            {
                _log.Info("Received {0} elements from Flux.", elements.Count);

                ISet<IGSAEntity> all = Receive(command, elements, existing);

                //update a list that manages elements from Flux
                IGSAList list = Flux.GSA.Interop.GSA.createListInstance();
                list.Name = ListName;
                list.Definition = GetListDefinition(all);
                list.Type = "ELEMENT";
                command.SetList(list);
            }
        }

        public ISet<IGSAEntity> Receive(IGSACommand command, ISet<IGSAEntity> received, 
            Dictionary<IGSAEntity,IGSAEntity> existing)
        {
            ISet<IGSAEntity> all = Merge(command, received, existing);
            
            _log.Info("Updating GSA views to reflect revisions.");
            command.Update();

            if (Analyze)
            {
                _log.Info("Running GSA analysis...");
                command.Analyze();
            }

            return all;
        }

        private ISet<IGSAEntity> Merge(IGSACommand command, ISet<IGSAEntity> received, 
            Dictionary<IGSAEntity, IGSAEntity> existing)
        {
            _log.Debug("Merging {0} entities from Flux with {1} entities in list '{2}'",
                received.Count, existing.Count, ListName);

            //keep a set of all entities managed by Flux
            ISet<IGSAEntity> all = new HashSet<IGSAEntity>(existing.Keys);

            //everything is removed until it is matched in the received set
            Dictionary<IGSAEntity, IGSAEntity> removed = 
                new Dictionary<IGSAEntity, IGSAEntity>(existing);

            foreach (IGSAEntity e in received)
            {
                if (existing != null && existing.ContainsKey(e))
                {
                    _log.Trace("Entity {0} was matched to id:{1} in the existing set.", 
                        e, existing[e].ID);
                    e.ID = existing[e].ID;

                    //we do not need to remove the entity from GSA
                    removed.Remove(e);
                }

                //insert or update the entity (and its nodes)
                e.ID = command.SetEntity(e);

                all.Add(e);
            }

            _log.Debug("Archiving entities not found in the received set.");
            foreach (IGSAEntity entity in removed.Keys)
                command.Archive(entity);

            return all;
        }

        private Dictionary<IGSAEntity, IGSAEntity> GetExistingElements(IGSACommand command)
        {
            IGSAEntity[] elements = command.GetElementsForList(ListName);
            Dictionary<IGSAEntity, IGSAEntity> d = new Dictionary<IGSAEntity, IGSAEntity>();
            if (elements != null)
            {
                foreach (IGSAEntity e in elements)
                {
                    if(!d.ContainsKey(e))
                        d.Add(e, e);
                }
            }
            return d;
        }

        private ISet<IGSAEntity> GetElements(Object[] list)
        {
            ISet<IGSAEntity> elements = new HashSet<IGSAEntity>();
            foreach (Object o in list)
            {
                if (typeof(IGSAElement).IsAssignableFrom(o.GetType()))
                {
                    IGSAElement e = (IGSAElement)o;
                    if(!elements.Contains(e))
                        elements.Add(e);
                }
            
            }
            return elements;
        }

        private string GetListDefinition(ISet<IGSAEntity> entities)
        {
            StringBuilder b = new StringBuilder();
            foreach (IGSAEntity e in entities)
                b.Append(" ").Append(e.ID);

            //remove the first char if we've added items to the list
            if (b.Length > 0)
                b.Remove(0, 1);

            return b.ToString();
        }
        

        #region SDK improvements
        private static string GenerateStringFromStream(Stream stream)
        {
            string streamStr;
            if (stream.CanSeek)
                stream.Position = 0;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                streamStr = reader.ReadToEnd();
            }

            stream.Close();

            return streamStr;
        }
        #endregion
    }
}
