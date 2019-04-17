using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ArrowDI
{
    public class Quiver
    {
        private readonly Dictionary<Type, object> _storage;

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
            var instance = Activator.CreateInstance(typeof(TImplements), parameters);
            var key = typeof(TInterface);

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
        public bool Bind<TFromInterface, TToInterface>()
        {
            var fromIF = typeof(TFromInterface);
            var toIF = typeof(TToInterface);

            var property = toIF
                            .GetProperties()
                            .Where(t => t.PropertyType == fromIF)
                            .FirstOrDefault();

            if (property == default)
                return false;

            if (!property.CanWrite)
                return false;

            if (!_storage.TryGetValue(fromIF, out object from))
                return false;

            if (!_storage.TryGetValue(toIF, out object to))
                return false;

            property.SetValue(to, from);

            return true;
        }
    }
}
