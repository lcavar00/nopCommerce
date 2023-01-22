using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Shipping.Core.Services
{
    /// <summary>
    /// Shipping permission provider
    /// </summary>
    public partial class ShippingPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManagePackagePickupLocations = new PermissionRecord
        {
            Name = "Admin area. Manage Package Pickup Locations",
            SystemName = "ManagePackagePickupLocations",
            Category = "Shipping",
        };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManagePackagePickupLocations,
                    }
                ),
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManagePackagePickupLocations
            };
        }
    }
}
