﻿using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Linq;
using System.Collections.Generic;

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

            var products = driver.FindElements(By.XPath("//li[contains(@class, 'product column')]"));

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

        [Test]
        public void CheckThatCountriesAreSortedInAlphabeticalOrder()
        {
            LoginToLitecart();
            driver.Url = $"{adminUrl}?app=countries&doc=countries";

            var countriesRowPath = By.XPath("//form[@name='countries_form']//tr[@class='row']");
            var countryNamePath = By.XPath(".//a");
            var numberOfZonesPath = By.XPath(".//td[@style='text-align: right;']//preceding-sibling::td[1]");

            // Country lists.
            var nonSortedArrayOfCountries = driver.FindElements(countriesRowPath).Select(x => x.FindElement(countryNamePath).Text).ToArray();
            var SortedArrayOfCountries = driver.FindElements(countriesRowPath).Select(x => x.FindElement(countryNamePath).Text).ToArray();

            isArraysAreEqualySorted(nonSortedArrayOfCountries, SortedArrayOfCountries);

            // Check if sublists are sorted.
            var links = new List<string>();

            foreach (var item in driver.FindElements(countriesRowPath).Select(x => x.FindElement(numberOfZonesPath)))
            {
                if (!item.Text.Equals("0"))
                {
                    links.Add(item.FindElement(By.XPath(".//preceding-sibling::td/a")).GetAttribute("href"));
                }
            }

            if (links.Count > 0)
            {
                foreach (var link in links)
                {
                    driver.Url = link;

                    var listOfZonesPath = By.XPath("//table[@id='table-zones']//tr[not(@class='header')]//td[@style]/preceding-sibling::td[1]//input[@value and not(@data-size)]");
                    var unsortedListOfZones = driver.FindElements(listOfZonesPath).Select(x => x.GetAttribute("value")).ToArray();
                    var ListOfZonesToSort = driver.FindElements(listOfZonesPath).Select(x => x.GetAttribute("value")).ToArray();

                    isArraysAreEqualySorted(ListOfZonesToSort, unsortedListOfZones);
                }
            }
        }

        [Test]
        public void CheckThatGeoZones()
        {
            LoginToLitecart();
            driver.Url = $"{adminUrl}?app=geo_zones&doc=geo_zones";

            var listOfCountris = driver.FindElements(By.XPath("//table//td[not(@style)]/a")).Select(x => x.GetAttribute("href")).ToList();

            foreach (var country in listOfCountris)
            {
                driver.Url = country;

                var listOfSelectedZonesToSort = driver.FindElements(By.XPath("//table[@id='table-zones']//td[@style]//preceding-sibling::td[1]//option[@selected='selected']")).Select(x => x.Text).ToArray();
                var listOfSelectedZones = driver.FindElements(By.XPath("//table[@id='table-zones']//td[@style]//preceding-sibling::td[1]//option[@selected='selected']")).Select(x => x.Text).ToArray();

                isArraysAreEqualySorted(listOfSelectedZonesToSort, listOfSelectedZones);
            }
        }

        [Test]
        public void CheckThatProperProductPageOpensWhenClickOnProduct()
        {
            driver.Url = storeUrl;

            var productName = "Yellow Duck";
            var expectedRegularPriceFontTextDecoration = "line-through";
            var expectedDiscountPriceColor = "0, 0";
            var expectedDiscountPriceFontWeight = driver.GetType().Name.Equals("FirefoxDriver") ? "900" : "700";
            var product = GetProductFromContext(GetProductsSection("Campaigns"), productName);
            var actualProductRegularPrice = GetRegularProductPrice(product).Text;
            var actualProductDiscountPrice = GetDiscountProductPrice(product).Text;

            //  Check style
            var expectedRegularPriceColor = "119, 119, 119";
            if (!GetRegularProductPrice(product).GetCssValue("color").Contains(expectedRegularPriceColor))
            {
                throw new Exception("Color mismatch");
            }

            if (!GetRegularProductPrice(product).GetCssValue("text-decoration").Contains(expectedRegularPriceFontTextDecoration))
            {
                throw new Exception("Text decoration mismatch");
            }

            if (!GetDiscountProductPrice(product).GetCssValue("color").Contains(expectedDiscountPriceColor))
            {
                throw new Exception("Color mismatch");
            }

            if (!GetDiscountProductPrice(product).GetCssValue("font-weight").Contains(expectedDiscountPriceFontWeight))
            {
                throw new Exception("Font weight mismatch");
            }

            product.Click();

            var productOnDetailsPage = GetProductOnDetailsPage(productName);
            // Check price
            if (!GetRegularProductPrice(productOnDetailsPage).Text.Equals(actualProductRegularPrice))
            {
                throw new Exception("Regular price mismatch");
            }

            if (!GetDiscountProductPrice(productOnDetailsPage).Text.Equals(actualProductDiscountPrice))
            {
                throw new Exception("Discount price mismatc");
            }

            //  Check style on details page
            expectedRegularPriceColor = "102, 102, 102";
            if (!GetRegularProductPrice(productOnDetailsPage).GetCssValue("color").Contains(expectedRegularPriceColor))
            {
                throw new Exception("Color mismatch");
            }

            if (!GetRegularProductPrice(productOnDetailsPage).GetCssValue("text-decoration").Contains(expectedRegularPriceFontTextDecoration))
            {
                throw new Exception("Text decoration mismatch");
            }

            if (!GetDiscountProductPrice(productOnDetailsPage).GetCssValue("color").Contains(expectedDiscountPriceColor))
            {
                throw new Exception("Color mismatch");
            }

            if (driver.GetType().Name.Equals("FirefoxDriver"))
                expectedDiscountPriceFontWeight = "700";

            if (!GetDiscountProductPrice(productOnDetailsPage).GetCssValue("font-weight").Contains(expectedDiscountPriceFontWeight))
            {
                throw new Exception("Font weight mismatch");
            }
        }

        #region Product

        public IWebElement GetProductsSection(string sectionName) => driver.FindElement(By.XPath($"//div[@class='box']//h3[text()='{sectionName}']/following-sibling::div"));
        public IWebElement GetProductFromContext(IWebElement context, string productName) => context.FindElement(By.XPath($".//li[contains(@class, 'product')]//div[@class='name' and text()='{productName}']//ancestor::li[contains(@class, 'product')]"));
        public IWebElement GetRegularProductPrice(IWebElement ProductContext) => ProductContext.FindElement(By.XPath(".//*[@class='regular-price']"));
        public IWebElement GetDiscountProductPrice(IWebElement ProductContext) => ProductContext.FindElement(By.XPath(".//*[@class='campaign-price']"));

        // Product details page
        public IWebElement GetProductOnDetailsPage(string productName) => driver.FindElement(By.XPath($"//div[@id='box-product']//h1[text()='{productName}']//ancestor::div[@id='box-product']"));

        #endregion

        #region Helpers

        public void isArraysAreEqualySorted(string[] arrayToSort, string[] unsortedAray)
        {
            Array.Sort(arrayToSort);

            if (!unsortedAray.SequenceEqual(arrayToSort))
            {
                throw new Exception("The List isn't sorted");
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

        #endregion

        [TearDown]
        public void stop()
        {
            driver.Quit();
            driver = null;
        }
    }
}
