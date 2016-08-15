using System;
using System.Collections.Generic;
using Oasys.Gsa.DotNetHelpers;
using Interop.gsa_8_7;
using Flux.SDK.Logger;

namespace Flux.GSA.Interop.Impl
{
    class GSACommand : IGSACommand
    {
        private String _filePath;
        private GsaComUtil _gsa;
        private IFluxLogger _log;

        public GSACommand(String filePath)
        {
            _filePath = filePath;
            _gsa = new GsaComUtil();
            _gsa.GsaOpenFile(ref _filePath);
            _log = LogHelper.GetLogger("Flux.GSAPlugin");
        }

        public string ModelFilePath
        {
            get
            {
                return _filePath;
            }
        }
        

        public void Update()
        {
            //_gsa.GsaObj().Save();
            _gsa.GsaObj().UpdateViews();
        }

        public void Analyze()
        {
            _gsa.GsaObj().Analyse();
        }

        public string FileName
        {
            get
            {
                return _filePath;
            }
        }

        public Dictionary<string, IGSAList> GetAllListsAsMap()
        {
            List<IGSAList> lists = GetAllLists();
            Dictionary<string, IGSAList> result = new Dictionary<string, IGSAList>();
            foreach(GSAList list in lists)
                result.Add(list.Name, list);
            return result;
        }

        public List<IGSAList> GetAllLists()
        {
            List<IGSAList> result = new List<IGSAList>();
            int highest = _gsa.GsaObj().GwaCommand("HIGHEST,LIST");
            for(int i = 0; i < highest; i++)
            {
                string data = _gsa.GsaObj().GwaCommand("GET,LIST," + (i+1));
                if(data != null && !data.Equals(""))
                {
                    string[] tokens = data.Split(',');

                    GSAList item = new GSAList();
                    item.ID = Int32.Parse(tokens[1]);
                    item.Name = tokens[2];
                    item.Type = tokens[3];
                    item.Definition = tokens[4];
                    result.Add(item);
                }
            }
            return result;
        }
        
        
        public int SetList(IGSAList list)
        {
            //get existing GSA list definitions
            Dictionary<string, IGSAList> lists = GetAllListsAsMap();
            if (lists.ContainsKey(list.Name))
                list.ID = lists[list.Name].ID;

            //if the list is new, make sure it has a valid id
            if (!list.ID.HasValue)
                list.ID = _gsa.GsaObj().GwaCommand("HIGHEST,LIST") + 1;

            string command = String.Format(
                "LIST,{0:d},{1:s},{2:s},{3:s}", list.ID, list.Name, list.Type, list.Definition);
            Execute(command);
            return list.ID.Value;
        }

        public int SetNode(IGSANode n)
        {
            /* this is an incredibly stupid/lazy/slow way to do this */
            return CreateNode(n.Name, n.Coor[0], n.Coor[1], n.Coor[2]);
        }

        /**deprecated */
        private int CreateNode(string name, double x, double y, double z)
        {
            int id = _gsa.NodeAt(x, y, z, 0.1);
            if (id > 0)
            {
                string command = string.Format("NODE,{0:d},{1:s},{0:d},{2:f},{3:f},{4:f}", id, name, x, y, z);
                Execute(command);
            }
            return id;
        }

        public IGSAElement GetElementById(int elementID)
        {
            string entity = "EL";
            if (!_gsa.EntExists(ref entity, elementID))
                return null;

            GsaElement e = new GsaElement();
            e.Ref = elementID;
            
            List<int> topo = new List<int>();
            string release1 = "";
            string release2 = "";
            string dummyStr = "";
            double[] release = { 0.0, 0.0, 0.0 };
            double[] offset = { 0.0, 0.0, 0.0 };

            bool found = _gsa.Elem1d(e.Ref, ref e.Property, ref e.Name, ref topo, ref e.OrientNode,
                ref e.Beta, ref release1, ref release2, ref offset, ref offset, ref dummyStr);

            if (!found)
                return null;

            GSAElement ret = new GSAElement();
            ret.ID = e.Ref;
            ret.Property = e.Property;
            ret.Name = e.Name;
            ret.OrientNode = e.OrientNode;
            ret.Beta = e.Beta;
            ret.Dummy = (dummyStr == "DUMMY");
            ret.Topo = new List<IGSANode>();

            foreach(int nodeID in topo)
            {
                IGSANode n = GetNodeById(nodeID);
                ret.Topo.Add(n);
            }
            
            return ret;
        }

