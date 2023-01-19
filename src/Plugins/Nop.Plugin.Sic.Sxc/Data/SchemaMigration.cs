using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.ToSic.Sxc.Data
{
    [NopMigration("2022/02/03 09:27:23:6455432", "Sic.Sxc base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("EntityType")
              .WithColumn("EntityTypeId").AsInt64().PrimaryKey().Identity()
              .WithColumn("Name").AsString(255).NotNullable()
              .WithColumn("Description").AsString(int.MaxValue).Nullable();

            Create.Table("Attribute")
                   .WithColumn("AttributeId").AsInt64().PrimaryKey().Identity()
                   .WithColumn("EntityTypeId").AsInt64().NotNullable()
                   .WithColumn("Name").AsString(255).NotNullable()
                   .WithColumn("Description").AsString(int.MaxValue).Nullable()
                   .WithColumn("DataType").AsString(255).NotNullable()
                   .WithColumn("Required").AsBoolean().NotNullable().WithDefaultValue(false)
                   .WithColumn("IsSearchable").AsBoolean().NotNullable().WithDefaultValue(false);

            Create.Table("AttributeSet")
              .WithColumn("AttributeSetId").AsInt64().PrimaryKey().Identity()
              .WithColumn("EntityTypeId").AsInt64().NotNullable()
              .WithColumn("Name").AsString(255).NotNullable()
              .WithColumn("Description").AsString(int.MaxValue).Nullable();

            Create.Table("AttributeValue")
              .WithColumn("AttributeValueId").AsInt64().PrimaryKey().Identity()
              .WithColumn("EntityId").AsInt64().NotNullable()
              .WithColumn("AttributeId").AsInt64().NotNullable()
              .WithColumn("Value").AsString(int.MaxValue).Nullable()
              .WithColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0);

            Create.Table("Entity")
               .WithColumn("EntityId").AsInt64().PrimaryKey().Identity()
               .WithColumn("EntityTypeId").AsInt64().NotNullable()
               .WithColumn("ParentId").AsInt64().Nullable()
               .WithColumn("Title").AsString(255).NotNullable()
               .WithColumn("Description").AsString(int.MaxValue).Nullable()
               .WithColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0)
               .WithColumn("CreatedOn").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
               .WithColumn("CreatedBy").AsString(255).NotNullable()
               .WithColumn("PublishedOn").AsDateTime().Nullable()
               .WithColumn("PublishedBy").AsString(255).Nullable()
               .WithColumn("ModifiedOn").AsDateTime().Nullable()
               .WithColumn("ModifiedBy").AsString(255).Nullable();

            Create.Table("Relationship")
              .WithColumn("RelationshipId").AsInt64().PrimaryKey().Identity()
              .WithColumn("EntityId1").AsInt64().NotNullable()
              .WithColumn("EntityId2").AsInt64().NotNullable()
              .WithColumn("Type").AsString(255).NotNullable()
              .WithColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0);


            Create.ForeignKey("FK_Entity_EntityType")
               .FromTable("Entity").ForeignColumn("EntityTypeId")
               .ToTable("EntityType").PrimaryColumn("EntityTypeId");

            Create.ForeignKey("FK_Entity_Entity")
               .FromTable("Entity").ForeignColumn("ParentId")
               .ToTable("Entity").PrimaryColumn("EntityId");

            Create.ForeignKey("FK_AttributeValue_Entity")
               .FromTable("AttributeValue").ForeignColumn("EntityId")
               .ToTable("Entity").PrimaryColumn("EntityId");

            Create.ForeignKey("FK_AttributeValues_Attribute")
               .FromTable("AttributeValue").ForeignColumn("AttributeId")
               .ToTable("Attribute").PrimaryColumn("AttributeId");

            Create.ForeignKey("FK_AttributeSet_EntityType")
               .FromTable("AttributeSet").ForeignColumn("EntityTypeId")
               .ToTable("EntityType").PrimaryColumn("EntityTypeId");

            Create.ForeignKey("FK_Attribute_EntityType")
               .FromTable("Attribute").ForeignColumn("EntityTypeId")
               .ToTable("EntityType").PrimaryColumn("EntityTypeId");

            Create.ForeignKey("FK_Relationship_Entity1")
              .FromTable("Relationship").ForeignColumn("EntityId1")
              .ToTable("Entity").PrimaryColumn("EntityId");

            Create.ForeignKey("FK_Relationship_Entity2")
               .FromTable("Relationship").ForeignColumn("EntityId2")
               .ToTable("Entity").PrimaryColumn("EntityId");
        }
    }
}
