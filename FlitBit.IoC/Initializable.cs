#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.Copy;

namespace FlitBit.IoC
{
	/// <summary>
	///   Intermediate class used by the framework to capture
	///   a newly created object and initialize it from data provided
	///   by another object.
	/// </summary>
	/// <typeparam name="T">object type T</typeparam>
	public sealed class Initialize<T>
	{
		readonly IContainer _container;
		readonly T _instance;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="it"></param>
		internal Initialize(IContainer c, T it)
		{
			Contract.Requires<ArgumentNullException>(c != null);

			_container = c;
			_instance = it;
		}

		/// <summary>
		///   The newly created instance.
		/// </summary>
		public T Instance
		{
			get { return _instance; }
		}

		/// <summary>
		///   Initializes the newly created instance from values given.
		/// </summary>
		/// <typeparam name="TInit"></typeparam>
		/// <param name="init"></param>
		/// <returns></returns>
		public T Init<TInit>(TInit init)
		{
			var copier = _container.New<ICopier<TInit, T>>();
			copier.CopyTo(_instance, init, CopyKind.Loose, _container);
			_container.NotifyObserversOfCreationEvent(typeof(T), _instance, null, CreationEventKind.Initialized);
			return _instance;
		}
	}
}