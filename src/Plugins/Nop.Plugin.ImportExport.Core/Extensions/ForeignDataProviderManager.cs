using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Configuration;

namespace Nop.Plugin.ImportExport.Core.Extensions
{
    public partial class ForeignDataProviderManager : IForeignDataProviderManager
    {

        #region Methods

        /// <summary>
        /// Gets data provider by specific type
        /// </summary>
        /// <param name="dataProviderType">Data provider type</param>
        /// <returns></returns>
        public static INopDataProvider GetDataProvider(DataProviderType dataProviderType)
        {
            return dataProviderType switch
            {
                DataProviderType.SqlServer => new ForeignMsSqlNopDataProvider(),
                DataProviderType.MySql => new ForeignMySqlNopDataProvider(),
                DataProviderType.PostgreSQL => new ForeignPostgreSqlDataProvider(),
                _ => throw new NopException($"Not supported data provider name: '{dataProviderType}'"),
            };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets data provider
        /// </summary>
        public INopDataProvider DataProvider
        {
            get
            {
                var dataProviderType = Singleton<DataConfig>.Instance.DataProvider;

                return GetDataProvider(dataProviderType);
            }
        }

        #endregion
    }
}
