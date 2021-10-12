using System.Collections.Generic;
using OpenQA.Selenium;

namespace CSExample.PageObjectsPattern.Pages
{
    class CartPage : Page
    {
        public CartPage(IWebDriver driver) : base(driver) { }

        internal By RemoveProductButtonLocator() => GetElementLocatorByTypeAndName("button", "remove_cart_item");

        internal IWebElement ProductShortcut() => driver.FindElement(By.XPath("//ul[@class='shortcuts']//a"));
        internal IList<IWebElement> ActualNumberOfItems() => driver.FindElements(By.XPath("//*[@id='box-checkout-summary']//tr//td[@class='sku']"));
        internal IWebElement RemoveProductButton() => driver.FindElement(RemoveProductButtonLocator());
    }
}
