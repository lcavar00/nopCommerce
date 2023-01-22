using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.MorePrices.Areas.Admin.Factories;
using Nop.Plugin.Misc.MorePrices.Areas.Admin.Models;
using Nop.Plugin.Misc.MorePrices.Domain;
using Nop.Plugin.Misc.MorePrices.Extensions;
using Nop.Plugin.Misc.MorePrices.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Controllers
{
    public class PriceController : BaseAdminController
    {
        private readonly IAclService _aclService;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceModelFactory _priceModelFactory;
        private readonly IPriceService _priceService;
        private readonly IWorkContext _workContext;
        private readonly VendorSettings _vendorSettings;

        public PriceController(IAclService aclService,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPriceModelFactory priceModelFactory,
            IPriceService priceService,
            IWorkContext workContext,
            VendorSettings vendorSettings)
        {
            _aclService = aclService;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _priceModelFactory = priceModelFactory;
            _priceService = priceService;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
        }

        #region Utilities

        //protected virtual void SavePriceAcl(Price price, PriceModel model)
        //{
        //    price.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        //    _priceService.UpdatePrice(price);

        //    var existingAclRecords = _aclService.GetAclRecords(price);
        //    var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
        //    foreach (var customerRole in allCustomerRoles)
        //    {
        //        if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
        //        {
        //            //new role
        //            if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
        //                _aclService.InsertAclRecord(price, customerRole.Id);
        //        }
        //        else
        //        {
        //            //remove role
        //            var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
        //            if (aclRecordToDelete != null)
        //                _aclService.DeleteAclRecord(aclRecordToDelete);
        //        }
        //    }
        //}

        //protected virtual void SaveCategoryMappings(Price price, PriceModel model)
        //{
        //    var existingPriceCategories = _categoryService.GetProductCategoriesByProductId(price.Id, true);

        //    //delete categories
        //    foreach (var existingPriceCategory in existingPriceCategories)
        //        if (!model.SelectedCategoryIds.Contains(existingPriceCategory.CategoryId))
        //            _categoryService.DeletePriceCategory(existingPriceCategory);

        //    //add categories
        //    foreach (var categoryId in model.SelectedCategoryIds)
        //    {
        //        if (_categoryService.FindProductCategory(existingPriceCategories, price.Id, categoryId) == null)
        //        {
        //            //find next display order
        //            var displayOrder = 1;
        //            var existingCategoryMapping = _categoryService.GetProductCategoriesByCategoryId(categoryId, showHidden: true);
        //            if (existingCategoryMapping.Any())
        //                displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
        //            _categoryService.InsertPriceCategory(new PriceCategory
        //            {
        //                PriceId = price.Id,
        //                CategoryId = categoryId,
        //                DisplayOrder = displayOrder
        //            });
        //        }
        //    }
        //}

        //protected virtual void SaveManufacturerMappings(Price product, PriceModel model)
        //{
        //    var existingPriceManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id, true);

        //    //delete manufacturers
        //    foreach (var existingPriceManufacturer in existingPriceManufacturers)
        //        if (!model.SelectedManufacturerIds.Contains(existingPriceManufacturer.ManufacturerId))
        //            _manufacturerService.DeletePriceManufacturer(existingPriceManufacturer);

        //    //add manufacturers
        //    foreach (var manufacturerId in model.SelectedManufacturerIds)
        //    {
        //        if (_manufacturerService.FindProductManufacturer(existingPriceManufacturers, product.Id, manufacturerId) == null)
        //        {
        //            //find next display order
        //            var displayOrder = 1;
        //            var existingManufacturerMapping = _manufacturerService.GetPriceManufacturersByManufacturerId(manufacturerId, showHidden: true);
        //            if (existingManufacturerMapping.Any())
        //                displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
        //            _manufacturerService.InsertPriceManufacturer(new PriceManufacturer
        //            {
        //                PriceId = product.Id,
        //                ManufacturerId = manufacturerId,
        //                DisplayOrder = displayOrder
        //            });
        //        }
        //    }
        //}

        #endregion


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProviderExtension.ManagePrices))
                return AccessDeniedView();

            //prepare model
            var model = _priceModelFactory.PreparePriceSearchModel(new PriceSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult PriceList(PriceSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProviderExtension.ManagePrices))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _priceModelFactory.PreparePriceListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 && _workContext.CurrentVendor != null)
            {
                _notificationService.ErrorNotification(string.Format(_localizationService.GetResource("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            //prepare model
            var model = _priceModelFactory.PreparePriceModel(new PriceModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(PriceModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 && _workContext.CurrentVendor != null)
            {
                _notificationService.ErrorNotification(string.Format(_localizationService.GetResource("Admin.Catalog.Prices.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                    model.VendorId = _workContext.CurrentVendor.Id;

                //price
                var price = model.ToEntity<Price>();
                price.CreatedOnUtc = DateTime.UtcNow;
                price.UpdatedOnUtc = DateTime.UtcNow;
                _priceService.InsertPrice(price);

                ////categories
                //SaveCategoryMappings(price, model);

                ////manufacturers
                //SaveManufacturerMappings(price, model);

                ////ACL (customer roles)
                //SavePriceAcl(price, model);

                //stores
                _priceService.UpdatePriceStoreMappings(price, model.SelectedStoreIds);

                //activity log
                _customerActivityService.InsertActivity("AddNewPrice",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewPrice"), price.Name), price);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Prices.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = price.Id });
            }

            //prepare model
            model = _priceModelFactory.PreparePriceModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProviderExtension.ManagePrices))
                return AccessDeniedView();

            //try to get a product with the specified id
            var price = _priceService.GetPriceById(id);
            if (price == null || price.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && price.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            //prepare model
            var model = _priceModelFactory.PreparePriceModel(null, price);

            return View(model);
        }
    }
}
