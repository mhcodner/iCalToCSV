using LINQtoCSV;
using System;

public class CSVEvent : IEquatable<CSVEvent>
{
    [CsvColumn(Name = "Name (required)", FieldIndex = 1)]
    public string EventName { get; set; }
    [CsvColumn(Name = "Cost", FieldIndex = 2, CanBeNull = true, OutputFormat = "C")]
    public string Cost { get; set; }
    [CsvColumn(Name = "Transport", FieldIndex = 3)]
    public string Transport { get; set; }
    [CsvColumn(Name = "Location", FieldIndex = 4)]
    public string Location { get; set; }
    [CsvColumn(Name = "Start Date (required)", FieldIndex = 5, OutputFormat = "dd/MM/yyyy")]
    public DateTime StartDate { get; set; }
    [CsvColumn(Name = "Start Time", FieldIndex = 6, OutputFormat = "hh:mmtt")]
    public DateTime? StartTime { get; set; }
    [CsvColumn(Name = "Finish Date", FieldIndex = 7, OutputFormat = "dd/MM/yyyy")]
    public DateTime? FinishDate { get; set; }
    [CsvColumn(Name = "Finish Time", FieldIndex = 8, OutputFormat = "hh:mmtt")]
    public DateTime? FinishTime { get; set; }
    [CsvColumn(Name = "Days Notice Before Announcement", FieldIndex = 9)]
    public int? DaysNotice { get; set; }
    [CsvColumn(Name = "Other Details", FieldIndex = 10)]
    public string OtherDetails { get; set; }
    [CsvColumn(Name = "Year Level", FieldIndex = 11)]
    public string YearLevel { get; set; }

    public override bool Equals(Object obj)
    {
        //Check whether the compared object is null.
        if (obj == null) return false;

        CSVEvent other = obj as CSVEvent;
        if ((Object)other == null)
        {
            return false;
        }

        return EventName.Equals(other.EventName) && YearLevel.Equals(other.YearLevel)
            && Cost.Equals(other.Cost) && Transport.Equals(other.Transport) && Location.Equals(other.Location)
            && StartDate.Equals(other.StartDate) && StartTime.Equals(other.StartTime) && FinishDate.Equals(other.FinishDate)
            && FinishTime.Equals(other.FinishTime) && DaysNotice.Equals(other.DaysNotice) && OtherDetails.Equals(other.OtherDetails);
    }

    public bool Equals(CSVEvent other)
    {
        //Check whether the compared object is null.
        if (Object.ReferenceEquals(other, null)) return false;

        //Check whether the compared object references the same data.
        if (Object.ReferenceEquals(this, other)) return true;

        return EventName.GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode()
    {
        return EventName.GetHashCode();
    }
}