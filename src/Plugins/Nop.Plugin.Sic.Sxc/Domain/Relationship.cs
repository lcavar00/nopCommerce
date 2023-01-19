namespace Nop.Plugin.ToSic.Sxc.Domain
{
    public partial class Relationship
    {
        public int RelationshipId { get; set; }
        public int EntityId1 { get; set; }
        public int EntityId2 { get; set; }
        public string Type { get; set; }
        public int SortOrder { get; set; } 
    }
}
