using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Globalization;
using Microsoft.Win32;

namespace tvNZB
{
    public partial class tvNZB : ServiceBase
    {
        Timer timer = new Timer();

        //Variables read from config file
        static string apiKey;
        static string newzbinUsr;
        static string newzbinPass;
        static string serverAddress;
        static string xmlLocation; // C:\Test\Shows.xml
        static string installPath;
        //static string log;


        public tvNZB()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {                        
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            //timer.Interval = 900000; //every 15 minutes
            timer.Interval = 300000; //every 5 minutes
            timer.Enabled = true;

            //System.Diagnostics.Debugger.Launch();  Luanches debugger when services starts
            
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            //log = ""; //Reset log file

            LoadConfig(); //Load user config file

            if (newzbinUsr == "username" || newzbinUsr == "") 
            {
                return; //Doesn't execute program until user has updated Config.xml
            }

            //Saves output to log file
            StreamWriter log_out;
            try
            {
                log_out = new StreamWriter(@installPath + "App_Data\\Log.txt");
            }
            catch
            {
                return; //Couldn't open log file
            }

            // Direct standard output to the log file. 
            Console.SetOut(log_out);

            GetTvRageData();
            Compare();
            SortXML(); //Sorts the xml file after updating
            //SaveLogEvent();

            log_out.Close(); 
        }        

