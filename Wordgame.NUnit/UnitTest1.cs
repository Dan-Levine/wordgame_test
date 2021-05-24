using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Wordgame.NUnit
{
    public enum categories
    {
        TVShows = 1,
        Movies = 2,
        Books = 3,
        Places = 4,
        Animals = 5,
        Food = 6
    }
    
    public class Tests
    {
        private IWebDriver _driver;
        private TimeSpan WAIT_TIMEOUT = TimeSpan.FromSeconds(1);
        
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _driver = new ChromeDriver("C:\\");
            _driver.Manage().Timeouts().ImplicitWait = WAIT_TIMEOUT;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _driver.Quit();
        }

        [SetUp]
        public void Setup()
        {
            _driver.Navigate().GoToUrl("https://clicktime-wordgame-exercise.vercel.app");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }

        [Test]
        [TestCase(categories.TVShows, "FRIENDS")]
        [TestCase(categories.Movies, "THEMARIX")]
        [TestCase(categories.Books, "CRIMEANDPUNSHT")]
        [TestCase(categories.Places, "NEWYORK")]
        [TestCase(categories.Food, "SUHI")]
        [TestCase(categories.Animals, "FOBAR", Ignore = "Animals not implemented yet")]
        public void TestGameWin(categories category, string guesses )
        {
            //Arrange
            selectCategory(category);
            
            //Act
            foreach (char c in guesses)
            {
                guessLetter(c);
            }
    
            //Assert
            Assert.That(gameWon(), Is.True);
        }

        [Test]
        public void TestGameLost()
        {
            //Arrange
            selectCategory(categories.Food);
            
            //Act
            string guesses = "ABCDEF";
            foreach (char c in guesses)
            {
                guessLetter(c);
            }
    
            //Assert
            var elements = _driver.FindElements(By.ClassName("text_standard__1U0DF"));
            IWebElement message = elements[elements.Count - 1];
            
            Assert.That(message.Text, Is.EqualTo("You have missed 6 guess(es), you have 0 attempt(s) left."));
            Assert.That(gameWon, Is.False);
        }

        [Test]
        public void TestDoubleLetter()
        {
            //Arrange
            selectCategory(categories.Books);
            
            //Act
            guessLetter('Z');
            guessLetter('Z');
            
            //Assert
            var elements = _driver.FindElements(By.ClassName("text_standard__1U0DF"));
            IWebElement message = elements[elements.Count - 1];
            
            Assert.That(message.Text, Is.EqualTo("You have missed 1 guess(es), you have 5 attempt(s) left."));
            
        }

        [Test]
        [TestCase("A")]
        [TestCase("BC")]
        [TestCase("CDE")]
        [TestCase("DFGJ")]
        [TestCase("KLMNO")]
        [TestCase("PQRXYZ")]
        public void TestCountDown(string guesses)
        {
            //Arrange
            selectCategory(categories.Food);
            
            //Act
            foreach (char c in guesses)
            {
                guessLetter(c);
            }
            
            //Assert
            var elements = _driver.FindElements(By.ClassName("text_standard__1U0DF"));
            IWebElement message = elements[elements.Count - 1];
            
            Assert.That(message.Text, Is.EqualTo($"You have missed {guesses.Length} guess(es), you have {6 - guesses.Length} attempt(s) left."));
        }

        [Test]
        public void TestGameRestart()
        {
            //Arrange
            selectCategory(categories.Food);
            
            //Act
            IWebElement button = _driver.FindElement(By.ClassName("button_primary__6jxn1"));
            button.Click();

            //Assert
            IWebElement element = _driver.FindElement(By.ClassName("load_container__2P2DD"));
            Assert.That(element.Displayed, Is.True);
        }

        [Test]
        public void TestUnicodeSupport()
        {
            //Arrange
            //Act
            String testString = "В чащах юга жил бы цитрус? Да, но фальшивый";
            IWebElement username = _driver.FindElement(By.XPath("//input[@placeholder='Enter a username']"));
            username.SendKeys(testString);
            
            WebDriverWait wait = new WebDriverWait(_driver, WAIT_TIMEOUT);
            IWebElement element =
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(
                    By.XPath($"//option[@value='2']/..")));
            SelectElement select = new SelectElement(element);
            select.SelectByIndex(2);
            element = _driver.FindElement(By.ClassName("button_primary__6jxn1"));
            element.Click();
            
            //Assert
            element = _driver.FindElement(By.ClassName("text_header__1XvnO"));
            Assert.That(element.Text, Is.EqualTo($"Welcome to WordGame {testString}!"));
        }
        
        private void guessLetter(char guess)
        {
            //Clickable button is two levels up from actual letter
            
            IWebElement element = _driver.FindElement(By.XPath($"//span[.='{Char.ToUpper(guess)}']/../.."));
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            element.Click();
        }
        private void selectCategory(categories category)
        {
            IWebElement username = _driver.FindElement(By.XPath("//input[@placeholder='Enter a username']"));
            username.SendKeys("FooBar");

            WebDriverWait wait = new WebDriverWait(_driver, WAIT_TIMEOUT);
            IWebElement element =
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(
                    By.XPath($"//option[@value='{(int) category}']/..")));
            SelectElement select = new SelectElement(element);
            select.SelectByIndex((int) category);
            element = _driver.FindElement(By.ClassName("button_primary__6jxn1"));
            element.Click();
        }

        private bool gameWon()
        {
            bool result = false;
            var elements = _driver.FindElements(By.ClassName("text_header__1XvnO"));
            //Need to check last header to get status of game
            IWebElement message = elements[elements.Count - 1];
            if (message.Text == "Congratulations you just won")
            {
                result = true;
            }

            return result;
        }
        
    }
}