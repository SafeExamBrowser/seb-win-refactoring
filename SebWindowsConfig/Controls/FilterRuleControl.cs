using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SebWindowsConfig.Entities;
using SebWindowsConfig.Utilities;

namespace SebWindowsConfig.Controls
{
	public partial class FilterRuleControl : UserControl
	{
		public FilterRuleControl()
		{
			InitializeComponent();
		}

		internal delegate void DataChangedHandler(IEnumerable<FilterRule> rules);

		internal event DataChangedHandler DataChanged;

		internal void AddRule(FilterRule rule)
		{
			RuleDataGridView.Rows.Add(rule.IsActive, rule.IsRegex, rule.Expression, rule.Action.ToString());
		}

		internal IEnumerable<FilterRule> GetRules()
		{
			foreach (DataGridViewRow row in RuleDataGridView.Rows)
			{
				var isValid = true;

				isValid &= row.Cells[ActiveColumn.Index].Value != null;
				isValid &= row.Cells[RegexColumn.Index].Value != null;
				isValid &= row.Cells[ActionColumn.Index].Value != null;

				if (row.Cells[RegexColumn.Index].Value as bool? == true)
				{
					isValid &= IsValidRegexRule(row.Cells[ExpressionColumn.Index].Value as string);
				}
				else
				{
					isValid &= IsValidUrlRule(row.Cells[ExpressionColumn.Index].Value as string);
				}

				if (isValid)
				{
					yield return new FilterRule
					{
						IsActive = (bool) row.Cells[ActiveColumn.Index].Value,
						IsRegex = (bool) row.Cells[RegexColumn.Index].Value,
						Expression = row.Cells[ExpressionColumn.Index].Value as string,
						Action = (FilterAction) Enum.Parse(typeof(FilterAction), row.Cells[ActionColumn.Index].Value as string)
					};
				}
			}
		}

		private void AddButton_Click(object sender, EventArgs e)
		{
			var isActive = true;
			var isRegex = false;

			RuleDataGridView.Rows.Add(isActive, isRegex);
		}

		private void RemoveButton_Click(object sender, EventArgs e)
		{
			if (RuleDataGridView.SelectedRows.Count != 0)
			{
				foreach (DataGridViewRow row in RuleDataGridView.SelectedRows)
				{
					RuleDataGridView.Rows.Remove(row);
				}
			} else
			{
				if (RuleDataGridView.CurrentRow != null)
				{
					RuleDataGridView.Rows.RemoveAt(RuleDataGridView.CurrentRow.Index);
				}

			}
		}

		private void RuleDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex == ExpressionColumn.Index)
			{
				var row = RuleDataGridView.Rows[e.RowIndex];
				var expression = row.Cells[ExpressionColumn.Index].Value as string;

				if (expression == null)
				{
					RuleDataGridView.Rows.Remove(row);
				}
			}

			DataChanged?.Invoke(GetRules());
		}

		private void RuleDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			DataChanged?.Invoke(GetRules());
		}

		private void RuleDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			DataChanged?.Invoke(GetRules());
		}

		private void RuleDataGridView_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				var row = RuleDataGridView.Rows[e.RowIndex];
				var expression = (row.Cells[ExpressionColumn.Index].Value as string)?.Trim();
				var isRegex = row.Cells[RegexColumn.Index].Value as bool? == true;
				var isValidExpression = isRegex ? IsValidRegexRule(expression) : IsValidUrlRule(expression);
				var isValidAction = !String.IsNullOrWhiteSpace(row.Cells[ActionColumn.Index].Value as string);

				row.Cells[ActionColumn.Index].ErrorText = isValidAction ? null : "Please choose an action!";
				row.Cells[ExpressionColumn.Index].ErrorText = isValidExpression ? null : (isRegex ? "Invalid regular expression!" : "Invalid URL rule!");
				row.Cells[ExpressionColumn.Index].Value = expression;
			}
		}

		private void RuleDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			// The row should be updated as soon as the checkbox is changed, not just when the cell looses focus...
			if (e.ColumnIndex == ActiveColumn.Index || e.ColumnIndex == RegexColumn.Index)
			{
				RuleDataGridView.EndEdit();
			}
		}

		private void RuleDataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// If the current cell has changes, they need to be commited immediately. Otherwise, changes can be lost if the user
			// chooses to save the settings via menu or key command since in that case the CellValueChanged event won't fire.
			// See: https://stackoverflow.com/questions/963601/datagridview-value-does-not-gets-saved-if-selection-is-not-lost-from-a-cell
			if (RuleDataGridView.IsCurrentCellDirty)
			{
				RuleDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private bool IsValidRegexRule(string regex)
		{
			try
			{
				new Regex(regex);
			}
			catch
			{
				return false;
			}

			return true;
		}

		private bool IsValidUrlRule(string rule)
		{
			return new SEBURLFilterExpression(rule) != null;
		}
	}
}
