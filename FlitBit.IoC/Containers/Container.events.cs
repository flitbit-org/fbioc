#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.IoC.Containers
{
	internal partial class Container
	{
		readonly Lazy<Dictionary<Type, IObservationKey>> _observers =
			new Lazy<Dictionary<Type, IObservationKey>>(LazyThreadSafetyMode.PublicationOnly);

		#region IContainer Members

		public void Subscribe<T>(Action<Type, T, string, CreationEventKind> observer)
		{
			var observers = _observers.Value;
			IObservationKey key;
			if (!observers.TryGetValue(typeof(T), out key))
			{
				key = new ObservationKey<T>(_options.HasFlag(CreationContextOptions.InstanceTracking));
				observers.Add(typeof(T), key);
			}
			key.AddObserver(observer);
		}

		public void NotifyObserversOfCreationEvent<T>(Type requestedType, T instance, string name, CreationEventKind evt)
		{
			var observers = _observers.Value;
			IObservationKey key;
			if (observers.TryGetValue(typeof(T), out key))
			{
				var okey = key as ObservationKey<T>;
				if (okey != null)
				{
					okey.NotifyObservers(requestedType, instance, name, evt);
				}
			}
			if (_parent != null)
			{
				_parent.NotifyObserversOfCreationEvent(requestedType, instance, name, evt);
			}
		}

		#endregion

		interface IObservationKey
		{
			Type TargetType { get; }
			void AddObserver(Delegate observer);
		}

		class ObservationKey<T> : IObservationKey
		{
			readonly object _innerLock = new Object();

			readonly List<Action<Type, T, string, CreationEventKind>> _observers =
				new List<Action<Type, T, string, CreationEventKind>>();

			readonly List<Core.WeakReference<T>> _instances;

			public ObservationKey(bool tracking)
			{
				if (tracking)
				{
          _instances = new List<Core.WeakReference<T>>();
				}
			}

			internal void NotifyObservers(Type requestedType, T instance, string name, CreationEventKind evt)
			{
				if (_instances != null)
				{
          _instances.Add(new Core.WeakReference<T>(instance));
				}
				foreach (var observer in _observers)
				{
					observer(requestedType, instance, name, evt);
				}
			}

			#region IObservationKey Members

			public Type TargetType
			{
				get { return typeof(T); }
			}

			public void AddObserver(Delegate observer)
			{
				lock (_innerLock)
				{
					_observers.Add((Action<Type, T, string, CreationEventKind>) observer);
				}
			}

			#endregion
		}
	}
}