#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC.Registry
{
	internal abstract class TypeRegistration : ContainerRegistrationParticipant, ITypeRegistration
	{
		protected TypeRegistration(IContainer container, Type registeredType)
			: base(container)
		{
			Contract.Requires<ArgumentNullException>(registeredType != null);

			RegisteredType = registeredType;
		}

		public bool HasEnded { get; protected set; }
		protected override void OnEnded() { HasEnded = true; }

		protected override bool PerformDispose(bool disposing) { return true; }

		#region ITypeRegistration Members

		public Type RegisteredType { get; private set; }
		public abstract Type TargetType { get; }
		public bool IsNamed { get; protected set; }
		public ScopeBehavior ScopeBehavior { get; protected set; }

		public ITypeRegistration ResolveAnInstancePerRequest()
		{
			if (ScopeBehavior.HasFlag(ScopeBehavior.SpecializationDisallowed))
			{
				throw new InvalidOperationException(String.Concat("Cannot reconfigure registration: ", ScopeBehavior));
			}

			ScopeBehavior = ScopeBehavior.InstancePerRequest;
			return this;
		}

		public ITypeRegistration ResolveAnInstancePerScope()
		{
			if (ScopeBehavior.HasFlag(ScopeBehavior.SpecializationDisallowed))
			{
				throw new InvalidOperationException(String.Concat("Cannot reconfigure registration: ", ScopeBehavior));
			}

			ScopeBehavior = ScopeBehavior.InstancePerScope;
			return this;
		}

		public ITypeRegistration ResolveAsSingleton()
		{
			if (ScopeBehavior.HasFlag(ScopeBehavior.SpecializationDisallowed))
			{
				throw new InvalidOperationException(String.Concat("Cannot reconfigure registration: ", ScopeBehavior));
			}

			ScopeBehavior = ScopeBehavior.Singleton;
			return this;
		}

		public ITypeRegistration DisallowSpecialization()
		{
			if (!ScopeBehavior.HasFlag(ScopeBehavior.SpecializationDisallowed))
			{
				ScopeBehavior = ScopeBehavior | ScopeBehavior.SpecializationDisallowed;
			}
			return this;
		}

		public abstract IResolver UntypedResolver { get; }

		#endregion
	}
}