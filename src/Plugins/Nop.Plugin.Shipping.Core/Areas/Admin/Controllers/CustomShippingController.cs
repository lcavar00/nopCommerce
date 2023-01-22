using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Shipping.Pickup;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CustomShippingController : ShippingController
    {
        #region Fields

        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IShippingModelFactory _shippingModelFactory;
        private readonly IShippingService _shippingService;

        #endregion

        #region Ctor

        public CustomShippingController(IAddressAttributeParser addressAttributeParser,
            IAddressService addressService, 
            ICountryService countryService, 
            ICustomerActivityService customerActivityService, 
            IDateRangeService dateRangeService, 
            IEventPublisher eventPublisher, 
            ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService, 
            INotificationService notificationService, 
            IPermissionService permissionService, 
            IPickupPluginManager pickupPluginManager, 
            ISettingService settingService, 
            IShippingModelFactory shippingModelFactory, 
            IShippingPluginManager shippingPluginManager, 
            IShippingService shippingService, 
            IGenericAttributeService genericAttributeService, 
            IWorkContext workContext, 
            ShippingSettings shippingSettings) : base(addressService, countryService, customerActivityService, dateRangeService, eventPublisher, localizationService, localizedEntityService, notificationService, permissionService, pickupPluginManager, settingService, shippingModelFactory, shippingPluginManager, shippingService, genericAttributeService, workContext, shippingSettings)
        {
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _shippingModelFactory = shippingModelFactory;
            _shippingService = shippingService;
        }

        #endregion

        #region Warehouses

        public override async Task<IActionResult> EditWarehouse(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            //try to get a warehouse with the specified id
            var warehouse = await _shippingService.GetWarehouseByIdAsync(id);
            if (warehouse == null)
                return RedirectToAction("Warehouses");

            //prepare model
            var model = await _shippingModelFactory.PrepareWarehouseModelAsync(null, warehouse);

            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomShipping/EditWarehouse.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public override async Task<IActionResult> EditWarehouse(WarehouseModel model, bool continueEditing)
        {
            var form = HttpContext.Request.Form;

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            //try to get a warehouse with the specified id
            var warehouse = await _shippingService.GetWarehouseByIdAsync(model.Id);
            if (warehouse == null)
                return RedirectToAction("Warehouses");

            if (ModelState.IsValid)
            {
                var address = await _addressService.GetAddressByIdAsync(warehouse.AddressId) ??
                    new Address
                    {
                        CreatedOnUtc = DateTime.UtcNow
                    };

                var city = form["Address.City"];
                var firstName = form["Address.FirstName"];
                var lastName = form["Address.LastName"];
                var phoneNumber = form["Address.PhoneNumber"];
                var company = form["Address.Company"];
                int.TryParse(form["Address.CountryId"], out int countryId);
                int.TryParse(form["Address.StateProvinceId"], out int stateProvinceId);
                var county = form["Address.County"];
                var email = form["Address.Email"];
                var faxNumber = form["Address.FaxNumer"];
                var address1 = form["Address.Address1"];
                var address2 = form["Address.Address2"];
                var zipPostalCode = form["Address.ZipPostalCode"];

                address.Address1 = address1;
                address.Address2 = address2;
                address.City = city;
                address.Company = company;
                address.CountryId = countryId;
                address.County = county;
                address.CustomAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                address.Email = email;
                address.FaxNumber = faxNumber;
                address.FirstName = firstName;
                address.LastName = lastName;
                address.PhoneNumber = phoneNumber;
                address.StateProvinceId = stateProvinceId;
                address.ZipPostalCode = zipPostalCode;
                if (address.Id > 0)
                {
                    address.CustomAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                    await _addressService.UpdateAddressAsync(address);
                }
                else
                {
                    address.CustomAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                    await _addressService.InsertAddressAsync(address);
                }

                //fill entity from model
                warehouse = model.ToEntity(warehouse);

                warehouse.AddressId = address.Id;

                await _shippingService.UpdateWarehouseAsync(warehouse);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditWarehouse",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditWarehouse"), warehouse.Id), warehouse);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Warehouses.Updated"));

                return continueEditing ? RedirectToAction("EditWarehouse", warehouse.Id) : RedirectToAction("Warehouses");
            }

            //prepare model
            model = await _shippingModelFactory.PrepareWarehouseModelAsync(model, warehouse, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomShipping/EditWarehouse.cshtml", model);
        }

        #endregion
    }
}
