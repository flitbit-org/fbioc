#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.IoC.Constructors
{
	/// <summary>
	///   Constructor adapter for types that have a default constructor.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class DefaultConstructorAdapter<T> : ConstructorAdapter<T>
		where T : new()
	{
		/// <summary>
		///   Executes the constructor and returns the resulting instance.
		/// </summary>
		/// <param name="container">scoping container</param>
		/// <param name="name">the registered name or null</param>
		/// <param name="parameters">parameters intended for the new instance</param>
		/// <returns>a new instance</returns>
		public override T Execute(IContainer container, string name, params object[] parameters)
		{
			var instance = new T();
			return instance;
		}
	}

	/// <summary>
	///   Constructor adapter for types that have a default constructor.
	/// </summary>
	/// <typeparam name="T">target type T</typeparam>
	/// <typeparam name="TConcrete">concrete type C</typeparam>
	public sealed class DefaultConstructorAdapter<T, TConcrete> : ConstructorAdapter<T, TConcrete>
		where TConcrete : T, new()
	{
		/// <summary>
		///   Executes the constructor and returns the resulting instance.
		/// </summary>
		/// <param name="container">scoping container</param>
		/// <param name="name">the registered name or null</param>
		/// <param name="parameters">parameters intended for the new instance</param>
		/// <returns>a new instance</returns>
		public override T Execute(IContainer container, string name, params object[] parameters)
		{
			T instance = new TConcrete();
			return instance;
		}
	}
}