using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
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
            if (countdownEvent.IsSet) 
                return false;
            try
            {
                return countdownEvent.TryAddCount();
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }



        public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            Type enumType = value.GetType();
            string name = Enum.GetName(enumType, value)!;
            return enumType.GetField(name)?.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

        public static TAttribute? GetAttribute<TAttribute, TObject>(this TObject prop, Expression<Func<TObject, object>> expression)
            where TAttribute : Attribute
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            }
            return default(TAttribute);
        }
        public static string? GetOptionName<T>(this T obj, Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo propertyInfo)
            {
                OptionNameAttribute? optionNameAttribute = propertyInfo
                    .GetCustomAttributes(typeof(OptionNameAttribute), true).FirstOrDefault() as OptionNameAttribute;
                return optionNameAttribute?.Name;
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
            Func<TSelect, bool>? validate,
            Func<TSelect, string>? convert)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            if (expression is null) throw new ArgumentNullException(nameof(expression));

            TSelect select = expression.Compile().Invoke(obj);
            if ((validate is null || validate.Invoke(select)) &&
                expression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                OptionNameAttribute? optionNameAttribute = propertyInfo
                    .GetCustomAttributes(typeof(OptionNameAttribute), true).FirstOrDefault() as OptionNameAttribute;
                if (optionNameAttribute is null)
                    throw new InvalidOperationException();


                if (convert is not null)
                {
                    return $"{optionNameAttribute?.Name}={convert(select)}";
                }
                else
                {
                    if (select is bool b)
                    {
                        return $"{optionNameAttribute.Name}={b.ToString().ToLower()}";
                    }
                    else if (select is Enum @enum)
                    {
                        OptionNameAttribute? enumOptionName = @enum.GetAttribute<OptionNameAttribute>();
                        if (enumOptionName is not null)
                        {
                            return $"{optionNameAttribute.Name}={enumOptionName.Name}";
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
                    else if (select is Size size)
                    {
                        return $"{optionNameAttribute.Name}={size.Width}x{size.Height}";
                    }
                    else if (select is int i)
                    {
                        return $"{optionNameAttribute.Name}={i}";
                    }
                    // float/double MUST use InvariantCulture: comma-decimal locales would emit
                    // "max_fps=24,5" which the scrcpy server fails to parse. Missing float case
                    // silently dropped every float option (e.g. MaxFps since the int->float change).
                    else if (select is float f)
                    {
                        return $"{optionNameAttribute.Name}={f.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                    }
                    else if (select is double d)
                    {
                        return $"{optionNameAttribute.Name}={d.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
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


        internal static void WriteHostToNetworkOrder(this MemoryStream memoryStream, params object[] values)
        {
            foreach (object value in values)
            {
                WriteHostToNetworkOrder(memoryStream, value);
            }
        }
        static void WriteHostToNetworkOrder(MemoryStream memoryStream, object value)
        {
            Type type = value.GetType();
            if (type == typeof(byte[]))
            {
                byte[] buffer = (byte[])value;
                memoryStream.Write(buffer, 0, buffer.Length);
            }
            else if (type == typeof(bool))
            {
                byte[] buffer = BitConverter.GetBytes((bool)value);
                memoryStream.Write(buffer, 0, 1);
            }
            else if (type == typeof(Rectangle))
            {
                Rectangle rectangle = (Rectangle)value;
                memoryStream.WriteHostToNetworkOrder(rectangle.X, rectangle.Y, (UInt16)rectangle.Width, (UInt16)rectangle.Height);
            }
            else if (type.IsValueType)
            {
                if (type.IsEnum)
                {
                    type = Enum.GetUnderlyingType(type);
                }

                int size = Marshal.SizeOf(type);
                byte[]? buffer = null;
                switch (size)
                {
                    case 1:
                        memoryStream.WriteByte((byte)value);
                        return;

                    case 2:
                        if (type == typeof(UInt16))
                            buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((Int16)(UInt16)value)));
                        else if (type == typeof(Int16))
                            buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int16)value));
                        break;
                    case 4:
                        if (type == typeof(UInt32))
                            buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((Int32)(UInt32)value)));
                        else if (type == typeof(Int32))
                            buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int32)value));
                        break;
                    case 8:
                        if (type == typeof(UInt64))
                            buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((Int64)(UInt64)value)));
                        else if (type == typeof(Int64))
                            buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int64)value));
                        break;
                }
                if (buffer == null)
                    throw new NotSupportedException(type.FullName);

                memoryStream.Write(buffer, 0, buffer.Length);
            }
            else throw new InvalidOperationException("value must be struct/enum");
        }
    }
}
