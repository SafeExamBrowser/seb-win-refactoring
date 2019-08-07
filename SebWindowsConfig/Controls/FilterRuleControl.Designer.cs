namespace SebWindowsConfig.Controls
{
	partial class FilterRuleControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SplitContainer = new System.Windows.Forms.SplitContainer();
			this.RuleDataGridView = new System.Windows.Forms.DataGridView();
			this.ActiveColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.RegexColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ExpressionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ActionColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.RemoveButton = new System.Windows.Forms.Button();
			this.AddButton = new System.Windows.Forms.Button();
			this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RuleDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SplitContainer.IsSplitterFixed = true;
			this.SplitContainer.Location = new System.Drawing.Point(0, 0);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.RuleDataGridView);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.RemoveButton);
			this.SplitContainer.Panel2.Controls.Add(this.AddButton);
			this.SplitContainer.Panel2MinSize = 50;
			this.SplitContainer.Size = new System.Drawing.Size(940, 671);
			this.SplitContainer.SplitterDistance = 617;
			this.SplitContainer.TabIndex = 0;
			// 
			// RuleDataGridView
			// 
			this.RuleDataGridView.AllowUserToAddRows = false;
			this.RuleDataGridView.AllowUserToResizeRows = false;
			this.RuleDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.RuleDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ActiveColumn,
            this.RegexColumn,
            this.ExpressionColumn,
            this.ActionColumn});
			this.RuleDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RuleDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.RuleDataGridView.Location = new System.Drawing.Point(0, 0);
			this.RuleDataGridView.Name = "RuleDataGridView";
			this.RuleDataGridView.Size = new System.Drawing.Size(940, 617);
			this.RuleDataGridView.TabIndex = 0;
			this.RuleDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.RuleDataGridView_CellContentClick);
			this.RuleDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.RuleDataGridView_CellValueChanged);
			this.RuleDataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.RuleDataGridView_CurrentCellDirtyStateChanged);
			this.RuleDataGridView.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.RuleDataGridView_RowsAdded);
			this.RuleDataGridView.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.RuleDataGridView_RowsRemoved);
			this.RuleDataGridView.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.RuleDataGridView_RowValidating);
			// 
			// ActiveColumn
			// 
			this.ActiveColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.ActiveColumn.HeaderText = "Active";
			this.ActiveColumn.Name = "ActiveColumn";
			this.ActiveColumn.Width = 58;
			// 
			// RegexColumn
			// 
			this.RegexColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.RegexColumn.HeaderText = "Regex";
			this.RegexColumn.Name = "RegexColumn";
			this.RegexColumn.Width = 61;
			// 
			// ExpressionColumn
			// 
			this.ExpressionColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ExpressionColumn.FillWeight = 80F;
			this.ExpressionColumn.HeaderText = "Expression";
			this.ExpressionColumn.Name = "ExpressionColumn";
			// 
			// ActionColumn
			// 
			this.ActionColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ActionColumn.FillWeight = 20F;
			this.ActionColumn.HeaderText = "Action";
			this.ActionColumn.Items.AddRange(new object[] {
            "Allow",
            "Block"});
			this.ActionColumn.Name = "ActionColumn";
			// 
			// RemoveButton
			// 
			this.RemoveButton.Location = new System.Drawing.Point(57, 3);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = new System.Drawing.Size(48, 45);
			this.RemoveButton.TabIndex = 1;
			this.RemoveButton.Text = "-";
			this.ToolTip.SetToolTip(this.RemoveButton, "Removes all selected rules");
			this.RemoveButton.UseVisualStyleBackColor = true;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// AddButton
			// 
			this.AddButton.Location = new System.Drawing.Point(3, 3);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = new System.Drawing.Size(48, 44);
			this.AddButton.TabIndex = 0;
			this.AddButton.Text = "+";
			this.ToolTip.SetToolTip(this.AddButton, "Adds a new rule");
			this.AddButton.UseVisualStyleBackColor = true;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// FilterRuleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SplitContainer);
			this.Name = "FilterRuleControl";
			this.Size = new System.Drawing.Size(940, 671);
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RuleDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer SplitContainer;
		private System.Windows.Forms.DataGridView RuleDataGridView;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ActiveColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn RegexColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn ExpressionColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn ActionColumn;
		private System.Windows.Forms.Button RemoveButton;
		private System.Windows.Forms.ToolTip ToolTip;
		private System.Windows.Forms.Button AddButton;
	}
}
