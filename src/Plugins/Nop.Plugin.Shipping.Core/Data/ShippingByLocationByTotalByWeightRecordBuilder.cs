using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Shipping.Core.Domain;

namespace Nop.Plugin.Shipping.Core.Data
{
    public class ShippingByLocationByTotalByWeightRecordBuilder : NopEntityBuilder<ShippingByLocationByTotalByWeightRecord>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.WeightFrom))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.WeightTo))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.OrderSubtotalFrom))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.OrderSubtotalTo))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.AdditionalFixedCost))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.PercentageRateOfSubtotal))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.RatePerWeightUnit))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.LowerWeightLimit))
                .AsDecimal(18, 2)
                .WithColumn(nameof(ShippingByLocationByTotalByWeightRecord.Zip))
                .AsString(400)
                .Nullable();
        }
    }
}
