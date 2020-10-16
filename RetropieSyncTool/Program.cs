using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace RetropieSyncTool
{
    class Program
    {
        protected static bool fetchArtwork = false;
        protected static bool fetchConfig = true;
        protected static bool isPushToHost = true;// false; //pull
        protected static bool isFetchArtwork = true;
        protected static string sourceHost = "192.168.1.149";
        protected static string destinationHost = "192.168.1.117";//"192.168.1.117";//"192.168.1.117";

        // Setup session options
        static SessionOptions sessionOptions = new SessionOptions
        {
            GiveUpSecurityAndAcceptAnySshHostKey = true,
            Protocol = Protocol.Sftp,
            HostName = isPushToHost ? destinationHost : sourceHost,//"192.168.1.117",//"192.168.1.149",
            UserName = "pi",
            Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
        };

        static void Main(string[] args)
        {
            if (fetchArtwork) FetchArtwork();
            if (fetchConfig) FetchConfig();
        }

        protected static void FetchConfig()
        {
            ///opt/retropie/configs/*.cfg
            try
            {
                using (Session session = new Session())
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(sessionOptions);

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    TransferOperationResult transferResult = null;

                    if (isPushToHost) //Push
                    {
                        string remotePath = $"/opt/retropie/configs/fba/";
                        string localPath = $@"D:\RetroPie\RetroPieSync\{sourceHost}\configs\fba\*";

                        Directory.CreateDirectory(localPath.Replace("*", ""));

                        transferResult = session.PutFiles(localPath, remotePath, false, transferOptions);

                    }
                    else //Pull Sync
                    {
                        string remotePath = $"/opt/retropie/configs/fba/*";
                        string localPath = $@"D:\RetroPie\RetroPieSync\{destinationHost}\configs\fba\";

                        Directory.CreateDirectory(localPath.Replace("*", ""));

                        transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);
                    }

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Debug.WriteLine("Download of {0} succeeded", transfer.FileName);
                    }

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: {0}", e);

            }
        }

        protected static void FetchArtwork()
        {
            try
            {
                List<string> artFolders = new List<string>() { "downloaded_images", "gamelists" };

                using (Session session = new Session())
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(sessionOptions);

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    foreach (string folder in artFolders)
                    {
                        TransferOperationResult transferResult = null;

                        if (isPushToHost) //Push
                        {
                            string remotePath = $"/home/pi/.emulationstation/{folder}/";
                            string localPath = $@"D:\RetroPie\RetroPieSync\{sourceHost}\{folder}\*";

                            transferResult = session.PutFiles(localPath, remotePath, false, transferOptions);

                        }
                        else //Pull Sync
                        {
                            string remotePath = $"/home/pi/.emulationstation/{folder}/*";
                            string localPath = $@"D:\RetroPie\RetroPieSync\{destinationHost}\{folder}\";

                            transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);
                        }

                        // Throw on any error
                        transferResult.Check();

                        // Print results
                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                        {
                            Debug.WriteLine("Download of {0} succeeded", transfer.FileName);
                        }
                    }

                }


            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: {0}", e);

            }
        }

        protected void FetchRoms()
        {
            try
            {

                List<string> romFolders = new List<string>() { "fba" };//, "mame-libretro" };

                using (Session session = new Session())
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(sessionOptions);

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    foreach (string folder in romFolders)
                    {
                        TransferOperationResult transferResult = null;

                        if (isPushToHost) //Push
                        {
                            string remotePath = $"/home/pi/RetroPie/roms/{folder}/*";
                            string localPath = $@"D:\RetroPie\RetroPieSync\{destinationHost}\roms\{folder}\*";

                            if (isFetchArtwork)
                            {
                                remotePath = $"/home/pi/.emulationstation/downloaded_images/*";
                                localPath = $@"D:\RetroPie\RetroPieSync\{sourceHost}\downloaded_images\";
                            }

                            transferResult = session.PutFiles(localPath, remotePath, false, transferOptions);

                        }
                        else //Pull Sync
                        {
                            string remotePath = $"/home/pi/RetroPie/roms/{folder}/*";
                            string localPath = $@"D:\RetroPie\RetroPieSync\{sourceHost}\roms\";

                            if (isFetchArtwork)
                            {
                                remotePath = $"/home/pi/.emulationstation/downloaded_images/*";
                                localPath = $@"D:\RetroPie\RetroPieSync\{destinationHost}\downloaded_images\";
                            }

                            transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);
                        }

                        // Throw on any error
                        transferResult.Check();

                        // Print results
                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                        {
                            Debug.WriteLine("Download of {0} succeeded", transfer.FileName);
                        }
                    }

                }


            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: {0}", e);

            }
        }

        private static void Session_Failed(object sender, FailedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void FileTransferred(object sender, TransferEventArgs e)
        {
            if (e.Error == null)
            {
                Debug.WriteLine("Upload of {0} succeeded", e.FileName);
            }
            else
            {
                Debug.WriteLine("Upload of {0} failed: {1}", e.FileName, e.Error);
            }

            if (e.Chmod != null)
            {
                if (e.Chmod.Error == null)
                {
                    Debug.WriteLine(
                        "Permissions of {0} set to {1}", e.Chmod.FileName, e.Chmod.FilePermissions);
                }
                else
                {
                    Debug.WriteLine(
                        "Setting permissions of {0} failed: {1}", e.Chmod.FileName, e.Chmod.Error);
                }
            }
            else
            {
                Debug.WriteLine("Permissions of {0} kept with their defaults", e.Destination);
            }

            if (e.Touch != null)
            {
                if (e.Touch.Error == null)
                {
                    Console.WriteLine(
                        "Timestamp of {0} set to {1}", e.Touch.FileName, e.Touch.LastWriteTime);
                }
                else
                {
                    Console.WriteLine(
                        "Setting timestamp of {0} failed: {1}", e.Touch.FileName, e.Touch.Error);
                }
            }
            else
            {
                // This should never happen during "local to remote" synchronization
                Debug.WriteLine(
                    "Timestamp of {0} kept with its default (current time)", e.Destination);
            }
        }

    }

}
