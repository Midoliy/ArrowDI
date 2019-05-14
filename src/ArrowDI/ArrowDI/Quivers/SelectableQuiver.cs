using ArrowDI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowDI
{
    public class SelectableQuiver
    {
        public static SelectableQuiver Shared { get; }

        private readonly Dictionary<Type, Dictionary<string, Container>> _storage;

        static SelectableQuiver() => Shared = new SelectableQuiver();
        public SelectableQuiver() => _storage = new Dictionary<Type, Dictionary<string, Container>>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplements"></typeparam>
        /// <param name="aura"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string Push<TInterface, TImplements>(string aura = "", params object[] parameters)
            where TImplements : TInterface
        {
            if (aura == null)
                throw new ArgumentNullException(nameof(aura));

            var key = typeof(TInterface);
            if (!key.IsInterface)
                throw new InvalidCastException($"{key} is not interface.");

            // 対象のコンテナが存在しない場合, コンテナを追加する.
            if (!_storage.TryGetValue(key, out Dictionary<string, Container> _))
                _storage.Add(key, new Dictionary<string, Container>());

            var containers = _storage[key];

            // auraが空文字の場合, Arrow属性を検索し, Nameプロパティ取得/設定する.
            if (aura == string.Empty)
            {
                if (Attribute.GetCustomAttribute(typeof(TImplements), typeof(ArrowAttribute)) is ArrowAttribute attr)
                    aura = attr.Aura;
            }

            // ストレージに保管するインスタンスを生成.
            var instance = new Lazy<object>(() => Activator.CreateInstance(typeof(TImplements), parameters));

            if (containers.TryGetValue(aura, out Container _))
                // コンテナリストの中に同一の名前で登録されているコンテナがある場合, 上書きする.
                containers[aura] = new Container(instance);
            else
                // コンテナを新規登録する.
                _storage[key].Add(aura, new Container(instance));

            return aura;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public TInterface Select<TInterface>(string aura = "")
        {
            if (aura == null)
                throw new ArgumentNullException(nameof(aura));

            var key = typeof(TInterface);
            if (!key.IsInterface)
                throw new InvalidCastException($"{key} is not interface.");

            if (!_storage.TryGetValue(key, out Dictionary<string, Container> containers))
                return default;

            if (!containers.TryGetValue(aura, out Container container))
                return default;

            foreach (var opt in container.Options)
                opt?.Invoke();

            return (TInterface)container.Instance.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFromInterface"></typeparam>
        /// <typeparam name="TToInterface"></typeparam>
        /// <param name="fromAura"></param>
        /// <param name="toAura"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Bind<TFromInterface, TToInterface>(string fromAura, string toAura, string name = "")
        {
            if (fromAura == null)
                throw new ArgumentNullException(nameof(fromAura));

            if (toAura == null)
                throw new ArgumentNullException(nameof(toAura));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var fromKey = typeof(TFromInterface);
            if (!fromKey.IsInterface)
                throw new InvalidCastException($"{fromKey} is not interface.");

            var toKey = typeof(TToInterface);
            if (!toKey.IsInterface)
                throw new InvalidCastException($"{toKey} is not interface.");

            if (!_storage.TryGetValue(fromKey, out Dictionary<string, Container> fromContainer))
                throw new NotFoundException($"{fromKey}");

            if (!_storage.TryGetValue(toKey, out Dictionary<string, Container> toContainer))
                throw new NotFoundException($"{toKey}");

            if (!fromContainer.TryGetValue(fromAura, out Container from))
                throw new NotFoundException($"{fromAura}");

            if (!toContainer.TryGetValue(toAura, out Container to))
                throw new NotFoundException($"{toAura}");

            to.Options.Add(() =>
            {
                var properties = to.Instance
                                       .Value
                                       .GetType()
                                       .GetProperties()
                                       .Where(t => t.PropertyType == fromKey);

                var property = string.IsNullOrEmpty(name)
                                   ? properties.First()
                                   : properties.SelectPropetyOrDefault(name);

                if (property == default)
                    throw new NotFoundException($"No property found with ArrowheadAttribute(Name= {name}).");

                if (!property.CanWrite)
                    throw new FieldAccessException($"{property.Name} cannot be writeable.");

                property.SetValue(to.Instance.Value, from.Instance.Value);
            });

            return true;
        }

        private class Container
        {
            public Lazy<object> Instance { get; }
            public List<Action> Options { get; }

            public Container(Lazy<object> instance) =>
                (Instance, Options) = (instance, new List<Action>());
        }
    }
}
