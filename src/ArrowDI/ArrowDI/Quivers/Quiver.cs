using System;
using System.Collections.Generic;
using System.Linq;

namespace ArrowDI
{
    public class Quiver
    {
        public static Quiver Shared { get; }
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
            if (!typeof(TInterface).IsInterface)
                throw new InvalidCastException($"{typeof(TInterface)} is not interface.");

            var instance = Activator.CreateInstance(typeof(TImplements), parameters);

            if (_storage.TryGetValue(typeof(TInterface), out object _))
                _storage[typeof(TInterface)] = instance;
            else
                _storage.Add(typeof(TInterface), instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public TInterface Pull<TInterface>()
        {
            if (!typeof(TInterface).IsInterface)
                throw new InvalidCastException($"{typeof(TInterface)} is not interface.");

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
        public void Bind<TFromInterface, TToInterface>(string name = "")
        {
            var fromIF = typeof(TFromInterface);
            var toIF = typeof(TToInterface);

            if (!fromIF.IsInterface)
                throw new InvalidCastException($"{fromIF} is not interface.");

            if (!toIF.IsInterface)
                throw new InvalidCastException($"{toIF} is not interface.");

            var p = toIF
                      .GetProperties()
                      .Where(t => t.PropertyType == fromIF)
                      .FirstOrDefault();

            if (p == default)
                throw new UndefinedPropertyException($"{fromIF}");

            if (!_storage.TryGetValue(fromIF, out object from))
                throw new NotFoundException($"{fromIF}");

            if (!_storage.TryGetValue(toIF, out object to))
                throw new NotFoundException($"{toIF}");

            var properties = to
                              .GetType()
                              .GetProperties()
                              .Where(t => t.PropertyType == fromIF);

            // [Arrowheadの指定があった場合にのみ実行]
            //      全プロパティの属性をチェックし, 指定されたauraと一致するプロパティにバインドする.
            if (!string.IsNullOrEmpty(name))
                foreach (var property in properties)
                {
                    var arrowhead = (ArrowheadAttribute)Attribute.GetCustomAttribute(property, typeof(ArrowheadAttribute));
                    if (arrowhead?.Name != name)
                        continue;

                    property.SetValue(to, from);
                    return;
                }

            // [auraの指定がなかった場合 or 指定したauraが見つからなかった場合に実行]
            //      一番最初に見つけたプロパティにバインドする.
            var prop = properties.FirstOrDefault();
            if (!(prop?.CanWrite ?? false))
                throw new UndefinedPropertyException();

            prop.SetValue(to, from);
        }
    }
}
