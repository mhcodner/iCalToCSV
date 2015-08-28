using DDay.iCal;
using LINQtoCSV;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

public class CSVHub : Hub
{
    string url = "http://web1.cronulla-h.schools.nsw.edu.au/?plugin=all-in-one-event-calendar&controller=ai1ec_exporter_controller&action=export_events&no_html=true";
    CsvContext cc = new CsvContext();
    CsvFileDescription InputFileDescription = new CsvFileDescription
    {
        SeparatorChar = ',',
        FirstLineHasColumnNames = true
    };

    public void GetAllEvents()
    {
        List<CSVEvent> Events = new List<CSVEvent>();
        try
        {
            IICalendarCollection feed = iCalUtility.Deserialize(url);
            foreach (IEvent e in feed.SelectMany(e => e.Events))
            {
                CSVEvent csvEvent = new CSVEvent
                {
                    EventName = e.Summary,
                    YearLevel = e.Categories.FirstOrDefault(),
                    Cost = e.Properties.Get<string>("X-COST"),
                    Location = e.Location,
                    StartDate = e.Start.Date,
                    FinishDate = e.DTEnd.Date,
                    OtherDetails = e.Description
                };
                if (!String.IsNullOrEmpty(csvEvent.EventName))
                    csvEvent.EventName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(csvEvent.EventName)); // Fixes encoding issues

                if (!String.IsNullOrEmpty(csvEvent.OtherDetails))
                    csvEvent.OtherDetails = Encoding.UTF8.GetString(Encoding.Default.GetBytes(csvEvent.OtherDetails)); // Fixes encoding issues

                if (e.Start.HasTime && !e.IsAllDay)
                    csvEvent.StartTime = e.Start.Date.Add(e.Start.TimeOfDay);

                if (e.End.HasTime)
                    csvEvent.FinishTime = e.DTEnd.Date.Add(e.DTEnd.TimeOfDay);

                int PlaceHolder; // Only used in the output of TryParse
                if (!String.IsNullOrEmpty(csvEvent.YearLevel))
                    csvEvent.YearLevel = csvEvent.YearLevel.Replace("Year ", "").Replace("Whole School", "");

                if (!int.TryParse(csvEvent.YearLevel, out PlaceHolder))
                    csvEvent.YearLevel = String.Empty; // eDiary can only handle numbers, blank year level is treated as whole school

                DateTime FirstDayOfYear = new DateTime(DateTime.Now.Year, 1, 1);
                if (csvEvent.StartDate > FirstDayOfYear) // Only get events from this year onwards to prevent massive files
                    Events.Add(csvEvent);
            }
        }
        catch (Exception ex)
        {
            using (StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/log.txt"), true))
            {
                writer.WriteLine("Message :" + ex.Message + "\n" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }
        cc.Write(Events, HttpContext.Current.Server.MapPath("~/events.csv"), InputFileDescription);
        Clients.Caller.Download("events.csv");
    }

    public void GetNewEvents()
    {
        List<CSVEvent> AllEvents = new List<CSVEvent>();
        List<CSVEvent> NewEvents = new List<CSVEvent>();
        List<CSVEvent> LastImport = new List<CSVEvent>();
        try
        {
            IICalendarCollection feed = iCalUtility.Deserialize(url);
            foreach (IEvent e in feed.SelectMany(e => e.Events))
            {
                CSVEvent ev = new CSVEvent
                {
                    EventName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(e.Summary)),
                    YearLevel = e.Categories.FirstOrDefault(),
                    Cost = e.Properties.Get<string>("X-COST"),
                    Location = e.Location,
                    StartDate = e.Start.Date,
                    FinishDate = e.DTEnd.Date,
                    OtherDetails = e.Description
                };
                if (e.Start.HasTime && !e.IsAllDay)
                {
                    ev.StartTime = e.Start.Date.Add(e.Start.TimeOfDay);
                }
                if (e.End.HasTime)
                {
                    ev.FinishTime = e.DTEnd.Date.Add(e.DTEnd.TimeOfDay);
                }
                AllEvents.Add(ev);
            }
            if (File.Exists(HttpContext.Current.Server.MapPath("~/LastImport.csv")))
            {
                LastImport = cc.Read<CSVEvent>(HttpContext.Current.Server.MapPath("~/LastImport.csv"), InputFileDescription).ToList();
            }
            NewEvents = AllEvents.Except(LastImport).ToList();
        }
        catch (Exception ex)
        {
            using (StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/log.txt"), true))
            {
                writer.WriteLine("Message :" + ex.Message + "\n" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }
        if (NewEvents.Count > 0)
        {
            cc.Write(NewEvents, HttpContext.Current.Server.MapPath("~/NewEvents.csv"), InputFileDescription);
            cc.Write(AllEvents, HttpContext.Current.Server.MapPath("~/LastImport.csv"), InputFileDescription);
            Clients.Caller.Download("NewEvents.csv");
        }
        else
        {
            Clients.Caller.NoNewEvents();
        }
    }

    public void reset()
    {
        File.Delete(HttpContext.Current.Server.MapPath("~/LastImport.csv"));
    }
}
