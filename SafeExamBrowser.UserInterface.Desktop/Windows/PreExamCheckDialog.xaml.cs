/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	public partial class PreExamCheckDialog : Window
	{
		private readonly IPreExamCheckService checkService;
		private readonly IReportGenerator reportGenerator;

		public PreExamCheckReport Report { get; private set; }
		public bool UserProceeded { get; private set; }

		public PreExamCheckDialog(IPreExamCheckService checkService, IReportGenerator reportGenerator = null)
		{
			InitializeComponent();

			this.checkService = checkService;
			this.reportGenerator = reportGenerator;

			RunChecks();
		}

		public void RunChecks()
		{
			Report = checkService.RunAllChecks();
			ResultsDataGrid.ItemsSource = Report.Results;

			bool passed = Report.PassedAllCritical;
			bool warnings = Report.HasWarnings;

			if (passed)
			{
				if (warnings)
				{
					StatusBanner.Background = System.Windows.Media.Brushes.LightYellow;
					StatusSummaryText.Text = "⚠️ Warning: Some criteria are not optimal but are not mandatory. Candidates may continue with the exam.";
				}
				else
				{
					StatusBanner.Background = System.Windows.Media.Brushes.Honeydew;
					StatusSummaryText.Text = "✅ Requirements Met: All configuration criteria meet the exam regulations.";
				}
				StartExamButton.IsEnabled = true;
			}
			else
			{
				StatusBanner.Background = System.Windows.Media.Brushes.MistyRose;
				StatusSummaryText.Text = "❌ Not Met: The computer does not meet the minimum requirements. Please fix the issues or contact the exam proctor.";
				StartExamButton.IsEnabled = false;
			}

			// Automatically export report to AppData if generator provided
			if (reportGenerator != null)
			{
				try
				{
					string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SafeExamBrowser", "Logs");
					string defaultReportPath = Path.Combine(logDir, "PreExamReport.csv");
					reportGenerator.SaveReportToFile(Report, defaultReportPath);
				}
				catch
				{
				}
			}
		}

		private void RetryButton_Click(object sender, RoutedEventArgs e)
		{
			RunChecks();
		}

		private void StartExamButton_Click(object sender, RoutedEventArgs e)
		{
			UserProceeded = true;
			DialogResult = true;
			Close();
		}

		private void AbortButton_Click(object sender, RoutedEventArgs e)
		{
			UserProceeded = false;
			DialogResult = false;
			Close();
		}

		private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
		{
			if (reportGenerator == null)
			{
				MessageBox.Show("CSV export functionality is not available.", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try
			{
				var saveFileDialog = new SaveFileDialog
				{
					Filter = "CSV File (*.csv)|*.csv",
					FileName = $"PreExamReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
					Title = "Save Pre-Exam Configuration Report"
				};

				if (saveFileDialog.ShowDialog() == true)
				{
					reportGenerator.SaveReportToFile(Report, saveFileDialog.FileName);
					MessageBox.Show($"Report exported successfully to file:\n{saveFileDialog.FileName}", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Unable to export CSV report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
