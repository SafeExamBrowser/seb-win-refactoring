/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Text;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Reporting
{
	public class ReportGenerator : IReportGenerator
	{
		public string GenerateCsv(PreExamCheckReport report)
		{
			if (report == null) throw new ArgumentNullException(nameof(report));

			var sb = new StringBuilder();

			// Header metadata
			sb.AppendLine($"# Safe Exam Browser - Pre-Exam Check Report");
			sb.AppendLine($"# Timestamp: {report.Timestamp:yyyy-MM-dd HH:mm:ss}");
			sb.AppendLine($"# Machine Name: {report.MachineName}");
			sb.AppendLine($"# Overall Result: {(report.PassedAllCritical ? "PASSED" : "FAILED")}");
			sb.AppendLine();

			// Table columns
			sb.AppendLine("Category,Title,Status,Actual Value,Required Value,Is Critical,Message");

			foreach (var item in report.Results)
			{
				string category = EscapeCsv(item.Category);
				string title = EscapeCsv(item.Title);
				string status = item.Status.ToString();
				string actual = EscapeCsv(item.ActualValue);
				string required = EscapeCsv(item.RequiredValue);
				string isCritical = item.IsCritical ? "Yes" : "No";
				string message = EscapeCsv(item.Message);

				sb.AppendLine($"{category},{title},{status},{actual},{required},{isCritical},{message}");
			}

			return sb.ToString();
		}

		public void SaveReportToFile(PreExamCheckReport report, string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path cannot be empty", nameof(filePath));

			string csvContent = GenerateCsv(report);
			string directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(filePath, csvContent, Encoding.UTF8);
		}

		private string EscapeCsv(string field)
		{
			if (string.IsNullOrEmpty(field)) return "\"\"";
			if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
			{
				return $"\"{field.Replace("\"", "\"\"")}\"";
			}
			return field;
		}
	}
}
