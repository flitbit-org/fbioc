﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	///   Interface for registration participants.
	/// </summary>
	public interface IContainerRegistrationParticipant : IContainerOwned
	{
		/// <summary>
		///   Indicates whether the registration has ended for this participant.
		/// </summary>
		bool IsEnded { get; }

		/// <summary>
		///   Ends registration for the participant and returns the
		///   owning container.
		/// </summary>
		/// <returns>The owner, container.</returns>
		IContainer End();
	}

	/// <summary>
	///   Abstract implementation of IContainerRegistrationParticipant
	/// </summary>
	public abstract class ContainerRegistrationParticipant : ContainerOwned, IContainerRegistrationParticipant
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="container">the container where the registration is occurring</param>
		public ContainerRegistrationParticipant(IContainer container)
			: base(container) { }

		/// <summary>
		///   Method called when registration is ended.
		/// </summary>
		protected abstract void OnEnded();

		#region IContainerRegistrationParticipant Members

		/// <summary>
		///   Ends registration for the participant and returns the
		///   owning container.
		/// </summary>
		/// <returns>The owner, container.</returns>
		public IContainer End()
		{
			if (IsEnded)
			{
				throw new InvalidOperationException("Already ended.");
			}
			OnEnded();
			IsEnded = true;
			return Container;
		}

		/// <summary>
		///   Indicates whether the registration has ended for this participant.
		/// </summary>
		public bool IsEnded { get; private set; }

		#endregion
	}
}