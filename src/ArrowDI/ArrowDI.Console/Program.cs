using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrowDI.Console
{
    public interface IHoge
    {
        IFuga Fuga { get; }
        IPiyo Piyo { get; }
    }

    public interface IFuga
    {
        int Value { get; }
    }

    public interface IPiyo
    {
        string Value { get; }
    }

    [Arrow(Aura = "Name1")]
    public class Hoge : IHoge
    {
        [Arrowhead(Name = "Fuga")]
        public IFuga Fuga { get; set; }

        [Arrowhead(Name = "Fuga2")]
        public IFuga Fuga2 { get; set; }

        public IPiyo Piyo { get; set; }
    }

    [Arrow(Aura = "Name1")]
    public class Fuga : IFuga
    {
        public int Value { get; }
        public Fuga() : this(10) { }
        public Fuga(int i) => Value = i;
    }

    [Arrow(Aura = "Name1")]
    public class Fuga2 : IFuga
    {
        public int Value { get; }
        public Fuga2() : this(10) { }
        public Fuga2(int i) => Value = i;
    }

    public class Piyo : IPiyo
    {
        public string Value => "string value.";
    }

    class Program
    {
        static void Main(string[] args)
        {
            //// create instance
            //var quiver = LazyQuiver.Shared;
            
            // register objects
            LazyQuiver.Shared.Push<IHoge, Hoge>();
            LazyQuiver.Shared.Push<IFuga, Fuga>(50);
            LazyQuiver.Shared.Push<IPiyo, Piyo>();

            // relate objects
            //   [syntax] <from, to>
            LazyQuiver.Shared.Bind<IFuga, IHoge>("Fuga");
            LazyQuiver.Shared.Bind<IPiyo, IHoge>();

            // take the instance
            var v = LazyQuiver.Shared.Pull<IHoge>() as Hoge;
            System.Console.WriteLine(v.Fuga.Value);
            System.Console.WriteLine(v.Piyo.Value);


            System.Console.WriteLine("--- --- ---");


            var h1 = SelectableQuiver.Shared.Push<IHoge, Hoge>();

            var f1 = SelectableQuiver.Shared.Push<IFuga, Fuga>(arrowName:"", 50);
            var f2 = SelectableQuiver.Shared.Push<IFuga, Fuga2>(arrowName:"IFuga2");
            var f3 = SelectableQuiver.Shared.Push<IFuga, Fuga>("fuga", 50);

            SelectableQuiver.Shared.Bind<IFuga, IHoge>(f1, h1);

            var hoge1 = SelectableQuiver.Shared.Select<IHoge>(h1);
            var fuga1 = SelectableQuiver.Shared.Select<IFuga>(f2);
            var fuga2 = SelectableQuiver.Shared.Select<IFuga>(f2);

            System.Console.WriteLine(hoge1.Fuga.Value);
            System.Console.WriteLine(fuga1.Value);


            System.Console.WriteLine("--- end ---");
            System.Console.ReadKey();
        }
    }
}
