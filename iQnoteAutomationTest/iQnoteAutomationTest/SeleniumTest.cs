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

           
            string jsonString = File.ReadAllText("credentials.json");
            // Deserialize to object
            String info = JsonSerializer.Deserialize<string>(jsonString);
        
                      
            //Act
            email_input.Clear();
            password_input.Clear();

            //email_input.SendKeys("jeroenb@quantaris.nl");
            email_input.SendKeys($"{info.username}");
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

        //[Test]
        //public void testMailSlurp()
        //{
        //    //var inboxId = Guid.Parse("dc9b5362 - af00 - 43ef - bbfd - cf1cdd688218");
        //    var inboxId = Guid.Parse("dc9b5362-af00-43ef-bbfd-cf1cdd688218");
        //    var emailadress = "user-dc9b5362-af00-43ef-bbfd-cf1cdd688218@mailslurp.biz";

        //    // Run `dotnet add package mailslurp`
        //    var config = new mailslurp.Client.Configuration();

        //    // create config with x-api-key
        //    config.ApiKey.Add("x-api-key", "158f63ab1858e4fffb5594aea3781ef7425e41294344ee7a7ffca38f8626874c");

        //    // create new controller instance
        //    //var inboxControllerApi = new mailslurp.Api.InboxControllerApi(config);
        //    //var body = "<html>" +
        //    //    "<body>" +
        //    //    "<p>gelukt?</p>" +
        //    //    "</body>" +
        //    //    "</html>";

        //    //inboxControllerApi.SendEmail(inboxId: inboxId, new mailslurp.Model.SendEmailOptions(to: [emailadress], subject: "hello test", body: body, isHTML:true));

        //    var waitController = new mailslurp.Api.WaitForControllerApi(config);
        //    var email = waitController.WaitForLatestEmail(inboxId: inboxId, timeout: 120_000, unreadOnly: true);

        //    Assert.That(email.Subject == "iQnote - Tijdelijke inlogcode", Is.True);

        //}



        //[Test]
        //public void testGMailAPI()
        //{
        //    UserCredential credential;

        //    //using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read)) 
        //    using (var stream = new FileStream(@"C:\iQnoteAutomationTest\iQnoteAutomationTest\iQnoteAutomationTest\credentials.json", FileMode.Open, FileAccess.Read))
        //    {
        //        string credPath = "token.json";
        //        //credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //        //    GoogleClientSecrets.Load(stream).Secrets,
        //        //                Scopes,
        //        //                "user",
        //        //                CancellationToken.None,
        //        //                new FileDataStore(credPath, true)).Result;
        //        //Console.WriteLine("Credential file saved to: " + credPath);
        //        ClientSecrets secrets = new ClientSecrets()
        //        {
        //            ClientId = "1043432413256-1715o6vn7ffr53jt24lpboabv5e8oeqt.apps.googleusercontent.com",
        //            ClientSecret = "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"
        //        };
        //        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //            secrets, Scopes, "user", CancellationToken.None, new LocalServerCodeReceiver(new LocalServerCodeReceiver.Options
        //            {
        //                Port = 5000 /* Zelfde poortnummer als in redirect URI */
        //            })).Result;

        //        //GoogleClientSecrets.Load(stream).Secrets,
        //        //            Scopes,
        //        //            "user",
        //        //            CancellationToken.None
        //        //            ).Result;
        //        Console.WriteLine("Credential file saved to: " + credPath);
        //    }

        //    var service = new GmailService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = ApplicationName,
        //    });



            //    // Get the list of messages
            //    UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List("me");
            //    request.Q = "is:unread"; // Example query: unread emails
            //    ListMessagesResponse response = request.Execute();

            //    Console.WriteLine("Messages:");
            //    if (response.Messages != null && response.Messages.Count > 0)
            //    {
            //        foreach (var message in response.Messages)
            //        {
            //            Console.WriteLine($"Message ID: {message.Id}");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("No messages found.");
            //    }
            //}

            //[Test]
            //public void testIMPAP()
            //{
            //    string server = "outlook.office365.com";
            //    int port = 993;
            //    bool useSsl = true;

            //    // Gebruikersgegevens
            //    string email = "testquantaris@hotmail.com";
            //    //string password = "iotlcfuvzclwijop";
            //    string password = "welkom123";

            //    try
            //    {
            //        using (var client = new ImapClient())
            //        {
            //            // Verbind met de IMAP-server
            //            client.Connect(server, port, useSsl);

            //            // Verifieer met gebruikersnaam en wachtwoord
            //            client.Authenticate(email, password);

            //            // Open de inbox
            //            client.Inbox.Open(FolderAccess.ReadOnly);

            //            // Print het aantal berichten in de inbox
            //            Console.WriteLine($"Aantal berichten: {client.Inbox.Count}");

            //            // Lees de nieuwste e-mail
            //            var message = client.Inbox.GetMessage(client.Inbox.Count - 1);
            //            Console.WriteLine($"Onderwerp: {message.Subject}");
            //            Console.WriteLine($"Inhoud: {message.TextBody}");

            //            // Verbinding verbreken
            //            client.Disconnect(true);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Er ging iets mis: {ex.Message}");
            //    }
            //}

            [Test]
        public void TestTOTPWithApplication()
        {
            //Arrange 
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));      
            string base32Secret = "CT2RIZEFHH7X2NZCAS6ONBCXWL2MMHBK";
            var secret = Base32Encoding.ToBytes(base32Secret);
            var totp = new Totp(secret);
            string identification_code = totp.ComputeTotp();

            IWebElement code_inputfield = wait.Until(e => e.FindElement(By.Id("_df_19")));
            IWebElement login_button = wait.Until(e => e.FindElement(By.Id("_df_21")));
            IWebElement landing_page = wait.Until(e => e.FindElement(By.ClassName("dfData_Text")));

            //IWebElement landing_page = wait.Until(e => e.FindElement(By.CssSelector("#OWEBAPP .iq-left-panel")));
            //IWebElement landing_page = wait.Until(e => e.FindElement(By.CssSelector(".WebMenuList .WebML_Panel")));
            //IWebElement landing_page = wait.Until(e => e.FindElement(By.("employee")));
            //IWebElement landing_page = wait.Until(e => e.FindElement(By.ClassName("WebLabel_content WebWrp_PreWrap")));

            //IWebElement incorrect_popup = wait.Until(e => e.FindElement(By.Id("_df_113")));



            //Console.WriteLine(totp.ComputeTotp());

            //Act 
            code_inputfield.SendKeys(identification_code);
            login_button.Click();

            //Assert
            //Assert.That(landing_page.Text, Is.EqualTo("Afspraken voor vandaag"));
            Assert.That(landing_page.Text, Is.EqualTo("Alle Notarissen\r\nHans van de Laar\r\nYassine Abdelkhalak\r\nErik Lucas\r\nBryan Huber\r\nNicole Jolanda van 't Hull\r\nJeroen\r\nBrenda van Hees\r\nWouter Sloothaak\r\nJason Ntezicimpa\r\nVincent Notaris\r\nBram Nijenkamp\r\nFilippa Ruiter\r\nKim Snels\r\nJeroen\r\nTest");



            // maak een test waarbij 10x een foute test volgt
            // maak een test waarbij een te lange code meegeeft





            //while (true)
            //{
            //    Console.Write("Enter code: ");
            //    string inputCode = Console.ReadLine();
            //    bool valid = totp.VerifyTotp(inputCode, out long timeStepMatched,
            //        VerificationWindow.RfcSpecifiedNetworkDelay);
            //    string validStr = valid ? "Valid" : "Invalid";
            //    var colour = valid ? ConsoleColor.Green : ConsoleColor.Red;
            //    Console.ForegroundColor = colour;
            //    Console.WriteLine(validStr);
            //    Console.ResetColor();
            //}



        }
               
        [OneTimeTearDown]
        public void Dispose()
        {
            driver.Quit();
        }

        [OneTimeTearDownAttribute]
        public void TearDownTest()
        {
            driver.Dispose();
        }

        // niet vergeten om foutieve/geen verificatie code te testen!!!!!!!!!!
    }

}
