using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.ImportExport.Core.Domain;

namespace Nop.Plugin.ImportExport.Core.Data
{
    [NopMigration("2022/02/16 12:00:00:0000000", "ImportExport.Core base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : Migration
    {
        public override void Up()
        {
            Create.TableFor<EntityMapping>();
            Create.TableFor<DataEntryMapping>();

        }

        public override void Down()
        {
            Delete.Table("EntityMapping");
            Delete.Table("DataEntryMapping");
        }
    }
}
