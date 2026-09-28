using System;

namespace BimCommands.EnvironmentConverter.Models
{
    public enum MappingCategory
    {
        SteelPart,
        ConcretePart,
        Rebar
    }

    /// <summary>
    /// Represents an individual material or rebar grade translation rule.
    /// </summary>
    public class MaterialMappingRule
    {
        public bool IsEnabled { get; set; } = true;
        public MappingCategory Category { get; set; }
        public string SourceValue { get; set; }
        public string TargetValue { get; set; }
        public int CountInModel { get; set; }
        public string Description { get; set; }

        public MaterialMappingRule() { }

        public MaterialMappingRule(MappingCategory category, string source, string target, string description = "")
        {
            Category = category;
            SourceValue = source;
            TargetValue = target;
            Description = description;
            IsEnabled = true;
        }

        public override string ToString()
        {
            return $"[{Category}] {SourceValue} -> {TargetValue} ({CountInModel} objects)";
        }
    }
}
