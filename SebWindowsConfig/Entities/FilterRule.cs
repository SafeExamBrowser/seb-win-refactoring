using System.Collections.Generic;

namespace SebWindowsConfig.Entities
{
	class FilterRule
	{
		public bool IsActive { get; set; }
		public bool IsRegex { get; set; }
		public string Expression { get; set; }
		public FilterAction Action { get; set; }

		internal static FilterRule FromConfig(IDictionary<string, object> config)
		{
			return new FilterRule
			{
				IsActive = config[SEBSettings.KeyURLFilterRuleActive] as bool? == true,
				IsRegex = config[SEBSettings.KeyURLFilterRuleRegex] as bool? == true,
				Expression = config[SEBSettings.KeyURLFilterRuleExpression] as string,
				Action = config[SEBSettings.KeyURLFilterRuleAction] as int? == 1 ? FilterAction.Allow : FilterAction.Block
			};
		}

		internal static IDictionary<string, object> ToConfig(FilterRule rule)
		{
			var config = new Dictionary<string, object>();

			config[SEBSettings.KeyURLFilterRuleActive] = rule.IsActive;
			config[SEBSettings.KeyURLFilterRuleRegex] = rule.IsRegex;
			config[SEBSettings.KeyURLFilterRuleExpression] = rule.Expression;
			config[SEBSettings.KeyURLFilterRuleAction] = rule.Action == FilterAction.Allow ? 1 : 0;

			return config;
		}
	}
}
