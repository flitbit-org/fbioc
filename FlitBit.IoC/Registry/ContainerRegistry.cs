#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using FlitBit.Core;

namespace FlitBit.IoC.Registry
{
	internal sealed class ContainerRegistry : ContainerOwned, IContainerRegistry
	{
		readonly ConcurrentDictionary<Type, ITypeRegistry> _registrations = new ConcurrentDictionary<Type, ITypeRegistry>();

		internal ContainerRegistry(IContainer container)
			: base(container) { }

		internal ContainerRegistry(IContainer container, IContainerRegistry baseRegistry)
			: base(container) { BaseRegistry = baseRegistry; }

		IContainerRegistry BaseRegistry { get; set; }

		protected override bool PerformDispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var reg in _registrations.Values)
				{
					var r = reg;
					Util.Dispose(ref r);
				}
				return true;
			}
			return false;
		}

		TTypeRegistry AddOrGetTypeRegistry<TTypeRegistry>(Type type, Func<IContainer, TTypeRegistry> factory)
			where TTypeRegistry : ITypeRegistry
		{
			ITypeRegistry r;
			do
			{
				ITypeRegistry value;
				if (_registrations.TryGetValue(type, out value))
				{
					r = value;
				}
				else
				{
					var underlying = BaseRegistry;
					ITypeRegistryManagement mgt;
					if (underlying != null && underlying.TryGetTypeRegistryManagement(type, out mgt))
					{
						r = mgt.MakeCopyForContainer(Container);
					}
					else
					{
						r = factory(Container);
					}
					if (!_registrations.TryAdd(type, r))
					{
						Util.Dispose(ref r);
					}
				}
			} while (r == null);

			return (TTypeRegistry) r;
		}

		#region IContainerRegistry Members

		public ITypeRegistry<T> ForType<T>() { return AddOrGetTypeRegistry(typeof(T), c => new TypeRegistry<T>(c)); }

		public IGenericTypeRegistry ForGenericType(Type generic)
		{
			if (generic == null)
			{
				throw new ArgumentNullException("generic");
			}
			if (!generic.IsGenericTypeDefinition)
			{
				throw new ArgumentException(
					String.Concat("Expected a generic type definition, received: ", generic.GetReadableFullName()), "generic");
			}

			return AddOrGetTypeRegistry(generic, c => new GenericTypeRegistry(c, generic));
		}

		public bool IsTypeRegistered<T>()
		{
			return _registrations.ContainsKey(typeof(T))
				|| (BaseRegistry != null && BaseRegistry.IsTypeRegistered(typeof(T)));
		}

		public bool IsTypeRegistered(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return _registrations.ContainsKey(type)
				|| (BaseRegistry != null && BaseRegistry.IsTypeRegistered(type));
		}

		public bool TryGetResolverForType(Type type, out IResolver value)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			ITypeRegistry temp;
			var gotItHere = _registrations.TryGetValue(type, out temp);
			if (gotItHere)
			{
				value = temp.UntypedResolver;
				return true;
			}
			if (this.BaseRegistry != null)
			{
				return this.BaseRegistry.TryGetResolverForType(type, out value);
			}
			value = null;
			return false;
		}

		public bool TryGetResolverForType<T>(out IResolver<T> value)
		{
			ITypeRegistry temp;
			var gotItHere = _registrations.TryGetValue(typeof(T), out temp);
			if (gotItHere)
			{
				value = ((ITypeRegistry<T>) temp).Resolver;
				return true;
			}
			if (this.BaseRegistry != null)
			{
				return this.BaseRegistry.TryGetResolverForType(out value);
			}
			value = null;
			return false;
		}

		public bool TryGetNamedResolverForType<T>(string name, out IResolver<T> value)
		{
			ITypeRegistry r;
			var gotItHere = _registrations.TryGetValue(typeof(T), out r);
			if (gotItHere)
			{
				return ((ITypeRegistry<T>) r).TryGetNamedResolver(name, out value);
			}
			if (this.BaseRegistry != null)
			{
				return this.BaseRegistry.TryGetNamedResolverForType(name, out value);
			}
			value = null;
			return false;
		}

		public bool TryGetTypeRegistryManagement(Type type, out ITypeRegistryManagement value)
		{
			ITypeRegistry temp;
			var gotItHere = _registrations.TryGetValue(type, out temp);
			if (gotItHere)
			{
				value = temp as ITypeRegistryManagement;
				return value != null;
			}
			if (this.BaseRegistry != null)
			{
				return this.BaseRegistry.TryGetTypeRegistryManagement(type, out value);
			}
			value = null;
			return false;
		}

		#endregion
	}
}