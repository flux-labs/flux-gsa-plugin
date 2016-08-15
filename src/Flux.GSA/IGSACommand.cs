using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flux.GSA.Interop
{
    public interface IGSAEntity
    {
        int? ID { get; set; }
        string Name { get; set; }
    }

    public interface IGSACommand
    {
        string ModelFilePath { get; }

        void Analyze();
        void Update();
        List<IGSAList> GetAllLists();
        Dictionary<string, IGSAList> GetAllListsAsMap();

        Int32? SetEntity(IGSAEntity entity);
        int SetList(IGSAList list);
        int SetElement(IGSAElement e);

        void RemoveAllEntities();
        bool RemoveAllNodes();
        void Remove(IGSAEntity entity);
        void RemoveElement(IGSAElement element);

        void Archive(IGSAEntity entity);

        IGSASection GetSectionById(int id);
        IGSAElement[] GetAllElements();
        IGSAElement[] GetElementsForList(string list);
        IGSAElement[] GetElementsForList(IGSAList list);
        IGSAElement GetElementById(int id);
        IGSAElement[] GetElementsForList(int[] ids);
        IGSANode[] GetAllNodes();
        IGSANode[] GetNodesForList(string list);
        IGSANode GetNodeById(int id);
        Dictionary<Int32, IGSANode> GetAllNodesAsMap();
        Dictionary<Int32, IGSANode> GetNodesForListAsMap(string list);
    }

    public interface IGSANode : IGSAEntity
    {
        int? Color { get; set; }
        double[] Coor { get; set; }
        int? Restraint { get; set; }
        double[] Stiffness { get; set; }
    }

    public interface IGSAElement : IGSAEntity
    {
        double? Beta { get; set; }
        int? Color { get; set; }
        int? eType { get; set; }
        int? Group { get; set; }
        int? NumTopo { get; set; }
        int? OrientNode { get; set; }
        int? Property { get; set; }
        List<IGSANode> Topo { get; set; }
        bool? Dummy { get; set; }
    }

    public interface IGSASection : IGSAEntity
    {
        int Color {get; set; }
        int Material { get; set; }
        string SectDesc { get; set; }
    }

    public interface IGSAList : IGSAEntity
    {
        string Definition{ get; set; }
        string Type { get; set; }
        int [] EntityIDs { get; }
    }
}
