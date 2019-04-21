using System;
using System.Collections.Generic;
using System.Linq;

namespace ArrowDI
{
    public class Quiver
    {
        private static Quiver Shared { get; }
        private readonly Dictionary<Type, object> _storage;

        static Quiver() => Shared = new Quiver();
        public Quiver() => _storage = new Dictionary<Type, object>();

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

            var instance = Activator.CreateInstance(typeof(TImplements), parameters);

            if (_storage.TryGetValue(key, out object _))
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
            return _storage.TryGetValue(typeof(TInterface), out object value)
                ? (TInterface)value
                : default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFromInterface"></typeparam>
        /// <typeparam name="TToInterface"></typeparam>
        /// <returns></returns>
        public bool Bind<TFromInterface, TToInterface>(string aura = "")
        {
            var fromIF = typeof(TFromInterface);
            var toIF = typeof(TToInterface);

            if (!fromIF.IsInterface)
                throw new InvalidCastException($"{fromIF} is not interface.");

            if (!toIF.IsInterface)
                throw new InvalidCastException($"{toIF} is not interface.");

            var p = toIF.GetProperties()
                        .Where(t => t.PropertyType == fromIF)
                        .FirstOrDefault();

            if (p == default)
                return false;

            if (!_storage.TryGetValue(toIF, out object to))
                return false;

            if (!_storage.TryGetValue(fromIF, out object from))
                return false;

            var properties = to.GetType()
                               .GetProperties()
                               .Where(t => t.PropertyType == fromIF)
                               .ToArray();
            
            foreach (var property in properties)
            {
                var attr = (ArrowAttribute)Attribute.GetCustomAttribute(property, typeof(ArrowAttribute));
                if (attr == null)
                    continue;

                if (attr.Aura != aura)
                    continue;

                property.SetValue(to, from);
                return true;
            }

            var prop = properties.First();
            if (!prop.CanWrite)
                return false;

            prop.SetValue(to, from);

            return true;
        }
    }
}
