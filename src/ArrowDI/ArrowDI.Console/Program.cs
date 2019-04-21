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
        [Arrow(Aura = "Fuga2")]
        public IFuga Fuga { get; set; }

        [Arrow(Aura = "Fuga")]
        public IFuga Fuga2 { get; set; }
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
            // create instance
            var quiver = new Quiver();
            
            // register objects
            quiver.Push<IHoge, Hoge>();
            quiver.Push<IFuga, Fuga>(50);
            
            // relate objects
            //   [syntax] <from, to>
            quiver.Bind<IFuga, IHoge>("Fuga");
            
            // take the instance
            var v = quiver.Pull<IHoge>() as Hoge;
            System.Console.WriteLine(v.Fuga.Value);

            System.Console.WriteLine("--- end ---");
            System.Console.ReadKey();
        }
    }
}
