using Renci.SshNet;
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
        protected static bool fetchConfig = false;
        protected static bool isPushToHost = true;// false; //pull
        protected static bool isFetchArtwork = true;
        protected static string sourceHost = "192.168.1.149";
        protected static string destinationHost = "192.168.1.117";//"192.168.1.117";//"192.168.1.117";

        protected static string killEmuStation = "sudo ps -ef | awk '/emulation/ {print $2}' | xargs sudo kill -9 2092";
        protected static string killMame = "sudo ps -ef | awk '/mame/ {print $2}' | xargs sudo kill -9 2092";
        protected static string shutDown = "sudo shutdown -h now";
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
            //emulationstation
            //ps -ef | awk '/emulation/ {print $2}' | xargs kill

            //kill $(pidof emulationstation)

            //var command = @"/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2000/mame2000_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg /home/pi/RetroPie/roms/arcade/robotron.zip --appendconfig /opt/retropie/configs/all/retroarch.cfg";
            //var command = "'/opt/retropie/supplementary/runcommand/runcommand.sh' 0 SYS arcade '/home/pi/RetroPie/roms/arcade/1941.zip'";//mame-mame4all
            //var command = "subprocess.Popen('/opt/retropie/supplementary/runcommand/runcommand.sh', '0', '_SYS_', 'arcade' '/home/pi/RetroPie/roms/arcade/1941.zip')";
            //
            //RunSSHCommand(new SshClient("192.168.1.149", "pi", "raspberry"), "sudo reboot");

            RunRandomRom("192.168.1.149");
            RunRandomRom("192.168.1.148");
            //RunSSHCommands(new SshClient("192.168.1.148", "pi", "raspberry"), new string[] { killEmuStation,  command });
            if (fetchArtwork) FetchArtwork();
            if (fetchConfig) FetchConfig();
        }

        protected static string GetRunRomCommand(RemoteFileInfo rfi) //string romPath, string system = "mame2000"
        {
            Uri uri = new Uri($"file:/{rfi.FullName}"); //, UriKind.Relative
            var parent = uri.Segments[uri.Segments.Length - 2].Replace("/", "");//.GetParentUriString(uri);

            if (parent == "fba") parent = "fbneo";
            if (parent == "arcade") parent = "mame2000";

            var command = $@"sudo /opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-{parent}/{parent}_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg {rfi.FullName} --appendconfig /opt/retropie/configs/all/retroarch.cfg";
            Console.WriteLine($"run command:\r\n{command}");
            return command;
        }

        static string GetParentUriString(Uri uri)
        {
            return uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
        }

        protected static void RunRandomRom(string ipClient)
        {
            RemoteFileInfo randomRom;
            using (var session = new WinSCP.Session())
            {
                //session.Timeout = TimeSpan.FromSeconds(10);
                session.Open(new SessionOptions
                {
                    GiveUpSecurityAndAcceptAnySshHostKey = true,
                    Protocol = Protocol.Sftp,
                    HostName = ipClient,
                    UserName = "pi",
                    Password = "raspberry"
                });

                var directories = session.EnumerateRemoteFiles("/home/pi/RetroPie/roms/", "", EnumerationOptions.MatchDirectories);
                var dirct = directories.Count();

                List<RemoteFileInfo> romsByPath = new List<RemoteFileInfo>();

                foreach(RemoteFileInfo di in directories)
                {
                    Console.WriteLine($"{di.FullName}");

                    var fileInfos = session.EnumerateRemoteFiles(di.FullName, "*.zip", EnumerationOptions.None).ToList();//.Select(fi => fi.FullName).ToList();

                    if (fileInfos.Count > 0)
                    {
                        Console.WriteLine($"Writing {fileInfos.Count} files from path: {di.FullName}");
                        romsByPath.AddRange(fileInfos);
                    }
                    else
                    {
                        //Console.WriteLine($"Nothing found at {di.FullName}");
                    }
                    
                    //romsByPath.Add(di.FullName);
                }



                var r = new Random();
                randomRom = romsByPath.ElementAt(r.Next(1, romsByPath.Count()));

                Console.WriteLine($"Attempting to run {randomRom.FullName}");
                //foreach (RemoteFileInfo fileInfo in fileInfos)
                //{
                //    Console.WriteLine("Adding {0} to listing", fileInfo.Name);
                //}
            }
            string command = GetRunRomCommand(randomRom);

            ExecuteSSHCommands(ipClient, new string[] { killEmuStation, killMame, command });
            //RunSSHCommands(new SshClient(ipClient, "pi", "raspberry"), new string[] { killEmuStation, killMame, command });
        }

        protected static void RunSSHCommand(SshClient client, string commandText)
        {
            using (client)// var client = new SshClient("192.168.1.149", "pi", "raspberry"))
            {
                client.Connect();
                var command = client.RunCommand(commandText);//.Execute();
                if (!string.IsNullOrEmpty(command.Error))
                {
                    var error = command.Error;
                } else
                {
                    var output = command.Result;
                }
                
            }
        }

        protected static void RunSSHCommands(SshClient client, string[] commands)
        {
            using (client)// var client = new SshClient("192.168.1.149", "pi", "raspberry"))
            {
                client.Connect();

                foreach(var cmd in commands)
                {
                    Task.Run(() => {

                        var command = client.RunCommand(cmd);//.Execute();
                        if (!string.IsNullOrEmpty(command.Error))
                        {
                            var error = command.Error;
                            Console.WriteLine(error);
                        }
                        else
                        {
                            var output = command.Result;
                            Console.WriteLine(output);
                        }

                    });
                }
                client.Disconnect();
            }
        }

        protected static void ExecuteSSHCommands(string ipClient, string[] commands)
        {
            using (var session = new WinSCP.Session())
            {
                session.Timeout = TimeSpan.FromSeconds(5);
                session.AddRawConfiguration("ExitCode1IsError", "1");

                session.Open(new SessionOptions
                {
                    GiveUpSecurityAndAcceptAnySshHostKey = true,
                    Protocol = Protocol.Sftp,
                    HostName = ipClient,
                    UserName = "pi",
                    Password = "raspberry"
                });

                foreach (var cmd in commands)
                {
                    //Task.Run(() => {

                    try
                    {
                        var result = session.ExecuteCommand(cmd);

                        if (result.IsSuccess)
                        {
                            Console.WriteLine(result.Output);
                        }
                        else
                        {
                            if (result.ErrorOutput != null)
                            {
                                if (result.ErrorOutput.Contains("No such process"))
                                {
                                    Console.WriteLine(result.ErrorOutput);
                                }
                                else if (result.ErrorOutput.Contains("not supported"))
                                {
                                    Console.WriteLine(result.ErrorOutput);
                                }
                                else
                                {
                                    Console.WriteLine(result.ErrorOutput);
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine($"{exc.Message}");
                    }


                }
                        //var command = client.RunCommand(cmd);//.Execute();
                        //if (!string.IsNullOrEmpty(command.Error))
                        //{
                        //    var error = command.Error;
                        //    Console.WriteLine(error);
                        //}
                        //else
                        //{
                        //    var output = command.Result;
                        //    Console.WriteLine(output);
                        //}

                    //});
                //}
            }
        }

        protected static void FetchConfig()
        {
            ///opt/retropie/configs/*.cfg
            try
            {
                using (var session = new WinSCP.Session())
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

                using (var session = new WinSCP.Session())
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

                using (var session = new WinSCP.Session())
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
