using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Misc.MorePrices.Areas.Admin.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Services.Themes;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Extensions
{
    public partial class SettingModelFactoryExtension : SettingModelFactory, ISettingModelFactoryExtension
    {
        public SettingModelFactoryExtension(CurrencySettings currencySettings,
            IAddressAttributeModelFactory addressAttributeModelFactory, 
            IAddressService addressService, 
            IBaseAdminModelFactory baseAdminModelFactory, 
            ICurrencyService currencyService, 
            ICustomerAttributeModelFactory customerAttributeModelFactory, 
            INopDataProvider dataProvider, 
            IDateTimeHelper dateTimeHelper, 
            IFulltextService fulltextService, 
            IGdprService gdprService, 
            ILocalizedModelFactory localizedModelFactory, 
            IGenericAttributeService genericAttributeService, 
            ILocalizationService localizationService, 
            IPictureService pictureService, 
            IReturnRequestModelFactory returnRequestModelFactory, 
            ISettingService settingService, 
            IStoreContext storeContext, 
            IStoreService storeService, 
            IThemeProvider themeProvider, 
            IVendorAttributeModelFactory vendorAttributeModelFactory, 
            IReviewTypeModelFactory reviewTypeModelFactory, 
            IWorkContext workContext) : base(currencySettings, addressAttributeModelFactory, addressService, baseAdminModelFactory, currencyService, customerAttributeModelFactory, dataProvider, dateTimeHelper, fulltextService, gdprService, localizedModelFactory, genericAttributeService, localizationService, pictureService, returnRequestModelFactory, settingService, storeContext, storeService, themeProvider, vendorAttributeModelFactory, reviewTypeModelFactory, workContext)
        {

        }

        public PriceEditorSettingsModel PreparePriceEditorSettingsModel()
        {
            throw new NotImplementedException();
        }
    }
}
