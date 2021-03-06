﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FlitBit.IoC.Constructors
{
	/// <summary>
	///   Gets a contructor set for a type.
	/// </summary>
	/// <typeparam name="T">target type T</typeparam>
	/// <typeparam name="TConcrete">concrete type C</typeparam>
	public sealed class ConstructorSet<T, TConcrete>
		where TConcrete : class, T
	{
		readonly Lazy<ConstructorCommand<T>[]> _constructors;

		readonly Param[] _parameters;
		ConstructorCommand<T> _default;
		volatile ConstructorCommand<T> _mostRecent;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="parameters"></param>
		public ConstructorSet(Param[] parameters)
		{
			_parameters = parameters;
			_constructors = new Lazy<ConstructorCommand<T>[]>(BuildConstructorCommands,
																												LazyThreadSafetyMode.ExecutionAndPublication);
		}

		internal bool TryMatchAndBind(Param[] parameters, out CommandBinding<T> command)
		{
			var constructors = _constructors.Value;

			if (parameters == null || parameters.Length == 0 && _default != null)
			{
				return _default.TryMatchAndBind(Param.EmptyParams, out command);
			}

			var mru = _mostRecent;
			if (mru != null && mru.ParameterCount == parameters.Length)
			{
				if (mru.TryMatchAndBind(parameters, out command))
				{
					return true;
				}
			}

			var plen = parameters.Length;
			if (plen > 0)
			{
				foreach (var c in constructors.Where(cc => cc.ParameterCount == plen))
				{
					if (c.TryMatchAndBind(parameters, out command))
					{
						_mostRecent = c;
						return true;
					}
				}
			}
			else
			{
				var c = constructors.OrderByDescending(ci => ci.ParameterCount).FirstOrDefault();
				if (c != null && c.TryMatchAndBind(Param.EmptyParams, out command))
				{
					return true;
				}
			}
			command = null;
			return false;
		}

		ConstructorCommand<T>[] BuildConstructorCommands()
		{
			var ord = 0;
			var result = (from c in typeof(TConcrete).GetConstructors(BindingFlags.Instance | BindingFlags.Public)
										let parms = c.GetParameters()
										orderby parms.Count()
										select new ConstructorCommand<T, TConcrete>(c, _parameters, ord++)).ToArray<ConstructorCommand<T>>();

			_default = result.FirstOrDefault(c => c.BoundToSuppliedDefaults) ?? result.FirstOrDefault(c => c.ParameterCount == 0);
			return result;
		}
	}
}