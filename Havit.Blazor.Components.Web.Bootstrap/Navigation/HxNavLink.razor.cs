﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Havit.Blazor.Components.Web.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Havit.Blazor.Components.Web.Bootstrap
{
	/// <summary>
	/// <a href="https://getbootstrap.com/docs/5.0/components/navs-tabs/">Bootstrap nav-link</a> component.
	/// </summary>
	public partial class HxNavLink : ICascadeEnabledComponent
	{
		[CascadingParameter] protected FormState FormState { get; set; }
		FormState ICascadeEnabledComponent.FormState { get => this.FormState; set => this.FormState = value; }

		/// <summary>
		/// Navigation target.
		/// </summary>
		[Parameter] public string Href { get; set; }

		/// <summary>
		/// Text of the item.
		/// </summary>
		[Parameter] public string Text { get; set; }

		/// <summary>
		/// URL matching behavior for the underlying <see cref="NavLink"/>.
		/// Default is <see cref="NavLinkMatch.Prefix"/>.
		/// You can set the value to <c>null</c> to disable the matching.
		/// </summary>
		[Parameter] public NavLinkMatch? Match { get; set; } = NavLinkMatch.Prefix;

		/// <summary>
		/// Raised when the item is clicked.
		/// </summary>
		[Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

		/// <summary>
		/// Additional CSS class.
		/// </summary>
		[Parameter] public string CssClass { get; set; }

		/// <inheritdoc cref="ICascadeEnabledComponent.Enabled" />
		[Parameter] public bool? Enabled { get; set; }

		/// <summary>
		/// Content.
		/// </summary>
		[Parameter] public RenderFragment ChildContent { get; set; }

		/// <summary>
		/// Additional attributes to be splatted onto an underlying <see cref="NavLink"/> component.
		/// </summary>
		[Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; }
	}
}
