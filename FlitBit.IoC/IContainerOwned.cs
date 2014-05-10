#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.Core;

namespace FlitBit.IoC
{
	/// <summary>
	///   Interface for objects owned by a container.
	/// </summary>
	public interface IContainerOwned : IDisposable
	{
		/// <summary>
		///   Gets the container owner.
		/// </summary>
		IContainer Container { get; }
	}

	/// <summary>
	///   abstract implementation of the IContainerOwned interface
	/// </summary>
	public abstract class ContainerOwned : Disposable, IContainerOwned
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="container">the container, owner</param>
		protected ContainerOwned(IContainer container)
		{
			Contract.Requires<ArgumentNullException>(container != null);

			this.Container = container;
		}

		#region IContainerOwned Members

		/// <summary>
		///   Returns the container, owner.
		/// </summary>
		public IContainer Container { get; private set; }

		#endregion
	}
}