        public static void LoadConfig()
        {
            installPath = Convert.ToString(Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\tvNZB\", "tvNZBInstallPath", ""));

            if (installPath == "") //Registry is located in a different part of the registry if the machine is 64bit windows
                installPath = Convert.ToString(Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\tvNZB\", "tvNZBInstallPath", ""));

            xmlLocation = installPath + "App_Data\\Shows.xml";

            //Gets current directory of application and appends Config.xml
            XDocument configXML = XDocument.Load(@installPath + "App_Data\\Config.xml");
            var config = from c in configXML.Descendants("Config")
                         select c;

            foreach (var settings in config)
            {
                if (settings.Element("ShowsXmlLocation").Value == "")
                    xmlLocation = @installPath + "App_Data\\Shows.xml"; //xml file located next to the exe
                else
                    xmlLocation = settings.Element("ShowsXmlLocation").Value;

                newzbinUsr = settings.Element("NewzbinUserName").Value;
                newzbinPass = settings.Element("NewzbinPassword").Value;
                serverAddress = settings.Element("ServerAddress").Value;
                apiKey = settings.Element("ApiKey").Value;
            }
        }

        public static void GetTvRageData()
        {
            TimeSpan diff = new TimeSpan();
            XDocument showsXML = XDocument.Load(@xmlLocation);
            WebClient quickInfoClient = new WebClient();
            WebClient absoluteEpisodeClient = new WebClient();
            ArrayList tvRageArray = new ArrayList();
            ArrayList tvRageAbsEpArray = new ArrayList();


            //This makes sure the tvrage api is called only every 2 hours
            var q = from c in showsXML.Descendants("Show")
                    select c;

            foreach (var v in q)
            {
                if (v.Element("LastUpdate").Value == "") //LastUpdate is blank so it must have just been added
                    break;

                diff = DateTime.Now - Convert.ToDateTime(v.Element("LastUpdate").Value);

                if (diff.TotalMinutes < 360) //Will only update tvrage data every 6 hours
                {
                    //Console.WriteLine("Will update tvrage data in " + diff.TotalMinutes.ToString() + " minutes.");
                    //Console.WriteLine();

                    //log += "<p>Will update tvrage data in " + diff.TotalMinutes.ToString() + " minutes.</p>";
                    return;
                }
                else
                {
                    break; //Breaks out of loop if tvrage needs to be updated
                }
            }

            var shows = from c in showsXML.Descendants("Show")
                        select c;

            foreach (var show in shows)
            {
                var tvRageHtml = quickInfoClient.DownloadString("http://www.tvrage.com/quickinfo.php?show=" + show.Element("Name").Value);
                tvRageArray.AddRange(tvRageHtml.Split(new char[] { '^', '@', '\n' }));

                //Updates all shows with the new Last Update
                show.SetElementValue("LastUpdate", Convert.ToString(DateTime.Now));

                //Clear next episode data just in case it isn't displayed on the quickinfo
                //Don't want outdated information
                show.SetElementValue("NextSeason", "");
                show.SetElementValue("NextEpisode", "");
                show.SetElementValue("NextAirtime", "");
                show.SetElementValue("NextTitle", "");


                for (int x = 0; x < tvRageArray.Count; x++)
                {
                    if (Convert.ToString(tvRageArray[x]) == "Latest Episode")
                    {
                        show.SetElementValue("LatestSeason", Convert.ToString(tvRageArray[x + 1]).Substring(0, 2));
                        show.SetElementValue("LatestEpisode", Convert.ToString(tvRageArray[x + 1]).Substring(3, 2));
                        show.SetElementValue("LatestTitle", tvRageArray[x + 2]);
                        show.SetElementValue("LatestAirtime", tvRageArray[x + 3]);
                    }

                    if (Convert.ToString(tvRageArray[x]) == "Next Episode")
                    {
                        show.SetElementValue("NextSeason", Convert.ToString(tvRageArray[x + 1]).Substring(0, 2));
                        show.SetElementValue("NextEpisode", Convert.ToString(tvRageArray[x + 1]).Substring(3, 2));
                        show.SetElementValue("NextTitle", tvRageArray[x + 2]);
                        show.SetElementValue("NextAirtime", tvRageArray[x + 3]);
                    }

                    if (Convert.ToString(tvRageArray[x]) == "Show URL")
                        show.SetElementValue("ShowURL", Convert.ToString(tvRageArray[x + 1]));

                    if (Convert.ToString(tvRageArray[x]) == "Airtime") //Gets the hour and date the show airs
                        show.SetElementValue("WeeklyAirtime", show.Element("NextAirtime").Value + " " + Convert.ToString(tvRageArray[x + 1]).Replace("at ", ""));



                }

                //Absolute episode search
                if (show.Element("SearchBy").Value == "AbsoluteEpisode")
                {
                    tvRageAbsEpArray.Clear();
                    var tvRageShowHtml = absoluteEpisodeClient.DownloadString(show.Element("ShowURL").Value);

                    if (tvRageShowHtml.IndexOf("<b>Latest Episode: </b>") > -1)
                    {
                        tvRageAbsEpArray.AddRange(tvRageShowHtml.Split(new string[] { "x" + show.Element("LatestEpisode").Value + "'>" }, StringSplitOptions.None));
                        var latestAbsoluteEpisode = Convert.ToString(tvRageAbsEpArray[1]).Split(new char[] { ':' });
                        show.SetElementValue("LatestAbsoluteEpisode", latestAbsoluteEpisode[0]);
                    }

                    if (tvRageShowHtml.IndexOf("<b>Next Episode: </b>") > -1)
                    {
                        tvRageAbsEpArray.AddRange(tvRageShowHtml.Split(new string[] { "x" + show.Element("NextEpisode").Value + "'>" }, StringSplitOptions.None));
                        var nextAbsoluteEpisode = Convert.ToString(tvRageAbsEpArray[3]).Split(new char[] { ':' });
                        show.SetElementValue("NextAbsoluteEpisode", nextAbsoluteEpisode[0]);
                    }
                }

                tvRageArray.Clear();
            }

            showsXML.Save(@xmlLocation);
        }

        public static void Compare()
        {
            XDocument showsXML = XDocument.Load(@xmlLocation);
            var shows = from c in showsXML.Descendants("Show")
                        select c;

            foreach (var show in shows)
            {
                //Latest Episode Comparison
                if (string.IsNullOrEmpty(show.Element("LatestEpisode").Value) == false) //Make sure data is available for latest episode info
                {
                    if (int.Parse(show.Element("LocalSeason").Value) == int.Parse(show.Element("LatestSeason").Value)
                        && int.Parse(show.Element("LocalEpisode").Value) < int.Parse((show.Element("LatestEpisode").Value))
                        || int.Parse(show.Element("LocalSeason").Value) < int.Parse(show.Element("LatestSeason").Value))
                    {
                        if (Convert.ToDateTime(show.Element("LatestAirtime").Value) <= DateTime.Today //LatestAirtime is greater than or equal to current date
                            && show.Element("LatestAirtime").Value.Length > 6 //ex Tvrage only posted '2008' for the airdate
                            && DateTime.Today < Convert.ToDateTime(show.Element("LatestAirtime").Value).AddDays(14)) //current time is less than two weeks from next air date
                        {
                            Console.Write("<p><b>Latest Episode Search: </b>");
                            //log += "<p><b>Latest Episode Search: </b>";
                            Search(show.Element("Name").Value, "Latest");
                        }
                    }
                }

                //Next Episode Comparison
                if (string.IsNullOrEmpty(show.Element("NextEpisode").Value) == false) //Make sure data is available for latest episode info
                {
                    if (int.Parse(show.Element("LocalSeason").Value) == int.Parse(show.Element("NextSeason").Value)
                        && int.Parse(show.Element("LocalEpisode").Value) < int.Parse((show.Element("NextEpisode").Value))
                        || int.Parse(show.Element("LocalSeason").Value) < int.Parse(show.Element("NextSeason").Value))
                    {
                        if (Convert.ToDateTime(show.Element("NextAirtime").Value) <= DateTime.Today //LatestAirtime is greater than or equal to current date
                            && show.Element("NextAirtime").Value.Length > 6 //ex Tvrage only posted '2008' for the airdate
                            && DateTime.Today < Convert.ToDateTime(show.Element("NextAirtime").Value).AddDays(14)) //current time is less than two weeks from next air date
                        {
                            Console.Write("<p><b>Next Episode Search: </b>");
                            //log += "<p><b>Next Episode Search: </b>";
                            Search(show.Element("Name").Value, "Next");
                        }
                    }
                }
            }
        }

        public static void Search(string showName, string episodeType)
        {
            String query = "";
            ArrayList responseArray = new ArrayList();
            ArrayList queryArray = new ArrayList();
            XDocument showsXML = XDocument.Load(@xmlLocation);

            var shows = from c in showsXML.Descendants("Show")
                        where c.Element("Name").Value == showName
                        select c;

            foreach (var show in shows)
            {
                var request = (HttpWebRequest)WebRequest.Create("http://www.newzbin.com/api/reportfind/");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                using (var requestStream = request.GetRequestStream())
                using (var writer = new StreamWriter(requestStream))
                {
                    query = "username=" + newzbinUsr + "&password=" + newzbinPass + "&retention=14&q=" + show.Element("Name").Value + " ";
                    //query = "username=" + newzbinUsr + "&password=" + newzbinPass + "&retention=14&u_post_smaller_than=600&q=" + show.Element("Name").Value + " ";

                    if (show.Element("SearchBy").Value.Equals("EpisodeTitle")) //Used for hard to find shows or weird season/episode numbers
                    {
                        query += episodeType == "Latest" ? show.Element("LatestTitle").Value : show.Element("NextTitle").Value;
                    }
                    else if (show.Element("SearchBy").Value.Equals("AbsoluteEpisode")) //Naruto Shippuuden - 149
                    {
                        query += episodeType == "Latest" ? show.Element("LatestAbsoluteEpisode").Value : show.Element("NextAbsoluteEpisode").Value;
                    }
                    else if (show.Element("SearchBy").Value.Equals("YYYY-MM-DD")) //The Daily Show - 2010-02-23 - Jeff Garlin
                    {
                        query += episodeType == "Latest" ? Convert.ToDateTime(show.Element("LatestAirtime").Value).ToString("yyyy-MM-dd") :
                                                           Convert.ToDateTime(show.Element("NextAirtime").Value).ToString("yyyy-MM-dd");
                    }
                    else //Lost - 6x05 - Lighthouse
                    {
                        query += episodeType == "Latest" ? show.Element("LatestSeason").Value.TrimStart('0') + "x" + show.Element("LatestEpisode").Value :
                                                           show.Element("NextSeason").Value.TrimStart('0') + "x" + show.Element("NextEpisode").Value;
                    }

                    queryArray.Clear();
                    queryArray.AddRange(query.Split(new string[] { "&q=" }, StringSplitOptions.None)); //Removes username and password info from being displayed
                    Console.WriteLine(Convert.ToString(queryArray[1])); //Displays the query
                    //log += Convert.ToString(queryArray[1]);
                    writer.Write(query);
                }
                using (var responseStream = request.GetResponse().GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    var result = reader.ReadToEnd();

                    if (string.IsNullOrEmpty(result) == false)
                    {
                        responseArray.Clear();
                        responseArray.AddRange(result.Split(new char[] { '\n', '\t' }));
                        Console.WriteLine("Match Found: " + Convert.ToString(responseArray[1])); //responseArray[1] is the first newzbin id result
                        Console.WriteLine();
                        //log += "<p>Match Found: " + Convert.ToString(responseArray[1]) + "<p>";

                        WebClient client = new WebClient();

                        //Sends nzbid to sabnzbd
                        client.DownloadString(serverAddress + "api?mode=addid&name=" + Convert.ToString(responseArray[1]) + "&apikey=" + apiKey + "&cat=tv");

                        //Updates what is currently saved to disk now
                        if (episodeType == "Latest")
                        {
                            show.SetElementValue("LocalSeason", show.Element("LatestSeason").Value);
                            show.SetElementValue("LocalEpisode", show.Element("LatestEpisode").Value);
                        }
                        else
                        {
                            show.SetElementValue("LocalSeason", show.Element("NextSeason").Value);
                            show.SetElementValue("LocalEpisode", show.Element("NextEpisode").Value);
                        }
                    }
                    else
                    {
                        //log += "<p>No Match: " + Convert.ToString(responseArray[1]) + "<p>";
                        Console.WriteLine("No Match");
                        Console.WriteLine();
                    }
                }
            }

            showsXML.Save(@xmlLocation); //Saves xml file after each search
        }

        public static void SortXML()
        {
            XDocument showsXML = XDocument.Load(@xmlLocation);
            var shows = from c in showsXML.Descendants("Show")
                        orderby GetSafeDate(c.Element("WeeklyAirtime").Value) ascending
                        select c;

            showsXML.Root.ReplaceAll(shows);
            showsXML.Save(@xmlLocation);
        }

        public static DateTime GetSafeDate(string proposedDate)
        {
            // Returns a non-null DateTime even if proposed date can't be parsed

            DateTime safeDate;
            string format = "MMM/dd/yyyy hh:mm tt"; //Mar/18/2010 10:00 pm
            string[] split = proposedDate.Split(new Char[] { ' ' });

            try
            {
                //Trys parsing Mar/19/2010 10:00 pm (removes day because some shows date and day are not the same)
                safeDate = DateTime.ParseExact(split[0] + " " + split[2] + " " + split[3], format, CultureInfo.InvariantCulture);
            }
            catch
            {
                safeDate = DateTime.MaxValue;
            }
            return safeDate;
        }



        protected override void OnStop()
        {
            timer.Enabled = false;
        }

        //public static void SaveLogEvent()
        //{
        //    string strMyApp = "tvNZB";

        //    if (!System.Diagnostics.EventLog.SourceExists(strMyApp))
        //        System.Diagnostics.EventLog.CreateEventSource(strMyApp, "Application");

        //    EventLog MyEventLog = new EventLog();
        //    MyEventLog.Source = strMyApp;
        //    MyEventLog.WriteEntry(log, EventLogEntryType.Information);
        //}

        
    }
}
