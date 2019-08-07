using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ionic.Zip;

namespace SebWindowsConfig.Utilities
{
	internal class LogCollector
	{
		private readonly IWin32Window owner;

		internal LogCollector(IWin32Window owner)
		{
			this.owner = owner;
		}

		internal void Run()
		{
			var description = "Please select the location where you would like to save the collected log files:";
			var password = default(string);
			var path = default(string);

			using (var dialog = new FolderBrowserDialog { Description = description })
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					path = dialog.SelectedPath;
				}
			}

			if (path != default(string))
			{
				var encrypt = AskUserForDataEncrpytion();

				if (!encrypt || (encrypt && TryAskForPassword(out password)))
				{
					CollectLogFiles(path, password);
				}
			}
		}

		private bool AskUserForDataEncrpytion()
		{
			var message = "Log files can contain sensitive information about you and your computer. Would you like to protect the data with a password?";
			var result = MessageBox.Show(owner, message, "Data Protection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			return result == DialogResult.Yes;
		}

		private bool TryAskForPassword(out string password)
		{
			password = default(string);

			using (var dialog = new SebPasswordDialogForm())
			{
				dialog.Text = "Data Protection";
				dialog.LabelText = "Please enter the password to be used to encrypt the data:";

				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					password = dialog.txtSEBPassword.Text;
				}
			}

			return password != default(string);
		}

		private void CollectLogFiles(string outputPath, string password = null)
		{
			try
			{
				var zipPath = Path.Combine(outputPath, $"SEB_Logs_{DateTime.Today.ToString("yyyy-MM-dd")}.zip");
				var logFiles = new[]
				{
					SEBClientInfo.SebClientLogFile,
					Path.Combine(SEBClientInfo.SebClientSettingsAppDataDirectory, SebWindowsConfigForm.SEB_CONFIG_LOG),
					Path.Combine(SEBClientInfo.SebClientSettingsAppDataDirectory, "seb.log"),
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"SafeExamBrowser\SebWindowsServiceWCF\sebwindowsservice.log")
				};
				var existingFiles = logFiles.Where(f => File.Exists(f));
				var missingFiles = logFiles.Except(existingFiles);

				using (var stream = new FileStream(zipPath, FileMode.Create))
				using (var zip = new ZipFile())
				{
					if (password != default(string))
					{
						zip.Password = password;
					}

					foreach (var file in existingFiles)
					{
						zip.AddFile(file, string.Empty);
					}

					zip.Save(stream);
				}

				if (missingFiles.Any())
				{
					var count = $"{existingFiles.Count()} of {logFiles.Count()}";
					var missing = $"The following file(s) could not be found:\n\n{String.Join("\n\n", missingFiles)}";

					MessageBox.Show(owner, $"{count} log files were collected and saved as '{zipPath}'.\n\n{missing}", "Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					MessageBox.Show(owner, $"The log files were successfully collected and saved as '{zipPath}'.", "Sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(owner, $"Failed to collect the log files. Reason: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
