using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ArrowDI
{
    public class Bow
    {

        private readonly Dictionary<Type, Dictionary<string, Func<object>>> _storage;


        public void Prepare<TInterface, TImplements>(string aura = "")
            where TImplements : TInterface
        {
            var key = typeof(TInterface);
            if (!key.IsInterface)
                throw new InvalidCastException($"{key} is not interface.");

            if (!typeof(TImplements).GetConstructors().Where(ctor => ctor.GetParameters().Length == 0).Any())
                throw new NotSupportedException($"{typeof(TImplements)} has no default constructors.");

            if (!_storage.TryGetValue(key, out Dictionary<string, Func<object>> dict))
                _storage.Add(key, new Dictionary<string, Func<object>>());

            #warning TODO: AuraAttribute を探索する処理を追加する.

            if (dict.TryGetValue(aura, out Func<object> _))
                throw new ConflictRegistrationException(aura);

            dict.Add(aura, () => Activator.CreateInstance(typeof(TImplements)));
        }

    }
}
