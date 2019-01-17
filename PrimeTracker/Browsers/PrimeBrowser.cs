using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private const string AmazonBaseUrl = "https://www.amazon.com/";
        private readonly string GP_VIDEO_DETAILS = "/gp/video/detail/";

        /*
* https://www.amazon.com/gp/video/detail/B07MBP7DTG/ref=atv_wtlp_wtl_4
* 
* https://www.amazon.com/s/gp/search/ref=sr_nr_p_n_feature_twelve_b_0?fst=as%3Aoff&rh=n%3A16183894011%2Cp_n_ways_to_watch%3A12007865011%2Cp_n_entity_type%3A14069184011%2Cp_n_feature_three_browse-bin%3A2651256011%7C2651255011%2Cp_72%3A3014476011%2Cp_n_feature_twelve_browse-bin%3A5824772011&bbn=16183894011&ie=UTF8&qid=1547393379&rnid=5824771011
* 
* https://www.amazon.com/gp/video/watchlist/movie/prime/ref=atv_wtlp_mv?page=1&sort=DATE_ADDED_DESC
* 
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

        public const int RetryTimeMS = 1000;

        public PrimeBrowser()
        {
            LoadBrowser();
        }

        private JsonSerializer jser = new JsonSerializer();
        private const int MaxAttempts = 5;
        private const string LeftNavContainer = "leftNavContainer";

        private void LoadBrowser()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            var options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true,
                UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore,
            };

            if (AppContext.Settings.HideChrome)
                options.AddArgument("headless");

            driver = new ChromeDriver(chromeDriverService, options);
        }

        internal void Quit()
        {
            driver.Quit();
        }

        private void Login()
        {
            //nav-signin-tooltip
            driver.Navigate().GoToUrl(AmazonBaseUrl);

            try
            {
                try
                {
                    driver.FindElementByLinkText("Start here.");
                }
                catch (Exception e)
                {
                    LoggedIn = true;
                    return;
                }

                //TODO: Need another check for already loogined in
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
                            input.SendKeys(AppContext.Settings.Username);
                            break;
                        case "password":
                            input.Click();
                            input.SendKeys(AppContext.Settings.Password);
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
             * 
             * * Navigate https://www.amazon.com/
* -Included in Prime
* -TV/Movie
* -Last 30 Days

             */

            if (!LoggedIn)
                Login();

            driver.Navigate().GoToUrl(AmazonBaseUrl + "s/ref=nb_sb_noss?url=search-alias%3Dinstant-video");

            FindAndGotoLink("Movies");
            FindAndGotoLink("Included with Prime");
            FindAndGotoLink("Last 30 Days");
            FindRatingLink("3 Stars & Up");
            FindAndClickSpan("2010 & Newer");
            FindAndClickSpan("2000 - 2009");
            FindAndGotoLink("High Definition [HD]");
            
            var videos = new List<Video>();

            return videos;
        }

        private void FindAndClickSpan(string text)
        {
            int attempt = 0;

            while (attempt < MaxAttempts)
            {
                try
                {
                    var ln = GetLeftNav(driver);
                    var el = ln.FindElement(By.XPath($".//span[text()='{text}']"));

                    el.Click(); // ln.FindElement(By.LinkText(linkText)).GetAttribute("href");
                    return;
                }
                catch (Exception e)
                {
                    attempt++;
                    Thread.Sleep(RetryTimeMS);
                }
            }

            throw new Exception("Cannot find span: " + text);


        }

        private void FindRatingLink(string v)
        {
            int attempt = 0;

            while (attempt < MaxAttempts)
            {
                try
                {
                    var ln = GetLeftNav(driver);
                    var el = ln.FindElement(By.ClassName("a-star-medium-3"));

                    el.Click(); // ln.FindElement(By.LinkText(linkText)).GetAttribute("href");
                    return;
                }
                catch (Exception e)
                {
                    attempt++;
                    Thread.Sleep(RetryTimeMS);
                }
            }

            throw new Exception("Cannot find Rating Button");
        }

        private void FindAndGotoLink(string linkText)
        {
            string link = null;
            int attempt = 0;

            while (attempt < MaxAttempts)
            {
                try
                {
                    var ln = GetLeftNav(driver);
                    link = ln.FindElement(By.LinkText(linkText)).GetAttribute("href");
                    break;
                }
                catch (Exception e)
                {
                    attempt++;
                    Thread.Sleep(RetryTimeMS);
                }
            }

            if (link == null)
                throw new Exception("Cannot find Link: " + linkText);

            driver.Navigate().GoToUrl(link);
        }

        private IWebElement GetLeftNav(ChromeDriver driver)
        {
            int attempt = 0;

            while (attempt < MaxAttempts)
            {
                try
                {
                    return driver.FindElementById(LeftNavContainer);
                }
                catch
                {
                    try
                    {
                        return driver.FindElementById("s-refinements");
                    }
                    catch
                    {
                        attempt++;
                        Thread.Sleep(RetryTimeMS);
                    }
                }
            }

            throw new Exception("Cannot find " + LeftNavContainer);
        }

        //gp/video/watchlist/tv/ref=atv_wtlp_tv? page = 1 & sort = DATE_ADDED_DESC
        //gp/video/watchlist/movie/ref=atv_wtlp_mv?page=1&sort=DATE_ADDED_DESC

        public List<Video> GetWatchListVideos(VideoType type = VideoType.Movie)
        {
            if (!LoggedIn)
                Login();

            //AmazonId, Video
            Dictionary<string, Video> videos = new Dictionary<string, Video>();

            //All
            int ctr = 1;


            while (true)
            {
                bool added = false;
                string url = CreateWatchListPath(type, ctr++, false);
                driver.Navigate().GoToUrl(url);

                foreach (var l in driver.FindElementsByTagName("a"))
                {
                    string link = l.GetAttribute("href");

                    if (link != null && link.Contains(GP_VIDEO_DETAILS))
                    {
                        string title = l.FindElement(By.TagName("img")).GetAttribute("alt");
                        string amazonId = ExtractId(link);

                        if (type == VideoType.TvSeason)
                            title += $" ({amazonId})";

                        videos[amazonId] = new Video()
                        {
                            AmazonId = amazonId,
                            Title = title,
                            Type = type,
                            Url = link
                        };

                        added = true;
                    }
                }

                if (!added)
                    break;
            }


            List<string> inPrime = new List<string>();

            ctr = 1;

            while (true)
            {
                bool added = false;
                string url = CreateWatchListPath(type, ctr++, true);
                driver.Navigate().GoToUrl(url);

                foreach (var l in driver.FindElementsByTagName("a"))
                {
                    string link = l.GetAttribute("href");

                    if (link != null && link.Contains(GP_VIDEO_DETAILS))
                    {
                        inPrime.Add(ExtractId(link));
                        added = true;
                    }
                }

                if (!added)
                    break;
            }

            var expired = videos.Keys.Except(inPrime);

            foreach (var ex in expired)
            {
                videos[ex].ExpiringDate = DateTime.Today;
            }

            return videos.Values.ToList();
        }

        private string ExtractId(string link)
        {
            //https://www.amazon.com/gp/video/detail/B00SZCMTEK/ref=atv_wtlp_wtl_0

            int start = link.IndexOf(GP_VIDEO_DETAILS) + GP_VIDEO_DETAILS.Length;
            int end = link.IndexOf('/', start + 1);

            string id = link.Substring(start, end - start);

            return id;
        }

        private string CreateWatchListPath(VideoType type, int page, bool isPrime)
        {
            String basePath = AmazonBaseUrl;

            switch (type)
            {
                case VideoType.TvSeason:
                    basePath += "gp/video/watchlist/tv/" + (isPrime ? "prime/" : "") + "ref=atv_wtlp_tv";
                    break;
                case VideoType.Movie:
                    basePath += "gp/video/watchlist/movie/" + (isPrime ? "prime/" : "") + "ref=atv_wtlp_mv";
                    break;
                default:
                    throw new NotImplementedException();
            }

            string qp = CreateWatchListParameters(page, "DATE_ADDED_DESC");

            return basePath + "?" + qp;
        }

        private string CreateWatchListParameters(int page, string sort = "DATE_ADDED_DESC")
        {
            string qp = string.Join("&", "page=" + page, "sort=" + sort);

            return qp;
        }

        public Video GetDetailsForVideo(Video dbRecord)
        {
            throw new NotImplementedException();
        }
    }
}
