using System;
using Nop.Core;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Photography.CMS.Areas.Admin.Infrastructure.Mapper.Extensions
{
    public static class MappingExtensions
    {
        private static TDestination Map<TDestination>(this object source)
        {
            return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }

        public static TModel ToModel<TModel>(this BaseEntity entity) where TModel : BaseNopEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }
    }
}
