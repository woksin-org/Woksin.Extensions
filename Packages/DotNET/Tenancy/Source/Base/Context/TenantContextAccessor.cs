// Copyright (c) woksin-org. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Woksin.Extensions.Tenancy.Context;

/// <summary>
/// Represents the async local implementation of <see cref="ITenantContextAccessor{TTenant}"/> and <see cref="ITenantContextAccessor"/>.
/// </summary>
/// <typeparam name="TTenant">The <see cref="Type"/> of the <see cref="ITenantInfo"/>.</typeparam>
public class TenantContextAccessor<TTenant> : ITenantContextAccessor<TTenant>, ITenantContextAccessor
    where TTenant : class, ITenantInfo, new()
{
    readonly IOptionsMonitor<TenancyOptions<TTenant>> _options;
    static readonly AsyncLocal<ITenantContext<TTenant>> _local = new();

    public TenantContextAccessor(IOptionsMonitor<TenancyOptions<TTenant>> options)
    {
        _options = options;
        CurrentTenant = TenantContext<TTenant>.Unresolved();
    }

    /// <inheritdoc />
    public ITenantContext<TTenant> CurrentTenant
    {
        get
        {
            // This is simply a minor performance optimization to avoid accessing the AsyncLocal when not necessary. 
            if (_options.CurrentValue.IsUsingStaticTenant(out var staticTenantId))
            {
                return TenantContext<TTenant>.Static(new TTenant
                {
                    Id = staticTenantId
                });
            }
            return _local.Value ?? TenantContext<TTenant>.Unresolved();
        }

        set
        {
            // Only set tenant when not using static tenant
            if (!_options.CurrentValue.IsUsingStaticTenant(out var _))
            {
                _local.Value = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
    }

    /// <inheritdoc />
    ITenantContext ITenantContextAccessor.CurrentTenant
    {
        get => CurrentTenant as ITenantContext ?? TenantContext<TTenant>.Unresolved();
        set => CurrentTenant = value as ITenantContext<TTenant> ?? throw new ArgumentNullException(nameof(value));
    }
}
