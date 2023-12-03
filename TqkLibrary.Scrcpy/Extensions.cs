using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TqkLibrary.Scrcpy.Attributes;

namespace TqkLibrary.Scrcpy
{
    internal static class Extensions
    {
        public static bool SafeTryAddCount(this CountdownEvent countdownEvent)
        {
            try
            {
                return countdownEvent.TryAddCount();
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }



        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name)?.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

        public static TAttribute GetAttribute<TAttribute, TObject>(this TObject prop, Expression<Func<TObject, object>> expression)
            where TAttribute : Attribute
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            }
            return default(TAttribute);
        }
        public static string GetOptionName<T>(this T obj, Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo propertyInfo)
            {
                OptionNameAttribute optionNameAttribute = propertyInfo
                    .GetCustomAttributes(typeof(OptionNameAttribute), true).FirstOrDefault() as OptionNameAttribute;
                return optionNameAttribute.Name;
            }
            return string.Empty;
        }
        public static string _GetArgument<T, TSelect>(
            this T obj,
            Expression<Func<T, TSelect>> expression)
            => obj._GetArgument(expression, null, null);
        public static string _GetArgument<T, TSelect>(
            this T obj,
            Expression<Func<T, TSelect>> expression,
            bool validate)
            => obj._GetArgument(expression, x => validate, null);
        public static string _GetArgument<T, TSelect>(
            this T obj,
            Expression<Func<T, TSelect>> expression,
            Func<TSelect, bool> validate)
            => obj._GetArgument(expression, validate, null);
        public static string _GetArgument<T, TSelect>(
            this T obj,
            Expression<Func<T, TSelect>> expression,
            Func<TSelect, string> convert)
            => obj._GetArgument(expression, null, convert);
        public static string _GetArgument<T, TSelect>(
            this T obj,
            Expression<Func<T, TSelect>> expression,
            Func<TSelect, bool> validate,
            Func<TSelect, string> convert)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            if (expression is null) throw new ArgumentNullException(nameof(expression));

            TSelect select = expression.Compile().Invoke(obj);
            if ((validate is null || validate.Invoke(select)) &&
                expression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                OptionNameAttribute optionNameAttribute = propertyInfo
                    .GetCustomAttributes(typeof(OptionNameAttribute), true).FirstOrDefault() as OptionNameAttribute;
                if (convert is not null)
                {
                    return $"{optionNameAttribute.Name}={convert(select)}";
                }
                else
                {
                    if (select is bool b)
                    {
                        return $"{optionNameAttribute.Name}={b.ToString().ToLower()}";
                    }
                    else if (select is Enum @enum)
                    {
                        OptionNameAttribute enumOptionName = @enum.GetAttribute<OptionNameAttribute>();
                        if (enumOptionName is not null)
                        {
                            return $"{optionNameAttribute.Name}={@enum.GetAttribute<OptionNameAttribute>().Name}";
                        }
                        else
                        {
                            return $"{optionNameAttribute.Name}={Convert.ToInt32(@enum)}";
                        }
                    }
                    else if (select is Rectangle rect)
                    {
                        return $"{optionNameAttribute.Name}={rect.Width}:{rect.Height}:{rect.X}:{rect.Y}";
                    }
                    else if (select is int i)
                    {
                        return $"{optionNameAttribute.Name}={i}";
                    }
                    else if (select is string s)
                    {
                        return $"{optionNameAttribute.Name}={s}";
                    }
#if DEBUG
                    else
                    {
                        //null
                    }
#endif
                }
            }
            return string.Empty;
        }
    }
}
