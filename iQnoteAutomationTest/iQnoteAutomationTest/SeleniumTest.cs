using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Http.Generated;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium;

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using OtpNet;
using System;

using System.Collections.ObjectModel;

using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace iQnoteAutomationTest
{


    public class Tests
    {

        IWebDriver driver;
        const string URL = "https://test.iqnote.nl/";
        static string[] Scopes = { GmailService.Scope.GmailReadonly };
        static string ApplicationName = "Gmail API .NET Example";


        //static const mailslurp = new MailSlurp({ 158f63ab1858e4fffb5594aea3781ef7425e41294344ee7a7ffca38f8626874c });
        //static const { id, emailAddress } = await mailslurp.createInbox();


        [OneTimeSetUp]
        public void Setup()
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            driver = new EdgeDriver(path + @"\drivers\");
            driver.Navigate().GoToUrl(URL); // this way when someone re-runs the tests, the same webbrowser will refresh
        }

        [Test]
        public void verifyLogo()
        {
            //Arrange 
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            //Act
            IWebElement logo2 = wait.Until(e => e.FindElement(By.CssSelector("#viewport > div > div > div.WebCon_Main > div.BaseApp_ViewRegion > dialog > div.WebWin_main_l > div > div.WebWin_main_c > div > div > div > div.WebContainer.WebPanel.Web_Enabled > div > div > div.WebControl.WebImage.Web_Enabled.Web_JS_Sizing > div > div > div > img"))); // instead of a regular variable the logo is now a webelement

            //Assert
            Assert.That(/*logo.Displayed, Is.True || */ logo2.Displayed, Is.True);
        }


        [Test]
        public void TestEmptyLoginFormDisplaysErrorMessage()
        {
            // Arrange
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            IWebElement button = wait.Until(e => e.FindElement(By.Id("_df_12")));

            // Act 
            button.Click();
            IWebElement popup = wait.Until(e => e.FindElement(By.CssSelector(".ErrorMessage .WebLabel_content")));

            //Assert
            Assert.That(popup.Displayed, Is.True); // assert that popup is shown
            Assert.That(popup.Text, Is.EqualTo("Ongeldige aanmelding.")); // assert the correct text is shown

        }

        [Test]
        public void TestLoginFormEmptyEmailFieldDisplaysError()
        {
            // Arrange
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            IWebElement email_input = wait.Until(e => e.FindElement(By.Id("_df_6")));
            IWebElement button = wait.Until(e => e.FindElement(By.Id("_df_12")));

            // Act 
            email_input.SendKeys("someone@gmail.com");
            button.Click();
            IWebElement popup = wait.Until(e => e.FindElement(By.CssSelector(".ErrorMessage .WebLabel_content")));

            //Assert
            Assert.That(popup.Displayed, Is.True); // assert that popup is shown
        }

        [Test]
        public void TestLoginFormEmptyPasswordFieldDisplaysError()
        {
            // Arrange
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            IWebElement password_input = wait.Until(e => e.FindElement(By.Id("_df_8")));
            IWebElement button = wait.Until(e => e.FindElement(By.Id("_df_12")));

            // Act 
            password_input.SendKeys("password");
            button.Click();
            IWebElement popup = wait.Until(e => e.FindElement(By.CssSelector(".ErrorMessage .WebLabel_content")));

            //Assert
            Assert.That(popup.Displayed, Is.True); // assert that popup is shown
        }

        [Test]
        public void TestSuccesfullLogin2FApopup()
        {
            // Arrange
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement email_input = wait.Until(e => e.FindElement(By.Id("_df_6")));
            IWebElement password_input = wait.Until(e => e.FindElement(By.Id("_df_8")));
            IWebElement login_button = wait.Until(e => e.FindElement(By.Id("_df_12")));


            string jsonString = File.ReadAllText("some file");
            // Deserialize to object
            login_info info = JsonSerializer.Deserialize<login_info>(jsonString);


            //Act
            email_input.Clear();
            password_input.Clear();

            //email_input.SendKeys("jeroenb@quantaris.nl");
            //email_input.SendKeys($"{info.username}");
            email_input.SendKeys("dit klopt niet");
            //password_input.SendKeys("DeDompelaar1e!");
            password_input.SendKeys($"{info.password}");
            login_button.Click();
            IWebElement ok_button = wait.Until(e => e.FindElement(By.Id("_df_113")));
            ok_button.Click();
            IWebElement verification_button = wait.Until(e => e.FindElement(By.Id("_df_21")));
            verification_button.Click();

            //Assert
            Assert.That(verification_button.Displayed, Is.True); // assert that 2fa_popup is shown

        }

        [Test]
        public void TestIncorrect2faCode()
        {

            //Arrange 
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            //Act
            //IWebElement logo = wait.Until(e => e.FindElement(By.Id("_df_12"))); // instead of a regular variable the logo is now a webelement
            IWebElement popup_button = wait.Until(e => e.FindElement(By.Id("_df_115")));

            //Assert
            Assert.That(popup_button.Displayed, Is.True);

        }

        [Test]
        public void TestTOTPWithApplication()
        {
            //Arrange 
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            string base32Secret = "removed for security reasons";
            var secret = Base32Encoding.ToBytes(base32Secret);
            var totp = new Totp(secret);
            string identification_code = totp.ComputeTotp();

            IWebElement code_inputfield = wait.Until(e => e.FindElement(By.Id("_df_19")));
            IWebElement login_button = wait.Until(e => e.FindElement(By.Id("_df_21")));
            IWebElement landing_page = wait.Until(e => e.FindElement(By.ClassName("dfData_Text")));


            //Act 
            code_inputfield.SendKeys(identification_code);
            login_button.Click();

            //Assert
            //Assert.That(landing_page.Text, Is.EqualTo("Afspraken voor vandaag"));
            Assert.That(landing_page.Text, Is.EqualTo("Alle Notarissen Test"));



            // maak een test waarbij 10x een foute test volgt
            // maak een test waarbij een te lange code meegeeft
            // niet vergeten om foutieve/geen verificatie code te testen!!!!!!!!!!
        }

    }
}
