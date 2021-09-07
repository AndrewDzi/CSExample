using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CSExample
{
    [TestFixture]
    public class MyFirstTest
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        [SetUp]
        public void start()
        {
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void FirstTest()
        {
            var mainSearchInputField = By.XPath(".//input[@name='search']");

            driver.Url = "https://rozetka.com.ua/";
            IWebElement element = wait.Until(d => d.FindElement(mainSearchInputField));
            driver.FindElement(mainSearchInputField).SendKeys("Xbox");
            driver.FindElement(By.XPath(".//button[@class='button button_color_green button_size_medium search-form__submit ng-star-inserted']")).Click();
            wait.Until(ExpectedConditions.ElementExists(By.XPath(".//h1[text()=' Игровые приставки Microsoft ']")));
        }

        [TearDown]
        public void stop()
        {
            driver.Quit();
            driver = null;
        }
    }
}