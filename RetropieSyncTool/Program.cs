using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace RetropieSyncTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool isPushToHost = false; //pull
                string sourceHost = "192.168.1.149";
                string destinationHost = "192.168.1.117";

                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    GiveUpSecurityAndAcceptAnySshHostKey = true,
                    Protocol = Protocol.Sftp,
                    HostName = destinationHost,//"192.168.1.117",//"192.168.1.149",
                    UserName = "pi",
                    Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                };


                List<string> romFolders = new List<string>() { "mame-libretro", "fba" };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    foreach (string folder in romFolders)
                    {
                        TransferOperationResult transferResult = null;

                        if (!isPushToHost) //Pull Sync
                        {
                            transferResult = session.GetFiles($"/home/pi/RetroPie/roms/{folder}/*", $@"D:\RetroPie\RetroPieSync\{sourceHost}\roms\{folder}\", false, transferOptions);
                        }
                        else //Push
                        {
                            transferResult = session.PutFiles($@"D:\RetroPie\RetroPieSync\{sourceHost}\roms\{folder}\", $"/home/pi/RetroPie/roms/{folder}/*", false, transferOptions);
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
                //using (Session session = new Session())
                //{
                //    // Connect
                //    session.Open(sessionOptions);

                //    // Download files
                //    TransferOptions transferOptions = new TransferOptions();
                //    transferOptions.TransferMode = TransferMode.Binary;

                //    foreach(string folder in romFolders)
                //    {
                //        TransferOperationResult transferResult = session.GetFiles($"/home/pi/RetroPie/roms/{folder}/*", $@"D:\RetroPie\RetroPieSync\{sessionOptions.HostName}\roms\{folder}\", false, transferOptions);

                //        // Throw on any error
                //        transferResult.Check();

                //        // Print results
                //        foreach (TransferEventArgs transfer in transferResult.Transfers)
                //        {
                //            Debug.WriteLine("Download of {0} succeeded", transfer.FileName);
                //        }
                //    }

                //}

            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: {0}", e);
                
            }
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
