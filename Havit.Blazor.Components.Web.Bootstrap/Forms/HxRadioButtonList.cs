﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havit.Blazor.Components.Web.Bootstrap
{
	/// <summary>
	/// Select. Consider creating custom picker derived from <see cref="HxRadioButtonListBase{TValueType, TItem}"/>.
	/// </summary>
	/// <typeparam name="TValue">Type of value.</typeparam>
	/// <typeparam name="TItem">Type of items.</typeparam>
	public class HxRadioButtonList<TValue, TItem> : HxRadioButtonListBase<TValue, TItem>
	{
		/// <summary>
		/// Selects value from item.
		/// Not required when TValueType is same as TItemTime.
		/// </summary>
		[Parameter]
		public Func<TItem, TValue> ValueSelector
		{
			get => ValueSelectorImpl;
			set => ValueSelectorImpl = value;
		}

		/// <summary>
		/// Items to display. 
		/// </summary>
		[Parameter]
		public IEnumerable<TItem> Data
		{
			get => DataImpl;
			set => DataImpl = value;
		}

		/// <summary>
		/// Selects text to display from item. Also used for chip text.
		/// When not set <c>ToString()</c> is used.
		/// </summary>
		[Parameter]
		public Func<TItem, string> TextSelector
		{
			get => TextSelectorImpl;
			set => TextSelectorImpl = value;
		}

		/// <summary>
		/// Gets html to display from item.
		/// When not set <see cref="TextSelector"/> is used.
		/// </summary>
		[Parameter]
		public RenderFragment<TItem> ItemTemplate
		{
			get => ItemTemplateImpl;
			set => ItemTemplateImpl = value;
		}

		/// <summary>
		/// Selects value to sort items. Uses <see cref="TextSelector"/> property when not set.
		/// When complex sorting required, sort data manually and don't let sort them by this component. Alternatively create a custom comparable property.
		/// </summary>
		[Parameter]
		public Func<TItem, IComparable> SortKeySelector
		{
			get => SortKeySelectorImpl;
			set => SortKeySelectorImpl = value;
		}

		/// <summary>
		/// When <c>true</c>, items are sorted before displaying in select.
		/// Default value is <c>true</c>.
		/// </summary>
		[Parameter]
		public bool AutoSort
		{
			get => AutoSortImpl;
			set => AutoSortImpl = value;
		}
	}
}
