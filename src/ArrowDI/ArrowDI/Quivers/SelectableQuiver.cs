﻿using ArrowDI.Extensions;
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
        /// <param name="arrowName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string Push<TInterface, TImplements>(string arrowName = "", params object[] parameters)
            where TImplements : TInterface
        {
            if (arrowName == default)
                throw new ArgumentNullException(nameof(arrowName));

            if (!typeof(TInterface).IsInterface)
                throw new InvalidCastException($"{typeof(TInterface)} is not interface.");

            // 対象のコンテナが存在しない場合, コンテナを追加する.
            if (!_storage.TryGetValue(typeof(TInterface), out Dictionary<string, Container> _))
                _storage.Add(typeof(TInterface), new Dictionary<string, Container>());

            var containers = _storage[typeof(TInterface)];

            // auraが空文字の場合, Arrow属性を検索し, Nameプロパティ取得/設定する.
            if (arrowName == string.Empty
            &&  Attribute.GetCustomAttribute(typeof(TImplements), typeof(ArrowAttribute)) is ArrowAttribute attr)
                arrowName = attr.Aura;

            // ストレージに保管するインスタンスを生成.
            var instance = new Lazy<object>(() => Activator.CreateInstance(typeof(TImplements), parameters));

            if (containers.TryGetValue(arrowName, out Container _))
                // コンテナリストの中に同一の名前で登録されているコンテナがある場合, 例外をスローする.
                throw new ConflictRegistrationException($"[{arrowName}] is already registerd.");
            else
                // コンテナを新規登録する.
                _storage[typeof(TInterface)].Add(arrowName, new Container(instance));

            return arrowName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public TInterface Select<TInterface>(string arrowName = "")
        {
            if (arrowName == default)
                throw new ArgumentNullException(nameof(arrowName));

            var key = typeof(TInterface);
            if (!key.IsInterface)
                throw new InvalidCastException($"{key} is not interface.");

            if (!_storage.TryGetValue(key, out Dictionary<string, Container> containers))
                return default;

            if (!containers.TryGetValue(arrowName, out Container container))
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
        /// <param name="arrowheadName"></param>
        /// <returns></returns>
        public void Bind<TFromInterface, TToInterface>(string fromAura, string toAura, string arrowheadName = "")
        {
            if (fromAura == default)
                throw new ArgumentNullException(nameof(fromAura));

            if (toAura == default)
                throw new ArgumentNullException(nameof(toAura));

            if (arrowheadName == default)
                throw new ArgumentNullException(nameof(arrowheadName));

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

                var property = string.IsNullOrEmpty(arrowheadName)
                                   ? properties.First()
                                   : properties.SelectPropetyOrDefault(arrowheadName);

                if (property == default)
                    throw new NotFoundException($"No property found with ArrowheadAttribute(Name= {arrowheadName}).");

                if (!property.CanWrite)
                    throw new FieldAccessException($"{property.Name} cannot be writeable.");

                property.SetValue(to.Instance.Value, from.Instance.Value);
            });
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
