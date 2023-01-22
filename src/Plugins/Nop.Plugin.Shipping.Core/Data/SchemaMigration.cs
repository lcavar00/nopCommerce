using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Shipping.Core.Domain;

namespace Nop.Plugin.Shipping.Core.Data
{
    [NopMigration("2021/04/19 08:40:55:0000013", "Shipping.Core base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {

        public override void Up()
        {
            Create.TableFor<ShipmentRequest>();
            Create.TableFor<ShippingByLocationByTotalByWeightRecord>();
        }
    }
}
