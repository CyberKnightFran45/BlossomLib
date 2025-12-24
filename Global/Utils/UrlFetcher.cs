using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

/// <summary> Comunicates to a Server through a URL by using HTTP Protocols. </summary>

public static class UrlFetcher
{
// Http Client used for Comunication

private static readonly HttpClient _client = new();

// Make request Line

private static string MakeReqLine(string type, string path)
{
return $"{type} {path} HTTP/1.1\r\n";
}

// Format Host Info

private static string GetHostInfo(Uri uri)
{
string host = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";

return $"Host: {host}\r\n";
}

// Build GET Request from URL as a String

public static string BuildGetRequest(string url)
{
Uri reqUri = new(url);

string requestLine = MakeReqLine("GET", reqUri.PathAndQuery);
string hostInfo = GetHostInfo(reqUri);

return requestLine + hostInfo + "\r\n";
}

// Build GET Request from URL and write it to a Stream

public static void BuildGetRequest(string url, Stream writer)
{
string request = BuildGetRequest(url);

writer.WriteString(request);
}

// Concat Headers

private static string MergeHeaders(HttpContentHeaders headers)
{
StringBuilder sb = new();

foreach(var header in headers)
{
sb.Append(header.Key);
sb.Append(": ");

sb.Append(string.Join(", ", header.Value) );
sb.Append("\r\n");
}

return sb.ToString();
}

// Format Content Body

private static string FormatContentBody(HttpContent content)
{

if(content is null)
return "\r\n";

string body = content.ReadAsStringAsync().GetAwaiter().GetResult();

return "\r\n" + body;
}

// Build POST Request (Inner)

private static string BuildPostRequest(string url, HttpContent content)
{
Uri reqUri = new(url);

string requestLine = MakeReqLine("POST", reqUri.PathAndQuery);
string hostInfo = GetHostInfo(reqUri);

string headers = MergeHeaders(content.Headers);
string contentStr = FormatContentBody(content);

return requestLine + hostInfo + headers + contentStr;
}

// Build POST Request from URL as a String

public static string BuildPostRequest(string url, string content,
                                      string contentType = "application/json")
{
using StringContent httpContent = new(content, Encoding.UTF8, contentType);

return BuildPostRequest(url, httpContent);
}

// Build POST Request from URL and write it to a Stream

public static void BuildPostRequest(string url, string content, Stream writer,
                                    string contentType = "application/json")
{
string request = BuildPostRequest(url, content, contentType);

writer.WriteString(request);
}

// Build Query String from a HttpDoc

public static string BuildQuery<T>(string baseUrl, T doc) where T : HttpUrlDoc<T>
{
using ChunkedMemoryStream httpStream = new();

doc.WriteForm(httpStream);
httpStream.Seek(0, SeekOrigin.Begin);

using var hOwner = httpStream.ReadString();

return $"{baseUrl}?{hOwner}";
}

// Normalize Data string

private static string EscapeStr(string src)
{

if(string.IsNullOrWhiteSpace(src) )
return string.Empty;

return Uri.EscapeDataString(src);
}

// Build Query String from a Dictionary

public static string BuildQuery(string baseUrl, Dictionary<string, string> parameters)
{

if(parameters == null || parameters.Count == 0)
return baseUrl;

List<string> queryParts = new();

foreach(var kvp in parameters)
{
string name = EscapeStr(kvp.Key);
string val = EscapeStr(kvp.Value);

queryParts.Add($"{name}={val}");
}

var queryString = string.Join("&", queryParts);

return $"{baseUrl}?{queryString}";
}

// Parse GET Request from Stream and get its base URL

public static string ParseGetRequest(Stream reader)
{
using var requestLine = reader.ReadLine();

long typeEnd = requestLine.IndexOf(' ');

if(typeEnd < 0)
return string.Empty;

long pathStart = typeEnd + 1;
var pathEnd = (int)requestLine.IndexOf(' ', pathStart);

if(pathEnd < 0)
return string.Empty;

string path = requestLine.Substring(pathStart, pathEnd);
string host = null;

while(true)
{
using var rawLine = reader.ReadLine();

if(rawLine is null)
break;

if(rawLine.StartsWith("Host:", StringComparison.OrdinalIgnoreCase) )
{
host = rawLine.Substring(5);
break;
}

}

if(host is null)
return string.Empty;

return $"http://{host}{path}";
}

// Parse POST Request from Stream and get its Body

public static NativeString ParsePostBody(Stream reader)
{
long contentLength = -1;

while(true)
{
using var rawLine = reader.ReadLine();

if(rawLine is null)
break;

if(rawLine.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase) )
contentLength = long.Parse(rawLine.GetView(15) );

}

if(contentLength <= 0) 
return new();

NativeString body = new(contentLength);
long totalRead = 0;

while(totalRead < contentLength)
{
long toRead = contentLength - totalRead;

using var chunk = reader.ReadString(toRead);
var read = chunk.Length;

if(read == 0)
break;

chunk.CopyTo(body, totalRead, read);

totalRead += read;
}

body.Realloc(totalRead);

return body;
}

// Parse Query String and Get Params as Dictionary

