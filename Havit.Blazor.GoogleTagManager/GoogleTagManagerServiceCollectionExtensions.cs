﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Havit.Blazor.GoogleTagManager
{
	public static class GoogleTagManagerServiceCollectionExtensions
	{
		/// <summary>
		/// Adds Google Tag Mananger (GTM) support. Use <see cref="IHxGoogleTagManager"/> to push data to <c>dataLayer</c> and/or <see cref="HxGoogleTagManagerPageViewTracker"/> to track page-views.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configureOptions"></param>
		/// <returns></returns>
		public static IServiceCollection AddHxGoogleTagManager(this IServiceCollection services, Action<HxGoogleTagManagerOptions> configureOptions)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("browser")))
			{
				services.AddSingleton<IHxGoogleTagManager, HxGoogleTagManager>();
			}
			else
			{
				services.AddScoped<IHxGoogleTagManager, HxGoogleTagManager>();
			}

			if (configureOptions != null)
			{
				services.Configure(configureOptions);
			}

			return services;
		}

	}
}