        public IGSANode GetNodeById(int nodeID)
        {
            double x = 0.0, y = 0.0, z = 0.0;
            string name = "";

            _gsa.Node(nodeID, ref name, ref x, ref y, ref z);

            GSANode node = new GSANode();
            node.ID = nodeID;
            node.Name = name;
            node.Coor = new double[] { x,y,z};
            return node;
        }
        
        /*
        public void SetDummyElement(int id, bool dummy)
        {

            _log.Trace("set element {0:d} as dummy {0:s}", id, dummy);

            GsaElement e = new GsaElement();
            e.Ref = id;

            List<int> topo = new List<int>(2);
            string release1 = "";
            string release2 = "";
            string dummyStr = "";

            double[] release = { 0.0, 0.0, 0.0 };
            double[] offset = { 0.0, 0.0, 0.0 };

            _gsa.Elem1d(e.Ref, ref e.Property, ref e.Name, ref topo, ref e.OrientNode, 
                ref e.Beta, ref release1, ref release2, ref offset, ref offset, ref dummyStr);

            e.Topo = topo.ToArray();

            _gsa.SetElem1d(e.Ref,e.Property, e.Name, e.Topo[0], e.Topo[1], 
                e.OrientNode, e.Beta,release1, release2, ref offset, ref offset, (dummy)?"DUMMY":"");
        }*/

        public int SetElement(IGSAElement e)
        {
            //now insert/update the element
            IGSAElement original = null;
            double[] release = { 0.0, 0.0, 0.0 };
            double[] offset = { 0.0, 0.0, 0.0 };
            string dummy = "";
            int property = 1;
            int orientNode = 0;
            double beta = 0;

            if (e.ID.HasValue)
            {
                original = GetElementById(e.ID.Value);
            }
            else
            {
                string strEnt = "EL";
                e.ID = _gsa.HighestEnt(ref strEnt) + 1;
            }

            if (e.Property.HasValue)
                property = e.Property.Value;
            else if (original != null)
                property = original.Property.Value;

            if (e.OrientNode.HasValue)
                orientNode = e.OrientNode.Value;
            else if (original != null)
                orientNode = original.OrientNode.Value;

            if (e.Beta.HasValue)
                beta = e.Beta.Value;
            else if (original != null)
                beta = original.Beta.Value;

            if (e.Dummy.HasValue) {
                if(e.Dummy.Value)
                    dummy = "DUMMY";
            }
            else if (original != null)
            {
                if (original.Dummy.Value)
                    dummy = "DUMMY";
            }

            List<int> nodeIds = new List<int>(e.Topo.Count);
            foreach (IGSANode n in e.Topo)
            {
                n.ID = this.SetNode(n);
                nodeIds.Add(n.ID.Value);
            }
            
            _gsa.SetElem1d(e.ID.Value, property, e.Name, nodeIds, orientNode, 
                beta, "", "", ref offset, ref offset, ref dummy);

            return e.ID.Value;
        }

        /*
        public int AddElement(int n1, int n2)
        {
            string strEnt = "EL";
            double[] release = { 0.0, 0.0, 0.0 };
            int e = _gsa.HighestEnt(ref strEnt) + 1;

            _gsa.SetElem1d(e, 1, "", n1, n2, 0, 0, "", "", ref release, ref release, null);
            return e;
        }*/

        public IGSASection GetSectionById(int property)
        {
            int[] sectRefs = {property};
            GsaSection[] sections = null;
            _gsa.GsaObj().Sections(sectRefs, out sections);
            if(sections != null && sections.Length > 0)
                return new GSASection(sections[0]);
            return null;
        }

        public void RemoveAllEntities()
        {
            RemoveAllElements();
            RemoveAllNodes();
        }

        public bool RemoveAllNodes()
        {
            IGSANode[] nodes = GetAllNodes();
            if (nodes != null)
            {
                Array.Sort(nodes, delegate (IGSANode a, IGSANode b) {
                    return -1 * a.ID.Value.CompareTo(b.ID);
                });

                foreach (IGSANode n in nodes)
                {
                    string command = String.Format("DELETE,NODE,{0:d}", n.ID);
                    Execute(command);
                }
                return true;
            }
            return false;
        }

        private bool RemoveAllElements()
        {
            IGSAElement[] elements = GetAllElements();

            if (elements != null)
            {
                Array.Sort(elements, delegate (IGSAElement a, IGSAElement b) {
                    return -1 * a.ID.Value.CompareTo(b.ID);
                });

                foreach (IGSAElement e in elements)
                {
                    RemoveElement(e);
                }
                return true;
            }
            return false;
        }

        

        public IGSAElement[] GetAllElements()
        {
            return GetElementsForList("all");
        }

