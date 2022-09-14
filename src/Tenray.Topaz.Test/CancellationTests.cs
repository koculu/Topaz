using NUnit.Framework;
using System;
using System.Threading;

namespace Tenray.Topaz.Test
{
    public sealed class CancellationTests
    {
        [Test]
        public void CancelInfiniteWhileLoop()
        {
            var engine = new TopazEngine();
            var thrown = 0;
            Func<CancellationTokenSource> getSource = 
                () => new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
            
            try
            {
                using var source = getSource();
                engine.ExecuteScript(@"
while(true) { }
",
source.Token);
            }
            catch (OperationCanceledException)
            {
                ++thrown;
            }

            try
            {
                using var source = getSource();
                engine.ExecuteScript(@"
while(true);
",
source.Token);
            }
            catch (OperationCanceledException)
            {
                ++thrown;
            }

            Assert.AreEqual(thrown, 2);
        }
    }
}