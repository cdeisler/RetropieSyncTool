﻿using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
        protected static string destinationHost = "192.168.1.149";//"192.168.1.117";//"192.168.1.117";

        protected static string killEmuStation = "sudo ps -ef | awk '/emulation/ {print $2}' | xargs sudo kill -9 2092";
        protected static string killMame = "sudo ps -ef | awk '/mame/ {print $2}' | xargs sudo kill -9 2092";
        protected static string shutDown = "sudo shutdown -h now";
        protected static string reboot = "sudo reboot";
        protected static string emulationstation = "sudo emulationstation";

        protected static string aptgetupdate = "sudo apt-get update";
        protected static string installpythonpip = "sudo apt install python3-pip --fix-missing";
        // sudo pip3 install paramiko


        protected static string test = @"import paramiko

ip='192.168.1.148'
port=22
username='pi'
password='raspberry'

cmd='some useful command' 

ssh=paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(ip,port,username,password)

stdin,stdout,stderr=ssh.exec_command(cmd)
outlines=stdout.readlines()
resp=''.join(outlines)
print(resp)

stdin,stdout,stderr=ssh.exec_command('sudo reboot')
outlines=stdout.readlines()
resp=''.join(outlines)
print(resp)";
        //        protected static string test = @"client = SSHClient()
        //client.connect(host, port, username)
        //session = client.get_transport().open_session()
        //AgentRequestHandler(session)
        //session.exec_command('{0}')";

        //"{} 0 _SYS_ {} '{}'".format( RUNCOMMAND, system, path )
        protected static string RUNCOMMAND = "/opt/retropie/supplementary/runcommand/runcommand.sh";
        protected static string EMULATIONSTATION = "/opt/retropie/supplementary/emulationstation/emulationstation.sh";
    
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

            var command = @"/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2000/mame2000_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg /home/pi/RetroPie/roms/arcade/robotron.zip --appendconfig /opt/retropie/configs/all/retroarch.cfg";
            //var command = "'/opt/retropie/supplementary/runcommand/runcommand.sh' 0 SYS arcade '/home/pi/RetroPie/roms/arcade/1941.zip'";//mame-mame4all
            //var command = "subprocess.Popen('/opt/retropie/supplementary/runcommand/runcommand.sh', '0', '_SYS_', 'arcade' '/home/pi/RetroPie/roms/arcade/1941.zip')";

            //RunSSHCommand(new SshClient("192.168.1.148", "pi", "raspberry"), command);//, //);

            //;
            //RunRandomRom("192.168.1.117");

            //RunRom("192.168.1.149", "/home/pi/RetroPie/roms/mame-libretro/mame2003/tmek.zip");

            //ExecuteSSHCommands("192.168.1.148", new string[] {  command });

            //C:\Users\CJ\Downloads\MAME2003_Reference_Set_MAME0.78_ROMs_CHDs_Samples\roms

            //foreach (var path in Directory.EnumerateFiles(@"C:\Users\CJ\Downloads\MAME2003_Reference_Set_MAME0.78_ROMs_CHDs_Samples\roms\").Where(s => s.EndsWith(".zip")))
            //{
            //    //Uri uri = new Uri(path);
            //    //uri.LocalPath
            //    var fileName = Path.GetFileName(path);
            //    WriteRom("192.168.1.149", path, $"/home/pi/RetroPie/roms/arcade/mame2003/"); //.zip
            //}

            string remoteIPClient = "192.168.1.117";
            ExecuteSSHCommands(remoteIPClient, new string[] { reboot });
            ProcessDATFile();


                //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatFiles", $@"mame2003_noclone.xml");

                //// var txlife = getOrderXmlTxLife(orderid);

                //XmlDocument testorder = new XmlDocument();

                //if (!string.IsNullOrEmpty(path))
                //{
                //    testorder.Load(path);//.LoadXml(txlife);
                //}

                //var allGameNodes = testorder.SelectNodes("//mame/game");
                //int ct = allGameNodes.Count;

                //Console.WriteLine($"{ct} games found.");

                //for(int i=0; i<ct; i++)
                //{
                //    var gameName = allGameNodes.Item(i).Attributes["name"].Value;
                //    var localPath = $@"C:\Users\CJ\Downloads\MAME2003_Reference_Set_MAME0.78_ROMs_CHDs_Samples\roms\{gameName}.zip";
                //    var fileName = Path.GetFileName(path);
                //    using (var session = new WinSCP.Session())
                //    {
                //        WriteRom("192.168.1.149", session, localPath, $"/home/pi/RetroPie/roms/arcade/mame2003/"); //.zip
                //    }
                //}

            

            //RunRandomRom("192.168.1.148");
            //RunRandomRom("192.168.1.149");


            //RunSSHCommands(new SshClient("192.168.1.148", "pi", "raspberry"), new string[] { killEmuStation,  command });
            if (fetchArtwork) FetchArtwork();
            if (fetchConfig) FetchConfig();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        protected static string GetRunRomCommand(string rfi) //string romPath, string system = "mame2000"
        {
            Uri uri = new Uri($"file:/{rfi}"); //, UriKind.Relative
            var parent = uri.Segments[uri.Segments.Length - 2].Replace("/", "");//.GetParentUriString(uri);

            if (parent == "fba") parent = "fbneo";
            //if (parent == "arcade") parent = "mame2000";
            if (parent == "mame2003") parent = "mame-libretro";

            //sudo /opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2000/mame2000_libretro.so --config /opt/retropie/configs/mame-mame4all/retroarch.cfg /home/pi/RetroPie/roms/arcade/hbarrel.zip
            var command = $@"sudo /opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-{parent}/{parent}_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg {rfi} --appendconfig /opt/retropie/configs/all/retroarch.cfg &";
            //var command = $@"sudo {RUNCOMMAND} 0 _SYS_ {parent} {rfi}";  //export DISPLAY=:0

//            test = @"client = SSHClient()
//client.connect({}, {}, {})
//session = client.get_transport().open_session()
//AgentRequestHandler(session)
//session.exec_command('{0}')";

            Console.WriteLine($"run command:\r\n{command}");
            return command;
        }

        protected static string GetRunMameCommand(string romName, string emulator = "mame4all")  //1943
        {
            ///opt/retropie/emulators/mame4all/mame "1943"
            ///   var command = $@"sudo {RUNCOMMAND} 0 _SYS_ {parent} {rfi}";  //export DISPLAY=:0

            var command = $@"nohup /opt/retropie/emulators/{emulator}/mame '{romName}'";  //
            Console.WriteLine($"run command:\r\n{command}");
            return command;
        }

        static string GetParentUriString(Uri uri)
        {
            return uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
        }

        protected static void RunRom(string ipClient, string romPath)
        {
            ///home/pi/RetroPie/roms/mame-libretro/mame2003/tmek.zip
            ///
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


                Console.WriteLine($"Attempting to run {romPath}");

                string command = GetRunRomCommand(romPath);

                ExecuteSSHCommands(ipClient, new string[] { killEmuStation, killMame, command });

            }

        }
        
        protected static void WriteRom(string ipClient, WinSCP.Session session, string romPathLocal, string romPathDestination)  //$"/home/pi/RetroPie/roms/{folder}/*
        {
            try
            {
                //List<string> romFolders = new List<string>() { "fba" };//, "mame-libretro" };

                //using (var session = new WinSCP.Session())
                //{

                if (!session.Opened)
                {
                    SessionOptions sessionOptions = new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipClient,
                        UserName = "pi",
                        Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                    };

                    //session.SessionLogPath = @"c:\temp\WinScp_Session_$Date.log";
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    session.Open(sessionOptions);
                }

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    //foreach (string folder in romFolders)
                    //{
                        TransferOperationResult transferResult = null;

                        //if (isPushToHost) //Push
                        //{
                        string remotePath = romPathDestination;//$"/home/pi/RetroPie/roms/{folder}/*";
                        //string localPath = $@"D:\RetroPie\RetroPieSync\{destinationHost}\roms\{folder}\*";
                        string localPath = romPathLocal;
                        //if (isFetchArtwork)
                        //{
                        //    remotePath = $"/home/pi/.emulationstation/downloaded_images/*";
                        //    localPath = $@"D:\RetroPie\RetroPieSync\{sourceHost}\downloaded_images\";
                        //}x

                        //var transferResult2 = session.PutFileToDirectory(localPath, remotePath, false, transferOptions);
                    var transferResult2 = session.PutFiles(localPath, remotePath, false, transferOptions);
                    //.PutFiles(localPath, remotePath, false, transferOptions);

                    //}


                    // Throw on any error
                    transferResult2.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult2.Transfers)
                    {
                        Console.WriteLine("WriteRom of {0} succeeded", transfer.FileName);
                    }
                    //}

                //}


            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine("InvalidOperationException: {0}", ioe);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);

            }
        }

        protected static void CreateDirectory(string ipClient, WinSCP.Session session, string folderPathRemote)  //$"/home/pi/RetroPie/roms/{folder}/*
        {
            try
            {
                //List<string> romFolders = new List<string>() { "fba" };//, "mame-libretro" };

                //using (var session = new WinSCP.Session())
                //{
                //session.SessionLogPath = @"c:\temp\WinScp_Session_$Date.log";
                //session.FileTransferred += FileTransferred;
                //session.Failed += Session_Failed;
                //// Connect
                //session.Open(sessionOptions);

                // Download files
                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;
                transferOptions.AddRawSettings("FtpListAll", "0");

                string remotePath = folderPathRemote;//$"/home/pi/RetroPie/roms/{folder}/*";

                if (!session.FileExists(remotePath)) session.CreateDirectory(remotePath);
                else Console.WriteLine($"File {remotePath} already exists.");


            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine("InvalidOperationException: {0}", ioe);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);

            }
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
                    try
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

                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
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

            string command = GetRunRomCommand(randomRom.FullName);
            //ddragon3.zip
            //string command = GetRunMameCommand("ddragon3");// randomRom.Name.Replace(".zip", "1943.zip"));
            ExecuteSSHCommands(ipClient, new string[] {  killEmuStation, killMame, command }); //killEmuStation, killMame,  
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

                        result.Check();

                        if (result.IsSuccess)
                        {
                            Console.WriteLine($"Success: {result.Output}");
                            if (result.Output.Contains("not supported"))
                            {
                                Console.WriteLine(result.Output);
                            }
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
            Console.WriteLine($@"Error Session_Failed: {e.Error.Message}");
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


        /**********************************************************************/
        /*********************** DAT File Processing **************************/
        /**********************************************************************/


        public static string ProcessDATFile(string remoteIPClient = "192.168.1.117")
        {
            try
            {
                //string remoteIPClient = "192.168.1.117";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatFiles", $@"mame2003-lr-no-clones-no-neogeo.xml");

                // var txlife = getOrderXmlTxLife(orderid);

                XmlDocument testorder = new XmlDocument();

                if (!string.IsNullOrEmpty(path))
                {
                    testorder.Load(path);//.LoadXml(txlife);
                }
                //return testorder.OuterXml.ToString();

                //var nsmgr = new XmlNamespaceManager(testorder.NameTable);
                //nsmgr.AddNamespace("ns", "http://ACORD.org/Standards/Life/2");
                //nsmgr.AddNamespace("ims", "http://imsparamed.com/schemas/acorn/ims");

                //var node = testorder.FirstChild;

                var xmlRootNodeNode = "datafile";// mame

                var allGameNodes = testorder.SelectNodes($"//{xmlRootNodeNode}/game");
                var gameNodes = testorder.SelectNodes($"//{xmlRootNodeNode}/game[not(@cloneof)]");//, nsmgr);

                var mameNode = testorder.SelectSingleNode($"//{xmlRootNodeNode}");
                int act = allGameNodes.Count;
                int ct = gameNodes.Count;

                mameNode.RemoveAll();

                List<string> categories = new List<string>();

                //var test = allGameNodes.GetEnumerator().ToList();

                for (int i = 0; i < act; i++)
                //            foreach(XmlNode node in gameNodes)
                {
                    var node = allGameNodes.Item(i);
                    var catNode = node.SelectSingleNode("/category");
                    if (catNode != null)
                    {
                        categories.Add(catNode.InnerText);
                    }

                    mameNode.AppendChild(node);
                }

                List<string> categoryExclusionList = new List<string>()
                {
                    "Casino",
                    "Quiz",
                    "Tabletop",
                    "Pinball",
                    "*Mature*",
                    "Unplayable",
                    "Rhythm"
                };

                List<string> descriptionExclusionList = new List<string>()
                {
                    "(Japan)"
                };

                var includedCategories = categories.Distinct().Where(c => !categoryExclusionList.Any(exc => c.Contains(exc))).OrderBy(c => c);
                var distinctCategories = string.Join("\r\n", includedCategories);
                Console.WriteLine($"Total categories: {distinctCategories}");

                var distinctFolderList = includedCategories.Select(c => c.IndexOf(" ", 0, c.Length - 1) > 0 ? c.Split(' ').First() : c).Distinct().OrderBy(c => c);
                Console.WriteLine($"Folder List: {string.Join("\r\n", distinctFolderList)}");

                using (var session = new WinSCP.Session())
                {
                    if (!session.Opened)
                    {
                        session.FileTransferred += FileTransferred;
                        session.Failed += Session_Failed;
                        session.Open(new SessionOptions
                        {
                            GiveUpSecurityAndAcceptAnySshHostKey = true,
                            Protocol = Protocol.Sftp,
                            HostName = remoteIPClient,
                            UserName = "pi",
                            Password = "raspberry"
                        });

                        CreateDirectory(remoteIPClient, session, $"/home/pi/RetroPie/roms/arcade/mame2003");
                        CreateDirectory(remoteIPClient, session, $"/home/pi/RetroPie/roms/arcade/mame2003/working");

                        foreach (var folderName in distinctFolderList)
                        {
                            CreateDirectory(remoteIPClient, session, $"/home/pi/RetroPie/roms/arcade/mame2003/working/{folderName}");
                        }
                    }
                }

                var xml = testorder.OuterXml.ToString();
                var finalDoc = new XmlDocument();
                XDocument doc = XDocument.Parse(xml);

                finalDoc.LoadXml(doc.ToString());
                var allGameNodesTest = finalDoc.SelectNodes($"//{xmlRootNodeNode}/game");
                int ctt = allGameNodesTest.Count;

                using (var session = new WinSCP.Session())
                {
                    foreach (var folder in distinctFolderList)
                    {
                        var folderNodes = testorder.SelectNodes($"//{xmlRootNodeNode}/game/category[starts-with(text(),'{folder}')]");

                        for (var i = 0; i < folderNodes.Count; i++)// gameNode in folderNodes)
                        {
                            var gameNode = folderNodes.Item(i).ParentNode;
                            var gameName = gameNode.Attributes["name"]?.Value;
                            var gameDesc = gameNode.SelectSingleNode("//description")?.InnerText;
                            var isDescNull = gameDesc == null;

                            if (isDescNull)
                            {
                                Console.WriteLine($"Game Desc Null: {gameName}");
                            }

                            if (!isDescNull && !descriptionExclusionList.Any(del => gameDesc.Contains(del)))
                            {
                                var localPath = $@"C:\Users\CJ\Downloads\MAME2003_Reference_Set_MAME0.78_ROMs_CHDs_Samples\roms\{gameName}.zip";

                                if (File.Exists(localPath))
                                {
                                    var fileName = Path.GetFileName(localPath);
                                    WriteRom(remoteIPClient, session, localPath, $"/home/pi/RetroPie/roms/arcade/mame2003/working/{folder}/{fileName}"); //.zip
                                }
                                else
                                {
                                    Console.WriteLine($"File doesn't exists {gameName}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Skipped {gameName} due to desc exclusion list item {gameDesc}");
                            }
                        }
                    }
                }

                //MemoryStream mStream = new MemoryStream();
                //XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);

                //finalDoc.Save(@"c:\temp\mame2003_noclone.xml");
                return xml;

            }
            catch (Exception exc) {
                Console.WriteLine($"Exception: {exc}");
            }
            return "";

        }

    }

}
