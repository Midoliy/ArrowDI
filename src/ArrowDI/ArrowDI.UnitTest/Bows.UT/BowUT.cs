using System;
using Xunit;

namespace ArrowDI.UnitTest
{
    public class BowUT
    {
        [Fact]
        public void PrepareTest()
        {
            var bow = new Bow();
            bow.Prepare<IHoge, Hoge>();
        }

        [Fact]
        public void FireTest()
        {
            
        }

        [Fact]
        public void BindTest()
        {

        }
    }

    public interface IHoge
    {

    }

    public class Hoge : IHoge
    {

    }
}
