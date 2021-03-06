﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FlitBit.Core;
using FlitBit.Emit;
using FlitBit.IoC.Properties;

namespace FlitBit.IoC
{
	/// <summary>
	///   Used by the framework for duck typing dynamic objects.
	/// </summary>
	public abstract class BasicDuck
	{
		readonly object _target;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="source"></param>
		protected BasicDuck(object source)
		{
			Contract.Requires<ArgumentNullException>(source != null);
			this._target = source;
		}

		/// <summary>
		///   Gets the source object that has been duck typed.
		/// </summary>
		public object DuckTarget
		{
			get { return _target; }
		}

		/// <summary>
		///   Gets a binder for a member property.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		protected virtual GetMemberBinder MakeGetMemberBinder(string member) { return new BasicDuckGetMemberBinder(member); }

		/// <summary>
		///   Gets a binder for a member property.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		protected virtual SetMemberBinder MakeSetMemberBinder(string member) { return new BasicDuckSetMemberBinder(member); }

		/// <summary>
		///   Casts the duck type as target type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected T CastDuckTarget<T>() { return (T) _target; }

		/// <summary>
		///   Gets a member by name.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		protected object GetMember(string member)
		{
			if (_target is IDictionary<string, object>)
			{
				return (_target as IDictionary<string, object>)[member];
			}
			object result;
			((DynamicObject) this._target).TryGetMember(MakeGetMemberBinder(member), out result);
			return result;
		}

		/// <summary>
		///   Sets the member by name.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="val"></param>
		protected void SetMember(string member, object val)
		{
			if (_target is IDictionary<string, object>)
			{
				(_target as IDictionary<string, object>)[member] = val;
			}
			else
			{
				((DynamicObject) this._target).TrySetMember(MakeSetMemberBinder(member), val);
			}
		}
	}

	/// <summary>
	///   Helper class for getting duck type'd members.
	/// </summary>
	public class BasicDuckGetMemberBinder : GetMemberBinder
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="name"></param>
		public BasicDuckGetMemberBinder(string name)
			: base(name, false) { }

