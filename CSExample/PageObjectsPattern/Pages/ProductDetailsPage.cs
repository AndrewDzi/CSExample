using System.Collections.Generic;
using OpenQA.Selenium;

namespace CSExample.PageObjectsPattern.Pages
{
    class ProductDetailsPage : Page
    {
        public ProductDetailsPage(IWebDriver driver) : base(driver) { }

        internal IWebElement GetProductSezeOption() => driver.FindElement(GetElementLocatorByTypeAndName("select", "options[Size]"));
        internal IList<IWebElement> GetProductSezeOptions() => driver.FindElements(GetElementLocatorByTypeAndName("select", "options[Size]"));
        internal IWebElement AddToCartButton() => driver.FindElement(GetElementLocatorByTypeAndName("button", "add_cart_product"));
        internal By QuantityOfItemsInCart(int i) => By.XPath($"//*[@id='cart']//span[@class='quantity' and text()='{i}']");
        internal IWebElement HomeButton() => GetElementById("site-menu").FindElement(By.XPath("//i[@title='Home']"));
    }
}