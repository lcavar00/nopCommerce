namespace Nop.Plugin.ImportExport.Core
{
    public static class ImportExportDefaults
    {
        //db connections file path
        public static string ConnectionStringFilePath => "App_Data/dataConnections.json";

        public static string PluginSiteMapNodeName => "Import Export";
        public static string PluginSiteMapNodeSystemName => "Import Export";

        public static string PluginDbConnectionSiteMapNodeName => "Database connection";
        public static string PluginDbConnectionSiteMapNodeSystemName => "Database connection";
    }
}
