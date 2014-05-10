# fbioc - FlitBit.IoC

An inversion of control container with constructor injection and synergy with dynamically emitted types.

## Why

The _FlitBit Frameworks_ rely heavily on meta-programming. In particular, several of the frameworks dynamically [emit entire implementations at runtime](https://github.com/flitbit-org/fbemit) from metadata, attributes, and descriptions of stereotypical behavior.

It was difficult hooking my [prior favorite IoC](https://github.com/autofac/Autofac) in a satisfactory way for my \[late-late-bound, dynamically emitted\] needs. Many implementations are simply not available or predictable at startup time, which is when most IoC containers must be constructed and made whole (ie. everything registered).

Furthermore, I am of the opinion that [an application's wireup activity (bootstrap)](https://github.com/flitbit-org/fbwireup) is a separate concern from inversion of control. I wanted an inversion of control container that was dynamic enough to be reconfigured throughout the life of an application, not just during the bootstrap, while still performing well in the object-creation cycle.

## How

Inversion of control containers serve primarily as a mechanism to acquire constructed instances of replaceable implementations. This description \[my own\] implies that you go to the container for new instances rather than newing them up in code. Indeed, this is our **strong** recommendation. Toward that end, `FlitBIt.IoC` has a few helper classes that makes creating things feel natural and makes semantic sense.

```csharp
var it = Create.New<WhatItIs>();
```

The above statement creates an instance of `WhatItIs`. `WhatItIs` may be any class, or any interface, provided that if it is abstract, a preferred implementation can be determined by the container.

In the vast majority of your code, this is the only call you'll make.

### Inside `Create.New<T>()`

`Create.New<T>()` is a convenience \[shortcut\] that uses the _ambient container_ to create the _**nearest**_ registered implementation.

To visualize what it does, consider this code:

```csharp
var container = Container.Current;
var it = container.New<WhatItIs>(LifespanTracking.External);
```

In the above code, the ambient container is accessed using the static `Container.Current` property. `FlitBit.IoC` supports nested container scopes. If the calling code doesn't create a child container then the ambient container corresponds to the `Container.Root` which is constructed and initialized at wireup time.

### Nested Containers

An alternative to using `Create.New<T>()` would be to work within a child container like the following:

```csharp
using(var create = Create.NewContainer())
{
  var it = create.New<WhatItIs>();

  // do something with it
}
```

By creating a child container, we establish a new container scope and the call to `New<T>()` now tracks the lifespan of the instance that it returns, ensuring that if it _is-a_ `IDisposable` it's `Dispose` method is called when the child container is disposed. This `lifespan tracking` is convenient for many code patterns.

Child containers inherit the registrations of the outer container, and if new registrations are made on child containers those registrations are valid only within the scope of the child. This is what is meant by _**nearest**_.

### Registering Implementations for Replaceable Types

Many of the _FlitBit Frameworks_ register their own implementations with the IoC container. This is nearly always true of dynamically emitted implementations. However, user supplied types will need to be registered in user code. The recommended way to register an implementation is by either annotating it with a `ContainerRegisteredAttribute` or explictly adding it to the ambient container's registry.

The sections below will refer to these types:

```csharp
public interface IBeNamed
{
  string Name { get; }
}

[ContainerRegister(typeof(IBeNamed), RegistrationBehaviors.Default)]
public class A : IBeNamed
{
	public A() { this.Name = this.GetType().Name; }
	public string Name { get; protected set; }
}

public class B : IBeNamed
{
	public B() { this.Name = this.GetType().Name; }
	public string Name { get; protected set; }
}

```

#### Implicit Registration via `ContainerRegistered`

If there is only one, default implementation of an abstract type, the type can be annotated with the `ContainerRegisterAttribute` like the class `A` in the example above. The wireup process will discover these attributes and automatically register those types with the root container. There is nothing more to do than use `Create.New<IBeNamed>()` when you need one.

#### Explicit Registration using the Registry

If you need to make a later decision about which implementation type will be registered, you can use explicit registration. For instance, the following code replaces the implicit registration of type `A` with type `B`:

```csharp
Container.Root.Registry
  .ForType<IBeNamed>()
  .Register<B>()
  .End();
```

Following this explicit registration, calls to `Create.New<IBeNamed>()` will result in an instance of `B` instead of an instance of `A`.

## More

So far I've only covered the most basic stuff.

```
// TODO: Add more documentation
```

I encourage you to take a look at the [unit tests](https://github.com/flitbit-org/fbioc/tree/master/FlitBit.IoC/FlitBit.IoC.Tests) for much more insightful stuff about this IoC.