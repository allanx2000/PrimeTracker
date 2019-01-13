using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PrimeTracker.Models;

namespace PrimeTracker.Browsers
{
    public class PrimeBrowser : IBrowser
    {
        /*
* https://www.amazon.com/gp/video/watchlist/tv/ref=atv_wtlp_tv?page=1&sort=DATE_ADDED_DESC
* https://www.amazon.com/gp/video/watchlist/movie/ref=atv_wtlp_mv?page=1&sort=DATE_ADDED_DESC
* https://www.amazon.com/gp/video/detail/B07MBP7DTG/ref=atv_wtlp_wtl_4
* 
* https://www.amazon.com/s/gp/search/ref=sr_nr_p_n_feature_twelve_b_0?fst=as%3Aoff&rh=n%3A16183894011%2Cp_n_ways_to_watch%3A12007865011%2Cp_n_entity_type%3A14069184011%2Cp_n_feature_three_browse-bin%3A2651256011%7C2651255011%2Cp_72%3A3014476011%2Cp_n_feature_twelve_browse-bin%3A5824772011&bbn=16183894011&ie=UTF8&qid=1547393379&rnid=5824771011
* 
* https://www.amazon.com/gp/video/watchlist/movie/prime/ref=atv_wtlp_mv?page=1&sort=DATE_ADDED_DESC
* 
* Navigate https://www.amazon.com/s/ref=nb_sb_noss?url=search-alias%3Dinstant-video
* -Included in Prime
* -TV/Movie
* -Last 30 Days
*/

        private ChromeDriver driver;
        protected ChromeDriver Driver
        {
            get
            {
                return driver;
            }

        }

        public bool LoggedIn { get; private set; }

        public PrimeBrowser()
        {
            LoadBrowser();
        }

        private JsonSerializer jser = new JsonSerializer();

        private void LoadBrowser()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            var options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true,
                UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore,
            };

            //options.AddArgument("headless");

            driver = new ChromeDriver(chromeDriverService, options);

            Login();
        }

        private void Login()
        {
            //nav-signin-tooltip
            driver.Navigate().GoToUrl("https://amazon.com");

            try
            { 
                var toggleButton = driver.FindElementById("nav-signin-tooltip");
                var link = toggleButton.FindElement(By.TagName("a"));
                link.Click();

                var form = driver.FindElementByName("signIn");

                var inputs = form.FindElements(By.TagName("input"));

                List<string> ids = new List<string>();

                foreach (var input in inputs)
                {
                    string name = input.GetAttribute("name");

                    switch (name)
                    {
                        case "email":
                            input.Click();
                            input.SendKeys("allan.xiao@gmail.com");
                            break;
                        case "password":
                            input.Click();
                            input.SendKeys("netsurfer@1");
                            break;
                        case "rememberMe":
                            input.Click();
                            break;
                    }
                    ids.Add(name);
                }

                var signInButton = form.FindElement(By.Id("signInSubmit"));
                signInButton.Click();

                LoggedIn = true;
            }
            catch (Exception e)
            {
                //Already logged in
            }
        }

        public List<Video> GetRecentlyAddedVideos()
        {
            /*
             * Check last checked date
             * Check recently Added
             */ 
            throw new NotImplementedException();
        }

        public List<Video> GetWatchListVideos(VideoType type = VideoType.Movie)
        {
            throw new NotImplementedException();
        }

        public Video GetDetailsForVideo(Video dbRecord)
        {
            throw new NotImplementedException();
        }
    }
}
