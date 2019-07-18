﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.Rules.Filters;

namespace SmartStore.Rules.Operators
{
    internal class NotInOperator : InOperator
    {
        internal NotInOperator()
            : base("NotIn", true) { }
    }

    internal class InOperator : RuleOperator
    {
        internal InOperator() 
            : this("In", false) { }

        protected InOperator(string op, bool negate)
            : base(op)
        {
            Negate = negate;
        }

        private bool Negate { get; set; }

        public override Expression GenerateExpression(Expression left /* member expression */, Expression right /* collection instance */)
        {
            var constantExpr = right as ConstantExpression;
            if (constantExpr == null)
                throw new ArgumentException($"The expression must be of type '{nameof(ConstantExpression)}'.", nameof(right));

            if (constantExpr.Value == null || !constantExpr.Type.IsSubClass(typeof(ICollection<>)))
            {
                throw new ArgumentException("The 'In' operator only supports non-null instances from types that implement 'ICollection<T>'.", nameof(right));
            }

            var itemType = right.Type.GetGenericArguments()[0];
            var containsMethod = ExpressionHelper.GetCollectionContainsMethod(itemType);

            return Expression.Equal(
                Expression.Call(right, containsMethod, left), 
                Negate ? ExpressionHelper.FalseLiteral : ExpressionHelper.TrueLiteral);
        }

        //private Expression GetExpressionHandlingNullables(MemberExpression left, ConstantExpression right, Type type, MethodInfo inInfo)
        //{
        //    var listUnderlyingType = Nullable.GetUnderlyingType(type.GetGenericArguments()[0]);
        //    var memberUnderlingType = Nullable.GetUnderlyingType(left.Type);
        //    if (listUnderlyingType != null && memberUnderlingType == null)
        //    {
        //        return Expression.Call(right, inInfo, left.Expression);
        //    }

        //    return null;
        //}
    }
}