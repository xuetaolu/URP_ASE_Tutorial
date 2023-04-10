
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// This class provides various low level services like network operations etc.
/// </summary>


namespace BrainFailProductions.PolyFew
{


    public static class SystemServices
    {

        /// <summary>  Contains different regex patterns used in the application.</summary>
        public static RegexPatterns regexPatterns;



        [System.Serializable]

        public struct RegexPatterns
        {
            public string netError;
            public string nullOrEmpty;
            public string generalError;
            public string apiMistmatch;
            public string parametersMismatch;
            public string nothing;
        }



        private static void SetPatterns()
        {
            regexPatterns.netError = "<neterror>";
            regexPatterns.nullOrEmpty = "<nullorempty>";
            regexPatterns.generalError = "<generalerror>";
            regexPatterns.apiMistmatch = "<apimismatch>";
            regexPatterns.parametersMismatch = "<parametersmismatch>";
            regexPatterns.nothing = "";
        }


        /// <summary> Make an asynchronous GET request to the recepient specified in the encodedUrl parameter.Optionally any headers can also be specified. </summary> 

        public static IEnumerator UnityAsyncGETRequest(string encodedUrl, Action<string, long> callback, int? timeout = null, Dictionary<string, string> headers = null)
        {

            SetPatterns();

            UnityWebRequest webRequest = new UnityWebRequest(encodedUrl);
            webRequest.timeout = timeout == null ? webRequest.timeout : (int)timeout;
            webRequest.method = "GET";
            DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();

            webRequest.downloadHandler = downloadHandler;


            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }


#if !UNITY_2017_1
            yield return webRequest.SendWebRequest();
#else
        yield return webRequest.Send();
#endif

            long responseCode = webRequest.responseCode;

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
            if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
            {
                callback("<neterror>" + webRequest.error, responseCode);
                //callback(webRequest.responseCode+"-<neterror>" + webRequest.error); 
            }

            else
            {
                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    callback("<nullorempty>" + "Error! server returned an empty response.", responseCode);
                }

                else
                {
                    callback(webRequest.downloadHandler.text, responseCode);
                }
            }


        }






        /// <summary> Make a blocking GET request to the recepient specified in the encodedUrl parameter.Optionally any headers can also be specified. </summary> 

        public static void UnityBlockingGETRequest(string encodedUrl, Action<string, long> callback, int? timeout = null, Dictionary<string, string> headers = null)
        {

            SetPatterns();

            UnityWebRequest webRequest = new UnityWebRequest(encodedUrl);
            webRequest.timeout = timeout == null ? webRequest.timeout : (int)timeout;
            webRequest.method = "GET";
            DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();

            webRequest.downloadHandler = downloadHandler;


            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }




#if !UNITY_2017_1
            webRequest.SendWebRequest();
#else
        webRequest.Send();
#endif

            while (!webRequest.isDone) { }

            long responseCode = webRequest.responseCode;

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
            if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
            {
                callback("<neterror>" + webRequest.error, responseCode);
            }

            else
            {
                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    callback("<nullorempty>" + "Error! server returned an empty response.", responseCode);
                }

                else
                {
                    callback(webRequest.downloadHandler.text, responseCode);
                }
            }


        }






        /// <summary> Make a blocking call to POST data to the recepient specified in the baseUrl parameter.Optionally any headers can also be specified. </summary> 

        public static void UnityBlockingPOSTRequest(string baseUrl, Action<string, long> callback, byte[] data, int? timeout = null, Dictionary<string, string> headers = null)
        {
            SetPatterns();

            UnityWebRequest webRequest = new UnityWebRequest(baseUrl);
            webRequest.timeout = timeout == null ? webRequest.timeout : (int)timeout;
            webRequest.method = "POST";
            UploadHandlerRaw uploadHandler = new UploadHandlerRaw(data);
            DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
            webRequest.uploadHandler = uploadHandler;
            webRequest.downloadHandler = downloadHandler;
            webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");


            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }



#if !UNITY_2017_1
            webRequest.SendWebRequest();
#else
        webRequest.Send();
#endif

            while (!webRequest.isDone) { }


            long responseCode = webRequest.responseCode;


