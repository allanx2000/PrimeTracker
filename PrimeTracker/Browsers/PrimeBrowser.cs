using System;
using System.Collections.Generic;
using System.Linq;
using Innouvous.Utils;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PrimeTracker.Models;

namespace PrimeTracker.Browsers
{
    public partial class PrimeBrowser : IBrowser
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


        public PrimeBrowser()
        {
            LoadBrowser();
        }

        private JsonSerializer jser = new JsonSerializer();
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

        //TODO: Turn into Enum
        public const string Days_30 = "Last 30 Days";
        public const string Days_60 = "Last 60 Days";
        public const string Days_90 = "Last 90 Days";


        public List<Video> GetRecentlyAddedVideos()
        {
            return GetRecentlyAddedVideos(Days_30);
        }

        public List<Video> GetRecentlyAddedVideos(string days)
        {
            /*
             * Check last checked date
             * Check recently Added
             * 
             * * Navigate https://www.amazon.com/
             */

            //if (!LoggedIn)
            //    Login();

            driver.Navigate().GoToUrl(AmazonBaseUrl + "s/ref=nb_sb_noss?url=search-alias%3Dinstant-video");

            FindAndGotoLink("Movies");

            try
            {
                FindAndGotoLink("Included with Prime");
            }
            catch (Exception e)
            {
                var el = driver.FindElementByLinkText("Included with Prime");
                el.Click();
            }

            FindAndGotoLink(days);

            FindRatingLink(4);

            FindAndClickCheckbox("Movies", 1);
            FindAndClickSpan("2010 & Newer");
            FindAndClickSpan("2000 - 2009");
            FindAndGotoLink("High Definition [HD]");

            var videos = ScrapeResults(VideoType.Movie);

            return videos;
        }

        private List<Video> ScrapeResults(VideoType type)
        {
            var videos = new List<Video>();

            try
            {
                ParseVideosV1(videos, type);
            }
            catch (Exception e)
            {
                ParseVideosV2(videos, type);
            }


            return videos;
        }

        private void ParseVideosV2(List<Video> videos, VideoType type)
        {
            while (true)
            {
                var resultList = driver.FindElementByClassName("s-result-list");
                var links = resultList.FindElements(By.XPath(".//h5/a"));

                foreach (var a in links)
                {
                    string link = a.GetAttribute("href");

                    if (link.Contains("/dp/"))
                    {

                        string id = ExtractAmazonIdFromLink(link);
                        string title = a.Text.Trim();

                        Video video = new Video()
                        {
                            AmazonId = id,
                            Title = title,
                            Type = type,
                            Url = link
                        };

                        videos.Add(video);
                    }
                }

                try
                {
                    var pagination = driver.FindElementByClassName("a-pagination");
                    var next = pagination.FindElement(By.PartialLinkText("Next"));
                    driver.Navigate().GoToUrl(next.GetAttribute("href"));
                }
                catch (Exception e)
                {
                    return;
                }
            }
        }

        private void ParseVideosV1(List<Video> videos, VideoType type)
        {
            do
            {
                var resultCol = driver.FindElementById("resultsCol");
                var items = resultCol.FindElements(By.ClassName("s-item-container"));

                foreach (var item in items)
                {
                    try
                    {
                        var a = item.FindElement(By.ClassName("s-access-detail-page"));

                        string title = a.GetAttribute("title");
                        string link = a.GetAttribute("href");
                        //string id =

                        string id = ExtractAmazonIdFromLink(link);

                        Video video = new Video()
                        {
                            AmazonId = id,
                            Title = title,
                            Type = type,
                            Url = link
                        };

                        videos.Add(video);
                    }
                    catch (Exception e)
                    {
                        //Not a link
                    }
                }

                try
                {
                    var pgNext = driver.FindElementById("pagnNextLink");
                    driver.Navigate().GoToUrl(pgNext.GetAttribute("href"));
                    //pgNext.Click();
                }
                catch (Exception e)
                {
                    //Last Page?
                    return;
                }
            } while (true);
        }

        private static string ExtractAmazonIdFromLink(string link)
        {
            int start = link.IndexOf("dp/") + 3;
            int end = link.IndexOf("/", start + 1);
            string id = link.Substring(start, end - start);
            return id;
        }


        private void FindAndClickSpan(string text)
        {
            SafeExecutor.ExecuteAction(() =>
            {
                var ln = GetLeftNav(driver);
                var el = ln.FindElement(By.XPath($".//span[text()='{text}']"));

                el.Click(); // ln.FindElement(By.LinkText(linkText)).GetAttribute("href");

            }, "Cannot find span: " + text);
        }

        private void FindAndClickCheckbox(string text, int idx)
        {
            try
            {
                var ln = GetLeftNav(driver);

                var matches = ln.FindElements(By.XPath(".//span[text()='" + text + "']"));

                var el = idx >= matches.Count ? matches[matches.Count - 1] : matches[idx];
           
                el.Click();

            }
            catch (Exception e)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    MessageBoxFactory.ShowError($"Click the {text} checkbox", "Manual Click");
                });
            }
        }

        private void FindRatingLink(int minRating)
        {
            SafeExecutor.ExecuteAction(() =>
            {
                var ln = GetLeftNav(driver);
                var el = ln.FindElement(By.ClassName("a-star-medium-" + minRating));

                el.Click(); // ln.FindElement(By.LinkText(linkText)).GetAttribute("href");

            }, "Cannot find Rating Button");
        }

        private void FindAndGotoLink(string linkText)
        {
            string link = SafeExecutor.ExecuteFunc(() =>
            {
                var ln = GetLeftNav(driver);
                return ln.FindElement(By.LinkText(linkText)).GetAttribute("href");
            });

            if (link == null)
                throw new Exception("Cannot find Link: " + linkText);

            driver.Navigate().GoToUrl(link);
        }

        private IWebElement GetLeftNav(ChromeDriver driver)
        {
            var ele = SafeExecutor.ExecuteFunc(() =>
            {
                try
                {
                    return driver.FindElementById(LeftNavContainer);
                }
                catch
                {
                    return driver.FindElementById("s-refinements");
                }
            });

            if (ele == null)
                throw new Exception("Cannot find " + LeftNavContainer);
            else
                return ele;
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

                if (!driver.PageSource.Contains("Your Lists"))
                    throw new Exception("Not logged in.");

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
                var v = videos[ex];
                v.AddTag(TagRecord.Create(-1, TagType.Expired));
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
