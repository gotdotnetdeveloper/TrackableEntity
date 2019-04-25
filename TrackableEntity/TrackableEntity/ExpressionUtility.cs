using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TrackableEntity
{
    /// <summary>
    /// Выражения.
    /// </summary>
    public static class ExpressionUtility
    {
        #region Публичные методы
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> selector)
        {
            MemberExpression memberExpression = selector.Body.RemoveConvert() as MemberExpression;
            if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException("Expression_InvalidPropertySelector", nameof(selector));
            return memberExpression.Member.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string GetPropertyName<TEntity>(Expression<Func<TEntity, object>> selector)
        {
            MemberExpression memberExpression = selector.Body.RemoveConvert() as MemberExpression;
            if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property || (!memberExpression.Member.DeclaringType.IsAssignableFrom(typeof(TEntity)) || memberExpression.Expression.NodeType != ExpressionType.Parameter))
                throw new ArgumentException("Expression_InvalidPropertySelector", nameof(selector));
            return memberExpression.Member.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string GetPropertyName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> selector)
        {
            MemberExpression memberExpression = selector.Body.RemoveConvert() as MemberExpression;
            if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property || (!memberExpression.Member.DeclaringType.IsAssignableFrom(typeof(TEntity)) || memberExpression.Expression.NodeType != ExpressionType.Parameter))
                throw new ArgumentException("Expression_InvalidPropertySelector", nameof(selector));
            return memberExpression.Member.Name;
        }
        #endregion
        #region Защищенные и внутренние методы
        internal static MemberExpression GetMemberExpression<T>(Expression<Func<T>> selector)
        {
            MemberExpression memberExpression = selector.Body.RemoveConvert() as MemberExpression;
            if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException("Expression_InvalidPropertySelector", nameof(selector));
            return memberExpression;
        }
        #endregion
        #region Приватные функции
        private static Expression RemoveConvert(this Expression expression)
        {
            while (expression != null && (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked))
                expression = ((UnaryExpression)expression).Operand.RemoveConvert();
            return expression;
        }
        #endregion
    }
}
