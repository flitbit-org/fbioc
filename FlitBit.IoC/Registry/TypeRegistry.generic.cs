#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.IoC.Registry
{
	internal class GenericTypeRegistry : ContainerOwned, IGenericTypeRegistry, ITypeRegistryManagement
	{
		readonly Lazy<ConcurrentDictionary<string, INamedTypeRegistration>> _named =
			new Lazy<ConcurrentDictionary<string, INamedTypeRegistration>>(LazyThreadSafetyMode.ExecutionAndPublication);

		readonly ConcurrentDictionary<Type, IResolver> _resolvers = new ConcurrentDictionary<Type, IResolver>();
		IGenericTypeRegistration _current;

		public GenericTypeRegistry(IContainer container, Type generic)
			: this(container, generic, null, null) { }

		GenericTypeRegistry(IContainer container, Type generic, IGenericTypeRegistration current)
			: this(container, generic, current, null) { }

		GenericTypeRegistry(IContainer container, Type generic, IGenericTypeRegistration current,
			IEnumerable<INamedTypeRegistration> named)
			: base(container)
		{
			this.RegisteredType = generic;
			this.CanSpecializeRegistration = false;
			_current = current;
			if (named == null)
			{
				return;
			}
			foreach (var n in named)
			{
				this._named.Value.TryAdd(n.Name, n);
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

		void CheckedSetRegistration(IGenericTypeRegistration reg, IGenericTypeRegistration current)
		{
			if (Interlocked.CompareExchange(ref _current, reg, current) != current)
			{
				Util.Dispose(ref current);
				throw new ContainerRegistryException("Victimized by concurrent registration.");
			}
		}

		#region IGenericTypeRegistry Members

		public Type RegisteredType { get; private set; }

		public bool CanSpecializeRegistration { get; private set; }

		public ITypeRegistration Register(Type type, params Param[] parameters)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			// type must be a closed constructed type, based on the RegisteredType generic type definition.
			//if (type.ContainsGenericParameters)
			//  throw new ArgumentException(String.Concat("Invalid type; must be a closed constructed type (no unassigned generic parameters): ", type.GetReadableFullName()));
			if (!type.IsTypeofGenericTypeDefinition(RegisteredType))
			{
				throw new ArgumentException(String.Concat("Invalid type; must be derived from: ",
																									RegisteredType.GetReadableFullName()));
			}

			Thread.MemoryBarrier();
			var current = _current;
			Thread.MemoryBarrier();
			IGenericTypeRegistration res = new GenericTypeRegistration(this.Container, this.RegisteredType, type);
			CheckedSetRegistration(res, current);
			return res;
		}

		public IResolver UntypedResolver
		{
			get { throw new NotImplementedException(); }
		}

		public IResolver<T> ResolverFor<T>()
		{
			var t = typeof(T);
			IResolver result;
			while (!_resolvers.TryGetValue(t, out result))
			{
				Thread.MemoryBarrier();
				var current = _current;
				Thread.MemoryBarrier();
				result = current.ResolverFor<T>();
				if (_resolvers.TryAdd(t, result))
				{
					break;
				}

				Util.Dispose(ref result);
			}
			return (IResolver<T>) result;
		}

		#endregion

		#region ITypeRegistryManagement Members

		public ITypeRegistry MakeCopyForContainer(IContainer container)
		{
			Thread.MemoryBarrier();
			var current = _current;
			Thread.MemoryBarrier();

			if (_named.IsValueCreated)
			{
				return new GenericTypeRegistry(container, RegisteredType, current, _named.Value.Values);
			}
			return new GenericTypeRegistry(container, this.RegisteredType, current);
		}

		#endregion
	}
}