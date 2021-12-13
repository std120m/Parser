using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parser
{
    static class Parser
    {
        public static string GetStringFromHtml(string url, Encoding encoding)
        {
            byte[] htmlData = new byte[0];
            using (WebClient client = new WebClient())
            {
                client.Encoding = encoding;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                try
                {
                    htmlData = client.DownloadData(url);
                    Console.WriteLine($"{url} --- was parsed");
                    Thread.Sleep(2000);
                }catch(Exception e)
                {
                    Console.WriteLine($"Error --- {e}");

                    Thread.Sleep(15000);
                    GetStringFromHtml(url, encoding);
                }
                return encoding.GetString(htmlData);
            }
        }

        public static string ParseQuotes(Company company)
        {
            if (!File.Exists(Program.DownloadsPath + "/" + $"{company.Code.Replace(':', company.Code.Contains('.') ? '_' : '.')}_{Program.DateFrom.ToString("yyMMdd")}_{Program.DateTo.ToString("yyMMdd")}" + ".txt"))
                DownloadScvFile(Program.FinamDomen + company.Url.Replace("quote", "profile") + "export");
            return Program.DownloadsPath + "/" + $"{company.Code.Replace(':', company.Code.Contains('.') ? '_' : '.')}_{Program.DateFrom.ToString("yyMMdd")}_{Program.DateTo.ToString("yyMMdd")}" + ".txt";
        }

        public static void DownloadScvFile(string url)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("no-sandbox");
            IWebDriver driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), chromeOptions, TimeSpan.FromMinutes(10));
            driver.Url = url;

            Thread.Sleep(10000);
            driver.FindElement(By.Id("issuer-profile-export-from")).Click();
            SelectElement selectMonth = new SelectElement(driver.FindElement(By.ClassName("ui-datepicker-month")));
            selectMonth.SelectByText(DateHelper.MonthNumberToShortName[Program.DateFrom.ToString("MM")]);
            SelectElement selectYear = new SelectElement(driver.FindElement(By.ClassName("ui-datepicker-year")));
            selectYear.SelectByText(Program.DateFrom.ToString("yyyy"));
            IWebElement calendarPicker = driver.FindElement(By.ClassName("ui-datepicker-calendar"));
            calendarPicker.FindElement(By.LinkText(Program.DateFrom.ToString("dd"))).Click();

            driver.FindElement(By.Id("issuer-profile-export-to")).Click();
            selectMonth = new SelectElement(driver.FindElement(By.ClassName("ui-datepicker-month")));
            selectMonth.SelectByText(DateHelper.MonthNumberToShortName[Program.DateTo.ToString("MM")]);
            selectYear = new SelectElement(driver.FindElement(By.ClassName("ui-datepicker-year")));
            selectYear.SelectByText(Program.DateTo.ToString("yyyy"));
            calendarPicker = driver.FindElement(By.ClassName("ui-datepicker-calendar"));
            calendarPicker.FindElement(By.LinkText(Program.DateTo.ToString("dd"))).Click();

            driver.FindElement(By.Id("at")).Click();

            driver.FindElement(By.Id("issuer-profile-export-button")).Click();

            Thread.Sleep(30000);

            driver.Close();
            driver.Quit();
        }
    }
}
