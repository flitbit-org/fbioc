#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.IoC.Properties;

namespace FlitBit.IoC.Stereotype
{
	/// <summary>
	/// Enum of stereotype behaviors.
	/// </summary>
	[Flags]
	public enum StereotypeBehaviors
	{
		/// <summary>
		/// Indicates the stereotype is a marker.
		/// </summary>
		Marker = 0,
		/// <summary>
		/// Indicates the stereotypical behavior is emitted for the marked element.
		/// </summary>
		AutoImplementedBehavior = 1,
		/// <summary>
		/// Indicates that overrides have been disallowed for the stereotype.
		/// </summary>
		OverridesDisallowed = 2,
		/// <summary>
		/// Indicates the stereotype contributes logic to emitted code.
		/// </summary>
		ContributesLogic = 3
	}

	/// <summary>
	/// Stereotype base class; stereotypes the target.
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	public class StereotypeAttribute : Attribute
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected StereotypeAttribute(StereotypeBehaviors behavior)
		{
			this.Behaviors = behavior;
		}

		/// <summary>
		/// Indicates the stereotype's behaviors.
		/// </summary>
		public StereotypeBehaviors Behaviors { get; private set; }

		/// <summary>
		/// Called by the framework for stereotypes with behavior including 
		/// StereotypeBehaviors.AutoImplementedBehavior.
		/// </summary>
		/// <typeparam name="T">stereotype type T</typeparam>
		/// <param name="container">container from which the instance was requested</param>
		/// <returns><em>true</em> if a concrete type is registered as a result of the call; otherwise <em>false</em></returns>
		public virtual bool RegisterStereotypeImplementation<T>(IContainer container)
		{
			return false;
		}

		/// <summary>
		/// Ensures the type T is an interface.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		protected void RequireTypeIsInterface<T>()
		{
			Contract.Requires<ArgumentException>(typeof(T).IsInterface, Resources.Err_StereotypeBehaviorGeneratedForInterfacesOnly);
		}
	}
}
