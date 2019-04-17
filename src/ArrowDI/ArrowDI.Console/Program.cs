using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrowDI.Console
{
    public interface IHoge
    {
        IFuga Fuga { get; set; }
    }

    public interface IFuga
    {
        int Value { get; }
    }

    public class Hoge : IHoge
    {
        public IFuga Fuga { get; set; }
    }

    public class Fuga : IFuga
    {
        public int Value { get; }
        public Fuga() : this(10) { }
        public Fuga(int i) => Value = i;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var quiver = new Quiver();

            quiver.Push<IHoge, Hoge>();
            quiver.Push<IFuga, Fuga>(50);

            quiver.Bind<IFuga, IHoge>();

            var v = quiver.Pull<IHoge>();
            System.Console.WriteLine(v.Fuga.Value);


            var ls = new List<object>();
            ls.Add(10);
            ls.Add(1.5);
            ls.Add("test");
            foreach (var item in ls)
            {
                System.Console.WriteLine(item.GetType());
            }





            System.Console.WriteLine("--- end ---");
            System.Console.ReadKey();
        }
    }
}