public static Dictionary<string, string> ParseQuery(ReadOnlySpan<char> query)
{
Dictionary<string, string> result = new();

if(query.IsEmpty || query.IsWhiteSpace() )
return result;

int paramsIndex = query.IndexOf('?');
var urlParams = paramsIndex >= 0 ? query[(paramsIndex + 1)..] : query;

foreach(var r in urlParams.Split('&') )
{
var pair = urlParams[r];

if(pair.IsEmpty)
continue;

int eq = pair.IndexOf('=');

string rawName;
string rawValue;

if(eq < 0)
{
rawName = pair.ToString();
rawValue = string.Empty;
}

else
{
rawName = pair[..eq].ToString();
rawValue = pair[(eq + 1)..].ToString();
}

string name = Uri.UnescapeDataString(rawName);
string val = Uri.UnescapeDataString(rawValue);

result[name] = val;
}

return result;
}

// Parse Query and Get Params as HttpDoc

public static T ParseQuery<T>(string query) where T : HttpUrlDoc<T>, new()
{
using ChunkedMemoryStream httpStream = new();
T doc = new();

int paramsIndex = query.IndexOf('?');

if(paramsIndex == -1)
return null;

string urlParams = query[(paramsIndex + 1)..];

httpStream.WriteString(urlParams);
httpStream.Seek(0, SeekOrigin.Begin);

doc.ReadForm(httpStream);

return doc;
}

// Write HttpStatus to Logger

private static void LogHttpStatus(HttpStatusCode status)
{
string flags = status == default ? "Unknown" : status.ToString();

TraceLogger.WriteInfo($"Status: {(int)status} ({flags})");
}

// Get Response from Server

public static async Task<string> GetResponseAsync(string url)
{
HttpStatusCode status = default;
string responseBody = null;

TraceLogger.WriteActionStart($"Getting Response from {url} ...");

try
{
using var response = await _client.GetAsync(url);

status = response.StatusCode;

if(status == HttpStatusCode.OK)
responseBody = await response.Content.ReadAsStringAsync();

TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError($"Error getting response: {error.Message}");
}

LogHttpStatus(status);

return responseBody;
}

// Get Response from Server as a Stream

public static async Task<Stream> GetResponseStreamAsync(string url)
{
HttpStatusCode status = default;
ChunkedMemoryStream res = null;

TraceLogger.WriteActionStart($"Getting Response from {url} ...");

try
{
using var response = await _client.GetAsync(url);

status = response.StatusCode;

if(status == HttpStatusCode.OK)
{
using var responseBody = await response.Content.ReadAsStreamAsync();
responseBody.Seek(0, SeekOrigin.Begin);

res = new();
FileManager.Process(responseBody, res);

res.Seek(0, SeekOrigin.Begin);
}

TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError($"Error getting response: {error.Message}");
}

LogHttpStatus(status);

return res;
}

// Post Request to Server

public static async Task<string> PostRequestAsync(string url, string content,
                                                  string contentType = "application/json")
{
HttpStatusCode status = default;
string responseBody = null;

TraceLogger.WriteActionStart($"Posting Request to {url} ...");

try
{
using StringContent httpContent = new(content, Encoding.UTF8, contentType);
using var response = await _client.PostAsync(url, httpContent);

status = response.StatusCode;

if(status == HttpStatusCode.OK)
responseBody = await response.Content.ReadAsStringAsync();

TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError($"Error posting request: {error.Message}");
}

LogHttpStatus(status);

return responseBody;
}

// Post Request to Server as Stream

public static async Task<Stream> PostRequestStreamAsync(string url, Stream content,
                                                        string contentType = "application/json")
{
HttpStatusCode status = default;
ChunkedMemoryStream res = null;

TraceLogger.WriteActionStart($"Posting Request to {url} ...");

try
{
using StreamContent httpContent = new(content);
httpContent.Headers.ContentType = new(contentType);

using var response = await _client.PostAsync(url, httpContent);

status = response.StatusCode;

if(status == HttpStatusCode.OK)
{
using var responseBody = await response.Content.ReadAsStreamAsync();
responseBody.Seek(0, SeekOrigin.Begin);

res = new();
FileManager.Process(responseBody, res);

res.Seek(0, SeekOrigin.Begin);
}

TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError($"Error posting request: {error.Message}");
}

LogHttpStatus(status);

return res;
}

// Download File Async

public static async Task DownloadFileAsync(string url, string filePath)
{
Stream responseStream = await GetResponseStreamAsync(url);

if(responseStream is null)
return;

using var outFile = FileManager.OpenWrite(filePath);
FileManager.Process(responseStream, outFile);

responseStream.Dispose();
}

// Upload File Async, and Optionally, Write Response in Local

public static async Task UploadFileAsync(string sourcePath, string url,
                                         string contentType = "application/json",
										 string responsePath = null)
{
using var inFile = FileManager.OpenRead(sourcePath);

Stream responseStream = await PostRequestStreamAsync(url, inFile, contentType);

if(responseStream is null)
return;

if(responsePath is not null)
{
using var outFile = FileManager.OpenWrite(responsePath);
FileManager.Process(responseStream, outFile);

responseStream.Dispose();
}

}

}