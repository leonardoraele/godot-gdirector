using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ValidatePropertyExtension
{
	extension(Godot.Collections.Dictionary property)
	{
		public void DefineProperty(
			PropertyHint? hint = null,
			string? hintString = null,
			IEnumerable<PropertyUsageFlags>? usage = null
		)
		{
			if (hint.HasValue)
			{
				property["hint"] = (long) hint.Value;
			}
			if (!string.IsNullOrWhiteSpace(hintString))
			{
				property["hint_string"] = hintString;
			}
			long? flags = usage?.Select(flag => (long) flag).Aggregate((a, b) => a | b);
			if (flags.HasValue)
			{
				property["usage"] = flags.Value;
			}
		}
	}
}
