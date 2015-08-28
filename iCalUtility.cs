using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using System;
using System.IO;
using System.Net;
using System.Text;

public class iCalUtility
{
    public static IICalendarCollection Deserialize(String iCalUri)
    {
        var wc = new WebClient();
        var result = wc.DownloadString(iCalUri);

        using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
        {
            var serializer = new iCalendarSerializer();
            var collection = (iCalendarCollection)serializer.Deserialize(memoryStream, Encoding.UTF8);
            return collection;
        }
    }
}