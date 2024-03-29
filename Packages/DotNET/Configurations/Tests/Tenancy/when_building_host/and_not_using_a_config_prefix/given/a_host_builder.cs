﻿// Copyright (c) woksin-org. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Woksin.Extensions.Configurations.Tenancy.when_building_host.and_not_using_a_config_prefix.given;

public class a_host_builder : when_building_host.given.a_host_builder
{
	void Establish()
	{
		host_builder
			.UseTenantConfigurationExtension(typeof(a_host_builder).Assembly, _ => _.ConfigureTenancy(configureTenancyOptions: _ => _.StaticTenantId = "the_tenant"));
	}
}
