using System;
using System.Collections.Generic;
using System.Reflection;

namespace TowerBreak.DI
{
    public sealed class DIContainer
    {
        private static readonly List<DIContainer> Containers = new();

        private readonly Dictionary<RegistrationKey, object> registrations = new();

        public static int RegisteredContainerCount => Containers.Count;

        public void Register<T>(T instance, string key = null)
        {
            Register(typeof(T), instance, key);
        }

        public void Register(Type type, object instance, string key = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!type.IsInstanceOfType(instance))
            {
                throw new ArgumentException($"Instance of type {instance.GetType().FullName} is not assignable to {type.FullName}.", nameof(instance));
            }

            registrations[new RegistrationKey(type, key)] = instance;
        }

        public T Resolve<T>(string key = null)
        {
            return (T)Resolve(typeof(T), key);
        }

        public object Resolve(Type type, string key = null)
        {
            if (!TryResolve(type, out object instance, key))
            {
                throw new InvalidOperationException($"No registration found for type '{type.FullName}' and key '{key ?? string.Empty}'.");
            }

            return instance;
        }

        public bool TryResolve<T>(out T instance, string key = null)
        {
            if (TryResolve(typeof(T), out object resolved, key))
            {
                instance = (T)resolved;
                return true;
            }

            instance = default;
            return false;
        }

        public bool TryResolve(Type type, out object instance, string key = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return registrations.TryGetValue(new RegistrationKey(type, key), out instance);
        }

        public static void AddContainer(DIContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            Containers.Add(container);
        }

        public static bool RemoveContainer(DIContainer container)
        {
            if (container == null)
            {
                return false;
            }

            for (int index = Containers.Count - 1; index >= 0; index--)
            {
                if (ReferenceEquals(Containers[index], container))
                {
                    Containers.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        public static T ResolveFromRegistered<T>(string key = null)
        {
            return (T)ResolveFromRegistered(typeof(T), key);
        }

        public static object ResolveFromRegistered(Type type, string key = null)
        {
            if (!TryResolveFromRegistered(type, out object instance, key))
            {
                throw new InvalidOperationException($"No registration found in registered containers for type '{type.FullName}' and key '{key ?? string.Empty}'.");
            }

            return instance;
        }

        public static bool TryResolveFromRegistered<T>(out T instance, string key = null)
        {
            if (TryResolveFromRegistered(typeof(T), out object resolved, key))
            {
                instance = (T)resolved;
                return true;
            }

            instance = default;
            return false;
        }

        public static bool TryResolveFromRegistered(Type type, out object instance, string key = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            for (int index = Containers.Count - 1; index >= 0; index--)
            {
                if (Containers[index].TryResolve(type, out instance, key))
                {
                    return true;
                }
            }

            instance = null;
            return false;
        }

        public static void Inject(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Type type = target.GetType();
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (FieldInfo field in type.GetFields(Flags))
            {
                DIInjectAttribute attribute = field.GetCustomAttribute<DIInjectAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                object dependency = ResolveFromRegistered(field.FieldType, attribute.Key);
                field.SetValue(target, dependency);
            }

            foreach (PropertyInfo property in type.GetProperties(Flags))
            {
                DIInjectAttribute attribute = property.GetCustomAttribute<DIInjectAttribute>();
                if (attribute == null || !property.CanWrite || property.SetMethod == null)
                {
                    continue;
                }

                object dependency = ResolveFromRegistered(property.PropertyType, attribute.Key);
                property.SetValue(target, dependency);
            }
        }

        private readonly struct RegistrationKey : IEquatable<RegistrationKey>
        {
            public RegistrationKey(Type type, string key)
            {
                Type = type;
                Key = key ?? string.Empty;
            }

            private Type Type { get; }

            private string Key { get; }

            public bool Equals(RegistrationKey other)
            {
                return Type == other.Type && Key == other.Key;
            }

            public override bool Equals(object obj)
            {
                return obj is RegistrationKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Type, Key);
            }
        }
    }
}
