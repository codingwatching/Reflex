using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Enums;
using Reflex.Extensions;
using Reflex.Generics;
using Reflex.Resolvers;

namespace Reflex.Core
{
    public sealed class ContainerBuilder
    {
        public string Name { get; private set; }
        public Container Parent { get; private set; }
        public List<Binding> Bindings { get; } = new();
        public event Action<Container> OnContainerBuilt;

        public Container Build()
        {
            var disposables = new DisposableCollection();
            var resolversByContract = new Dictionary<Type, List<IResolver>>();

            // Inherited resolvers
            if (Parent != null)
            {
                foreach (var kvp in Parent.ResolversByContract)
                {
                    resolversByContract[kvp.Key] = kvp.Value.ToList();
                }
            }

            // Owned resolvers
            foreach (var binding in Bindings)
            {
                disposables.Add(binding.Resolver);

                foreach (var contract in binding.Contracts)
                {
                    if (!resolversByContract.TryGetValue(contract, out var resolvers))
                    {
                        resolvers = new List<IResolver>();
                        resolversByContract.Add(contract, resolvers);
                    }

                    resolvers.Add(binding.Resolver);
                }
            }

            var container = new Container(Name, Parent, resolversByContract, disposables);
            OnContainerBuilt?.Invoke(container);
            return container;
        }

        public ContainerBuilder SetName(string name)
        {
            Name = name;
            return this;
        }
        
        public ContainerBuilder SetParent(Container parent)
        {
            Parent = parent;
            return this;
        }

        public ContainerBuilder AddSingleton(Type concrete, Type[] contracts)
        {
            return Add(concrete, contracts, new SingletonTypeResolver(concrete));
        }

        public ContainerBuilder AddSingleton(Type concrete)
        {
            return AddSingleton(concrete, new[] { concrete });
        }

        public ContainerBuilder AddSingleton(object instance, Type[] contracts)
        {
            return Add(instance.GetType(), contracts, new SingletonValueResolver(instance));
        }

        public ContainerBuilder AddSingleton(object instance)
        {
            return AddSingleton(instance, new[] { instance.GetType() });
        }

        public ContainerBuilder AddSingleton<T>(Func<Container, T> factory, Type[] contracts)
        {
            var resolver = new SingletonFactoryResolver(Proxy);
            return Add(typeof(T), contracts, resolver);

            object Proxy(Container container)
            {
                return factory.Invoke(container);
            }
        }

        public ContainerBuilder AddSingleton<T>(Func<Container, T> factory)
        {
            return AddSingleton(factory, new[] { typeof(T) });
        }

        public ContainerBuilder AddTransient(Type concrete, Type[] contracts)
        {
            return Add(concrete, contracts, new TransientTypeResolver(concrete));
        }

        public ContainerBuilder AddTransient(Type concrete)
        {
            return AddTransient(concrete, new[] { concrete });
        }

        public ContainerBuilder AddTransient<T>(Func<Container, T> factory, Type[] contracts)
        {
            var resolver = new TransientFactoryResolver(Proxy);
            return Add(typeof(T), contracts, resolver);

            object Proxy(Container container)
            {
                return factory.Invoke(container);
            }
        }

        public ContainerBuilder AddTransient<T>(Func<Container, T> factory)
        {
            return AddTransient(factory, new[] { typeof(T) });
        }
        
        public ContainerBuilder AddScoped(Type concrete, Type[] contracts)
        {
            return Add(concrete, contracts, new ScopedTypeResolver(concrete));
        }

        public ContainerBuilder AddScoped(Type concrete)
        {
            return AddScoped(concrete, new[] { concrete });
        }

        public ContainerBuilder AddScoped<T>(Func<Container, T> factory, Type[] contracts)
        {
            var resolver = new ScopedFactoryResolver(Proxy);
            return Add(typeof(T), contracts, resolver);

            object Proxy(Container container)
            {
                return factory.Invoke(container);
            }
        }

        public ContainerBuilder AddScoped<T>(Func<Container, T> factory)
        {
            return AddScoped(factory, new[] { typeof(T) });
        }

        public bool HasBinding(Type type)
        {
            return Bindings.Any(binding => binding.Contracts.Contains(type));
        }

        private ContainerBuilder Add(Type concrete, Type[] contracts, IResolver resolver)
        {
            var binding = Binding.Validated(resolver, concrete, contracts);
            Bindings.Add(binding);
            return this;
        }
    }
}