using NUnit.Framework;
using CSExample.PageObjectsPattern.Application;

namespace CSExample.PageObjectsPattern.Tests
{
    class TestBase
    {
        public LiteCartApp app;

        [SetUp]
        public void start()
        {
            app = new LiteCartApp();
        }

        [TearDown]
        public void stop()
        {
            app.Quit();
            app = null;
        }
    }
}