namespace Common.Utility.LinqConditionalOperators
{
    using System.Linq.Expressions;

    /// <summary>
    /// Extension class that supports extensions for And, Or, etc linq operators
    /// </summary>
    public static partial class ExpressionExt
    {

        /// <summary>
        /// Logical and operator for LINQ query
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<TDelegate> And<TDelegate>(this Expression<TDelegate> left, Expression<TDelegate> right)
        {
            if (left == null) return right;
            return Expression.Lambda<TDelegate>(Expression.And(left.Body, new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body)), left.Parameters);
        }

        /// <summary>
        /// Logical or operator for LINQ query
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<TDelegate> Or<TDelegate>(this Expression<TDelegate> left, Expression<TDelegate> right)
        {
            if (left == null) return right;
            return Expression.Lambda<TDelegate>(Expression.OrElse(left.Body, new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body)), left.Parameters);
        }
    }
}
