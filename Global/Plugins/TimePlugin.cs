using System;
using System.Diagnostics;

public static class TimePlugin
{
// Get ExactTime in Timer

public static string GetExactTime(this Stopwatch timer)
{

if(timer == null)
return "00:00:00.000";

var elapsed = timer.Elapsed;

if(elapsed.TotalMicroseconds < 1)
return $"{elapsed.TotalNanoseconds:F2} ns";

if(elapsed.TotalMilliseconds < 1)
return $"{elapsed.TotalMicroseconds:F2} μs";

if(elapsed.TotalSeconds < 1)
return $"{(int)elapsed.TotalMilliseconds} ms";

if(elapsed.TotalMinutes < 1)
return $"{(int)elapsed.TotalSeconds} s";

if(elapsed.TotalHours < 1)
return $"{(int)elapsed.TotalMinutes} min {(int)elapsed.TotalSeconds} s";

if(elapsed.TotalDays < 1)
return $"{(int)elapsed.TotalHours} h {(int)elapsed.TotalMinutes} min {(int)elapsed.TotalSeconds} s";

int days = (int)elapsed.TotalDays;

return $"{days} d {(int)elapsed.TotalHours} h {(int)elapsed.TotalMinutes} min {(int)elapsed.TotalSeconds} s";
}

public static bool SecondEquals(this DateTime a, DateTime b)
{
return a.Ticks / TimeSpan.TicksPerSecond == b.Ticks / TimeSpan.TicksPerSecond;
}

}