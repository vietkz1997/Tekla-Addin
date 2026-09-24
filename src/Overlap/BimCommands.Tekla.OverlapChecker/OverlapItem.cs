using Tekla.Structures.Model;
using TeklaPoint = global::Tekla.Structures.Geometry3d.Point;

namespace BimCommands.Tekla.OverlapChecker
{
    public class OverlapItem
    {
        public bool IsSelected { get; set; }
        public ModelObject DuplicateObject { get; set; }
        public ModelObject OriginalObject { get; set; }
        public int DuplicateId { get; set; }
        public int OriginalId { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Profile { get; set; }
        public string CenterStr { get; set; }
        public string OverlapType { get; set; }
        public TeklaPoint CenterPoint { get; set; }
        public TeklaPoint MinPoint { get; set; }
        public TeklaPoint MaxPoint { get; set; }
    }
}
