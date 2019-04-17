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
            // create instance
            var quiver = new Quiver();
            
            // register objects
            quiver.Push<IHoge, Hoge>();
            quiver.Push<IFuga, Fuga>(50);
            
            // relate objects
            //   [syntax] <from, to>
            quiver.Bind<IFuga, IHoge>();
            
            // take the instance
            var v = quiver.Pull<IHoge>();
            System.Console.WriteLine(v.Fuga.Value);

            System.Console.WriteLine("--- end ---");
            System.Console.ReadKey();
        }
    }
}
