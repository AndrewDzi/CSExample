using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace CSExample.PageObjectsPattern.Pages
{
    class HomePage : Page
    {
        public HomePage(IWebDriver driver) : base(driver) { }

        internal HomePage Open()
        {
            driver.Url = "http://localhost/litecart/en/";
            return this;
        }

        internal IWebElement GetProductsSection(string sectionName) => driver.FindElement(By.XPath($"//div[@class='box']//h3[text()='{sectionName}']/following-sibling::div"));
        internal List<IWebElement> GetProductsFromContext(IWebElement context) => context.FindElements(By.XPath($".//li[contains(@class, 'product')]//div[@class='name' and text()]//ancestor::li[contains(@class, 'product')]")).ToList();
    }
}