using System;
using System.Diagnostics;

public static class TimePlugin
{
// Get ExactTime from Span

public static string GetExactTime(this TimeSpan time)
{

if(time.TotalMicroseconds < 1)
return $"{(long)time.TotalNanoseconds} ns";

if(time.TotalMilliseconds < 1)
return $"{(long)time.TotalMicroseconds} μs";

if(time.TotalSeconds < 1)
return $"{(long)time.TotalMilliseconds} ms";

if(time.TotalMinutes < 1)
return $"{time.Seconds} s";

if(time.TotalHours < 1)
return $"{time.Minutes} min {time.Seconds} s";

if(time.TotalDays < 1)
return $"{time.Hours} h {time.Minutes} min {time.Seconds} s";

return $"{time.Days} d {time.Hours} h {time.Minutes} min {time.Seconds} s";
}

// Get ExactTime in Timer

public static string GetExactTime(this Stopwatch timer)
{

if(timer is null)
return "00:00:00.000";

return GetExactTime(timer.Elapsed);
}

public static bool SecondEquals(this DateTime a, DateTime b)
{
return a.Ticks / TimeSpan.TicksPerSecond == b.Ticks / TimeSpan.TicksPerSecond;
}

}