		/// <summary>
		///   Fallback handler for missing members.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="errorSuggestion"></param>
		/// <returns></returns>
		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) { return null; }
	}

	/// <summary>
	///   Helper class for setting duck type'd members.
	/// </summary>
	public class BasicDuckSetMemberBinder : SetMemberBinder
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="name"></param>
		public BasicDuckSetMemberBinder(string name)
			: base(name, false) { }

		/// <summary>
		///   Fallback handler for missing members.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		/// <param name="errorSuggestion"></param>
		/// <returns></returns>
		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value,
			DynamicMetaObject errorSuggestion) { return null; }
	}

	/// <summary>
	///   Utility class for accomplishing limited duck typing.
	/// </summary>
	public static class Duck
	{
		static readonly MethodInfo BasicDuckCastDuckTargetMethod = typeof(BasicDuck).GetMethod("CastDuckTarget",
																																													BindingFlags.Instance | BindingFlags.NonPublic);

		static readonly ConstructorInfo BasicDuckConstructor =
			typeof(BasicDuck).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] {typeof(object)},
																			null);

		static readonly MethodInfo BasicDuckGetMemberMethod = typeof(BasicDuck).GetMethod("GetMember",
																																											BindingFlags.Instance | BindingFlags.NonPublic);

		static readonly MethodInfo BasicDuckGetDuckTarget = typeof(BasicDuck).GetMethod("get_DuckTarget",
																																										BindingFlags.Instance | BindingFlags.Public);

		static readonly MethodInfo BasicDuckSetMemberMethod = typeof(BasicDuck).GetMethod("SetMember",
																																											BindingFlags.Instance | BindingFlags.NonPublic);

		static readonly Lazy<EmittedModule> DuckTypeModule =
			new Lazy<EmittedModule>(() => RuntimeAssemblies.DynamicAssembly.DefineModule("DuckTypes", null),
															LazyThreadSafetyMode.ExecutionAndPublication
				);

		/// <summary>
		///   Creates a duck type proxy over a source object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Design", "CA1004", Justification = "By design.")]
		public static T TypeAs<T>(object source)
		{
			Contract.Requires(source != null);
			Contract.Requires(typeof(T).IsInterface, Resources.Chk_TypeofTIsInterface);

			var sourceType = source.GetType();
			var targetType = typeof(T);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType,
																											String.Concat("Duck$", sourceType.GetReadableSimpleName().Replace(',', '_').Replace('.', '_')));

			lock (targetType.GetLockForType())
			{
				var module = DuckTypeModule.Value;
				var type = module.Builder.GetType(typeName, false, false) ?? BuildDuckType(module, targetType, typeName, sourceType);
				return (T) Activator.CreateInstance(type, source);
			}
		}

		static Type BuildDuckType(EmittedModule module, Type targetType, string typeName, Type sourceType)
		{
			var duckType = module.DefineClass(typeName,
																				TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass |
																					TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
																				typeof(BasicDuck),
																				Type.EmptyTypes
				);

			ImplementConstructor(duckType);

			// Implement all interfaces of the target type
			var set = new HashSet<Type>();
			ImplementInterface(duckType, sourceType, targetType);
			foreach (var intf in targetType.FindInterfaces((t, c) => !set.Contains(t), null))
			{
				ImplementInterface(duckType, sourceType, intf);
			}
			duckType.Compile();
			return duckType.Ref.Target;
		}

		static void ContributeDirectCallToSourceType(MethodInfo method, EmittedMethod impl,
			Type sourceType, MethodInfo sourceMethod, Type returnType)
		{
			impl.ContributeInstructions((m, il) =>
			{
				il.Nop();
				if (returnType != null)
				{
					il.DeclareLocal(method.ReturnType);
				}
				il.LoadArg_0();
				il.Call(BasicDuckCastDuckTargetMethod.MakeGenericMethod(sourceType));
				foreach (var p in impl.Parameters)
				{
					il.LoadArg(p.Index + 1);
				}
				il.CallVirtual(sourceMethod);
				if (returnType != null)
				{
					il.StoreLocal_0();
					il.LoadLocal_0();
				}
			});
		}

		/// <summary>
		///   Overrides the base constructor on the concrete type.
		/// </summary>
		/// <param name="type"></param>
		static void ImplementConstructor(EmittedClass type)
		{
			var ctor = type.DefineCtor();
			ctor.DefineParameter("source", typeof(Object));
			ctor.ContributeInstructions((m, il) =>
			{
				il.LoadArg_0();
				il.LoadArg_1();
				il.Call(BasicDuckConstructor);
				il.Nop();
			});
		}

		static void ImplementInterface(EmittedClass duckType, Type sourceType, Type intf)
		{
			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(sourceType))
			{
				ImplementInterfaceProxyForDynamicType(duckType, sourceType, intf);
			}
			else
			{
				ImplementInterfaceProxyForSourceType(duckType, sourceType, intf);
			}
		}

		static void ImplementInterfaceProxyForDynamicType(EmittedClass duckType, Type sourceType, Type intf)
		{
			duckType.AddInterfaceImplementation(intf);

			foreach (var member in intf.GetProperties())
			{
				ImplementPropertyProxyForDynamicType(duckType, member);
			}
			foreach (var member in intf.GetMethods().Where(m =>
																											m.Name.StartsWith("get_") == false
																												&& m.Name.StartsWith("set_") == false))
			{
				ImplementMethodProxyForDynamic(duckType, sourceType, member);
			}
		}

		static void ImplementInterfaceProxyForSourceType(EmittedClass duckType, Type sourceType, Type intf)
		{
			duckType.AddInterfaceImplementation(intf);

			foreach (var member in intf.GetProperties())
			{
				ImplementPropertyProxyForSourceType(duckType, sourceType, member);
			}
			foreach (var member in intf.GetMethods().Where(m =>
																											m.Name.StartsWith("get_") == false
																												&& m.Name.StartsWith("set_") == false))
			{
				ImplementMethodProxyForSourceType(duckType, sourceType, member);
			}
		}

		static void ImplementMethodProxyForDynamic(EmittedClass duckType, Type sourceType, MethodInfo method)
		{
			var impl = duckType.DefineOverrideMethod(method);
			var sourceMethod = sourceType.GetMethod(method.Name, impl.ParameterTypes, null);

			var returnType = impl.ReturnType.Target;
			if (returnType == typeof(void))
			{
				returnType = null;
			}
			if (sourceMethod != null && sourceMethod.ReturnType == method.ReturnType)
			{
				ContributeDirectCallToSourceType(method, impl, sourceType, sourceMethod, returnType);
			}
			else
			{
				impl.ContributeInstructions((m, il) =>
				{
					il.DeclareLocal(typeof(Delegate));
					il.DeclareLocal(typeof(bool));
					il.DeclareLocal(typeof(object[]));
					if (returnType != null)
					{
						il.DeclareLocal(returnType);
					}
					il.Nop();
					il.LoadArg_0();
					il.LoadValue(method.Name);
					il.Call(BasicDuckGetMemberMethod);
					il.IsInstance(typeof(Delegate));
					il.StoreLocal_0();
					il.LoadLocal_0();
					il.LoadNull();
					il.CompareEqual();
					il.StoreLocal_1();
					il.LoadLocal_1();
					var fail = il.DefineLabel();
					var success = il.DefineLabel();
					il.BranchIfTrue(fail);
					il.LoadLocal_0();
					il.NewArr(typeof(object), impl.Parameters.Count());
					il.StoreLocal_2();
					if (impl.Parameters.Any())
					{
						foreach (var p in impl.Parameters)
						{
							il.LoadLocal_2();
							il.LoadValue(p.Index);
							il.LoadArg(p.Index + 1);
							if (p.ParameterType.Target.IsValueType)
							{
								il.Box(p.ParameterType.Target);
							}
							il.StoreElementRef();
						}
					}
					il.LoadLocal_2();
					il.CallVirtual(typeof(Delegate).GetMethod("DynamicInvoke"));
					if (returnType != null)
					{
						if (returnType.IsValueType)
						{
							il.UnboxAny(returnType);
						}
						il.StoreLocal_3();
					}
					else
					{
						il.Pop();
					}
					il.Branch_ShortForm(success);
					il.MarkLabel(fail);
					il.ThrowException(typeof(NotImplementedException));
					il.MarkLabel(success);
					if (returnType != null)
					{
						il.LoadLocal_3();
					}
				});
			}
		}

		static void ImplementMethodProxyForSourceType(EmittedClass duckType, Type sourceType, MethodInfo method)
		{
			// GENERATES:
			// [return] base.CastDuckTarget<${sourceType}>().${method}(new object[] { [arg1, arg2, ...] });

			var impl = duckType.DefineOverrideMethod(method);
			var sourceMethod = sourceType.GetMethod(method.Name, impl.ParameterTypes, null);

			var returnType = impl.ReturnType.Target;
			if (returnType == typeof(void))
			{
				returnType = null;
			}
			if (sourceMethod != null)
			{
				ContributeDirectCallToSourceType(method, impl, sourceType, sourceMethod, returnType);
			}
			else
			{
				impl.ContributeInstructions((m, il) =>
				{
					il.Nop();
					il.ThrowException(typeof(NotImplementedException));
				});
			}
		}

		static void ImplementPropertyProxyForDynamicType(EmittedClass duckType, PropertyInfo property)
		{
			EmittedProperty prop;
			if (!duckType.TryGetProperty(property.Name, out prop))
			{
				prop = duckType.DefinePropertyFromPropertyInfo(property);
			}
			if (property.CanRead && prop.Getter == null)
			{
				var getter = prop.AddGetter();
				getter.ContributeInstructions((m, il) =>
				{
					il.DeclareLocal(property.PropertyType);
					il.Nop();
					il.LoadArg_0();
					il.LoadValue(property.Name);
					il.Call(BasicDuckGetMemberMethod);
					if (property.PropertyType.IsValueType)
					{
						il.UnboxAny(property.PropertyType);
					}
					il.StoreLocal_0();
					il.LoadLocal_0();
				});
			}
			if (property.CanWrite && prop.Setter == null)
			{
				var setter = prop.AddSetter();
				setter.ContributeInstructions((m, il) =>
				{
					il.Nop();
					il.LoadArg_0();
					il.LoadValue(property.Name);
					il.LoadArg_1();
					il.Call(BasicDuckSetMemberMethod);
				});
			}
		}

		static void ImplementPropertyProxyForSourceType(EmittedClass duckType, Type sourceType, PropertyInfo property)
		{
			EmittedProperty prop;
			var sourceProperty = sourceType.GetProperty(property.Name);
			if (!duckType.TryGetProperty(property.Name, out prop))
			{
				prop = duckType.DefinePropertyFromPropertyInfo(property);
			}
			if (property.CanRead && prop.Getter == null)
			{
				var getter = prop.AddGetter();
				getter.ContributeInstructions((m, il) =>
				{
					if (sourceProperty == null)
					{
						il.Nop();
						il.LoadValue(String.Concat(sourceType.GetReadableSimpleName(), " does not implement the property: ", property.Name));
						il.NewObj(typeof(NotImplementedException).GetConstructor(new[] {typeof(String)}));
						il.Throw();
					}
					else
					{
						il.DeclareLocal(sourceType);
						il.DeclareLocal(property.PropertyType);
						il.LoadArg_0();
						il.Call(BasicDuckGetDuckTarget);
						if (sourceType.IsValueType)
						{
							il.UnboxAny(sourceType);
						}
						else
						{
							il.CastClass(sourceType);
						}
						il.StoreLocal_0();
						il.LoadLocal_0();
						il.CallVirtual(sourceProperty.GetGetMethod());
						il.StoreLocal_1();
						il.LoadLocal_1();
					}
				});
			}
			if (property.CanWrite && prop.Setter == null)
			{
				var setter = prop.AddSetter();
				setter.ContributeInstructions((m, il) =>
				{
					if (sourceProperty == null || sourceProperty.CanWrite == false)
					{
						il.Nop();
						if (sourceProperty == null)
						{
							il.LoadValue(String.Concat(sourceType.GetReadableSimpleName(), " does not implement the property: ",
																				property.Name));
						}
						else
						{
							il.LoadValue(String.Concat(sourceType.GetReadableSimpleName(), "'s ", property.Name,
																				" property does not have a setter."));
						}
						il.NewObj(typeof(NotImplementedException).GetConstructor(new[] {typeof(String)}));
						il.Throw();
					}
					else
					{
						il.DeclareLocal(sourceType);
						il.DeclareLocal(property.PropertyType);
						il.LoadArg_0();
						il.Call(BasicDuckGetDuckTarget);
						if (sourceType.IsValueType)
						{
							il.UnboxAny(sourceType);
						}
						else
						{
							il.CastClass(sourceType);
						}
						il.StoreLocal_0();
						il.LoadLocal_0();
						il.LoadArg_1();
						il.CallVirtual(sourceProperty.GetSetMethod());
						il.Nop();
					}
				});
			}
		}
	}
}