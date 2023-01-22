using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Configuration;
using Nop.Plugin.ImportExport.Core.Models;
using Nop.Plugin.ImportExport.Core.Services;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Infrastructure.Installation;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.ImportExport.Core.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DbConnectionController : BasePluginController
    {
        private IInstallationLocalizationService _localizationService;
        private IWorkContext _workContext;
        private INopFileProvider _fileProvider;
        private IDbConnectionService _dbConnectionService;

        public DbConnectionController(IInstallationLocalizationService localizationService,
            IWorkContext workContext,
            INopFileProvider fileProvider,
            IDbConnectionService dbConnectionService)
        {
            _localizationService = localizationService;
            _workContext = workContext;
            _fileProvider = fileProvider;
            _dbConnectionService = dbConnectionService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DbConnectionModel
            {
                ConnectionString = "",
                DataProvider = DataProviderType.SqlServer,
                //fast installation service does not support SQL compact
                SqlConnectionInfo = "sqlconnectioninfo_values",
            };

            var dataSettings = await _dbConnectionService.GetForeignDataSettingsAsync();

            if (dataSettings != null)
            {
                model.ConnectionString = dataSettings.ConnectionString;
                model.DataProvider = dataSettings.DataProvider;
                model.DatabaseName = await _dbConnectionService.GetForeignDbNameAsync();
                model.ServerName = await _dbConnectionService.GetForeignServerNameAsync();
            }

            return View("~/Plugins/ImportExport.Core/Views/DbConnection.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Index(DbConnectionModel model)
        {
            if (model.ConnectionString != null)
                model.ConnectionString = model.ConnectionString.Trim();

            //SQL Server
            if (model.DataProvider == DataProviderType.SqlServer)
            {
                if (model.SqlConnectionInfo.Equals("sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
                {
                    //raw connection string
                    if (string.IsNullOrEmpty(model.ConnectionString))
                        ModelState.AddModelError("", _localizationService.GetResource("ConnectionStringWrongFormat"));

                }
                else
                {
                    //values
                    if (string.IsNullOrEmpty(model.ServerName))
                        ModelState.AddModelError("", _localizationService.GetResource("SqlServerNameRequired"));
                    if (string.IsNullOrEmpty(model.DatabaseName))
                        ModelState.AddModelError("", _localizationService.GetResource("DatabaseNameRequired"));

                }
            }

            //Consider granting access rights to the resource to the ASP.NET request identity. 
            //ASP.NET has a base process identity 
            //(typically {MACHINE}\ASPNET on IIS 5 or Network Service on IIS 6 and IIS 7, 
            //and the configured application pool identity on IIS 7.5) that is used if the application is not impersonating.
            //If the application is impersonating via <identity impersonate="true"/>, 
            //the identity will be the anonymous user (typically IUSR_MACHINENAME) or the authenticated request user.
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            //validate permissions
            

            if (ModelState.IsValid)
            {
                try
                {
                    var dataProvider = DataProviderManager.GetDataProvider(model.DataProvider);
                    var connectionString = model.ConnectionStringRaw ? model.ConnectionString : dataProvider.BuildConnectionString(model);

                    //raw connection string
                    if (string.IsNullOrEmpty(model.ConnectionString))
                        ModelState.AddModelError("", _localizationService.GetResource("ConnectionStringWrongFormat"));


                    var dataConfig = new DataConfig
                    {
                        DataProvider = model.DataProvider,
                        ConnectionString = connectionString
                    };

                    var filePath = _fileProvider.MapPath(ImportExportDefaults.ConnectionStringFilePath);
                    if(!_fileProvider.FileExists(ImportExportDefaults.ConnectionStringFilePath))
                    {
                        _fileProvider.CreateFile(filePath);
                    }

                    var text = JsonConvert.SerializeObject(dataConfig, Formatting.Indented);
                    _fileProvider.WriteAllText(filePath, text, Encoding.UTF8);

                    webHelper.RestartAppDomain();
                }
                catch (Exception exception)
                {
                    //TODO: clear provider settings if something got wrong

                    ModelState.AddModelError("", exception.Message);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
