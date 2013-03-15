#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FlitBit.IoC.Constructors
{
	/// <summary>
	///   Base constructor command; matches incomming parameters to the most suitable
	///   constructor declared on the target type.
	/// </summary>
	/// <typeparam name="T">target type T</typeparam>
	public abstract class ConstructorCommand<T>
	{
		/// <summary>
		///   Indicates whether the command is bound to supplied defaults.
		/// </summary>
		public abstract bool BoundToSuppliedDefaults { get; }

		/// <summary>
		///   Gets the parameter count.
		/// </summary>
		public abstract int ParameterCount { get; }

		/// <summary>
		///   Tries to match supplied params to a constructor and provides a command binding.
		/// </summary>
		/// <param name="parameters">the supplied parameters</param>
		/// <param name="binding">variable to hold the binding upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		public abstract bool TryMatchAndBind(Param[] parameters, out CommandBinding<T> binding);
	}

	/// <summary>
	///   Default implementation of the constructor command type.
	/// </summary>
	/// <typeparam name="T">target type T</typeparam>
	/// <typeparam name="TConcrete">concrete type C</typeparam>
	public sealed class ConstructorCommand<T, TConcrete> : ConstructorCommand<T>
		where TConcrete : class, T
	{
		readonly Lazy<ConstructorAdapter<T>> _adapter;
		readonly bool _bound;
		readonly ConstructorInfo _ctor;
		readonly bool _isMissingParameters;
		readonly int _ordinal;
		readonly Param[] _params;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="ci">reflected constructor info</param>
		/// <param name="defaults">default params supplied during registration</param>
		/// <param name="ordinal">ordinal position of the constructor among constructors for the concrete type</param>
		public ConstructorCommand(ConstructorInfo ci, Param[] defaults, int ordinal)
		{
			_ctor = ci;
			_ordinal = ordinal;
			_bound = Param.TryBindSuppliedDefaults(_ctor, defaults, out _params);
			if (!this.BoundToSuppliedDefaults)
			{
				_params = Param.GetDefaultParamsUsingReflection(_ctor);
			}
			_isMissingParameters = _params.Count(p => p.Kind == ParamKind.Missing) > 0;
			_adapter = new Lazy<ConstructorAdapter<T>>(ActivateConstructorAdapter, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>
		///   Indicates whether the command is bound to supplied defaults.
		/// </summary>
		public override bool BoundToSuppliedDefaults
		{
			get { return _bound; }
		}

		/// <summary>
		///   Gets the parameter count.
		/// </summary>
		public override int ParameterCount
		{
			get { return _params.Length; }
		}

		/// <summary>
		///   Tries to match supplied params to a constructor and provides a command binding.
		/// </summary>
		/// <param name="parameters">the supplied parameters</param>
		/// <param name="binding">variable to hold the binding upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		public override bool TryMatchAndBind(Param[] parameters, out CommandBinding<T> binding)
		{
			var plen = parameters.Length;
			if (plen == 0 && !_isMissingParameters)
			{
				binding = CommandBinding<T>.Create(_adapter.Value, _params);
				return true;
			}
			if (this._params.Length == plen)
			{
				var result = new Param[plen];
				for (var i = 0; i < plen; i = i + 1)
				{
					result[i] = (parameters[i].Kind == ParamKind.ContainerSupplied) ? this._params[i] : parameters[i];

					if (result[i].Kind == ParamKind.Missing)
					{
						binding = null;
						return false;
					}
				}
				binding = CommandBinding<T>.Create(this._adapter.Value, result);
				return true;
			}

			binding = null;
			return false;
		}

		ConstructorAdapter<T> ActivateConstructorAdapter()
		{
			var typ = ConstructorAdapter<T, TConcrete>.GetConstructorAdapterByOrdinal(_ordinal, _ctor);
			return (ConstructorAdapter<T>) Activator.CreateInstance(typ);
		}
	}
}