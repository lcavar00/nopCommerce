using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.MorePrices.Domain;

namespace Nop.Plugin.Misc.MorePrices.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/02/07 15:49:17:6455422", "Misc.MorePrices base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<Price>(Create);
        }
    }
}
