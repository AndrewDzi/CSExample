using NUnit.Framework;

namespace CSExample.PageObjectsPattern.Tests
{
    [TestFixture]
    class CartTests : TestBase
    {
        [Test]
        public void CheckThatUserCanAddAndRemoveProductsToCart()
        {
            app.AddFirstProductOnHomePageToCart(3);
            app.NavigateToCart();
            app.RemoveAllProductsFromCart();
        }
    }
}