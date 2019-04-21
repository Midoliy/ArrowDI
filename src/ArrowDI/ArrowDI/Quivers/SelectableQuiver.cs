using System;
using System.Collections.Generic;
using System.Text;

namespace ArrowDI
{
    public class SelectableQuiver
    {
        private readonly Dictionary<Type, List<QuiverContainer>> _storage;

        public SelectableQuiver() => _storage = new Dictionary<Type, List<QuiverContainer>>();

        private class QuiverContainer
        {
            public string Name { get; }
            public object Instance { get; }
            public QuiverContainer(string name, object instance) => (Name, Instance) = (name, instance);
        }
    }
}
