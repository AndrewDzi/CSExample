using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CSExample.LitecartTests
{
    [TestFixture]
    class LoginScenario
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        [SetUp]
        public void start()
        {
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void LoginToLitecart()
        {
            var login = "admin";
            var password = login;

            driver.Url = "http://localhost/litecart/admin/";

            IWebElement element = wait.Until(d => d.FindElement(GetElementByTypeAndName("input", "username")));
            
            driver.FindElement(GetElementByTypeAndName("input" ,"username")).SendKeys(login);
            driver.FindElement(GetElementByTypeAndName("input", "password")).SendKeys(password);
            driver.FindElement(GetElementByTypeAndName("button", "login")).Click();

            wait.Until(ExpectedConditions.ElementExists(By.XPath(".//div[@class='notice success']")));
        }

        // Element types are: input, button.  
        public By GetElementByTypeAndName(string elementType, string buttonName)
        {
            return By.XPath($".//{elementType}[@name='{buttonName}']");
        }

        [TearDown]
        public void stop()
        {
            driver.Quit();
            driver = null;
        }
    }
}