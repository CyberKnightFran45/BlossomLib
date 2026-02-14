using System;

/// <summary> Initializes calculating Functions for Time between Windows and Unix. </summary>

public static class UnixTimestamp
{
/// <summary> The epoch Time used as a Reference when Parsing DateTimes. </summary>

private static readonly DateTime epochTime = DateTime.Parse("1970/1/1");

/** <summary> Calculates a TimeStamp from a given DateTime Value. </summary>

<param name = "dateTime"> The DateTime where the TimeStamp will be Calculated from. </param>

<returns> The TimeStamp Calculated. </returns> */

public static double ConvertTo(DateTime dateTime) 
{
var diff = dateTime.Subtract(epochTime);

return Math.Truncate(diff.TotalSeconds);
}

/** <summary> Calculates a DateTime from a given TimeStamp Value. </summary>

<param name = "timeStamp"> The TimeStamp where the DateTime will be Calculated from. </param>

<returns> The DateTime Calculated. </returns> */

public static DateTime ConvertFrom(double timeStamp) => epochTime.AddSeconds(timeStamp);
}