#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.IoC.Registry
{
	internal abstract class TypeRegistry : ContainerOwned, ITypeRegistry
	{		
		protected TypeRegistry(IContainer container, Type registeredType, ITypeRegistration current)
			: base(container)
		{
			if (registeredType == null) throw new ArgumentNullException("registeredType");
			this.RegisteredType = registeredType;
		}

		public Type RegisteredType { get; private set; }
		
		public bool CanSpecializeRegistration
		{
			get
			{
				var r = UntypedRegistration;
				return r == null 
					|| (!r.ScopeBehavior.HasFlag(ScopeBehavior.SpecializationDisallowed));
			}
		}

		public abstract ITypeRegistration UntypedRegistration { get; }

		public abstract IResolver UntypedResolver { get; }

		public virtual ITypeRegistration Register(Type typ)
		{
			if (typ == null) throw new ArgumentNullException("typ");
			if (!RegisteredType.IsAssignableFrom(typ))
				throw new ArgumentNullException(
					String.Concat("Expected a type assignable to: ", RegisteredType.GetReadableFullName()),
					"typ"
					);

			var current = UntypedRegistration;
			CheckCanSpecializeRegistration(current);

			ITypeRegistration reg = (ITypeRegistration)Activator.CreateInstance(
				typeof(TypeRegistration<,>).MakeGenericType(RegisteredType, typ),
				Container, this, null
				);
			CheckedSetRegistration(reg, current);
			return reg;
		}

		protected void CheckCanSpecializeRegistration(ITypeRegistration current)
		{
			if (current != null)
			{
				if (current.ScopeBehavior.HasFlag(ScopeBehavior.SpecializationDisallowed))
					throw new ContainerRegistryException(String.Concat("The type is registered as a singleton in an outer scope; its registration cannot be specialized: ", RegisteredType.GetReadableFullName()));
			}			
		}

		protected abstract void CheckedSetRegistration(ITypeRegistration reg, ITypeRegistration current);		
	}
}
