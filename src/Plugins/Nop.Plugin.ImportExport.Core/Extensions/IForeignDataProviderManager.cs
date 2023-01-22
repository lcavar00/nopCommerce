using Nop.Data;

namespace Nop.Plugin.ImportExport.Core.Extensions
{
    public interface IForeignDataProviderManager
    {
        #region Properties

        /// <summary>
        /// Gets data provider
        /// </summary>
        INopDataProvider DataProvider { get; }

        #endregion
    }
}