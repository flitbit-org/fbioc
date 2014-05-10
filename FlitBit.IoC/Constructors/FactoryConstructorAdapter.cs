#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC.Constructors
{
	/// <summary>
	///   Factory constructor adapter.
	/// </summary>
	/// <typeparam name="T">target type T</typeparam>
	public sealed class FactoryConstructorAdapter<T> : ConstructorAdapter<T>
	{
		readonly Func<IContainer, T> _factory;

		internal FactoryConstructorAdapter(Func<IContainer, T> factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			_factory = factory;
		}

		/// <summary>
		///   Executes the constructor and returns the resulting instance.
		/// </summary>
		/// <param name="container">scoping container</param>
		/// <param name="name">the registered name or null</param>
		/// <param name="parameters">parameters intended for the new instance</param>
		/// <returns>a new instance</returns>
		public override T Execute(IContainer container, string name, params object[] parameters)
		{
			var instance = _factory(container);
			return instance;
		}
	}

	/// <summary>
	///   Factory constructor adapter.
	/// </summary>
	/// <typeparam name="T">target type T</typeparam>
	/// <typeparam name="TConcrete">concrete type C</typeparam>
	internal sealed class FactoryConstructorAdapter<T, TConcrete> : ConstructorAdapter<T>
		where TConcrete : T
	{
		readonly Func<IContainer, TConcrete> _factory;

		internal FactoryConstructorAdapter(Func<IContainer, TConcrete> factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			_factory = factory;
		}

		/// <summary>
		///   Executes the constructor and returns the resulting instance.
		/// </summary>
		/// <param name="container">scoping container</param>
		/// <param name="name">the registered name or null</param>
		/// <param name="parameters">parameters intended for the new instance</param>
		/// <returns>a new instance</returns>
		public override T Execute(IContainer container, string name, params object[] parameters)
		{
			T instance = _factory(container);
			return instance;
		}
	}
}