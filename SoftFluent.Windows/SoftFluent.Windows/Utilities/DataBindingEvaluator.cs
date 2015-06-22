﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace SoftFluent.Windows.Utilities
{
    /// <summary>
    /// A utility class equivalent to the System.Web.DataBinder class, but that does not require a reference to the System.Web assembly.
    /// </summary>
	internal static class DataBindingEvaluator
	{
		private static readonly char[] expressionPartSeparator = new char[] { '.' };
		private static readonly char[] indexExprEndChars = new char[] { ']', ')' };
		private static readonly char[] indexExprStartChars = new char[] { '[', '(' };

        /// <summary>
        /// Evaluates data-binding expressions at run time.
        /// </summary>
        /// <param name="container">The object reference against which the expression is evaluated. This must be a valid object identifier in the page's specified language.</param>
        /// <param name="expression">The navigation path from the container object to the public property value to be placed in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <param name="format">A .NET Framework format string (like those used by String.Format) that converts the Object instance returned by the data-binding expression to a String object.</param>
        /// <returns>
        /// A String object that results from evaluating the data-binding expression and converting it to a string type.
        /// </returns>
        public static string Eval(object container, string expression, string format)
        {
            return Eval(container, expression, null, format);
        }

        /// <summary>
        /// Evaluates data-binding expressions at run time.
        /// </summary>
        /// <param name="container">The object reference against which the expression is evaluated. This must be a valid object identifier in the page's specified language.</param>
        /// <param name="expression">The navigation path from the container object to the public property value to be placed in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <param name="provider">The format provider.</param>
        /// <param name="format">A .NET Framework format string (like those used by String.Format) that converts the Object instance returned by the data-binding expression to a String object.</param>
        /// <returns>A String object that results from evaluating the data-binding expression and converting it to a string type.</returns>
        public static string Eval(object container, string expression, IFormatProvider provider, string format)
        {
            if (provider == null)
                return string.Format(format, Eval(container, expression));

            return string.Format(provider, format, Eval(container, expression));
        }

        /// <summary>
        /// Evaluates data-binding expressions at run time.
        /// </summary>
        /// <param name="container">The object reference against which the expression is evaluated. This must be a valid object identifier in the page's specified language.</param>
        /// <param name="expression">The navigation path from the container object to the public property value to be placed in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <returns>
        /// A String object that results from evaluating the data-binding expression and converting it to a string type.
        /// </returns>
        public static object Eval(object container, string expression)
        {
            return Eval(container, expression, true);
        }

        /// <summary>
        /// Evaluates data-binding expressions at run time.
        /// </summary>
        /// <param name="container">The object reference against which the expression is evaluated. This must be a valid object identifier in the page's specified language.</param>
        /// <param name="expression">The navigation path from the container object to the public property value to be placed in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <param name="throwOnError">if set to <c>true</c> errors may be throw.</param>
        /// <returns>
        /// A String object that results from evaluating the data-binding expression and converting it to a string type.
        /// </returns>
        public static object Eval(object container, string expression, bool throwOnError)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

            expression = ConvertUtilities.Nullify(expression, true);
			if (expression == null)
				throw new ArgumentException(null, "expression");

            if (container == null)
				return null;

            string[] expressionParts = expression.Split(expressionPartSeparator);
			return Eval(container, expressionParts, throwOnError);
		}

		private static object Eval(object container, string[] expressionParts, bool throwOnError)
		{
			object propertyValue = container;
			for (int i = 0; (i < expressionParts.Length) && (propertyValue != null); i++)
			{
				string propName = expressionParts[i];
				if (propName.IndexOfAny(indexExprStartChars) < 0)
				{
                    propertyValue = GetPropertyValue(propertyValue, propName, throwOnError);
				}
				else
				{
                    propertyValue = GetIndexedPropertyValue(propertyValue, propName, throwOnError);
				}
			}
			return propertyValue;
		}

        /// <summary>
        /// Retrieves the value of the specified property of the specified object.
        /// </summary>
        /// <param name="container">The object that contains the property.</param>
        /// <param name="propName">The name of the property that contains the value to retrieve.</param>
        /// <returns>The value of the specified property.</returns>
        public static object GetPropertyValue(object container, string propName)
        {
            return GetPropertyValue(container, propName, true);
        }

        /// <summary>
        /// Retrieves the value of the specified property of the specified object.
        /// </summary>
        /// <param name="container">The object that contains the property.</param>
        /// <param name="propName">The name of the property that contains the value to retrieve.</param>
        /// <param name="throwOnError">if set to <c>true</c> errors may be throw.</param>
        /// <returns>
        /// The value of the specified property.
        /// </returns>
		public static object GetPropertyValue(object container, string propName, bool throwOnError)
		{
			if (container == null)
				throw new ArgumentNullException("container");

            propName = ConvertUtilities.Nullify(propName, true);
            if (propName == null)
				throw new ArgumentException(null, "propName");

#if SILVERLIGHT
			PropertyInfo info = container.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
			if (info == null)
            {
                if (throwOnError)
			    	throw new ArgumentException(
				    	string.Format(@"DataBindingEvaluator: '{0}' does not contain a property with the name '{1}'.",
					    new object[] { container.GetType().FullName, propName }), "propName");

                return null;
            }
			
            return info.GetValue(container, null);
#else
#if NETFX_CORE
            PropertyInfo info = container.GetType().GetRuntimeProperty(propName);
            if (info == null)
            {
                if (throwOnError)
                   throw new ArgumentException(
                        string.Format(@"DataBindingEvaluator: '{0}' does not contain a property with the name '{1}'.",
                        new object[] { container.GetType().FullName, propName }), "propName");

                return null;
            }

            return info.GetValue(container, null);
#else
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(container).Find(propName, true);
            if (descriptor == null)
            {
                if (throwOnError)
                    throw new ArgumentException(
                        string.Format(@"DataBindingEvaluator: '{0}' does not contain a property with the name '{1}'.",
                        new object[] { container.GetType().FullName, propName }), "propName");

                return null;
            }
            return descriptor.GetValue(container);
#endif
#endif
        }

        /// <summary>
        /// Retrieves the value of a property of the specified container and navigation path.
        /// </summary>
        /// <param name="container">The object reference against which expr is evaluated. This must be a valid object identifier in the specified language for the page.</param>
        /// <param name="expression">The navigation path from the container object to the public property value to place in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <returns>An object that results from the evaluation of the data-binding expression.</returns>
        public static object GetIndexedPropertyValue(object container, string expression)
        {
            return GetIndexedPropertyValue(container, expression, true);
        }

        /// <summary>
        /// Retrieves the value of a property of the specified container and navigation path.
        /// </summary>
        /// <param name="container">The object reference against which expr is evaluated. This must be a valid object identifier in the specified language for the page.</param>
        /// <param name="expression">The navigation path from the container object to the public property value to place in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <param name="throwOnError">if set to <c>true</c> errors may be throw.</param>
        /// <returns>
        /// An object that results from the evaluation of the data-binding expression.
        /// </returns>
		public static object GetIndexedPropertyValue(object container, string expression, bool throwOnError)
		{
			if (container == null)
				throw new ArgumentNullException("container");

            expression = ConvertUtilities.Nullify(expression, true);
            if (expression == null)
				throw new ArgumentException(null, "expression");

			bool numberIndex = false;
			int length = expression.IndexOfAny(indexExprStartChars);
			int num2 = expression.IndexOfAny(indexExprEndChars, length + 1);
            if (((length < 0) || (num2 < 0)) || (num2 == (length + 1)))
            {
                if (throwOnError)
                    throw new ArgumentException(string.Format(@"DataBindingEvaluator: '{0}' is not a valid indexed expression.", new object[] { expression }));
                
                return null;
            }

            string propName = null;
			object index = null;
			string s = expression.Substring(length + 1, (num2 - length) - 1).Trim();
			if (length != 0)
			{
				propName = expression.Substring(0, length);
			}
			if (s.Length != 0)
			{
				if (((s[0] == '"') && (s[s.Length - 1] == '"')) || ((s[0] == '\'') && (s[s.Length - 1] == '\'')))
				{
					index = s.Substring(1, s.Length - 2);
				}
				else if (char.IsDigit(s[0]))
				{
					int nums;
					numberIndex = int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out nums);
					if (numberIndex)
					{
						index = nums;
					}
					else
					{
						index = s;
					}
				}
				else
				{
					index = s;
				}
			}
            if (index == null)
            {
                if (throwOnError)
                    throw new ArgumentException(string.Format(@"DataBindingEvaluator: '{0}' is not a valid indexed expression.", new object[] { expression }));
                
                return null;
            }
			
            object propertyValue = null;
			if ((propName != null) && (propName.Length != 0))
			{
				propertyValue = GetPropertyValue(container, propName);
			}
			else
			{
				propertyValue = container;
			}

			if (propertyValue == null)
				return null;

            Array array = propertyValue as Array;
			if ((array != null) && numberIndex)
				return array.GetValue((int)index);

            if ((propertyValue is IList) && numberIndex)
				return ((IList)propertyValue)[(int)index];

#if NETFX_CORE
            PropertyInfo info = propertyValue.GetType().GetRuntimeProperty("Item");
#else
            PropertyInfo info = propertyValue.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, new Type[] { index.GetType() }, null);
#endif
            if (info == null)
            {
                if (throwOnError)
                    throw new ArgumentException(string.Format(@"DataBindingEvaluator: '{0}' does not allow indexed access.", new object[] { propertyValue.GetType().FullName }));

                return null;
            }

            return info.GetValue(propertyValue, new object[] { index });
		}
	}
}