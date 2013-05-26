using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Drawing;
using System.Net;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Diagnostics;


public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack) //first page load
        {

            //Tests to see if config.xml file was configured by user
            XDocument configXML = XDocument.Load(Server.MapPath("App_Data/Config.xml"));
            var config = from c in configXML.Descendants("Config")
                         select c;

            foreach (var settings in config)
            {
                if (settings.Element("NewzbinUserName").Value == "") //Config not set yet
                {
                    //Bolds the config menu item which should be currently selected
                    LinkButton_Config.Style["font-weight"] = "bold";
                    LinkButton_Log.Style.Remove("font-weight");
                    LinkButton_Home.Style.Remove("font-weight");

                    Session["Config"] = "new";
                    BindData();
                    MultiView_tvNZB.SetActiveView(View_Config);
                }
                else
                {
                    //Bolds the home menu item which should be currently selected
                    LinkButton_Home.Style["font-weight"] = "bold";
                    LinkButton_Log.Style.Remove("font-weight");
                    LinkButton_Config.Style.Remove("font-weight");

                    MultiView_tvNZB.SetActiveView(View_Home); //Default view is the gridview
                    BindData();
                }
            }
        }

    }

    void BindData()
    {
        DataSet ds = new DataSet();
        ds.ReadXml(Server.MapPath("App_Data/Shows.xml"));
        gv.DataSource = ds;
        gv.DataBind();
    }

    protected void gv_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gv.PageIndex = e.NewPageIndex;
        BindData();
    }
    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gv.EditIndex = -1;
        BindData();
    }
    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        BindData();
        DataSet ds = gv.DataSource as DataSet;
        ds.Tables[0].Rows[gv.Rows[e.RowIndex].DataItemIndex].Delete();
        ds.WriteXml(Server.MapPath("App_Data/Shows.xml"));
        BindData();
    }
    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        int i = e.RowIndex;

        string name = (gv.Rows[e.RowIndex].FindControl("TextBox_Name") as TextBox).Text.Trim();
        string localSeason = (gv.Rows[e.RowIndex].FindControl("TextBox_LocalSeason") as TextBox).Text.Trim();
        string localEpisode = (gv.Rows[e.RowIndex].FindControl("TextBox_LocalEpisode") as TextBox).Text.Trim();
        string searchBy = (gv.Rows[e.RowIndex].FindControl("DropDownList_SearchBy") as DropDownList).SelectedValue;

        gv.EditIndex = -1;

        BindData();
        DataSet ds = gv.DataSource as DataSet;

        ds.Tables[0].Rows[i]["Name"] = name;
        ds.Tables[0].Rows[i]["LocalSeason"] = localSeason;
        ds.Tables[0].Rows[i]["LocalEpisode"] = localEpisode;
        ds.Tables[0].Rows[i]["SearchBy"] = searchBy;

        ds.WriteXml(Server.MapPath("App_Data/Shows.xml"));
        BindData();
    }
    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gv.EditIndex = e.NewEditIndex;
        BindData();
    }

    protected void Button_AddShow_Click(object sender, EventArgs e)
    {
        if (TextBox_Name.Text != "")
        {
            BindData();
            DataSet ds = new DataSet();
            ds.ReadXml(Server.MapPath("App_Data/Shows.xml"));
            ds.Tables[0].Rows.InsertAt(ds.Tables[0].NewRow(), ds.Tables[0].Rows.Count); //the new row is the last one in the dataset 
            int i = ds.Tables[0].Rows.Count - 1; //Gets index of the last row

            //Sets the latest episode data to the local season/episode so user doesn't have to look it up on tvrage.com
            ArrayList tvRageArray = new ArrayList();
            WebClient quickInfoClient = new WebClient();
            String localSeason = "";
            String localEpisode = "";

            var tvRageHtml = quickInfoClient.DownloadString("http://www.tvrage.com/quickinfo.php?show=" + TextBox_Name.Text);
            tvRageArray.AddRange(tvRageHtml.Split(new char[] { '^', '@', '\n' }));


            for (int x = 0; x < tvRageArray.Count; x++)
            {
                if (Convert.ToString(tvRageArray[x]) == "Latest Episode")
                {
                    localSeason = Convert.ToString(tvRageArray[x + 1]).Substring(0, 2);
                    localEpisode = Convert.ToString(tvRageArray[x + 1]).Substring(3, 2);
                }
            }

            ds.Tables[0].Rows[i]["Name"] = TextBox_Name.Text.Trim();
            ds.Tables[0].Rows[i]["LocalSeason"] = localSeason;
            ds.Tables[0].Rows[i]["LocalEpisode"] = localEpisode;
            ds.Tables[0].Rows[i]["LatestSeason"] = "";
            ds.Tables[0].Rows[i]["LatestEpisode"] = "";
            ds.Tables[0].Rows[i]["LatestAirtime"] = "";
            ds.Tables[0].Rows[i]["LatestAbsoluteEpisode"] = "";
            ds.Tables[0].Rows[i]["LatestTitle"] = "";
            ds.Tables[0].Rows[i]["NextEpisode"] = "";
            ds.Tables[0].Rows[i]["NextAirtime"] = "";
            ds.Tables[0].Rows[i]["NextTitle"] = "";
            ds.Tables[0].Rows[i]["NextAbsoluteEpisode"] = "";
            ds.Tables[0].Rows[i]["Format"] = "";
            ds.Tables[0].Rows[i]["Language"] = "";
            ds.Tables[0].Rows[i]["SearchBy"] = DropDownList_NewSearchBy.SelectedValue;
            ds.Tables[0].Rows[i]["ShowURL"] = "";
            ds.Tables[0].Rows[i]["WeeklyAirtime"] = "";
            ds.Tables[0].Rows[i]["LastUpdate"] = "";
            ds.Tables[0].Rows[i]["NextSeason"] = "";

            //Removes entries in the textboxes
            TextBox_Name.Text = "";
            DropDownList_NewSearchBy.SelectedIndex = 0;

            ds.WriteXml(Server.MapPath("App_Data/Shows.xml"));
            gv.DataBind();
            BindData();
        }
    }
    protected string CheckEval(string eval) //Prevents from just an "x" showing up in Next Title when there's no next info
    {
        if (eval == "x ")
            return "";
        else
            return eval;
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string weeklyDate = (e.Row.FindControl("Label_WeeklyAirtime") as Label).Text;

            if (weeklyDate.Length > 10 && weeklyDate.Substring(0, 11) == DateTime.Now.ToString("MMM/dd/yyyy"))
                e.Row.BackColor = Color.Pink;
        }
    }

    public static void SortXML()
    {
        XDocument showsXML = XDocument.Load(HttpContext.Current.Server.MapPath("App_Data/Shows.xml"));
        var shows = from c in showsXML.Descendants("Show")
                    orderby GetSafeDate(c.Element("WeeklyAirtime").Value) ascending
                    select c;

        showsXML.Root.ReplaceAll(shows);
        showsXML.Save(HttpContext.Current.Server.MapPath("App_Data/Shows.xml"));
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

    protected void LinkButton_Config_Click(object sender, EventArgs e)
    {
        LinkButton_Config.Style["font-weight"] = "bold";
        LinkButton_Log.Style.Remove("font-weight");
        LinkButton_Home.Style.Remove("font-weight");
        Label_Warning.Visible = false;

        MultiView_tvNZB.SetActiveView(View_Config); //Default view is the gridview
    }
    protected void LinkButton_Log_Click(object sender, EventArgs e)
    {
        LinkButton_Log.Style["font-weight"] = "bold";
        LinkButton_Home.Style.Remove("font-weight");
        LinkButton_Config.Style.Remove("font-weight");
        MultiView_tvNZB.SetActiveView(View_Log); //Default view is the gridview
    }

    protected void LinkButton_Home_Click(object sender, EventArgs e)
    {
        LinkButton_Home.Style["font-weight"] = "bold";
        LinkButton_Log.Style.Remove("font-weight");
        LinkButton_Config.Style.Remove("font-weight");
        MultiView_tvNZB.SetActiveView(View_Home); //Default view is the gridview
    }
    protected void Button_Config_Save_Click(object sender, EventArgs e)
    {
        String errorText = "";

        if (TextBox_newzbinUserName.Text == "")
            errorText = "Newzbin username missing. ";

        if (TextBox_newzbinPassword.Text == "")
            errorText += "Newzbin password missing. ";

        if (TextBox_sabAddress.Text == "")
            errorText += "Sabnzb address missing. ";

        if (TextBox_sabAPI.Text == "")
            errorText += "Sabnzb api missing. ";

        if (TextBox_newzbinPassword.Text != TextBox_newzbinPasswordReenter.Text)
            errorText += "Passwords do not match. ";

        if (errorText.Length > 1)
        {
            Label_Warning.Visible = true;
            Label_Warning.Text = errorText;
        }
        else
        {
            SaveConfig();
            Label_Warning.Text = "Saved";
            Label_Warning.Visible = true;
            Session["Config"] = "old"; //User updated so no longer "new"
        }
    }

    protected void SaveConfig()
    {
        XDocument configXML = XDocument.Load(Server.MapPath("App_Data/Config.xml"));
        var config = from c in configXML.Descendants("Config")
                     select c;

        foreach (var settings in config)
        {
            settings.SetElementValue("NewzbinUserName", TextBox_newzbinUserName.Text);
            settings.SetElementValue("NewzbinPassword", TextBox_newzbinPassword.Text);
            settings.SetElementValue("ServerAddress", TextBox_sabAddress.Text);
            settings.SetElementValue("ApiKey", TextBox_sabAPI.Text);
        }

        configXML.Save(Server.MapPath("App_Data/Config.xml"));
    }
    protected void View_Config_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack) //first page load
        {
            Label_Warning.Visible = false;

            if ((String)Session["Config"] != "new") //User has already updated the config file
            {
                XDocument configXML = XDocument.Load(Server.MapPath("App_Data/Config.xml"));
                var config = from c in configXML.Descendants("Config")
                             select c;

                foreach (var settings in config)
                {
                    TextBox_newzbinUserName.Text = settings.Element("NewzbinUserName").Value;
                    TextBox_newzbinPassword.Attributes["value"] = settings.Element("NewzbinPassword").Value;
                    TextBox_newzbinPasswordReenter.Attributes["value"] = settings.Element("NewzbinPassword").Value;
                    TextBox_sabAddress.Text = settings.Element("ServerAddress").Value;
                    TextBox_sabAPI.Text = settings.Element("ApiKey").Value;
                }
            }
        }
    }

    protected void View_Log_Load(object sender, EventArgs e)
    {
        try
        {
            using (StreamReader rdr = File.OpenText(Server.MapPath("App_Data/Log.txt")))
            {
                Label_log.Text = rdr.ReadToEnd();
                rdr.Close();
            }
        }
        catch
        {
            Label_log.Text = "Couldn't open " + Convert.ToString(Server.MapPath("App_Data/Log.txt"));
        }


        //EventLog aLog = new EventLog();
        //aLog.Log = "Application";
        //aLog.MachineName = ".";  // Local machine
        //ArrayList eventLogEntries = new ArrayList();

        //foreach (EventLogEntry entry in aLog.Entries)
        //{

        //    if (entry.EntryType == EventLogEntryType.Information)
        //    {
        //        if (entry.Source == "tvNZB")
        //        {
        //            if (entry.Message != "Service stopped successfully." && entry.Message != "Service started successfully." && entry.Message != "Test Message")
        //            {
        //                eventLogEntries.Add("<p>" + entry.Message + "</p>");
        //            }
        //        }
        //    }
        //}

        //Label_log.Text = Convert.ToString(eventLogEntries[eventLogEntries.Count - 1]); //Gets the most recent entry
    }
}
