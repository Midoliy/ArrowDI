using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

namespace ArrowDI
{
    public class LazyQuiver
    {
        public static LazyQuiver Shared { get; }

        private readonly Dictionary<Type, Lazy<object>> _storage;
        private readonly Dictionary<Type, List<Action>> _options;

        static LazyQuiver() => Shared = new LazyQuiver();
        public LazyQuiver() => (_storage, _options) = (new Dictionary<Type, Lazy<object>>(),
                                                       new Dictionary<Type, List<Action>>());

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplements"></typeparam>
        /// <param name="parameters"></param>
        public void Push<TInterface, TImplements>(params object[] parameters)
            where TImplements : TInterface
        {
            var key = typeof(TInterface);
            if (!key.IsInterface)
                throw new InvalidCastException($"{key} is not interface.");

            var instance = new Lazy<object>(() => Activator.CreateInstance(typeof(TImplements), parameters));

            if (_storage.TryGetValue(key, out Lazy<object> _))
                _storage[key] = instance;
            else
                _storage.Add(key, instance);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public TInterface Pull<TInterface>()
        {
            var key = typeof(TInterface);

            if (!_storage.TryGetValue(key, out Lazy<object> value))
                return default;

            if(value.IsValueCreated)
                return (TInterface)value.Value;

            if (_options.TryGetValue(key, out List<Action> options))
                foreach (var option in options) option();

            return (TInterface)value.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFromInterface"></typeparam>
        /// <typeparam name="TToInterface"></typeparam>
        /// <returns></returns>
        public bool Bind<TFromInterface, TToInterface>(string name = "")
        {
            var fromIF = typeof(TFromInterface);
            var toIF = typeof(TToInterface);

            if (!fromIF.IsInterface)
                throw new InvalidCastException($"{fromIF} is not interface.");

            if (!toIF.IsInterface)
                throw new InvalidCastException($"{toIF} is not interface.");

            var hasTargetProperty = toIF
                                      .GetProperties()
                                      .Where(t => t.PropertyType == fromIF)
                                      .Any();

            if (!hasTargetProperty)
                return false;

            if (!_storage.TryGetValue(fromIF, out Lazy<object> from))
                return false;

            if (!_storage.TryGetValue(toIF, out Lazy<object> to))
                return false;

            if (!_options.TryGetValue(toIF, out List<Action> _))
                _options.Add(toIF, new List<Action>());

            _options[toIF].Add(() =>
            {
                var properties = to.Value
                                      .GetType()
                                      .GetProperties()
                                      .Where(t => t.PropertyType == fromIF);

                var property = string.IsNullOrEmpty(name)
                                   ? properties.First()
                                   : SelectPropetyOrDefault(properties, name);

                if (property == default)
                    throw new NotFoundException($"No property found with ArrowAttribute(Aura= {name}).");

                if (!property.CanWrite)
                    throw new FieldAccessException($"{property.Name} cannot be writeable.");

                property.SetValue(to.Value, from.Value);
            });

            return true;

            static PropertyInfo SelectPropetyOrDefault(IEnumerable<PropertyInfo> properties_, string name_)
            {
                foreach (var property_ in properties_)
                {
                    var arrowhead = (ArrowheadAttribute)Attribute.GetCustomAttribute(property_, typeof(ArrowheadAttribute));
                    if (arrowhead == null)
                        continue;

                    if (arrowhead.Name != name_)
                        continue;

                    return property_;
                }

                return default;
            }
        }
    }
}
