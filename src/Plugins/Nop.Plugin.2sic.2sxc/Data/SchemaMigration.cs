using FluentMigrator;
using FluentMigrator.Infrastructure;
using Nop.Data.Migrations;

namespace Nop.Plugin.Sic.Sxc.Data
{
    [NopMigration("2022/02/03 09:27:23:6455432", "Sic.Sxc base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("EntityTypes")
              .WithColumn("EntityTypeId").AsInt64().PrimaryKey().Identity()
              .WithColumn("Name").AsString(255).NotNullable()
              .WithColumn("Description").AsString(int.MaxValue).Nullable();

            Create.Table("Attributes")
                   .WithColumn("AttributeId").AsInt64().PrimaryKey().Identity()
                   .WithColumn("EntityTypeId").AsInt64().NotNullable()
                   .WithColumn("Name").AsString(255).NotNullable()
                   .WithColumn("Description").AsString(int.MaxValue).Nullable()
                   .WithColumn("DataType").AsString(255).NotNullable()
                   .WithColumn("Required").AsBoolean().NotNullable().WithDefaultValue(false)
                   .WithColumn("IsSearchable").AsBoolean().NotNullable().WithDefaultValue(false);

            Create.Table("AttributeSets")
              .WithColumn("AttributeSetId").AsInt64().PrimaryKey().Identity()
              .WithColumn("EntityTypeId").AsInt64().NotNullable()
              .WithColumn("Name").AsString(255).NotNullable()
              .WithColumn("Description").AsString(int.MaxValue).Nullable();

            Create.Table("AttributeValues")
              .WithColumn("AttributeValueId").AsInt64().PrimaryKey().Identity()
              .WithColumn("EntityId").AsInt64().NotNullable()
              .WithColumn("AttributeId").AsInt64().NotNullable()
              .WithColumn("Value").AsString(int.MaxValue).Nullable()
              .WithColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0);

            Create.Table("Entities")
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

            Create.Table("Relationships")
              .WithColumn("RelationshipId").AsInt64().PrimaryKey().Identity()
              .WithColumn("EntityId1").AsInt64().NotNullable()
              .WithColumn("EntityId2").AsInt64().NotNullable()
              .WithColumn("Type").AsString(255).NotNullable()
              .WithColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0);


            Create.ForeignKey("FK_Entities_EntityTypes")
               .FromTable("Entities").ForeignColumn("EntityTypeId")
               .ToTable("EntityTypes").PrimaryColumn("EntityTypeId");

            Create.ForeignKey("FK_Entities_Entities")
               .FromTable("Entities").ForeignColumn("ParentId")
               .ToTable("Entities").PrimaryColumn("EntityId");

            Create.ForeignKey("FK_AttributeValues_Entities")
               .FromTable("AttributeValues").ForeignColumn("EntityId")
               .ToTable("Entities").PrimaryColumn("EntityId");

            Create.ForeignKey("FK_AttributeValues_Attributes")
               .FromTable("AttributeValues").ForeignColumn("AttributeId")
               .ToTable("Attributes").PrimaryColumn("AttributeId");

            Create.ForeignKey("FK_AttributeSets_EntityTypes")
               .FromTable("AttributeSets").ForeignColumn("EntityTypeId")
               .ToTable("EntityTypes").PrimaryColumn("EntityTypeId");

            Create.ForeignKey("FK_Attributes_EntityTypes")
               .FromTable("Attributes").ForeignColumn("EntityTypeId")
               .ToTable("EntityTypes").PrimaryColumn("EntityTypeId");

            Create.ForeignKey("FK_Relationships_Entities1")
              .FromTable("Relationships").ForeignColumn("EntityId1")
              .ToTable("Entities").PrimaryColumn("EntityId");

            Create.ForeignKey("FK_Relationships_Entities2")
               .FromTable("Relationships").ForeignColumn("EntityId2")
               .ToTable("Entities").PrimaryColumn("EntityId");
        }
    }
}
