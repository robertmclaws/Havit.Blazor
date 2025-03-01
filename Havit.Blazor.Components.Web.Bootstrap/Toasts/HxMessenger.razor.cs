﻿using System;
using System.Collections.Generic;
using System.Text;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Havit.Blazor.Components.Web.Bootstrap
{
	/// <summary>
	/// <see cref="HxToastContainer"/> wrapper for displaying <see cref="HxToast"/> messages dispatched through <see cref="IHxMessengerService"/>.
	/// </summary>
	public partial class HxMessenger : ComponentBase, IDisposable
	{
		/// <summary>
		/// Position of the messages. Default is <see cref="HxToastContainerPosition.None"/>.
		/// </summary>
		[Parameter] public HxToastContainerPosition Position { get; set; } = HxToastContainerPosition.None;

		/// <summary>
		/// Additional CSS class.
		/// </summary>
		[Parameter] public string CssClass { get; set; }

		[Inject] protected IHxMessengerService Messenger { get; set; }
		[Inject] protected NavigationManager NavigationManager { get; set; }

		private List<MessengerMessage> messages = new List<MessengerMessage>();

		protected override void OnInitialized()
		{
			base.OnInitialized();

			Messenger.OnMessage += HandleMessage;
			Messenger.OnClear += HandleClear;
		}

		private void HandleMessage(MessengerMessage message)
		{
			InvokeAsync(() =>
			{
				messages.Add(message);

				StateHasChanged();
			});
		}

		private void HandleClear()
		{
			InvokeAsync(() =>
			{
				messages.Clear();

				StateHasChanged();
			});
		}

		/// <summary>
		/// Receive notification from <see cref="HxToast"/> when message is hidden.
		/// </summary>
		private void HandleToastHidden(MessengerMessage message)
		{
			messages.Remove(message);
		}

		public void Dispose()
		{
			Messenger.OnMessage -= HandleMessage;
			Messenger.OnClear -= HandleClear;
		}
	}
}
