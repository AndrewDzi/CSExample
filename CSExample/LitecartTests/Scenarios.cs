using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Linq;

namespace CSExample.LitecartTests
{
    [TestFixture]
    class Scenarios
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        private string adminUrl = "http://localhost/litecart/admin/";
        private string storeUrl = "http://localhost/litecart/en/";

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

            driver.Url = adminUrl;

            IWebElement element = wait.Until(d => d.FindElement(GetElementLocatorByTypeAndName("input", "username")));

            driver.FindElement(GetElementLocatorByTypeAndName("input", "username")).SendKeys(login);
            driver.FindElement(GetElementLocatorByTypeAndName("input", "password")).SendKeys(password);
            driver.FindElement(GetElementLocatorByTypeAndName("button", "login")).Click();

            wait.Until(ExpectedConditions.ElementExists(By.XPath(".//div[@class='notice success']")));
        }

        [Test]
        public void CheckHeadersOfSidebarMenu()
        {
            var menuItemPath = By.XPath("//ul[@id='box-apps-menu']//*[@id='app-']");
            var sumMenuItemPath = By.XPath(".//ul/li");
            var header = By.XPath("//h1");

            LoginToLitecart();

            var numberOfItemsInMenu = GetElementById("sidebar").FindElements(menuItemPath).Count;

            for (int i = 0; i < numberOfItemsInMenu; i++)
            {
                var menuItem = driver.FindElements(menuItemPath).ElementAt(i);

                menuItem.Click();
                wait.Until(ExpectedConditions.ElementIsVisible(header));
                menuItem = driver.FindElements(menuItemPath).ElementAt(i);

                if (menuItem.FindElements(sumMenuItemPath).Count > 0)
                {
                    var numberOfItemsInSubMenu = menuItem.FindElements(sumMenuItemPath).Count;

                    for (int si = 1; si < numberOfItemsInSubMenu; si++)
                    {
                        menuItem = driver.FindElements(menuItemPath).ElementAt(i);

                        menuItem.FindElements(sumMenuItemPath).ElementAt(si).Click();
                        wait.Until(ExpectedConditions.ElementIsVisible(header));
                    }
                }
            }
        }

        [Test]
        public void CheckThatEachProductOnHomePageHasOnlyOneSticker()
        {
            driver.Url = storeUrl;

            var products = driver.FindElements(By.XPath("//li[@class='product column shadow hover-light']"));

            foreach (var product in products)
            {
                var productName = product.FindElement(By.XPath(".//div[@class='name']")).Text;
                var numberOfStickers = product.FindElements(By.XPath(".//div[contains(@class, 'sticker')]")).Count;

                if (numberOfStickers == 0)
                {
                    throw new Exception($"{productName} has no Sticker!");
                }

                if (numberOfStickers > 1)
                {
                    throw new Exception($"{productName} has more than one sticker!");
                }
            }
        }

        public IWebElement GetElementById(string id)
        {
            return driver.FindElement(By.XPath($"//*[@id='{id}']"));
        }

        public By GetElementLocatorByXpath(string xpath)
        {
            return By.XPath(xpath);
        }

        // Element types are: input, button.  
        public By GetElementLocatorByTypeAndName(string elementType, string buttonName)
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