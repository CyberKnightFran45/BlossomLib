using System;
using System.Diagnostics;
using System.IO;

// Enables logging info to a File, then save it sorted by Date

public static class TraceLogger
{
// Fields

private const int INITIAL_SIZE = 4096;
private static readonly string baseDir = Path.Combine(LibInfo.CurrentDllDirectory, "Logs");

private static NativeMemoryOwner<char> _buffer;
private static bool _isInitialized;

private static ulong _position;

private static DateTime _startTime;
private static Stopwatch _timer;

private static bool _enabled;

// Enable Logger for Writing

public static void Enable() => _enabled = true;

// Disable Logger

public static void Disable() => _enabled = false;

// Setups the Logger

public static void Init()
{

if(_isInitialized)
{
var now = DateTime.Now;

if(_timer.IsRunning)
_timer.Reset();

AutoSpacing(2);

if(!_startTime.SecondEquals(now) )
{
_startTime = now;

WriteDate();
}

else
{
Span<char> bars = stackalloc char[32];
bars.Fill('=');

WriteLine(bars);

AutoSpacing(2);
}

return;
}

_startTime = DateTime.Now;
_buffer = new(INITIAL_SIZE);

_timer ??= new();
_isInitialized = true;

_enabled = true;

WriteDate();
}

// Make sure Logger is ready before Writing

private static void EnsureInit()
{

if(!_isInitialized)
Init();

}

// Starts the timer

private static void StartTimer() => _timer.Restart();

// Stops the timer

private static void StopTimer()
{

if(!_timer.IsRunning)
return;

_timer.Stop();
}

// Write [HEADER]

private static void WriteHeader(ReadOnlySpan<char> msg, bool appendLine = true)
{

if(!_enabled)
return;

Write("[");
Write(msg);

if(appendLine)
{
WriteLine("]");
WriteLine();
}

else
Write("] ");

}

// Write SubHeader

private static void WriteSubHeader(ReadOnlySpan<char> type, ReadOnlySpan<char> msg, bool appendLine)
{

if(!_enabled)
return;

EnsureInit();

WriteHeader(type, false);

if(appendLine)
{
WriteLine(msg);

AutoSpacing(2);
}

else
Write(msg);

}

// Write Date

private static void WriteDate() => WriteHeader($"{_startTime:d} {_startTime:T}");

// Write msg

public static void Write(ReadOnlySpan<char> msg)
{

if(!_enabled || msg.IsEmpty)
return;

EnsureInit();

var required = (ulong)msg.Length;

if(_position + required >= _buffer.Size)
Grow(required + 512);

_buffer.CopyFrom(msg, _position);

_position += (ulong)msg.Length;
}

// Write action triggered

public static void WriteActionStart(ReadOnlySpan<char> msg)
{

if(!_enabled || msg.IsEmpty)
return;

WriteSubHeader("TRACE", msg, false);
AutoSpacing();

StartTimer();
}

// Write elapsed time between Action

public static void WriteActionEnd()
{

if(!_enabled)
return;

EnsureInit();
StopTimer();

string elapsed = _timer.GetExactTime();
Write("Elapsed: ");

WriteLine(elapsed);
WriteLine();
}

// Ensure line breakes

private static void AutoSpacing(int maxLines = 1)
{

if(_position == 0)
return;

int newLines = 0;
var startPos = (int)(_position - 1);

for(int i = startPos; i >= 0 && newLines < maxLines; i--)
{

if(_buffer[i] == '\n')
newLines++;

else if(_buffer[i] == '\r')
continue;

else
break;

}

int linesToAdd = maxLines - newLines;

if(linesToAdd > 0)
{
Span<char> space = stackalloc char[256];
space.Fill('\n');

int count = linesToAdd;

while(count > 0)
{
int chunk = Math.Min(count, space.Length);
Write(space[..chunk]);

count -= chunk;
}

}

}

// Write new line

public static void WriteLine()
{

if(!_enabled)
return;

EnsureInit();

if(_position < _buffer.Size)
{
_buffer[_position] = '\n';

_position += 1;
}

}

// Write msg in new line

public static void WriteLine(ReadOnlySpan<char> msg)
{

if(!_enabled || msg.IsEmpty)
return;

Write(msg);
AutoSpacing();
}

// Write debug msg

public static void WriteDebug(ReadOnlySpan<char> msg, bool appendLine = true)
{

if(!_enabled || msg.IsEmpty)
return;

WriteSubHeader("DEBUG", msg, appendLine);
}

// Write info

public static void WriteInfo(ReadOnlySpan<char> msg, bool appendLine = true)
{

if(!_enabled || msg.IsEmpty)
return;

WriteSubHeader("INFO", msg, appendLine);
}

// Write warning msg

public static void WriteWarn(ReadOnlySpan<char> msg, bool appendLine = true)
{

if(msg.IsEmpty)
return;

WriteSubHeader("WARNING", msg, appendLine);
}

// Write error msg

public static void WriteError(ReadOnlySpan<char> msg, bool appendLine = true)
{

if(msg.IsEmpty)
return;

WriteSubHeader("ERROR", msg, appendLine);
}

// Write critical error trace

public static void WriteError(Exception error, ReadOnlySpan<char> msg = default)
{

if(!msg.IsEmpty)
WriteSubHeader("CRITICAL", msg, true);

WriteLine("-------- Exception Details --------");
WriteLine();

WriteLine(error.ToString() );
WriteLine();
}

// Save buffer to file

public static void SaveLogs(string outputDir = null)
{

if(!_isInitialized || _position == 0)
return;

string loggerDate = _startTime.ToString("d").Replace('/', '-').Replace('.', '-');
string loggerTime = _startTime.ToString("T").Replace(':', '-').Replace('.', '-');

string logFolder = Path.Combine(outputDir ?? baseDir, loggerDate);
PathHelper.EnsurePathExists(logFolder);

string logPath = Path.Combine(logFolder, $"{loggerTime}.log");

using FileStream writer = FileManager.OpenWrite(logPath);

var data = _buffer.AsSpan(0, (int)_position);
writer.WriteString(data);
}

// Realloc memory

private static void Grow(ulong additional)
{
ulong newSize = (_buffer.Size + additional) * 2;

_buffer.Realloc(newSize);
}

// Dispose memory

public static void Dispose()
{

if(_isInitialized)
{
_buffer.Dispose();
_position = 0;

_isInitialized = false;
_enabled = false;
}

}

}