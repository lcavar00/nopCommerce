using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using LinqToDB.Mapping;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;

namespace Nop.Plugin.ImportExport.Core.Extensions
{
    public static class FluentMigratorExtensions
    {
        #region Utils

        private static Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>> TypeMapping { get; } = new Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>>
        {
            [typeof(int)] = c => c.AsInt32(),
            [typeof(long)] = c => c.AsInt64(),
            [typeof(string)] = c => c.AsString(int.MaxValue).Nullable(),
            [typeof(bool)] = c => c.AsBoolean(),
            [typeof(decimal)] = c => c.AsDecimal(18, 4),
            [typeof(DateTime)] = c => c.AsDateTime2(),
            [typeof(byte[])] = c => c.AsBinary(int.MaxValue),
            [typeof(Guid)] = c => c.AsGuid()
        };

        private static void DefineByOwnType(string columnName, Type propType, CreateTableExpressionBuilder create, bool canBeNullable = false)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentException("The column name cannot be empty");

            if (propType == typeof(string) || propType.FindInterfaces((t, o) => t.FullName?.Equals(o.ToString(), StringComparison.InvariantCultureIgnoreCase) ?? false, "System.Collections.IEnumerable").Length > 0)
                canBeNullable = true;

            var column = create.WithColumn(columnName);

            TypeMapping[propType](column);

            if (canBeNullable)
                create.Nullable();
        }

        #endregion

        /// <summary>
        /// Retrieves expressions into ICreateExpressionRoot
        /// </summary>
        /// <param name="expressionRoot">The root expression for a CREATE operation</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        public static void TableForNonBaseEntity<TEntity>(this ICreateExpressionRoot expressionRoot) where TEntity : class
        {
            var type = typeof(TEntity);
            var builder = expressionRoot.Table(NameCompatibilityManager.GetTableName(type)) as CreateTableExpressionBuilder;
            builder.RetrieveNonBaseEntityTableExpressions(type);
        }

        /// <summary>
        /// Retrieves expressions for building an entity table
        /// </summary>
        /// <param name="builder">An expression builder for a FluentMigrator.Expressions.CreateTableExpression</param>
        /// <param name="type">Type of entity</param>
        public static void RetrieveNonBaseEntityTableExpressions(this CreateTableExpressionBuilder builder, Type type)
        {
            var typeFinder = Singleton<ITypeFinder>.Instance
                .FindClassesOfType(typeof(IEntityBuilder))
                .FirstOrDefault(t => t.BaseType?.GetGenericArguments().Contains(type) ?? false);

            if (typeFinder != null)
                (EngineContext.Current.ResolveUnregistered(typeFinder) as IEntityBuilder)?.MapEntity(builder);

            var expression = builder.Expression;
            //if there are no columns named Id, create one. If there are no primary keys columns, make the Id column primary key
            if (!expression.Columns.Any(c => c.Name == nameof(BaseEntity.Id)))
            {
                var pk = new ColumnDefinition
                {
                    Name = nameof(BaseEntity.Id),
                    Type = DbType.Int32,
                    IsIdentity = true,
                    TableName = NameCompatibilityManager.GetTableName(type),
                    ModificationType = ColumnModificationType.Create,
                    IsPrimaryKey = !expression.Columns.Any(c => c.IsPrimaryKey)
                };
                expression.Columns.Insert(0, pk);
                builder.CurrentColumn = pk;
            }

            var propertiesToAutoMap = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
                .Where(pi => !pi.HasAttribute<NotMappedAttribute>() && !pi.HasAttribute<NotColumnAttribute>() &&
                !expression.Columns.Any(x => x.Name.Equals(NameCompatibilityManager.GetColumnName(type, pi.Name), StringComparison.OrdinalIgnoreCase)) &&
                TypeMapping.ContainsKey(GetTypeToMap(pi.PropertyType).propType));

            foreach (var prop in propertiesToAutoMap)
            {
                var columnName = NameCompatibilityManager.GetColumnName(type, prop.Name);
                var (propType, canBeNullable) = GetTypeToMap(prop.PropertyType);
                DefineByOwnType(columnName, propType, builder, canBeNullable);
            }
        }

        public static (Type propType, bool canBeNullable) GetTypeToMap(this Type type)
        {
            if (Nullable.GetUnderlyingType(type) is Type uType)
                return (uType, true);

            return (type, false);
        }
    }
}
