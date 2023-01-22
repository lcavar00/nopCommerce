using Nop.Web.Framework.Models;

namespace Nop.Plugin.Shipping.Core.Models
{
    public partial record ShippingByTotalByLocationListModel : BasePagedListModel<ShippingByLocationByTotalByWeightModel>
    {
    }
}
