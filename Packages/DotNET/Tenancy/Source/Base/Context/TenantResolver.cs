// Copyright (c) woksin-org. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Woksin.Extensions.Tenancy.Strategies;

namespace Woksin.Extensions.Tenancy.Context;

/// <summary>
/// Represents an implementation of <see cref="IResolveTenant"/> and <see cref="IResolveTenant{TTenant}"/> that resolves tenant context using the configured strategies.
/// </summary>
/// <param name="strategies">The <see cref="ITenantResolutionStrategy"/> strategies to resolve tenant identifier.</param>
/// <param name="options">The <see cref="IOptionsMonitor{TOptions}"/> for the configured <see cref="TenancyOptions{TTenant}"/>.</param>
/// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
/// <typeparam name="TTenant">The <see cref="Type"/> of the <see cref="ITenantInfo"/>.</typeparam>
public partial class TenantResolver<TTenant>(IEnumerable<ITenantResolutionStrategy> strategies, IOptionsMonitor<TenancyOptions<TTenant>> options, ILoggerFactory loggerFactory) : IResolveTenant<TTenant>, IResolveTenant
    where TTenant : class, ITenantInfo, new()
{
    /// <inheritdoc />
    public IEnumerable<ITenantResolutionStrategy> Strategies { get; } = strategies;

    readonly ILogger<TenantResolver<TTenant>> _logger = loggerFactory?.CreateLogger<TenantResolver<TTenant>>() ?? NullLogger<TenantResolver<TTenant>>.Instance;

    /// <inheritdoc />
    public async Task<ITenantContext<TTenant>> Resolve(object context)
    {
        var config = options.CurrentValue;
        if (config.IsUsingStaticTenant(out var staticTenantId))
        { 
            LogUsingStaticTenantId(_logger, staticTenantId!);
            return TenantContext<TTenant>.Static(new TTenant
            {
                Id = staticTenantId
            });
        }
        foreach (var strategy in Strategies)
        {        
            var wrappedStrategy = new SafeStrategyWrapper(strategy, loggerFactory?.CreateLogger(strategy.GetType()) ?? NullLogger.Instance);
            if (!wrappedStrategy.CanResolveFromContext(context, out var cannotResolveReason))
            {
                LogCannotNotResolveFromContext(_logger, strategy.GetType(), cannotResolveReason);
                continue;
            }
            LogTryingToResolve(_logger, strategy.GetType());
            var (resolvedIdentifier, identifier) = await TryResolveIdentifier(config, wrappedStrategy, context);
            if (resolvedIdentifier && TryGetTenantContext(config, identifier!, strategy, out var tenantContext))
            {
                return tenantContext;
            }
        }

        LogCouldNotResolveTenant(_logger);
        if (!Strategies.Any())
        {
            LogNoStrategiesConfigured(_logger);
        }
        return TenantContext<TTenant>.Unresolved();
    }

    async Task<(bool, string?)> TryResolveIdentifier(TenancyOptions<TTenant> config, SafeStrategyWrapper strategy, object context)
    {
        var identifier = await strategy.Resolve(context);
        if (!config.Ignored.Contains(identifier, StringComparer.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(identifier)
                ? (false, null)
                : (true, identifier);
        }
        LogIgnoreTenant(_logger, identifier!);
        return (false, null);
    }

    bool TryGetTenantContext(TenancyOptions<TTenant> config, string identifier, ITenantResolutionStrategy strategy, out ITenantContext<TTenant> tenantContext)
    {
        var (isResolved, isResolvedFromConfig) = config.TryGetTenantContext(identifier, strategy, out tenantContext);
        if (!isResolved)
        {
            LogTenantNotConfigured(_logger, identifier);
            return false;
        }
        
        if (isResolvedFromConfig)
        {
            tenantContext.Resolved(out var tenantInfo, out _);
            LogUsingConfiguredTenant(_logger, identifier, tenantInfo!.Name);
        }
        else
        {
            LogUsingNonConfiguredTenant(_logger, identifier);
        }
        return true;
    }

    async Task<ITenantContext> IResolveTenant.Resolve(object context)
        => await Resolve(context) as ITenantContext ?? throw new InvalidCastException("Could not cast tenant context");

    [LoggerMessage(0, LogLevel.Debug, "Resolved tenant identifier {Identifier} is configured to be ignored")]
    static partial void LogIgnoreTenant(ILogger logger, string identifier);

    [LoggerMessage(1, LogLevel.Warning, "Resolved tenant identifier {Identifier} is not configured and should not be used")]
    static partial void LogTenantNotConfigured(ILogger logger, string identifier);

    [LoggerMessage(2, LogLevel.Debug, "Resolved tenant identifier {Identifier} is not configured but will be used")]
    static partial void LogUsingNonConfiguredTenant(ILogger logger, string identifier);

    [LoggerMessage(3, LogLevel.Debug, "Resolved tenant identifier {Identifier} with name {TenantName} is configured")]
    static partial void LogUsingConfiguredTenant(ILogger logger, string identifier, string? tenantName);

    [LoggerMessage(4, LogLevel.Debug, "Could not resolve tenant from any strategy")]
    static partial void LogCouldNotResolveTenant(ILogger logger);
    
    [LoggerMessage(5, LogLevel.Debug, "Could not resolve tenant from context using strategy {StrategyType}. {Reason}")]
    static partial void LogCannotNotResolveFromContext(ILogger logger, Type strategyType, string reason);
    
    [LoggerMessage(6, LogLevel.Debug, "Trying to resolve tenant from context using strategy {StrategyType}")]
    static partial void LogTryingToResolve(ILogger logger, Type strategyType);
    
    [LoggerMessage(7, LogLevel.Warning, "No tenant resolution strategy is configured")]
    static partial void LogNoStrategiesConfigured(ILogger logger);
    
    [LoggerMessage(8, LogLevel.Debug, "Tenant resolution is skipped because global static tenant id {StaticTenantId}")]
    static partial void LogUsingStaticTenantId(ILogger logger, string staticTenantId);
}
