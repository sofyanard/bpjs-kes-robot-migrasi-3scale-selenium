using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace selenium2
{
    internal class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            /* DRIVER SETUP */

            var options = new ChromeOptions();
            // options.AddArguments("--headless");
            options.AddArguments("--start-maximized");

            // string DriverPath = @"D:\SOURCE\Test\Selenium\chromedriver_win32";
            // var driver = new ChromeDriver(DriverPath, options, TimeSpan.FromSeconds(300));
            var driver = new ChromeDriver(options);
            //driver.Manage().Window.Maximize();

            /* OUTPUT FILE SETUP */

            string path = @"output.txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    
                }
            }

            string outDate, outAccount, outApiType, outUserKey;

            /* LOGIN PAGE */

            driver.Navigate().GoToUrl("https://api-admin.apps.ocp4dvlp.bpjs-kesehatan.go.id/");

            var titleLogin = driver.Title;
            Assert.AreEqual("3scale Login", titleLogin);

            Thread.Sleep(300);

            var txtUsername = driver.FindElement(By.Id("session_username"));
            var txtPassword = driver.FindElement(By.Id("session_password"));
            var btnSignin = driver.FindElement(By.CssSelector(".pf-c-button"));

            txtUsername.SendKeys("admin");
            txtPassword.SendKeys("DrdswCdD");
            btnSignin.Click();

            /* DASHBOARD PAGE */

            var titleDashboard = driver.Title;
            Assert.AreEqual("Dashboards - Show | Red Hat 3scale API Management", titleDashboard);

            Thread.Sleep(300);

            /* LOOP FOR ACCOUNT FROM TEXTFILE */

            foreach (string accountName in System.IO.File.ReadLines(@"input.txt"))
            {
                Console.WriteLine("PROCESSING: " + accountName);
                log.Info("PROCESSING: " + accountName);

                outDate = DateTime.Now.ToString() + "|";
                outAccount = accountName + "|";
                outApiType = String.Empty;
                outUserKey = String.Empty;

                /* ACCOUNT LIST PAGE */

                driver.Navigate().GoToUrl("https://api-admin.apps.ocp4dvlp.bpjs-kesehatan.go.id/buyers/accounts");

                var titleAudience = driver.Title;
                Assert.AreEqual("Accounts - Index | Red Hat 3scale API Management", titleAudience);

                Thread.Sleep(300);

                var txtSearch = driver.FindElement(By.Id("search_query"));
                var btnSearch = driver.FindElement(By.XPath("//input[@value='Search']"));

                txtSearch.SendKeys(accountName);
                btnSearch.Click();

                /* ACCOUNT SEARCH RESULT PAGE */

                var titleResult = driver.Title;
                Assert.AreEqual("Accounts - Index | Red Hat 3scale API Management", titleResult);

                Thread.Sleep(300);

                try
                {
                    var lnkResult = driver.FindElement(By.LinkText(accountName));

                    if (lnkResult != null)
                    {
                        Console.WriteLine("ACCOUNT FOUND: " + accountName);
                        log.Info("ACCOUNT FOUND: " + accountName);

                        lnkResult.Click();

                        /* ACCOUNT DETAIL PAGE */

                        var titleShow = driver.Title;
                        Assert.AreEqual("Accounts - Show | Red Hat 3scale API Management", titleShow);

                        Thread.Sleep(300);

                        var lnkAmtApplication = driver.FindElement(By.XPath("//main/ul/li[2]/a"));

                        string[] numberOfApplicationsArr = lnkAmtApplication.Text.Split(' ');
                        Console.WriteLine("NUMBER OF APPLICATIONS: " + numberOfApplicationsArr[0]);
                        log.Info("NUMBER OF APPLICATIONS: " + numberOfApplicationsArr[0]);

                        int numberOfApplications = int.Parse(numberOfApplicationsArr[0]);
                        if (numberOfApplications > 0)
                        {
                            lnkAmtApplication.Click();

                            /* APPLICATION LIST PAGE */

                            var titleApplicationIndex = driver.Title;
                            Assert.AreEqual("Applications - Index | Red Hat 3scale API Management", titleApplicationIndex);

                            Thread.Sleep(300);

                            /* APPLICATION DETAIL PAGE BELOW... */

                            if (numberOfApplications > 1)
                            {
                                var lnkApplication1 = driver.FindElement(By.XPath("//td[2]/a"));
                                var lnkApplication2 = driver.FindElement(By.XPath("//tr[2]/td[2]/a"));

                                if (lnkApplication1.Text.ToLower().Contains("antr"))
                                {
                                    outApiType = "ANTRIAN|";
                                    Console.WriteLine("API TYPE: ANTRIAN");
                                    log.Info("API TYPE: ANTRIAN");
                                    lnkApplication1.Click();
                                }
                                else
                                {
                                    if (lnkApplication2.Text.ToLower().Contains("antr"))
                                    {
                                        outApiType = "ANTRIAN|";
                                        Console.WriteLine("API TYPE: ANTRIAN");
                                        log.Info("API TYPE: ANTRIAN");
                                    }
                                    else
                                    {
                                        outApiType = "VCLAIM|";
                                        Console.WriteLine("API TYPE: VCLAIM");
                                        log.Info("API TYPE: VCLAIM");
                                    }
                                    lnkApplication2.Click();
                                }
                            }
                            else
                            {
                                var lnkApplication0 = driver.FindElement(By.XPath("//td[2]/a"));

                                if (lnkApplication0.Text.ToLower().Contains("antr"))
                                {
                                    outApiType = "ANTRIAN|";
                                    Console.WriteLine("API TYPE: ANTRIAN");
                                    log.Info("API TYPE: ANTRIAN");
                                }
                                else
                                {
                                    outApiType = "VCLAIM|";
                                    Console.WriteLine("API TYPE: VCLAIM");
                                    log.Info("API TYPE: VCLAIM");
                                }
                                lnkApplication0.Click();
                            }

                            var titleApplicationShow = driver.Title;
                            Assert.AreEqual("Applications - Show | Red Hat 3scale API Management", titleApplicationShow);

                            Thread.Sleep(300);

                            var lblAppDesc = driver.FindElement(By.Id("cinstance-user-key"));
                            outUserKey = lblAppDesc.Text;
                            Console.WriteLine("USER KEY: " + lblAppDesc.Text);
                            log.Info("USER KEY: " + lblAppDesc.Text);

                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("ACCOUNT NOT FOUND: " + accountName);
                    log.Info("ACCOUNT NOT FOUND: " + accountName);
                }

                /* WRITE TO OUTPUT FILE */

                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(outDate + outAccount + outApiType + outUserKey);
                }
            }

            Thread.Sleep(300);

            /* LOGOUT PAGE */
        
            driver.Navigate().GoToUrl("https://api-admin.apps.ocp4dvlp.bpjs-kesehatan.go.id/p/logout");

            var titleLogout = driver.Title;
            Assert.AreEqual("3scale Login", titleLogout);

            Thread.Sleep(1000);

            driver.Quit();

            log.Info("Finish!");
            Console.WriteLine("Finish!");
            Console.ReadLine();
        }
    }
}
