using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Abp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Abp.EntityFrameworkCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ConfigureSoftDeleteDbFunction(this ModelBuilder modelBuilder, MethodInfo methodInfo, AbpEfCoreCurrentDbContext abpEfCoreCurrentDbContext)
    {
        modelBuilder.HasDbFunction(methodInfo)
            .HasTranslation(args =>
            {
                // (bool isDeleted, bool boolParam)
                var isDeleted = args[0];
                var boolParam = args[1];

                if (abpEfCoreCurrentDbContext.Context?.IsSoftDeleteFilterEnabled == true)
                {
                    // IsDeleted == false
                    return new SqlBinaryExpression(
                        ExpressionType.Equal,
                        isDeleted,
                        new SqlConstantExpression(false, boolParam.TypeMapping),
                        boolParam.Type,
                        boolParam.TypeMapping);
                }

                // empty where sql
                return new SqlConstantExpression(true, boolParam.Type, boolParam.TypeMapping);
            });

        return modelBuilder;
    }

    public static ModelBuilder ConfigureMayHaveTenantDbFunction(this ModelBuilder modelBuilder, MethodInfo methodInfo, AbpEfCoreCurrentDbContext abpEfCoreCurrentDbContext)
    {
        modelBuilder.HasDbFunction(methodInfo)
            .HasTranslation(args =>
            {
                // (int? tenantId, int? currentTenantId, bool boolParam)
                var tenantId = args[0];
                var currentTenantId = args[1];
                var boolParam = args[2];

                if (abpEfCoreCurrentDbContext.Context?.IsMayHaveTenantFilterEnabled == true)
                {
                    // TenantId == CurrentTenantId
                    return new SqlBinaryExpression(
                        ExpressionType.Equal,
                        tenantId,
                        currentTenantId,
                        boolParam.Type,
                        boolParam.TypeMapping);
                }

                // empty where sql
                return new SqlConstantExpression(true, boolParam.Type, boolParam.TypeMapping);
            });

        return modelBuilder;
    }

    public static ModelBuilder ConfigureMustHaveTenantDbFunction(this ModelBuilder modelBuilder, MethodInfo methodInfo, AbpEfCoreCurrentDbContext abpEfCoreCurrentDbContext)
    {
        modelBuilder.HasDbFunction(methodInfo)
            .HasTranslation(args =>
            {
                // (int tenantId, int? currentTenantId, bool boolParam)
                var tenantId = args[0];
                var currentTenantId = args[1];
                var boolParam = args[2];

                if (abpEfCoreCurrentDbContext.Context?.IsMustHaveTenantFilterEnabled == true)
                {
                    // TenantId == CurrentTenantId
                    return new SqlBinaryExpression(
                        ExpressionType.Equal,
                        tenantId,
                        currentTenantId,
                        boolParam.Type,
                        boolParam.TypeMapping);
                }

                // empty where sql
                return new SqlConstantExpression(true, boolParam.Type, boolParam.TypeMapping);
            });

        return modelBuilder;
    }

    private static ModelBuilder AddTenantIdIndexForEntities<TInterface>(
        this ModelBuilder modelBuilder, 
        string propertyName)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(TInterface).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            var clrType = entityType.ClrType;
            if (clrType.GetProperty(propertyName) != null)
            {
                modelBuilder.Entity(clrType)
                    .HasIndex(propertyName);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds Index for TenantId fields for all entities that implement <see cref="IMayHaveTenant"/>.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ModelBuilder AddMayHaveTenantIndex(this ModelBuilder modelBuilder)
        => modelBuilder.AddTenantIdIndexForEntities<IMayHaveTenant>(nameof(IMayHaveTenant.TenantId));

    /// <summary>
    /// Adds Index for TenantId fields for all entities that implement <see cref="IMustHaveTenant"/>.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ModelBuilder AddMustHaveTenantIndex(this ModelBuilder modelBuilder)
        => modelBuilder.AddTenantIdIndexForEntities<IMustHaveTenant>(nameof(IMustHaveTenant.TenantId));
}
