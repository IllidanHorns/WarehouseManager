using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManager.Services.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public static class UniqueValidationExtensions
    {
        public static async Task<bool> IsUniqueAsync<T>(
        this IQueryable<T> queryable,
        Expression<Func<T, object>> propertySelector,
        object value)
        {
            var propertyName = GetPropertyName(propertySelector); //Email
            var param = Expression.Parameter(typeof(T), "x"); //User
            var property = Expression.Property(param, propertyName); //User.Email
            var equal = Expression.Equal(
                Expression.Convert(property, typeof(object)),
                Expression.Constant(value, typeof(object))
            ); //User.Email == value
            var lambda = Expression.Lambda<Func<T, bool>>(equal, param);

            return !await queryable.AnyAsync(lambda);
        }

        public static async Task<bool> IsUniqueAsync<T>(
            this IQueryable<T> queryable,
            Expression<Func<T, object>> propertySelector,
            object value,
            int excludedId)
        {
            var propertyName = GetPropertyName(propertySelector);
            var idProperty = typeof(T).GetProperty("Id")
                ?? throw new InvalidOperationException($"Entity {typeof(T).Name} must have 'Id' property.");

            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, propertyName);
            var idProp = Expression.Property(param, idProperty);
            var condition1 = Expression.NotEqual(idProp, Expression.Constant(excludedId));
            var condition2 = Expression.Equal(
                Expression.Convert(property, typeof(object)),
                Expression.Constant(value, typeof(object))
            );
            var body = Expression.AndAlso(condition1, condition2);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return !await queryable.AnyAsync(lambda);
        }

        private static string GetPropertyName<T>(Expression<Func<T, object>> propertySelector)
        {
            var body = propertySelector.Body is UnaryExpression unary
                ? unary.Operand
                : propertySelector.Body;

            return (body as MemberExpression)?.Member.Name
                ?? throw new ArgumentException("Несуществующее  поле!");
        }
    }
}
