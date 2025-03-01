﻿using System.Collections.Generic;
using Havit.Blazor.Components.Web.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Havit.Blazor.Components.Web.Bootstrap
{
	/// <summary>
	/// <see href="https://getbootstrap.com/docs/5.1/components/list-group/#links-and-buttons">Bootstrap 5 List group link item</see> component.
	/// </summary>
	public partial class HxListGroupItemNavLink : ICascadeEnabledComponent
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
		/// Indicates the current active selection.
		/// </summary>
		[Parameter] public bool Active { get; set; }

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

		protected string GetCssClasses()
		{
			return CssClassHelper.Combine(
				"list-group-item list-group-item-action",
				CascadeEnabledComponent.EnabledEffective(this) ? null : "disabled",
				this.Active ? "active" : null,
				this.CssClass);
		}
	}
}
