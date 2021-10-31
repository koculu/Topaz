using Esprima;

namespace Tenray.Topaz.Options
{
    public class TopazEngineOptions
    {
        public SecurityPolicy SecurityPolicy { get; set; } = SecurityPolicy.DisableReflection;

        public ParserOptions ParserOptions { get; set; } = new()
        {
            AdaptRegexp = false,
            Comment = false,
            Tokens = false,
            Tolerant = true,
            TopLevelAsync = true
        };

        public VarScopeBehavior VarScopeBehavior { get; set; }

        public AssignmentWithoutDefinitionBehavior
            AssignmentWithoutDefinitionBehavior
        { get; set; }

        /// <summary>
        /// If set to true, undefined variables or methods will be null.
        /// </summary>
        public bool NoUndefined { get; set; }

        public bool AllowNullReferenceMemberAccess { get; set; }

        public bool AllowUndefinedReferenceMemberAccess { get; set; }

        public bool AllowUndefinedReferenceAccess { get; set; } = true;
    }
}
