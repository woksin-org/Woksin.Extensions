﻿// Copyright (c) woksin-org. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Woksin.Extensions.IoC.Registry.Types;

namespace Woksin.Extensions.IoC.Registry.Attributes;

/// <summary>
/// Represents a static class for creating <see cref="ICanAddServices{T}"/>.
/// </summary>
static class ServicesForTypesWith
{
    /// <summary>
    /// Creates an instance of <see cref="ICanAddServices{T}"/> by reflection for the given <see cref="Type"/> that can addservices for types with an attribute.
    /// </summary>
    /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
    /// <param name="adderType">The <see cref="Type"/> of the tenant services adder.</param>
    /// <param name="discoveredClasses">The discovered classes implementing </param>
    /// <returns>The <see cref="ICanAddServices{T}"/> that can add the tenant services .</returns>
    /// <exception cref="CouldNotCreateInstanceOfType">Thrown when failing to create an instance of the needed types.</exception>
    public static ICanAddServices<TContainerBuilder> CreateBuilderFor<TContainerBuilder>(Type adderType, IEnumerable<Type> discoveredClasses)
	    where TContainerBuilder : notnull
    {
        var attributeType = adderType.GetImplementedGenericInterfaceFirstGenericArgumentType(typeof(ICanAddServicesForTypesWith<,>));
        var builderType = typeof(ServicesBuilderForTypesWith<,>).MakeGenericType(attributeType, typeof(TContainerBuilder));
        var adder = Activator.CreateInstance(adderType);
        if (adder == default)
        {
            throw new CouldNotCreateInstanceOfType(adderType);
        }

        var builder = Activator.CreateInstance(builderType, adder, discoveredClasses);
        if (builder is not ICanAddServices<TContainerBuilder> typedBuilder)
        {
            throw new CouldNotCreateInstanceOfType(builderType);
        }

        return typedBuilder;
    }
}
