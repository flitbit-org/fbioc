#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.IoC.Registry
{
	internal abstract class TypeRegistry : ContainerOwned, ITypeRegistry
	{
		ITypeRegistration _current;

		protected TypeRegistry(IContainer container, Type registeredType, ITypeRegistration current)
			: base(container)
		{
			_current = current;
			if (registeredType == null)
			{
				throw new ArgumentNullException("registeredType");
			}
			this.RegisteredType = registeredType;
		}

		public ITypeRegistration UntypedRegistration
		{
			get { return Util.VolatileRead(ref _current); }
		}

		protected virtual void CheckedSetRegistration(ITypeRegistration reg, ITypeRegistration current)
		{
			if (current != reg)
			{
				if (Interlocked.CompareExchange(ref _current, reg, current) != current)
				{
					Util.Dispose(ref current);
					throw new ContainerRegistryException("Victimized by concurrent registration");
				}
			}
		}

		protected override bool PerformDispose(bool disposing)
		{
			if (disposing)
			{
				Util.Dispose(ref _current);
			}
			return disposing;
		}

		protected void CheckCanSpecializeRegistration(ITypeRegistration current)
		{
			if (current != null)
			{
				if (current.ScopeBehavior.HasFlag(ScopeBehavior.SpecializationDisallowed))
				{
					throw new ContainerRegistryException(
						String.Concat(
												 "The type does not allow specialized registrations: ",
												RegisteredType.GetReadableFullName()));
				}
			}
		}

		#region ITypeRegistry Members

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

		public abstract IResolver UntypedResolver { get; }

		public virtual ITypeRegistration Register(Type typ, params Param[] parameters)
		{
			if (typ == null)
			{
				throw new ArgumentNullException("typ");
			}
			if (!RegisteredType.IsAssignableFrom(typ))
			{
				throw new ArgumentNullException(
					String.Concat("Expected a type assignable to: ", RegisteredType.GetReadableFullName())
					);
			}

			var current = UntypedRegistration;
			CheckCanSpecializeRegistration(current);

			var reg = (ITypeRegistration) Activator.CreateInstance(
																														 typeof(TypeRegistration<,>).MakeGenericType(RegisteredType, typ),
																														Container, this, parameters
				);
			CheckedSetRegistration(reg, current);
			return reg;
		}

		#endregion
	}
}