using System;
using System.Linq;
using CSExample.PageObjectsPattern.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CSExample.PageObjectsPattern.Application
{
    class LiteCartApp
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        private HomePage homePage;
        private ProductDetailsPage productDetailsPage;
        private CartPage cartPage;

        public LiteCartApp()
        {
            driver = new ChromeDriver();

            homePage = new HomePage(driver);
            productDetailsPage = new ProductDetailsPage(driver);
            cartPage = new CartPage(driver);
        }

        internal void AddFirstProductOnHomePageToCart(int numberOfProductsToAdd)
        {
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            homePage.Open();

            for (int i = 0; i < numberOfProductsToAdd;)
            {
                homePage.GetProductsFromContext(homePage.GetProductsSection("Most Popular")).First().Click();

                if (productDetailsPage.GetProductSezeOptions().Any())
                {
                    new SelectElement(productDetailsPage.GetProductSezeOption()).SelectByText("Small");
                }

                productDetailsPage.AddToCartButton().Click();

                i++;

                wait.Until(ExpectedConditions.ElementExists(productDetailsPage.QuantityOfItemsInCart(i)));
                productDetailsPage.HomeButton().Click();
            }
        }

        internal void NavigateToCart()
        {
            homePage.GetLinkByName("Checkout »").Click();
        }

        internal void RemoveAllProductsFromCart()
        {
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            cartPage.ProductShortcut().Click();
            cartPage.RemoveProductButton().Click();

            while (cartPage.ActualNumberOfItems().Count > 0)
            {
                wait.Until(ExpectedConditions.ElementExists(cartPage.RemoveProductButtonLocator()));

                var btn = driver.FindElement(cartPage.RemoveProductButtonLocator());

                btn.Click();
                wait.Until(ExpectedConditions.StalenessOf(btn));
            }
        }

        public void Quit()
        {
            driver.Quit();
        }
    }
}