        public IGSAElement[] GetElementsForList(string name)
        {
            IGSAList list = GetListByName(name);
            if(list != null)
                return GetElementsForList(list.EntityIDs);
            return null;
        }

        public IGSAElement[] GetElementsForList(IGSAList list)
        {
            return GetElementsForList(list.EntityIDs);
        }

        public IGSAElement[] GetElementsForList(int [] ids)
        {
            List<IGSAElement> elements = new List<IGSAElement>();
            foreach (int id in ids) {
                IGSAElement e = GetElementById(id);
                if (e != null)
                    elements.Add(e);
            }
            
            return elements.ToArray();
        }

        private IGSAList GetListByName(string name)
        {
            Dictionary<string, IGSAList> lists = GetAllListsAsMap();
            if (lists != null && lists.ContainsKey(name))
            {
                return lists[name];
            }
            return null;
        }
        /*
        private Dictionary<string, IGSAList> GetListsAsMap()
        {
            List<IGSAList> lists = GetAllLists();
            Dictionary<string, IGSAList> result = new Dictionary<string, IGSAList>();
            foreach (IGSAList list in lists)
                result.Add(list.Name, list);
            return result;
        }

        private List<IGSAList> GetAllLists()
        {
            List<IGSAList> result = new List<IGSAList>();
            int highest = _gsa.GsaObj().GwaCommand("HIGHEST,LIST");
            for (int i = 0; i < highest; i++)
            {
                string data = _gsa.GsaObj().GwaCommand("GET,LIST," + (i + 1));
                if (data != null && !data.Equals(""))
                {
                    string[] tokens = data.Split(',');

                    GSAList item = new GSAList();
                    item.Ref = Int32.Parse(tokens[1]);
                    item.Name = tokens[2];
                    item.Type = tokens[3];
                    item.Definition = tokens[4];
                    result.Add(item);
                }
            }
            return result;
        }*/

        public IGSANode[] GetAllNodes()
        {
            return GetNodesForList("all");
        }

        public IGSANode[] GetNodesForList(string list)
        {
            int[] nodeRefs = null;
            GsaNode[] nodes = null;
            GsaEntity Ent = GsaEntity.NODE;
            short s = _gsa.GsaObj().EntitiesInList(list, ref Ent, out nodeRefs);
            if (nodeRefs != null)
                s = _gsa.GsaObj().Nodes(nodeRefs, out nodes);

            if (nodes != null) { 
                IGSANode[] ret = new IGSANode[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                    ret[i] = new GSANode(nodes[i]);
                
                return ret;
            }
            return null;
        }

        public Dictionary<Int32, IGSANode> GetAllNodesAsMap()
        {
            return GetNodesForListAsMap("all");
        }

        public Dictionary<Int32, IGSANode> GetNodesForListAsMap(string list)
        {
            IGSANode[] nodes = GetNodesForList(list);
            Dictionary<Int32, IGSANode> map = new Dictionary<Int32, IGSANode>();
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    IGSANode node = nodes[i];
                    map.Add(node.ID.Value, node);
                }
            }
            return map;
        }

        private int Execute(string command)
        {
            int value = _gsa.GsaObj().GwaCommand(command);
            _log.Trace(
                "command:{0:s}, value={1:d}", command, value);
            return value;
        }

        public int? SetEntity(IGSAEntity entity)
        {
            Int32? result = null;
            if (typeof(IGSAElement).IsAssignableFrom(entity.GetType()))
                result = this.SetElement((IGSAElement)entity);
            else if (typeof(IGSANode).IsAssignableFrom(entity.GetType()))
                result = this.SetNode((IGSANode)entity);
            else if (typeof(IGSAList).IsAssignableFrom(entity.GetType()))
                result = this.SetList((IGSAList)entity);
            return result;
        }

        public void Remove(IGSAEntity entity)
        {
            if (typeof(IGSAElement).IsAssignableFrom(entity.GetType()))
                this.RemoveElement((IGSAElement)entity);
            //else if (entity.GetType().IsAssignableFrom(typeof(IGSANode)))
            //    this.RemoveNode();
        }

        public void RemoveElement(IGSAElement element)
        {
            if (element.ID.HasValue)
            {
                string command = String.Format("DELETE,EL,{0:d}", element.ID.Value);
                Execute(command);
            }
        }

        public void Archive(IGSAEntity entity)
        {
            if (typeof(IGSAElement).IsAssignableFrom(entity.GetType()))
            {
                IGSAElement e = (IGSAElement)entity;
                e.Dummy = true;
                this.SetElement(e);
            }
        }
    }

}
