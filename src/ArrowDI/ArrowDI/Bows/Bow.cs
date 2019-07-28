using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ArrowDI
{
    public class Bow
    {
        private readonly Dictionary<Type, Dictionary<string, Func<object>>> _storage;

        public void Prepare<TInterface, TImplements>(string name = "")
            where TImplements : TInterface
        {
            if (!typeof(TInterface).IsInterface)
                throw new InvalidCastException($"{typeof(TInterface)} is not interface.");

            if (!typeof(TImplements).GetConstructors().Where(ctor => ctor.GetParameters().Length == 0).Any())
                throw new NotSupportedException($"{typeof(TImplements)} has no default constructors.");

            if (!_storage.TryGetValue(typeof(TInterface), out Dictionary<string, Func<object>> _))
                _storage.Add(typeof(TInterface), new Dictionary<string, Func<object>>());

            var dict = _storage[typeof(TInterface)];

            #warning TODO: ArrowAttribute を探索する処理を追加する.


            if (dict.TryGetValue(name, out Func<object> _))
                throw new ConflictRegistrationException(name);

            dict.Add(name, () => Activator.CreateInstance(typeof(TImplements)));
        }

    }
}
