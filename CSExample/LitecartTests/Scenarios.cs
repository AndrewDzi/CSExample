using System;
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
            //driver = new FirefoxDriver();
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
            var expectedDiscountPriceFontWeight = driver.GetType().Name.Equals("FirefoxDriver") ? "900" : "700";
            var product = GetProductFromContext(GetProductsSection("Campaigns"), productName);
            var actualProductRegularPrice = GetRegularProductPrice(product).Text;
            var actualProductDiscountPrice = GetDiscountProductPrice(product).Text;

            //  Check style
            IsColorGray(ExtractRgbChannels(GetRegularProductPrice(product).GetCssValue("color"), driver.GetType().Name));

            if (!GetRegularProductPrice(product).GetCssValue("text-decoration").Contains(expectedRegularPriceFontTextDecoration))
            {
                throw new Exception("Text decoration mismatch");
            }

            IsColorRed(ExtractRgbChannels(GetDiscountProductPrice(product).GetCssValue("color"), driver.GetType().Name));

            if (!GetDiscountProductPrice(product).GetCssValue("font-weight").Contains(expectedDiscountPriceFontWeight))
            {
                throw new Exception("Font weight mismatch");
            }

            var regularPrice = double.Parse(GetRegularProductPrice(product).GetCssValue("font-size").Replace("px", string.Empty));
            var discountPrice = double.Parse(GetDiscountProductPrice(product).GetCssValue("font-size").Replace("px", string.Empty));

            if (discountPrice < regularPrice)
            {
                throw new Exception("Discount price is smaller that regular");
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
            IsColorGray(ExtractRgbChannels(GetRegularProductPrice(productOnDetailsPage).GetCssValue("color"), driver.GetType().Name));

            if (!GetRegularProductPrice(productOnDetailsPage).GetCssValue("text-decoration").Contains(expectedRegularPriceFontTextDecoration))
            {
                throw new Exception("Text decoration mismatch");
            }

            IsColorRed(ExtractRgbChannels(GetDiscountProductPrice(productOnDetailsPage).GetCssValue("color"), driver.GetType().Name));

            if (driver.GetType().Name.Equals("FirefoxDriver"))
                expectedDiscountPriceFontWeight = "700";

            if (!GetDiscountProductPrice(productOnDetailsPage).GetCssValue("font-weight").Contains(expectedDiscountPriceFontWeight))
            {
                throw new Exception("Font weight mismatch");
            }

            var regularPriceDetails = double.Parse(GetRegularProductPrice(productOnDetailsPage).GetCssValue("font-size").Replace("px", string.Empty));
            var discountPriceDetails = double.Parse(GetDiscountProductPrice(productOnDetailsPage).GetCssValue("font-size").Replace("px", string.Empty));

            if (discountPriceDetails < regularPriceDetails)
            {
                throw new Exception("Discount price is smaller that regular");
            }
        }

        [Test]
        public void RegisterNewUserInLitecard()
        {
            driver.Url = storeUrl;
            var userEmail = $"user_{RandomString(10)}@gmail.com";
            var userPhone = $"+120{RandomInt(9)}";
            var password = RandomString(10);
            var state = "Illinois";

            GetLinkByName("New customers click here").Click();

            new SelectElement(driver.FindElement(GetElementLocatorByTypeAndName("select", "country_code"))).SelectByText("United States");
            wait.Until(d => d.FindElement(By.XPath($"//select[@name='zone_code']//option[text()='{state}']")));
            new SelectElement(driver.FindElement(GetElementLocatorByTypeAndName("select", "zone_code"))).SelectByText(state);

            driver.FindElement(GetElementLocatorByTypeAndName("input", "firstname")).SendKeys("Bob");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "lastname")).SendKeys("Bobee");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "address1")).SendKeys("3322 S Morgan St");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "postcode")).SendKeys("60608");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "city")).SendKeys("Chicago");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "email")).SendKeys(userEmail);
            driver.FindElement(GetElementLocatorByTypeAndName("input", "phone")).SendKeys(userPhone);
            driver.FindElement(GetElementLocatorByTypeAndName("input", "password")).SendKeys(password);
            driver.FindElement(GetElementLocatorByTypeAndName("input", "confirmed_password")).SendKeys(password);
            driver.FindElement(GetElementLocatorByTypeAndName("button", "create_account")).Click();
            GetLinkByName("Logout").Click();
            driver.FindElement(GetElementLocatorByTypeAndName("input", "email")).SendKeys(userEmail);
            driver.FindElement(GetElementLocatorByTypeAndName("input", "password")).SendKeys(password);
            driver.FindElement(GetElementLocatorByTypeAndName("button", "login")).Click();
            GetLinkByName("Logout").Click();
        }

        [Test]
        public void CheckThatItsPossibleToAddNewProductToCatalog()
        {
            driver.Url = adminUrl;
            LoginToLitecart();

            var productName = "Dust";
            var absoluteFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(@"bin\Debug\CSExample.dll", @"\Content\dust.png");

            GetLitecartAdminMenuItem("Catalog").Click();
            driver.FindElement(By.XPath("//*[@class='button' and  contains(text(),'Add New Product')]")).Click();
            driver.FindElement(GetElementLocatorByTypeAndName("input", "name[en]")).SendKeys(productName);
            driver.FindElement(GetElementLocatorByTypeAndName("input", "code")).SendKeys("001");
            GetCheckboxByName("Unisex").Click();
            driver.FindElement(GetElementLocatorByTypeAndName("input", "quantity")).Clear();
            driver.FindElement(GetElementLocatorByTypeAndName("input", "quantity")).SendKeys("1");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "new_images[]")).SendKeys(absoluteFilePath);
            driver.FindElement(GetElementLocatorByTypeAndName("input", "date_valid_from")).SendKeys("01/09/2021");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "date_valid_from")).SendKeys("01/09/2022");

            GetLinkByName("Information").Click();

            new SelectElement(driver.FindElement(GetElementLocatorByTypeAndName("select", "manufacturer_id"))).SelectByText("ACME Corp.");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "keywords")).SendKeys("dst");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "short_description[en]")).SendKeys("Exotic decoration");
            driver.FindElement(GetElementLocatorByTypeAndName("textarea", "description[en]")).SendKeys("Dust is made of fine particles of solid matter. On Earth, it generally consists of particles in the atmosphere that come from various sources such as soil lifted by wind (an aeolian process), volcanic eruptions, and pollution. Dust in homes is composed of about 20–50% dead skin cells.");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "head_title[en]")).SendKeys("Dust like a real dust");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "meta_description[en]")).SendKeys("meta dust disc.");

            GetLinkByName("Prices").Click();

            driver.FindElement(GetElementLocatorByTypeAndName("input", "purchase_price")).Clear();
            driver.FindElement(GetElementLocatorByTypeAndName("input", "purchase_price")).SendKeys("9.99");
            new SelectElement(driver.FindElement(GetElementLocatorByTypeAndName("select", "purchase_price_currency_code"))).SelectByText("Euros");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "gross_prices[USD]")).SendKeys("1");
            driver.FindElement(GetElementLocatorByTypeAndName("input", "gross_prices[EUR]")).SendKeys("1");
            driver.FindElement(GetElementLocatorByTypeAndName("button", "save")).Click();

            if (!driver.FindElement(GetElementLocatorByTypeAndName("form", "catalog_form")).FindElement(By.XPath($".//a[text()='{productName}']")).Displayed)
            {
                throw new Exception($"Product '{productName}' wasn't created.");
            }
        }

        #region Litecart Admin

        public IWebElement GetLitecartAdminMenuItem(string menuItemName) => driver.FindElement(By.XPath($"//ul[@id='box-apps-menu']//*[@id='app-']//span[@class='name' and text()='{menuItemName}']"));

        public IWebElement GetCheckboxByName(string checkboxName) => driver.FindElement(By.XPath($"//div[@class='input-wrapper']//td[text()='{checkboxName}']/preceding-sibling::td//input[@type='checkbox']"));

        #endregion

        #region Product

        public IWebElement GetProductsSection(string sectionName) => driver.FindElement(By.XPath($"//div[@class='box']//h3[text()='{sectionName}']/following-sibling::div"));
        public IWebElement GetProductFromContext(IWebElement context, string productName) => context.FindElement(By.XPath($".//li[contains(@class, 'product')]//div[@class='name' and text()='{productName}']//ancestor::li[contains(@class, 'product')]"));
        public IWebElement GetRegularProductPrice(IWebElement ProductContext) => ProductContext.FindElement(By.XPath(".//*[@class='regular-price']"));
        public IWebElement GetDiscountProductPrice(IWebElement ProductContext) => ProductContext.FindElement(By.XPath(".//*[@class='campaign-price']"));

        // Product details page
        public IWebElement GetProductOnDetailsPage(string productName) => driver.FindElement(By.XPath($"//div[@id='box-product']//h1[text()='{productName}']//ancestor::div[@id='box-product']"));

        #endregion

        #region Helpers

        public List<string> ExtractRgbChannels(string rawRgb, string driverName)
        {
            var rgbChannels = new List<string>();

            if (driverName.Equals("FirefoxDriver"))
            {
                rgbChannels = rawRgb.Replace("rgb(", string.Empty).Replace(")", string.Empty).Replace(" ", string.Empty).Split(",".ToCharArray()).ToList();
            }
            else
            {
                rgbChannels = rawRgb.Replace("rgba(", string.Empty).Replace(")", string.Empty).Replace(" ", string.Empty).Split(",".ToCharArray()).ToList();
                rgbChannels.RemoveAt(3);
            }

            return rgbChannels;
        }

        public void IsColorGray(List<string> rgbChannels)
        {
            foreach (var channel in rgbChannels)
            {
                if (!rgbChannels[0].Equals(channel))
                {
                    throw new Exception("It isn't gray color");
                }
            }
        }

        public void IsColorRed(List<string> rgbChannels)
        {
                if (!rgbChannels[1].Equals("0") && rgbChannels[2].Equals("0"))
                {
                    throw new Exception("It isn't red color");
                }
        }

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

        public IWebElement GetLinkByName(string name)
        {
            return driver.FindElement(By.XPath($"//a[@href and text()='{name}']"));
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


        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static Random randomNumber = new Random();

        public static string RandomInt(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[randomNumber.Next(s.Length)]).ToArray());
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