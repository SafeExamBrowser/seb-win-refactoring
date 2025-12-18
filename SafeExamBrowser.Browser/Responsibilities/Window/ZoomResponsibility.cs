/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Browser.Handlers;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class ZoomResponsibility : WindowResponsibility
	{
		private const double ZOOM_FACTOR = 0.2;

		private readonly KeyboardHandler keyboardHandler;

		private double zoomLevel;

		public ZoomResponsibility(BrowserWindowContext context, KeyboardHandler keyboardHandler) : base(context)
		{
			this.keyboardHandler = keyboardHandler;
		}

		public override void Assume(WindowTask task)
		{
			switch (task)
			{
				case WindowTask.InitializeZoom:
					InitializeZoom();
					break;
				case WindowTask.RegisterEvents:
					RegisterEvents();
					break;
			}
		}

		private void InitializeZoom()
		{
			Window.UpdateZoomLevel(CalculateZoomPercentage());
		}

		private void RegisterEvents()
		{
			keyboardHandler.ZoomInRequested += ZoomInRequested;
			keyboardHandler.ZoomOutRequested += ZoomOutRequested;
			keyboardHandler.ZoomResetRequested += ZoomResetRequested;

			Window.ZoomInRequested += ZoomInRequested;
			Window.ZoomOutRequested += ZoomOutRequested;
			Window.ZoomResetRequested += ZoomResetRequested;
		}

		private void ZoomInRequested()
		{
			if (Settings.AllowPageZoom && CalculateZoomPercentage() < 300)
			{
				zoomLevel += ZOOM_FACTOR;
				Control.Zoom(zoomLevel);
				Window.UpdateZoomLevel(CalculateZoomPercentage());
				Logger.Debug($"Increased page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private void ZoomOutRequested()
		{
			if (Settings.AllowPageZoom && CalculateZoomPercentage() > 25)
			{
				zoomLevel -= ZOOM_FACTOR;
				Control.Zoom(zoomLevel);
				Window.UpdateZoomLevel(CalculateZoomPercentage());
				Logger.Debug($"Decreased page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private void ZoomResetRequested()
		{
			if (Settings.AllowPageZoom)
			{
				zoomLevel = 0;
				Control.Zoom(0);
				Window.UpdateZoomLevel(CalculateZoomPercentage());
				Logger.Debug($"Reset page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private double CalculateZoomPercentage()
		{
			return (zoomLevel * 25.0) + 100.0;
		}
	}
}
