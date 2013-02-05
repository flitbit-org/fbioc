#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.IoC
{
	/// <summary>
	/// Abstract class for parameters used with a container.
	/// </summary>
	public abstract class Param
	{
		/// <summary>
		/// An empty param array.
		/// </summary>
		public static readonly Param[] EmptyParams = new Param[0];

		/// <summary>
		/// Creates a param on a value.
		/// </summary>
		/// <typeparam name="T">value type T</typeparam>
		/// <param name="value">the value</param>
		/// <returns>a param</returns>
		public static Param Value<T>(T value)
		{
			return new ParamValue<T>(ParamKind.UserSupplied, value);
		}
		/// <summary>
		/// Creates a param that will resolve type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>a param</returns>
		public static Param Resolve<T>()
		{
			return new ParamFromContainer<T>();
		}

		/// <summary>
		/// Creates a param that will resolve type T from the container by registered name.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="name">the registered name</param>
		/// <returns>a param</returns>
		public static Param ResolveNamed<T>(string name)
		{
			return new ParamFromContainerNamed<T>(name);
		}

		/// <summary>
		/// Creates a param with a name and value.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="name">the name</param>
		/// <param name="value">the value</param>
		/// <returns>a param</returns>
		public static Param Named<T>(string name, T value)
		{
			return new ParamValueNamed<T>(ParamKind.UserSupplied | ParamKind.Named, name, value);
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="kind"></param>
		/// <param name="typeofValue"></param>
		protected Param(ParamKind kind, Type typeofValue)
		{
			this.Kind = kind;
			this.TypeofValue = typeofValue;
		}

		/// <summary>
		/// Gets the param's kind.
		/// </summary>
		public ParamKind Kind { get; private set; }

		/// <summary>
		/// Gets the type of the param's value.
		/// </summary>
		public Type TypeofValue { get; private set; }

		/// <summary>
		/// Gets the parameter's value.
		/// </summary>
		/// <param name="container">scoping container</param>
		/// <returns>the param's value</returns>
		public abstract object GetValue(IContainer container);

		/// <summary>
		/// Indicates whether the param is named.
		/// </summary>
		public bool HasName { get { return Kind.HasFlag(ParamKind.Named); } }

		/// <summary>
		/// Gets the param's name.
		/// </summary>
		public string Name { get; protected set; }

		internal static Param Missing(string name, int position, Type type)
		{
			return new ParamMissing(name, position, type);
		}

		internal static Param Declared<T>(T value)
		{
			return new ParamValue<T>(ParamKind.DeclaredDefault, value);
		}

		internal static Param MakeValueUsingReflection(ParamKind kind, Type type, object value)
		{
			Contract.Requires<ArgumentException>(typeof(ParamValue<>).GetGenericArguments().Length == 1);
			Type tt = typeof(ParamValue<>).MakeGenericType(type);
			return (Param)Activator.CreateInstance(tt, kind, value);
		}

		internal static Param MakeContainterSupplied(Type type)
		{
			Type tt = typeof(ParamFromContainer<>).MakeGenericType(type);
			return (Param)Activator.CreateInstance(tt);
		}

		internal static Param[] GetDefaultParamsUsingReflection(ConstructorInfo ci)
		{
			var defined = ci.GetParameters();
			Param[] result = new Param[defined.Length];

			for (var i = 0; i < defined.Length; i++)
			{
				var p = defined[i];
				// Make a default for the parameter...
				if (p.Attributes.HasFlag(ParameterAttributes.HasDefault))
				{
					result[i] = Param.MakeValueUsingReflection(ParamKind.DeclaredDefault, p.ParameterType, p.DefaultValue);
				}
				else if (p.ParameterType.IsClass || p.ParameterType.IsInterface)
				{
					result[i] = Param.MakeContainterSupplied(p.ParameterType);
				}
				else
				{
					result[i] = Param.Missing(p.Name, p.Position, p.ParameterType);
				}
			}
			return result;
		}

		internal static bool TryBindSuppliedDefaults(ConstructorInfo ci, Param[] defaults, out Param[] bound)
		{
			Contract.Requires<ArgumentNullException>(ci != null);

			var defined = ci.GetParameters();
			Param[] result = new Param[defined.Length];
			if (defaults != null)
			{
				if (defaults.Length == defined.Length)
				{
					var matched = 0;
					for (int i = 0; i < defined.Length; i++)
					{
						if (defaults[i].Kind.HasFlag(ParamKind.Named)
							&& defaults[i].Name != defined[i].Name) break;

						if (!defined[i].ParameterType.IsAssignableFrom(defaults[i].TypeofValue)) break;

						result[i] = defaults[i];
						matched++;
					}
					if (matched == defined.Length)
					{
						bound = result;
						return true;
					}
				}
			}
			bound = null;
			return false;
		}
	}
	
	}
