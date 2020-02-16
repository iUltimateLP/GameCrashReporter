using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace GameCrashReporter
{
    // Handles all the logic for the crash reporter
    class CrashReporter
    {
        // Sets the user description
        public void SetUserDescription(string userDescription)
        {
            this.userDescription = userDescription;
        }

        // Loads the files for the crash into our buffers
        public bool LoadCrashData()
        {
            try
            {
                // Crashes are stored in the %AppDataLocal% path
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\GameName\\Saved";

                // Folders we take into account
                string configPath = basePath + "\\Config\\WindowsNoEditor";
                string crashesPath = basePath + "\\Crashes";

                // Figure out the crash GUID and version we were called for
                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    if (arg.StartsWith("-CrashGUID"))
                    {
                        crashGUID = arg.Split('=')[1];
                    }
                    else if (arg.StartsWith("-CrashVersion"))
                    {
                        crashVersion = arg.Split('=')[1];
                    }
                }

                // Make sure we got a crash GUID and version
                if (crashGUID == "" || crashVersion == "")
                    return false;

                // Get the folder for this crash
                string crashPath = crashesPath + "\\" + crashGUID;
                DirectoryInfo crashDir = new DirectoryInfo(crashPath);

                if (crashDir.Exists)
                {
                    // Make sure our data is up to date
                    crashDir.Refresh();

                    {
                        // Get the .runtime-xml file
                        FileInfo runtimeXMLFile = crashDir.GetFiles("*.runtime-xml").First();

                        // Make sure it exists
                        if (runtimeXMLFile != null && runtimeXMLFile.Exists)
                        {
                            // Remember it
                            crashContextFile = runtimeXMLFile;
                        }
                    }

                    {
                        // Get the .dmp file
                        FileInfo dmpFile = crashDir.GetFiles("*.dmp").First();

                        // Make sure it exists
                        if (dmpFile != null && dmpFile.Exists)
                        {
                            // Remember it
                            minidumpFile = dmpFile;
                        }
                    }
                }

                // Get the config files from the user
                DirectoryInfo configDir = new DirectoryInfo(configPath);

                if (configDir.Exists)
                {
                    // Make sure our data is up to date
                    configDir.Refresh();

                    // Collect all .ini files in the config folder and remember them
                    FileInfo[] allIniFiles = configDir.GetFiles("*.ini");

                    // Remember them
                    configFiles = new List<FileInfo>(allIniFiles);
                }

                // Write some stats
                if (crashContextFile != null)
                    Console.WriteLine("Found " + crashContextFile.Name + " (" + crashContextFile.Length + " bytes)");

                if (minidumpFile != null)
                    Console.WriteLine("Found " + minidumpFile.Name + " (" + minidumpFile.Length + " bytes)");

                if (configFiles != null)
                    Console.WriteLine("Found " + configFiles.Count + " config files");

                // Return successful if all files were found
                return crashContextFile != null && minidumpFile != null && configFiles != null;
            }
            catch
            {
                // Return false on error
                return false;
            }
        }

        // Compresses the crash data in a zip archive and returns it
        public FileInfo CompressCrashData()
        {
            try
            {
                // Copy all file in one temporary directory
                string tempPath = Path.GetTempPath() + "\\crash_" + crashGUID;

                // Create the temporary directory
                Directory.CreateDirectory(tempPath);

                // Copy the crash context file
                crashContextFile.CopyTo(tempPath + "\\" + crashContextFile.Name, true);

                // Copy the minidump file
                minidumpFile.CopyTo(tempPath + "\\" + minidumpFile.Name, true);

                // Copy the config files
                Directory.CreateDirectory(tempPath + "\\Config");
                foreach (FileInfo cfgFile in configFiles)
                {
                    cfgFile.CopyTo(tempPath + "\\Config\\" + cfgFile.Name, true);
                }

                // Create a story.txt file containing the description the user gave us
                if (userDescription != "")
                {
                    File.WriteAllText(tempPath + "\\story.txt", userDescription);
                }

                // Now zip the folder up
                string zipPath = tempPath + "\\..\\crash_" + crashGUID + ".zip"; // We need to go one level back so we won't zip the zip

                // Remove the zip if it already exists for some reason
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                // Zip up the temp path
                ZipFile.CreateFromDirectory(tempPath, zipPath, CompressionLevel.Optimal, false);

                // Now remove the temp path
                Directory.Delete(tempPath, true);

                // Return the handle to the zip file
                return new FileInfo(zipPath);
            }
            catch
            {
                return null;
            }
        }

        // Sends the crash archive
        public bool SendCrashData(FileInfo crashArchive)
        {
            // Make sure the crash archive exists
            if (!crashArchive.Exists)
                return false;

            Console.WriteLine("Uploading crash data (" + crashArchive.Length + " bytes)");

            // Create a web client
            WebClient webClient = new WebClient();

            // Make sure that the data is sent as a zip
            webClient.Headers.Add("Content-Type", "application/zip");

            // Open a stream to read the zip data
            Stream crashArchiveStream = crashArchive.OpenRead();

            // Open a stream to write the data to the endpoint and also send over the version and guid via query parameters
            string remoteURL = Properties.Resources.UploadEndpoint + "/crash?version=" + crashVersion + "&guid=" + crashGUID;
            Stream remoteStream = webClient.OpenWrite(remoteURL, "POST");

            // Now write the crash archive to the remote stream (the webclient will take care of the rest)
            crashArchiveStream.CopyTo(remoteStream);

            // Close the remote stream
            remoteStream.Close();

            return true;
        }

        // Attributes
        public string crashGUID = "";
        public string crashVersion = "";
        public string userDescription = "";
        public FileInfo logFile; // Game.log
        public FileInfo crashContextFile; // CrashContext.runtime-xml
        public FileInfo minidumpFile; // UE4Minidump.dmp
        public List<FileInfo> configFiles; // All config files in Saved/Config/WindowsNoEditor
    }
}
