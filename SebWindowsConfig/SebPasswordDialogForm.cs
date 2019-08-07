using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SebWindowsConfig.Utilities;

namespace SebWindowsConfig
{
	public partial class SebPasswordDialogForm : Form
	{
		[DllImportAttribute("User32.dll")]
		public static extern IntPtr SetForegroundWindow(IntPtr hWnd);


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Show SEB Password Dialog Form.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static string ShowPasswordDialogForm(string title, string passwordRequestText)
		{
			using (SebPasswordDialogForm sebPasswordDialogForm = new SebPasswordDialogForm())
			{
				SetForegroundWindow(sebPasswordDialogForm.Handle);
				sebPasswordDialogForm.TopMost = true;
				// Set the title of the dialog window
				sebPasswordDialogForm.Text = title;
				// Set the text of the dialog
				sebPasswordDialogForm.LabelText = passwordRequestText;
				sebPasswordDialogForm.txtSEBPassword.Focus();
				// If we are running in SebWindowsClient we need to activate it before showing the password dialog
				// Don't do this; it will fail when the password dialog is running in a separate thread
				//if (SEBClientInfo.SebWindowsClientForm != null) SEBClientInfo.SebWindowsClientForm.Activate();
				// Show password dialog as a modal dialog and determine if DialogResult = OK.
				if (sebPasswordDialogForm.ShowDialog() == DialogResult.OK)
				{
					// Read the contents of testDialog's TextBox.
					string password = sebPasswordDialogForm.txtSEBPassword.Text;
					sebPasswordDialogForm.txtSEBPassword.Text = "";
					//sebPasswordDialogForm.txtSEBPassword.Focus();
					return password;
				}
				else
				{
					return null;
				}
			}
		}

		public SebPasswordDialogForm()
		{
			InitializeComponent();
			try
			{
				if ((Boolean) SEBClientInfo.getSebSetting(SEBSettings.KeyTouchOptimized)[SEBSettings.KeyTouchOptimized] ==
				true)
			{
				InitializeForTouch();
			}
			else
			{
				InitializeForNonTouch();
			}
			}
			//if used to decrypt the settings, then the settings are not yet initialized...
			catch{}
			
		}

		public void InitializeForTouch()
		{
			this.Font = new Font(FontFamily.GenericSansSerif, 12);
			IntPtr hwnd = this.Handle;
			this.FormBorderStyle = FormBorderStyle.None;
			this.Top = 0;
			this.Left = 0;
			this.Width = Screen.PrimaryScreen.Bounds.Width;
			this.Height = Screen.PrimaryScreen.Bounds.Height;
			this.btnCancel.BackColor = Color.Red;
			this.btnCancel.FlatStyle = FlatStyle.Flat;
			this.btnCancel.Height = 35;
			this.btnCancel.Width = 120;
			this.btnCancel.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.btnCancel.Width / 2) + 100;
			this.btnOk.BackColor = Color.Green;
			this.btnOk.FlatStyle = FlatStyle.Flat;
			this.btnOk.Height = 35;
			this.btnOk.Width = 120;
			this.btnOk.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.btnOk.Width / 2) - 100;
			this.txtSEBPassword.Width = 400;
			this.txtSEBPassword.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.txtSEBPassword.Width / 2);
			this.txtSEBPassword.Height = 30;
		}

		public void InitializeForNonTouch()
		{
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.txtSEBPassword.Text = "";
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			this.Visible = false;
		}

		// Expose the label for changing from outside of the form
		public string LabelText
		{
			get
			{
				return this.lblSEBPassword.Text;
			}
			set
			{
				this.lblSEBPassword.Text = value;
				try
				{
					if ((Boolean)SEBClientInfo.getSebSetting(SEBSettings.KeyTouchOptimized)[SEBSettings.KeyTouchOptimized] == true)
					{
						this.lblSEBPassword.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.lblSEBPassword.Width / 2);
					}
				}
				catch (Exception)
				{
				}
				
			}
		}

		private void txtSEBPassword_Enter(object sender, EventArgs e)
		{
		}

		private void txtSEBPassword_Leave(object sender, EventArgs e)
		{
		}

	}
}
