namespace Nop.Plugin.ToSic.Sxc.Domain
{
    public partial class AttributeValue
    {
        public int AttributeValueId { get; set; }
        public int EntityId { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public int SortOrder { get; set; }
    }
}
