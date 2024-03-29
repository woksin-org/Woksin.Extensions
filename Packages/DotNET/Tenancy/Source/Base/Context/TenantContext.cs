// Copyright (c) woksin-org. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Woksin.Extensions.Tenancy.Strategies;

namespace Woksin.Extensions.Tenancy.Context;

/// <summary>
/// Represents an implementation of <see cref="ITenantContext"/> and <see cref="ITenantContext{TTenant}"/>.
/// </summary>
/// <typeparam name="TTenant">The <see cref="Type"/> of the <see cref="ITenantInfo"/>.</typeparam>
public record TenantContext<TTenant> : ITenantContext<TTenant>, ITenantContext
    where TTenant : class, ITenantInfo, new()
{
    public static TenantContext<TTenant> Resolved(TTenant tenant, StrategyInfo? strategyInfo = null) => new(tenant, strategyInfo);
    public static TenantContext<TTenant> Static(TTenant tenant) => new(tenant, new StrategyInfo(typeof(StaticStrategy), new StaticStrategy(tenant.Id)));
    public static TenantContext<TTenant> Unresolved() => new();

    TenantContext()
    {
    }

    TenantContext(TTenant tenant, StrategyInfo? strategyInfo)
    {
        Tenant = tenant;
        Strategy = strategyInfo;
    }

    public TTenant? Tenant { get; }

    /// <inheritdoc cref="ITenantContext{TTenantInfo}" />
    public StrategyInfo? Strategy { get;  }

    /// <inheritdoc />
    ITenantInfo? ITenantContext.Tenant => Tenant;

    public bool Resolved([NotNullWhen(true)] out TTenant? tenantInfo, out StrategyInfo? strategyInfo)
    {
        tenantInfo = Tenant;
        strategyInfo = Strategy;
        return Tenant is not null;
    }
}


/// <summary>
/// Represents an implementation of <see cref="ITenantContext"/> and <see cref="ITenantContext{TTenant}"/> for <see cref="TenantInfo"/>.
/// </summary>
public record TenantContext : ITenantContext<TenantInfo>, ITenantContext
{
    public static TenantContext Resolved(TenantInfo tenant, StrategyInfo? strategyInfo = null) => new(tenant, strategyInfo);
    
    public static TenantContext Static(TenantInfo tenant) => new(tenant, new StrategyInfo(typeof(StaticStrategy), new StaticStrategy(tenant.Id)));
    public static TenantContext Static(string tenantId) => new(new TenantInfo(){Id = tenantId}, new StrategyInfo(typeof(StaticStrategy), new StaticStrategy(tenantId)));
    public static TenantContext Unresolved() => new();

    TenantContext()
    {
    }

    TenantContext(TenantInfo tenant, StrategyInfo? strategyInfo)
    {
        Tenant = tenant;
        Strategy = strategyInfo;
    }

    public TenantInfo? Tenant { get; }

    /// <inheritdoc cref="ITenantContext{TTenantInfo}" />
    public StrategyInfo? Strategy { get;  }

    /// <inheritdoc />
    ITenantInfo? ITenantContext.Tenant => Tenant;

    public bool Resolved([NotNullWhen(true)] out TenantInfo? tenantInfo, out StrategyInfo? strategyInfo)
    {
        tenantInfo = Tenant;
        strategyInfo = Strategy;
        return Tenant is not null;
    }
}
