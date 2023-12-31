// Copyright (c) woksin-org. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Woksin.Extensions.IoC.Tenancy;

/// <summary>
/// Type extensions.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Checks whether the <see cref="Type"/> is a class decorated with <see cref="PerTenantAttribute"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>True if per tenant type, false if not.</returns>
    public static bool IsPerTenantDecoratedClass(this Type type) => Attribute.IsDefined(type, typeof(PerTenantAttribute));
}
