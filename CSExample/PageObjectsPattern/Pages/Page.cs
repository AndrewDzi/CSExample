using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CSExample.PageObjectsPattern.Pages
{
    internal class Page
    {
        protected IWebDriver driver;
        protected WebDriverWait wait;

        public Page(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Element types are: input, button.  
        public By GetElementLocatorByTypeAndName(string elementType, string elementName)
        {
            return By.XPath($".//{elementType}[@name='{elementName}']");
        }

        public IWebElement GetElementById(string id)
        {
            return driver.FindElement(By.XPath($"//*[@id='{id}']"));
        }

        public IWebElement GetLinkByName(string name)
        {
            return driver.FindElement(By.XPath($"//a[@href and text()='{name}']"));
        }
    }
}