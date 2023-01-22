using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.WsPay.Domain;

namespace Nop.Plugin.Payments.WsPay.Data
{
    [NopMigration("2021/07/19 10:00:00:0000013", "Payments.WsPay base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : Migration
    {

        public override void Up()
        {
            Create.TableFor<RatePayment>();
        }

        public override void Down()
        {
            Delete.Table("RatePayment");
        }
    }
}