#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
            if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
            {
                callback("<neterror>" + webRequest.error, responseCode);
                //callback(webRequest.responseCode + "-<neterror>" + webRequest.error);
            }

            else
            {

                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    callback("<nullorempty>" + "Error! server returned an empty response.", responseCode);
                    //callback(webRequest.responseCode + "-<nullorempty>" + "Error! server returned an empty response.");
                }

                else
                {
                    callback(webRequest.downloadHandler.text, responseCode);
                }

            }


        }






        /// <summary> POST form data asynchronously to the recipient specified in the baseUrl parameter.Optionally any headers can also be specified.Any response will be passed to the callback as a string. </summary> 

        public static IEnumerator UnityAsyncPOSTRequest(string baseUrl, Action<string, long> callback, byte[] data, int? timeout = null, Dictionary<string, string> headers = null)
        {

            SetPatterns();

            UnityWebRequest webRequest = new UnityWebRequest(baseUrl);
            webRequest.timeout = timeout == null ? webRequest.timeout : (int)timeout;
            webRequest.method = "POST";
            UploadHandlerRaw uploadHandler = new UploadHandlerRaw(data);
            DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
            webRequest.uploadHandler = uploadHandler;
            webRequest.downloadHandler = downloadHandler;
            webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");


            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }


            // This sometimes get stuck and clogs the main thread
#if !UNITY_2017_1
            yield return webRequest.SendWebRequest();
#else
        yield return webRequest.Send();
