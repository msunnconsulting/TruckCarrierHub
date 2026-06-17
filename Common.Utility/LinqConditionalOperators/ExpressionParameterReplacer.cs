namespace Common.Utility.LinqConditionalOperators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// This is ExpressionParameterReplacer class which is used for implementing LINQ AND, OR, OrElse etc. methods
    /// </summary>
    internal sealed class ExpressionParameterReplacer : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the ExpressionParameterReplacer class.
        /// </summary>
        /// <param name="fromParameters">Enter parameter from to replace</param>
        /// <param name="toParameters">Enter parameter to </param>
        public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
        {
            this.ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();
            for (int i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
                this.ParameterReplacements.Add(fromParameters[i], toParameters[i]);
        }

        /// <summary>
        /// Gets or sets a value of the Parameter Replacements
        /// </summary>
        private IDictionary<ParameterExpression, ParameterExpression> ParameterReplacements
        {
            get;
            set;
        }

        /// <summary>
        /// This is a method for visit parameter
        /// </summary>
        /// <param name="node">Enter Parameter Expression</param>
        /// <returns>returns visit parameter for specific Parameter Expression</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            ParameterExpression replacement;
            if (this.ParameterReplacements.TryGetValue(node, out replacement))
                node = replacement;
            return base.VisitParameter(node);
        }
    }
}
