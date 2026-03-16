using System;

/// <summary> Calculates Time between Windows and Unix. </summary>

public static class UnixTimestamp
{
/** <summary> Calculates a TimeStamp from a given DateTime Value. </summary>

<param name = "dateTime"> The DateTime where the TimeStamp will be Calculated from. </param>

<returns> The TimeStamp Calculated. </returns> */

public static long ConvertTo(DateTime dateTime) 
{
var utc = dateTime.ToUniversalTime();
DateTimeOffset timeOffset = new(utc);

return timeOffset.ToUnixTimeSeconds();
}

/** <summary> Calculates a DateTime from a given TimeStamp </summary>

<param name = "timeStamp"> The TimeStamp where the DateTime will be Calculated from. </param>

<returns> The DateTime Calculated. </returns> */

public static DateTime ConvertFrom(long timeStamp)
{
var timeOffset = DateTimeOffset.FromUnixTimeSeconds(timeStamp);

return timeOffset.UtcDateTime;
}

}
