using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;
using System.IO;
using System.IO.Compression;

namespace GameCrashReceiver
{
    // Implements the receiving logic
    class CrashReceiver
    {
        // Initiates the HTTP server
        public void Run()
        {
            // Log some info
            Log.LogStatus("GameCrashReceiver v." + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Log.LogStatus("Landing zone: " + Properties.Resources.StoragePath);

            // Fire up a HTTP server
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://*:57005/");
            httpListener.Start();
            httpListener.BeginGetContext(AsyncHandleHttpRequest, this);

            // Let it run
            Console.Read();
        }

        // Called when we receive a HTTP request
        private void AsyncHandleHttpRequest(IAsyncResult ClientRequest)
        {
            // "Close" this context by handling it
            HttpListenerContext context = httpListener.EndGetContext(ClientRequest);

            // Start a new polling cycle
            httpListener.BeginGetContext(AsyncHandleHttpRequest, ClientRequest.AsyncState);

            try
            {
                // Get the initial request when handling this context
                HttpListenerRequest request = context.Request;

                // Log the request
                Log.LogStatus("Received request from " + request.RemoteEndPoint.ToString() + " (content length: " + request.ContentLength64 + ")");

                // Create a response object
                HttpListenerResponse response = context.Response;

                // What command was this?
                // http://*:57005/ping does check if we are alive
                // http://*:57005/crash does upload a new crash
                string rawUrl = request.RawUrl.Split('?')[0];
                string[] urlParts = rawUrl.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // Just a simple call to http://*:57005/ which we won't handle
                if (urlParts.Length == 0)
                {
                    // Make the response OK and send it
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                    return;
                }

                switch (urlParts[0].ToLower())
                {
                    case "ping":
                        // Answer "pong" to the request
                        byte[] buffer = Encoding.UTF8.GetBytes("Pong!");
                        response.ContentType = "text/plain";
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);

                        break;

                    case "crash":
                        // Make sure we're receiving a zip file and got the nessecary query params
                        if (request.ContentType == "application/zip" && request.QueryString.AllKeys.Contains("guid") && request.QueryString.AllKeys.Contains("version"))
                        {
                            // Handle the received data to store a new crash
                            StoreCrash(request.InputStream, request.QueryString["guid"], request.QueryString["version"]);
                        }
                        else
                        {
                            // Send back an error
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            response.Close();
                            return;
                        }

                        break;

                    default:
                        break;
                }

                // Make the response OK and send it
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
            }
            catch (Exception e)
            {
                Log.LogError("Error during async listen: " + e.Message);

                // Create a response object
                HttpListenerResponse response = context.Response;

                // Fail it
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Close();
            }
        }

        // Stores a crash by decompressing the data and saving it to the landing zone
        public void StoreCrash(Stream crashDataStream, string crashGUID, string crashVersion)
        {
            // Make up a name for the zip file (the GUID is just a random component to prevent any duplicates)
            string crashZipPath = Properties.Resources.StoragePath + "\\" + crashGUID + ".zip";

            // Create a file info for the zip
            FileInfo crashZipFile = new FileInfo(crashZipPath);

            // Open the zip file for write
            FileStream crashZipFileWriter = crashZipFile.OpenWrite();

            // Copy over the data from the stream into the zip file
            crashDataStream.CopyTo(crashZipFileWriter);

            // Close both streams
            crashZipFileWriter.Close();
            crashDataStream.Close();

            // Now that the zip has been received, unzip it
            string targetCrashPath = Properties.Resources.StoragePath + "\\" + crashVersion + "\\" + crashGUID;
            Directory.CreateDirectory(targetCrashPath);

            ZipFile.ExtractToDirectory(crashZipPath, targetCrashPath);

            // Finally delete the zip
            crashZipFile.Delete();

            Log.LogStatus("Stored crash " + crashGUID + " for version " + crashVersion + ".");
        }

        // Attributes
        private HttpListener httpListener;
    }
}