#endif

            long responseCode = webRequest.responseCode;

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
            if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
            {
                callback("<neterror>" + webRequest.error, responseCode);
            }

            else
            {

                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    callback("<nullorempty>" + "Error! server returned an empty response.", responseCode);
                }

                else
                {
                    callback(webRequest.downloadHandler.text, responseCode);
                }

            }


        }






        /// <summary> Make an asynchronous web request using the HTTP method provided to the recepient specified in the encodedUrl parameter.Optionally a timeout value in milliSeconds and any header containing fields such as ContentType can also be specified. Please note that in case of any error the response string will be appended with the string 'neterror'. See regex patterns for more info. </summary> 

        public static async Task SendHTTPRequestAsync(string baseUrl, HTTPMethod requestMethod, Action<string, HttpStatusCode?> callback, Dictionary<string, string> requestParameters, byte[] postData, string contentType, int? timeout = null, Dictionary<string, string> header = null)
        {

            SetPatterns();

            await Task.Delay(0);

            //Debug.Log("Async GET request has just started");

            // Make a get call and respond back with the resuts to the lambda expression

            HttpWebRequest request;

            try { request = (HttpWebRequest)WebRequest.Create(baseUrl); }
            catch (Exception e) { callback(regexPatterns.generalError + "+" + e.ToString(), null); return; }


            HttpWebResponse httpResponse = null;

            try
            {
                request.Timeout = timeout == null ? 100000 : (int)timeout;
                request.Method = requestMethod.methodName;
                request.Headers = new WebHeaderCollection();
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;


                if (header != null)
                {
                    // Append fields from the header parameter to the request header 
                    foreach (KeyValuePair<string, string> pair in header)
                    {
                        request.Headers.Add(pair.Key, pair.Value);
                    }
                }



                if (requestParameters != null)
                {
                    string queryString = GetQueryStringFromKeyValues(requestParameters);

                    if (requestMethod.methodName == "GET")
                    {
                        baseUrl += queryString;
                    }

                    else
                    {
                        byte[] paramsData = Encoding.UTF8.GetBytes(queryString);

                        request.ContentLength = paramsData.Length;

                        using (var dataStream = await request.GetRequestStreamAsync())
                        {
                            dataStream.Write(paramsData, 0, paramsData.Length);
                        }
                    }
                }




                if (requestParameters == null && postData != null && requestMethod.methodName == "POST")
                {

                    request.ContentLength = postData.Length;

                    await Task.Run(() =>
                    {

                        using (var dataStream = request.GetRequestStream())
                        {
                            dataStream.Write(postData, 0, postData.Length);
                        }
                    });
                    /*
                    using (var dataStream = await request.GetRequestStreamAsync())
                    {
                        dataStream.Write(postData, 0, postData.Length);
                    }
                    */
                }

                //Task<WebResponse> webTask = request.GetResponseAsync();

                //httpResponse = (HttpWebResponse)await webTask;

                await Task.Run(() =>
                {
                    httpResponse = (HttpWebResponse)request.GetResponse();
                });

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {

                    //Debug.Log($"Failed to make HTTP request to resource");
                    callback(regexPatterns.netError + "+" + httpResponse.StatusDescription, httpResponse.StatusCode);
                }


                else
                {
                    Stream stream = httpResponse.GetResponseStream();

                    StreamReader reader = new StreamReader(stream);

                    string response = await reader.ReadToEndAsync();

                    //Debug.Log("Response from the web request is   " + response);

                    callback(response, httpResponse.StatusCode);

                }


                httpResponse.Dispose();
            }


            catch (Exception e)
            {

                HttpStatusCode? statusCode = (httpResponse == null) ? null : (HttpStatusCode?)httpResponse.StatusCode;


                if (e.InnerException is WebException || e.InnerException is SocketException)
                {
                    WebException ex = e as WebException;
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        callback(regexPatterns.generalError + "+" + ex.ToString(), statusCode);
                    }

                    else
                    {
                        callback(regexPatterns.netError + "+" + ex.ToString(), statusCode);
                    }
                }

                else
                {
                    callback(regexPatterns.generalError + "+" + e.ToString(), statusCode);
                }

            }

        }







        /// <summary> Make a Blocking web request using the HTTP method provided to the recepient specified in the encodedUrl parameter.Optionally a timeout value in milliSeconds and any header containing fields such as ContentType can also be specified. Please note that in case of any error the response string will be appended with the string 'neterror'. See regex patterns for more info. </summary> 

        public static void SendHTTPRequestBlocking(string baseUrl, HTTPMethod requestMethod, Action<string, HttpStatusCode?> callback, Dictionary<string, string> requestParameters, byte[] postData, string contentType, int? timeout = null, Dictionary<string, string> header = null)
        {

            SetPatterns();

            HttpWebResponse httpResponse = null;


            try
            {

                if (requestParameters != null && requestMethod.methodName == "GET")
                {
                    string queryString = GetQueryStringFromKeyValues(requestParameters);
                    baseUrl += "?" + queryString;
                }


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl); ;


                request.Timeout = timeout == null ? 100000 : (int)timeout;
                request.Method = requestMethod.methodName;
                request.Headers = new WebHeaderCollection();
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentType = contentType;

                if (header != null)
                {
                    // Append fields from the header parameter to the request header 
                    foreach (KeyValuePair<string, string> pair in header)
                    {
                        request.Headers.Add(pair.Key, pair.Value);
                    }
                }



                if (requestParameters != null && requestMethod.methodName == "POST")
                {

                    string queryString = GetQueryStringFromKeyValues(requestParameters);

                    byte[] paramsData = Encoding.ASCII.GetBytes(queryString);
                    request.ContentLength = paramsData.Length;

                    using (var dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(paramsData, 0, paramsData.Length);
                    }

                }

                else if (requestParameters == null && requestMethod.methodName != "GET")
                {
                    request.ContentLength = 0;
                }


                if (requestParameters == null && postData != null && requestMethod.methodName == "POST")
                {
                    request.ContentLength = postData.Length;

                    using (var dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(postData, 0, postData.Length);
                    }
                }



                httpResponse = (HttpWebResponse)request.GetResponse();

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {

                    //Debug.Log($"Failed to make HTTP request to resource  {encodedUrl}");
                    callback(regexPatterns.netError + "+" + httpResponse.StatusDescription, httpResponse.StatusCode);
                }


                else
                {
                    Stream stream = httpResponse.GetResponseStream();

                    StreamReader reader = new StreamReader(stream);

                    string response = reader.ReadToEnd();

                    //Debug.Log("Response from the web request is   " + response);

                    callback(response, httpResponse.StatusCode);

                }


                httpResponse.Dispose();


            }


            catch (Exception e)
            {
                HttpStatusCode? statusCode = (httpResponse == null) ? null : (HttpStatusCode?)httpResponse.StatusCode;

                if (e.InnerException is WebException || e.InnerException is SocketException)
                {
                    WebException ex = e as WebException;
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        callback(regexPatterns.generalError + "+" + ex.ToString(), statusCode);
                    }

                    else
                    {
                        callback(regexPatterns.netError + "+" + ex.ToString(), statusCode);
                    }
                }

                else
                {
                    callback(regexPatterns.generalError + "+" + e.ToString(), statusCode);
                }

            }

        }






        /// <summary> Download the resource given by the absolute URL. The callback is passed the bytes of the resource, an error message if any and the HTTPStatus code.</summary>

        public static async Task AsyncResourceDownload(string resourceUrl, Action<byte[], string, HttpStatusCode?> callback, int? timeout = null)
        {

            SetPatterns();

            await Task.Delay(0);

            //Debug.Log("Async GET request has just started");

            // Make a get call and respond back with the resuts to the lambda expression

            HttpWebRequest request;

            try { request = (HttpWebRequest)WebRequest.Create(resourceUrl); }
            catch (Exception e) { callback(null, e.ToString(), null); return; }


            HttpWebResponse httpResponse = null;

            try
            {
                request.Timeout = timeout == null ? 100000 : (int)timeout;

                await Task.Run(() =>
                {
                    httpResponse = (HttpWebResponse)request.GetResponse();
                });

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    callback(null, httpResponse.StatusDescription, httpResponse.StatusCode);
                }


                else
                {

                    Stream stream = httpResponse.GetResponseStream();
                    Byte[] bytes = null;

                    try
                    {
                        using (BinaryReader br = new BinaryReader(stream))
                        {
                            bytes = br.ReadBytes((int)stream.Length);
                        }
                    }

                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                        callback(bytes, ex.ToString(), httpResponse.StatusCode);
                    }



                    callback(bytes, "", httpResponse.StatusCode);

                }


                httpResponse.Dispose();
            }


            catch (Exception e)
            {

                HttpStatusCode? statusCode = (httpResponse == null) ? null : (HttpStatusCode?)httpResponse.StatusCode;


                if (e.InnerException is WebException || e.InnerException is SocketException)
                {
                    WebException ex = e as WebException;
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        callback(null, ex.ToString(), statusCode);
                    }

                    else
                    {
                        callback(null, ex.ToString(), statusCode);
                    }
                }

                else
                {
                    callback(null, e.ToString(), statusCode);
                }

            }

        }




        /// <summary> Checks asynchronously whether the url given is reachable. Passes true to the callback provided or false if the url is not reachable.</summary>

        public static async Task AsyncReachabilityCheck(string testUrl, Action<bool> callback)
        {

            var method = new HTTPMethod(HTTPMethod.HTTPMethods.GET);

            await SendHTTPRequestAsync(testUrl, method, (string response, HttpStatusCode? statusCode) =>
            {

            /*
            if (Regex.IsMatch(response, regexPatterns.netError, RegexOptions.Compiled))
            {
                Debug.Log("No internet connection or system services are down");
                if (callback != null) { callback(false); }
            }

            else
            {
                Debug.Log("Connection to the internet exists");
                if (callback != null) { callback(true); }
            }
            */

                if (statusCode != null && statusCode == HttpStatusCode.OK)
                {
                    callback(true);
                }

                else
                {
                    callback(false);
                }

            }, null, null, "application/json");

        }




        /// <summary> Checks in a blocking manner whether the url given is reachable. Passes true to the callback provided or false if the url is not reachable.</summary>

        public static void BlockingReachabilityCheck(string url, Action<bool> callback)
        {

            var method = new HTTPMethod(HTTPMethod.HTTPMethods.GET);

            SendHTTPRequestBlocking(url, method, (string response, HttpStatusCode? statusCode) =>
            {

            /*
            if (Regex.IsMatch(response, regexPatterns.netError, RegexOptions.Compiled))
            {
                Debug.Log("No internet connection or system services are down");
                if (callback != null) { callback(false); }
            }

            else
            {
                Debug.Log("Connection to the internet exists");
                if (callback != null) { callback(true); }
            }
            */

                if (statusCode != null && statusCode == HttpStatusCode.OK)
                {
                    callback(true);
                }

                else
                {
                    callback(false);
                }
            }, null, null, "application/json");

        }


        /// <summary>
        /// Parses a message appended with any of the "RegexPatterns" and then returns the appropriate type.
        /// </summary>
        /// <param name="message"> The message appended with a "RegexPatterns"</param>
        /// <returns>A struct consisting of the parsed message and the "RegexPatterns" that was originally appended with the message.</returns>
        public static MessagePatternPair ParseResponseMessage(string message)
        {
            string msg = null;
            string pattern = regexPatterns.nothing;

            if (Regex.IsMatch(message, regexPatterns.netError, RegexOptions.Compiled))
            {
                msg = message.Replace(regexPatterns.netError + "+", "");
                pattern = regexPatterns.netError;
            }

            else if (Regex.IsMatch(message, regexPatterns.apiMistmatch, RegexOptions.Compiled))
            {
                msg = message.Replace(regexPatterns.apiMistmatch + "+", "");
                pattern = regexPatterns.apiMistmatch;
            }

            else if (Regex.IsMatch(message, regexPatterns.generalError, RegexOptions.Compiled))
            {
                msg = message.Replace(regexPatterns.generalError + "+", "");
                pattern = regexPatterns.generalError;
            }

            else if (Regex.IsMatch(message, regexPatterns.parametersMismatch, RegexOptions.Compiled))
            {
                msg = message.Replace(regexPatterns.parametersMismatch + "+", "");
                pattern = regexPatterns.parametersMismatch;
            }

            else if (Regex.IsMatch(message, regexPatterns.nullOrEmpty, RegexOptions.Compiled))
            {
                msg = message.Replace(regexPatterns.nullOrEmpty + "+", "");
                pattern = regexPatterns.nullOrEmpty;
            }

            else
            {
                msg = null;
                pattern = regexPatterns.nothing;
            }

            return new MessagePatternPair(pattern, msg);

        }



        public struct MessagePatternPair
        {
            public string patternAppended { private set; get; }
            public string parsedMessage { private set; get; }

            public MessagePatternPair(string patternAppended, string parsedMessage)
            {
                this.patternAppended = patternAppended;
                this.parsedMessage = parsedMessage;
            }
        }



        public static bool IsSuccessStatusCode(long statusCode)
        {
            return ((int)statusCode >= 200) && ((int)statusCode <= 299);
        }



        public class HTTPMethod
        {

            public readonly string methodName;
            public HTTPMethod(HTTPMethods method) { this.methodName = Enum.GetName(typeof(HTTPMethods), method); }

            public enum HTTPMethods
            {
                POST,
                GET
            }

        }



        public static string GetQueryStringFromKeyValues(Dictionary<string, string> parameters)
        {
            var list = new List<string>();
            foreach (var item in parameters)
            {
                list.Add(item.Key + "=" + Uri.EscapeDataString(item.Value));
            }
            return string.Join("&", list);
        }




        public static async Task RunDelayedCommand(float secs, Action command)
        {
            await Task.Delay((int)(secs * 1000));
            command();
        }



        public static byte[] ReadAllBytes(Stream source)
        {
            long originalPosition = source.Position;
            source.Position = 0;

            try
            {
                byte[] readBuffer = new byte[4096];
                int totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = source.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = source.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                source.Position = originalPosition;
            }
        }





        /// <summary>
        /// Writes the given texture asynchronously to the disk at the given path. If a file already exists at the given path it will be overwritten
        /// </summary>
        /// <param name="texture"> The texture to write</param>
        /// <param name="format"> The image format to save the texture in. Note that EXR format requires an uncompressed HDR texture</param>
        /// <param name="fileName"> The file name without the extension.</param>
        /// <param name="path"> The absolute path where the file will be saved</param>
        /// <param name="callback"> The method to call when the operation ends. The method will be passed an error if any error occurred</param>

        public static async Task WriteTextureAsync(Texture2D texture, ImageFormat format, string fileName, string path, Action<string> callback)
        {

            try
            {

                byte[] data = null;

                switch (format)
                {
                    case ImageFormat.PNG:
                        data = ImageConversion.EncodeToPNG(texture);
                        if (!fileName.ToLower().Contains(".png")) { fileName += ".png"; }
                        break;

                    case ImageFormat.JPG:
                        data = ImageConversion.EncodeToJPG(texture);
                        if (!fileName.ToLower().Contains(".jpg")) { fileName += ".jpg"; }
                        break;

                    case ImageFormat.EXR:
                        data = ImageConversion.EncodeToEXR(texture);
                        if (!fileName.ToLower().Contains(".exr")) { fileName += ".exr"; }
                        break;

                }

                if (data == null) { Debug.Log("Failed encoding"); }
                if (path.EndsWith("/") || path.EndsWith("\\")) { path += fileName; }
                else { path += "/" + fileName; }

                using (FileStream fileStream = File.Create(path))
                {
                    await fileStream.WriteAsync(data, 0, data.Length);
                }

                callback("");
                Debug.Log(data.Length / 1024 + "Kb was saved as: " + path);

            }


            catch (Exception ex)
            {
                callback(ex.ToString());
            }


        }


        /// <summary>
        /// Writes the given texture asynchronously to the disk
        /// </summary>
        /// <param name="data"> The given bytes to write</param>
        /// <param name="fullPath"> The fully qualified path of the file. If a file already exists at this path it will be overwritten</param>
        /// <param name="callback"> The method to call when the operation ends. The method will be passed an error if any error occurred</param>

        public static async Task WriteBytesAsync(byte[] data, string fullPath, Action<string> callback)
        {

            try
            {
                using (FileStream fileStream = File.Create(fullPath))
                {
                    await fileStream.WriteAsync(data, 0, data.Length);
                }

                callback("");
                Debug.Log(data.Length / 1024 + "Kb was saved as: " + fullPath);

            }


            catch (Exception ex)
            {
                callback(ex.ToString());
            }
        }




        public enum ImageFormat { PNG, JPG, EXR }

    }
}