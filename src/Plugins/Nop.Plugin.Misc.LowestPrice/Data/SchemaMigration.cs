using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.LowestPrice.Domain;

namespace Nop.Plugin.Misc.LowestPrice.Data
{
    [NopMigration("2022/08/22 11:00:00:0000000", "Misc.LowestPrice base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<ProductPriceLog>();
            Create.TableFor<ProductAttributeCombinationPriceLog>();
            Create.TableFor<DiscountLog>();
            Create.TableFor<PriceLogDiscountLog>();
        }
    }
}
