namespace Nop.Plugin.ToSic.Sxc.Domain
{
    public partial class Attribute
    {
        public int AttributeId { get; set; }
        public int EntityTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool Required { get; set; }
        public bool IsSearchable { get; set; }
    }
}
