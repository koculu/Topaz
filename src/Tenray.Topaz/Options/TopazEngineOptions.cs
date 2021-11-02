using Esprima;

namespace Tenray.Topaz.Options
{
    public class TopazEngineOptions
    {
        public SecurityPolicy SecurityPolicy { get; set; } = SecurityPolicy.Default;

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

        /// <summary>
        /// C# is a type-safe language but Javascript is not.
        /// Topaz encapsulates the differences by using auto type conversions.
        /// If you want explicit behavior for literal number evaluation,
        /// you can use this option.
        /// 
        /// if true, literal numbers that are written in script are converted to double,
        /// if false the type is deducted by string itself.
        /// 
        /// Deducted type can be int, long or double.
        /// For other types use type conversion functions in your script.
        /// For example: 3 => int, 2147483648 => long, 3.2 => double
        /// 
        /// Default option value is false.
        /// </summary>
        public bool LiteralNumbersAreConvertedToDouble { get; set; } = false;

        /// <summary>
        /// If true, arithmetic operations will change numeric value types to double to avoid overflows.
        /// If false, the types stay same only if no overflow happens.
        /// If overflow happens, types are converted in the following order:
        /// int -> long -> double
        /// That operation is slow because of thrown overflow exception.
        /// Especially, if so many overflow exceptions occurs in a loop,
        /// execution duration increases dramatically.
        /// 
        /// Option Default value is true.
        /// 
        /// If the option is set to false,
        /// binary/unary operations do automatic type conversion in case of overflow using
        /// checked statements which is slow when it overflows.
        /// Hence, it is a good decision to convert everything
        /// to double to avoid unexpected slow overflow exceptions
        /// in binary/unary operations unless you need exact integer arithmetics
        /// for non-floating values.
        /// 
        /// If you don't want to automatically change value types in arithmetic operations,
        /// disable this option and keep in mind that 
        /// your script should care about slowness 
        /// if there are too many overflows.
        /// 
        /// On the other hand, operations on int, long are faster than double.
        /// If you want to explicitly handle number types in the script runtime
        /// you may choose false.
        /// 
        /// Both options have pros and cons, choose wisely.
        /// </summary>
        public bool NumbersAreConvertedToDoubleInArithmeticOperations { get; set; } = true;

        /// <summary>
        /// If true and TopazEngine is thread safe,
        /// If false, javascript objects are not thread safe.
        /// 
        /// Default value is based on Engine's thread safety.
        /// </summary>
        public bool UseThreadSafeJsObjects { get; set; }
    }
}
