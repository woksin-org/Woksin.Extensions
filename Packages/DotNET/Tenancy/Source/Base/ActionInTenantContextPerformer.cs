using Microsoft.Extensions.Options;
using Woksin.Extensions.Tenancy.Context;

namespace Woksin.Extensions.Tenancy;

/// <summary>
/// Represents an implementation of <see cref="IPerformActionInTenantContext"/>.
/// </summary>
/// <typeparam name="TTenant">The <see cref="Type"/> of the <see cref="ITenantInfo"/>.</typeparam>
public class ActionInTenantContextPerformer<TTenant> : IPerformActionInTenantContext<TTenant>, IPerformActionInTenantContext
    where TTenant : class, ITenantInfo, new()
{
    readonly ITenantContextAccessor<TTenant> _tenantContextAccessor;
    readonly ITenantContextAccessor _nonTypedTenantContextAccessor;
    readonly IOptionsMonitor<TenancyOptions<TTenant>> _options;

    public ActionInTenantContextPerformer(ITenantContextAccessor<TTenant> tenantContextAccessor, ITenantContextAccessor nonTypedTenantContextAccessor, IOptionsMonitor<TenancyOptions<TTenant>> options)
    {
        _tenantContextAccessor = tenantContextAccessor;
        _nonTypedTenantContextAccessor = nonTypedTenantContextAccessor;
        _options = options;
    }

    void ThrowIfTenantContextDisabled()
    {
        ITenantContextAccessor<TTenant>.ThrowIfDisabledTenantContext(_tenantContextAccessor, _options.CurrentValue);
    }
    
    public async Task<TResult> Perform<TResult>(ITenantContext<TTenant> tenant, Func<ITenantContext<TTenant>, TResult> callback)
    {
        ThrowIfTenantContextDisabled();
        return await Task.Run(() =>
        {
            _tenantContextAccessor.CurrentTenant = tenant;
            return callback(tenant);
        });
    }
    public async Task<TResult> Perform<TResult>(ITenantContext<TTenant> tenant, Func<ITenantContext<TTenant>, Task<TResult>> callback)
    {
        ThrowIfTenantContextDisabled();
        return await Task.Run(async() =>
        {
            _tenantContextAccessor.CurrentTenant = tenant;
            return await callback(tenant);
        });
    }

    public async Task Perform<TResult>(ITenantContext<TTenant> tenant, Action<ITenantContext<TTenant>> callback)
    {
        ThrowIfTenantContextDisabled();
        await Task.Run(() =>
        {
            _tenantContextAccessor.CurrentTenant = tenant;
            callback(tenant);
        });
    }
    
    public async Task Perform<TResult>(ITenantContext<TTenant> tenant, Func<ITenantContext<TTenant>, Task> callback)
    {
        ThrowIfTenantContextDisabled();
        await Task.Run(async () =>
        {
            _tenantContextAccessor.CurrentTenant = tenant;
            await callback(tenant);
        });
    }

    public async Task<TResult> Perform<TResult>(TenantContext tenant, Func<TenantContext, TResult> callback)
    {
        return await Task.Run(() =>
        {
            _nonTypedTenantContextAccessor.CurrentTenant = tenant;
            return callback(tenant);
        });
    }
    
    public async Task<TResult> Perform<TResult>(TenantContext tenant, Func<TenantContext, Task<TResult>> callback)
    {
        return await Task.Run(async () =>
        {
            _nonTypedTenantContextAccessor.CurrentTenant = tenant;
            return await callback(tenant);
        });
    }

    public async Task Perform<TResult>(TenantContext tenant, Action<TenantContext> callback)
    {
        await Task.Run(() =>
        {
            _nonTypedTenantContextAccessor.CurrentTenant = tenant;
            callback(tenant);
        });
    }
    public async Task Perform<TResult>(TenantContext tenant, Func<TenantContext, Task> callback)
    {
        await Task.Run(async () =>
        {
            _nonTypedTenantContextAccessor.CurrentTenant = tenant;
            await callback(tenant);
        });
    }
}
