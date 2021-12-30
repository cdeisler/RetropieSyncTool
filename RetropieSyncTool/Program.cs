using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        protected static string cachingServerHost = "127.0.0.1";
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




        #region paramiko test script

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

        #endregion

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

            //var command = @"/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2000/mame2000_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg /home/pi/RetroPie/roms/arcade/robotron.zip --appendconfig /opt/retropie/configs/all/retroarch.cfg";
            //var command = "'/opt/retropie/supplementary/runcommand/runcommand.sh' 0 SYS arcade '/home/pi/RetroPie/roms/arcade/mame2003/working/Platform/joust.zip && emulationstation'";//mame-mame4all

            //"/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2003/mame2003_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg /home/pi/RetroPie/roms/arcade/mame2003/working/Platform/joust.zip --appendconfig /opt/retropie/configs/all/retroarch.cfg && emulationstation"

            //var command = "subprocess.Popen('/opt/retropie/supplementary/runcommand/runcommand.sh', '0', '_SYS_', 'arcade' '/home/pi/RetroPie/roms/arcade/1941.zip')";

            string remoteIPClient = "192.168.0.1";// "192.168.1.149";
            var v3Clients = new string[] { "192.168.1.149", "192.168.1.117" };

            #region Mame / Tmek Files

            //RebootComputerOverSSH("192.168.1.149");

            //RunRomTests();
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
            //GetArtwork("192.168.1.117");


            //CleanGamelistNodes();
            //return;
            //BuildGamelistXml();

            #endregion 

            //using (var session = new WinSCP.Session())
            //{
            var sourceIP = "192.168.0.141";// .137";// 214";

            List<string> continueInstall = new List<string>()
            {
                "echo modify default.conf",

                "cd /etc/apache2/sites-available/",
                "sudo chown -R www-data:www-data 000-default.conf",
                
                //"sudo chown -R www-data /var/www",
                //"sudo chgrp -R www-data /var/www",
                "a2ensite 004-pihole.conf",

                "echo a2enmod",
                "sudo a2enmod rewrite",
                "sudo service apache2 restart",

                "sudo chmod -R /var/www/webcore 400",
                @"find /var/www/webcore -type d -exec chmod -R u+x {} \;",

                "sudo chown www-data:www-data /etc/apache2/apache2.conf",
                "sudo chown -R www-data:www-data /etc/apache2/sites-available/",
                "sudo chown www-data:www-data /etc/apache2/apache2.conf",
                "sudo chown www-data:www-data /etc/pihole/setupVars.conf",
                
                "cd /etc/apache2/",

                //"echo \"<Directory /var/www/webcore/>\" >> apache2.conf",
                //"echo \"Options Indexes FollowSymLinks\" >> apache2.conf",
                //"echo \"AllowOverride None\" >> apache2.conf",
                //"echo \"Require all granted\" >> apache2.conf",
                //"echo \"</Directory>\" >> apache2.conf",

                "sudo service apache2 reload",
                "echo exit"
            };

            //sudo apt-get upgrade -y
            string installApacheScript = @"
#sudo apt-get update -y
#sudo apt-get install git make -y
#sudo apt-get install apache2 -y
   
cd /home/pi/

sudo chown -R pi /tmp/install-apache.sh
sudo chown -R pi /tmp/dhcpcd.conf
sudo chown -R pi /tmp/004-pihole.conf
sudo chown -R pi /tmp/setupVars.conf
sudo chown -R pi /tmp/wpa_supplicant.conf

cd /
sudo -u root mkdir /etc/pihole/

cd /tmp/
mv -f wpa_supplicant.conf ~/
mv -f ssh ~/
mv -f setupVars.conf /etc/pihole/

cd /tmp/
mv -f setupVars.conf /etc/pihole/

echo ""running pihole install""
cd /home/pi/

if [ -d \""/home/pi/Pi-hole\"" ]
then
echo  ""Pi-hole exists, removing existing install first""
sudo rm -f -r /home/pi/Pi-hole
else
echo  ""Pi-hole folder missing""
fi

if [ -d \""/home/pi/Pi-hole\"" ]
then
echo ""Pi-hole exists, skipping install"" 
else
echo git clone pihole
git clone --depth 1 https://github.com/pi-hole/pi-hole.git Pi-hole
cd ""Pi-hole/automated install/""

sudo bash basic-install.sh --disable-install-webserver --unattended
fi

echo  ""pihole now installed""

echo  ""evaluating webCoRE install ""
cd /home/pi/

if [ -d \""/home/pi/webCoRE\"" ]
then
echo  ""webCoRE exists, removing existing install first""
sudo rm -f -r /home/pi/webCoRE
else
echo  ""webCoRE folder missing""
fi

if [ -d \""/home/pi/webCoRE\"" ]
then
echo ""webCoRE exists, skipping install"" 
else
echo git clone webcore
#git clone https://github.com/ajayjohn/webCoRE
#git clone https://github.com/jp0550/webCoRE
git clone https://github.com/imnotbob/webCoRE
fi


cd webCoRE
echo checkout patches
git checkout hubitat-patches
cd dashboard

sudo rm /var/www/webcore
sudo ln -s `pwd` /var/www/webcore

cd ~/
cd /tmp
mv -f 000-default.conf /etc/apache2/sites-available/
mv -f 004-pihole.conf /etc/apache2/sites-available/
mv -f apache2.conf /etc/apache2/


cd /etc/apache2/
sed -i '1s/^\xEF\xBB\xBF//' apache2.conf

sudo chown -R www-data /etc/apache2/sites-available/
sudo chmod 775 '/etc/apache2/sites-available/'
sudo chown www-data /etc/apache2/apache2.conf

sudo find /var/www -type d -exec chmod 2750 {} \+
sudo find /var/www -type f -exec chmod 640 {} \+

sudo chown -R www-data /var/www
sudo chgrp -R www-data /var/www

echo install php
sudo usermod -a -G pihole www-data
sudo apt install php libapache2-mod-php
sudo service apache2 restart


echo ""adding hubitat backups chron""

cd /home/pi/
sudo mkdir hubitat
sudo chown pi hubitat
cd hubitat
sudo mkdir backups
sudo chown pi backups

if [ -e \""/etc/cron.daily/backup-hubitat\"" ]
then
echo  ""backup-hubitat exists, erasing existing script first""
sudo truncate -s 0 /etc/cron.daily/backup-hubitat
fi

#echo $'cd /home/pi/hubitat/backups/\nwget --output-document=`date +"" % Y -% m -% d""`.lzf http://192.168.0.129/hub/backupDB?fileName=latest' | sudo tee -a /etc/cron.daily/backup-hubitat

echo exit
";

            string pathSiteConf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"000-default.conf");
            string pathPiHoleSiteConf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"004-pihole.conf");
            string pathApacheConf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"apache2.conf");
            string pathDhcpcdConf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"dhcpcd.conf");
            string pathSetupVarsConf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"setupVars.conf");
            string pathWpaSupplicantConf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"wpa_supplicant.conf");
            string pathSSHFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ssh");
            string pathInterfacesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"interfaces");

            //var ssh = new SshClient("192.168.0.214", "pi", "raspberry");

            //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"install-apache.sh");
            string path = Path.Combine("c:/temp/", $@"install-apache.sh");
            File.WriteAllLines(path, installApacheScript.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));//.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());

            string pathContinue = Path.Combine("c:/temp/", $@"install-apache-cont.sh");
            File.WriteAllLines(pathContinue, continueInstall);


            //RunSSHCommandStream(new SshClient("192.168.0.214", "pi", "raspberry"), installApache2);
            //RunSSHCommands(new SshClient("192.168.0.214", "pi", "raspberry"), installApache2.ToArray());


            //installApache2.ForEach(cmd => {
            //    Console.WriteLine($"running {cmd} on {sourceIP}");
            //    RunSSHCommands(new SshClient("192.168.0.214", "pi", "raspberry"), installApache2.ToArray());
            //    //ExecuteSSHCommands(sourceIP, new string[] { cmd });
            //});

            //ProcessBlacklist(sourceIP);
            //SynchronizeClientToServerDirectories(sourceIP, session, $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\home\pi\RetroPie\roms\arcade\", "/home/pi/RetroPie/roms/arcade/", true);
            //SynchronizeClientToServerDirectories(sourceIP, session, $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\configs\arcade\", "/opt/retropie/configs/arcade/", true);
            //SyncArtwork(sourceIP);
            //}


            //var destIP = sourceIP;// "192.168.1.149";
            bool isFirstRun = false;
            if (isFirstRun)
            {
                //session.PutFileToDirectory(pathWpaSupplicantConf, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                //session.PutFileToDirectory(pathSSHFile, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                var sdcard = new DirectoryInfo(@"F:\");
                if (sdcard.Exists)
                {
                    File.Copy(pathSSHFile, $@"F:\ssh");
                    File.Copy(pathWpaSupplicantConf, $@"F:\wpa_supplicant.conf");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadLine();
                    return;
                }
            }


            using (var session = new WinSCP.Session())
            {
                session.Open(new SessionOptions
                {
                    GiveUpSecurityAndAcceptAnySshHostKey = true,
                    Protocol = Protocol.Sftp,
                    HostName = sourceIP,
                    UserName = "pi",
                    Password = "raspberry"
                });

                var permissions = new FilePermissions() {
                    UserExecute = true,
                    UserWrite = true,
                    OtherWrite = true,
                    OtherRead = true,
                    GroupRead = true,
                    GroupWrite = true,
                    GroupExecute = true
                };


                //RunSSHCommandStream(new SshClient("192.168.0.214", "pi", "raspberry"), new List<string>() { "echo here", "cd /etc/apache2/sites-available/", "sudo chown pi: 000-default.conf" });


                bool isSecondRun = true;
                if (isSecondRun)
                {

                    var commandsDhcp = @"
sudo chown pi:root /tmp/dhcpcd.conf
sudo chown pi:root /etc/dhcpcd.conf

sudo chown pi:root /tmp/interfaces
sudo chown pi:root /etc/network/interfaces

cd /tmp
sudo mv -f interfaces /etc/network/
sudo chown root:root /etc/network/interfaces

sudo mv -f dhcpcd.conf /etc/
sudo chown root:root /etc/dhcpcd.conf


#sudo reboot
";

                    var commandsRemoveCronJob = new string[] { "sudo chown pi:root /etc/cron.daily/backup-hubitat",
                                                                "sudo rm -f -r /etc/cron.daily/backup-hubitat",
                                                                "sudo reboot"
                                                            };
                    

                    session.PutFileToDirectory(pathDhcpcdConf, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, TransferMode = TransferMode.Automatic });
                    session.PutFileToDirectory(pathInterfacesFile, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, TransferMode = TransferMode.Automatic });

                    string[] splitted = commandsDhcp.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    ExecuteSSHCommands(sourceIP, splitted);

                    Console.WriteLine("Press any key to exit.");
                    Console.ReadLine();
                    return;
                }

                session.PutFileToDirectory(path, "/tmp/", true, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                session.PutFileToDirectory(pathContinue, "/tmp/", true, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                session.PutFileToDirectory(pathSiteConf, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                session.PutFileToDirectory(pathApacheConf, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                
                session.PutFileToDirectory(pathPiHoleSiteConf, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                session.PutFileToDirectory(pathSetupVarsConf, "/tmp/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });


                //session.MoveFile(path, "/usr/local/install-apache.sh");

                RunSSHCommandStream(new SshClient("192.168.0.214", "pi", "raspberry"), new List<string>() { "sudo /tmp/install-apache.sh" });

                //session.RemoveFile("/etc/apache2/sites-available/000-default.conf");

                //session.PutFileToDirectory(pathSiteConf, "/etc/apache2/sites-available/", false, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, FilePermissions = permissions, TransferMode = TransferMode.Automatic });
                //session.PutFileToDirectory(pathApacheConf, "/etc/apache2/");//, true, new TransferOptions() { OverwriteMode = OverwriteMode.Overwrite, TransferMode = TransferMode.Automatic });

                RunSSHCommandStream(new SshClient("192.168.0.214", "pi", "raspberry"), new List<string>() { "sudo /tmp/install-apache-cont.sh" });


                //ExecuteSSHCommands(destIP, new string[] { killEmuStation });
                //SynchronizeServerToClientDirectories(destIP, session, $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\home\pi\RetroPie\roms\arcade\", "/home/pi/RetroPie/roms/arcade/", true);
                //SynchronizeServerToClientDirectories(destIP, session, $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\configs\arcade\", "/opt/retropie/configs/arcade/", true);
                //SyncArtwork(destIP);
            }


            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            return;


            foreach (var ip in v3Clients)
            {
                RebootComputerOverSSH(ip);// "192.168.1.149");
            }

            //if (true) GetArtwork("192.168.1.117");

            // replaced with sync above
            //if (true) FetchConfig("192.168.1.117");



            //foreach (var ip in v3Clients) {
            //    ProcessBlacklist(ip, v3Clients);
            //}

            //foreach (var ip in v3Clients)
            //{
            //    GetArtwork(ip);
            //}


            //ProcessDATFile();
            //NewDatFromNeoGeoGames();

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
            //GetArtwork("192.168.1.149");
            //PutArtwork(remoteIPClient);

            //RunSSHCommands(new SshClient("192.168.1.148", "pi", "raspberry"), new string[] { killEmuStation,  command });

            //fetchArtwork = true;
            //if (fetchArtwork) GetArtwork("192.168.1.117");
            //if (fetchConfig) FetchConfig();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        protected static void CleanGamelistNodes()
        {
            string path = Path.Combine(@"D:\RetroPie\RetroPieSync\127.0.0.1\gamelists\arcade\", $@"gamelist.xml");

            XmlDocument xmlDoc = new XmlDocument();

            if (!string.IsNullOrEmpty(path))
            {
                xmlDoc.Load(path);
            }

            var allGameNodes = xmlDoc.SelectNodes("//gameList/game/desc");
            int ct = allGameNodes.Count;

            for (int i = 0; i < ct; i++)
            {
                var node = allGameNodes.Item(i);
                node.InnerText = "";
            }

            var xml = xmlDoc.OuterXml.ToString();
            xmlDoc.Save(path);

        }

        protected static void RunRomTests()
        {
            string remoteIPClient = "192.168.1.149";
            var v3Clients = new string[] { "192.168.1.149", "192.168.1.117" };


            var romPath = "";
            //mame-mame4all

            List<string> testRomList = new List<string>();

            using (var session = new WinSCP.Session())
            {
                //session.Timeout = TimeSpan.FromSeconds(10);
                session.Open(new SessionOptions
                {
                    GiveUpSecurityAndAcceptAnySshHostKey = true,
                    Protocol = Protocol.Sftp,
                    HostName = remoteIPClient,
                    UserName = "pi",
                    Password = "raspberry"
                });

                var remoteFileList = session.EnumerateRemoteFiles("/home/pi/RetroPie/roms/arcade/mame2003/working/", "*.zip", EnumerationOptions.AllDirectories);

                testRomList = remoteFileList.Select(file => file.FullName).ToList();

                foreach (var rom in testRomList)
                {
                    //var command = $"subprocess.Popen('/opt/retropie/supplementary/runcommand/runcommand.sh', '0', '_SYS_', 'arcade' '{rom}')";
                    var command = $"/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2003/mame2003_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg {rom} --appendconfig /opt/retropie/configs/all/retroarch.cfg | tee stdout"; // | awk {{print $2}}  // | tee stdout
                    //var command = $"'/opt/retropie/supplementary/runcommand/runcommand.sh' 0 SYS arcade '{rom}'";
                    //ExecuteSSHCommands(remoteIPClient, new string[] { killEmuStation, killMame });
                    //RunSSHCommand(new SshClient(remoteIPClient, "pi", "raspberry"), command);//, //);
                    ExecuteSSHCommands(remoteIPClient, new string[] { killEmuStation, killMame, command });

                    //RunSSHCommands(new SshClient("192.168.1.149", "pi", "raspberry"), new string[] { killEmuStation, killMame, command });
                }
                //Console.WriteLine($"Attempting to run {romPath}");



            }
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

        public static string SendCommand(string cmd, ShellStream sh)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(sh);
                StreamWriter writer = new StreamWriter(sh);
                writer.AutoFlush = true;
                writer.WriteLine(cmd);
                while (sh.Length == 0)
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: " + ex.ToString());
            }
            return reader.ReadToEnd();
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

        protected static void RebootComputerOverSSH(string ipAddress)
        {
            ExecuteSSHCommands(ipAddress, new string[] { reboot });
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
                //RunSSHCommands(new SshClient("192.168.1.148", "pi", "raspberry"), new string[] { killEmuStation,  command });
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

        protected static void SynchronizeClientToServerDirectories(string ipAddressClient, WinSCP.Session session, string localPath, string remotePath, bool removeFiles = false)
        {

            try
            {
                //using (var session = new WinSCP.Session())
                //{
                if (!session.Opened)
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipAddressClient,//"192.168.1.117",//"192.168.1.149",
                        UserName = "pi",
                        Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                    });
                }

                // Download files
                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;
                transferOptions.AddRawSettings("FtpListAll", "0");

                SynchronizationResult transferResult = null;
               
                //string remotePath = $"/home/pi/.emulationstation/{folder}/*";
                //string localPath = $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\{folder}\";

                transferResult = session.SynchronizeDirectories(SynchronizationMode.Local, localPath, remotePath, removeFiles, false, SynchronizationCriteria.Either);

                // Throw on any error
                transferResult.Check();

                // Print results
                foreach (TransferEventArgs transfer in transferResult.Downloads)
                {
                    Console.WriteLine("Synchronize of {0} succeeded", transfer.FileName);
                }

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

        protected static void SynchronizeServerToClientDirectories(string ipAddressClient, WinSCP.Session session, string localPath, string remotePath, bool removeFiles = false)
        {

            try
            {
                //using (var session = new WinSCP.Session())
                //{
                if (!session.Opened)
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipAddressClient,//"192.168.1.117",//"192.168.1.149",
                        UserName = "pi",
                        Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                    });
                }

                // Download files
                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;
                transferOptions.AddRawSettings("FtpListAll", "0");

                SynchronizationResult transferResult = null;

                //string remotePath = $"/home/pi/.emulationstation/{folder}/*";
                //string localPath = $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\{folder}\";

                transferResult = session.SynchronizeDirectories(SynchronizationMode.Remote, localPath, remotePath, removeFiles, false, SynchronizationCriteria.Either);

                // Throw on any error
                transferResult.Check();

                // Print results
                foreach (TransferEventArgs transfer in transferResult.Downloads)
                {
                    Console.WriteLine("Synchronize of {0} succeeded", transfer.FileName);
                }

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

                foreach (RemoteFileInfo di in directories)
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
            ExecuteSSHCommands(ipClient, new string[] { killEmuStation, killMame, command }); //killEmuStation, killMame,  
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
                }
                else
                {
                    var output = command.Result;
                }

            }
        }

        protected static bool IsScriptRunning = false;

        protected static void RunSSHCommandStream(SshClient vclient, List<string> commands)
        {
            using (vclient)// var vclient = new SshClient("host", "username", "password"))
            {
                vclient.Connect();

                IsScriptRunning = true;

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                using (ShellStream shell = vclient.CreateShellStream("dumb", 80, 24, 800, 600, 1024, terminalMode))
                {
                    shell.DataReceived += Shell_DataReceived;
                    shell.ErrorOccurred += Shell_ErrorOccurred;
                    
                    commands.ForEach(command => 
                    {
                        if (command == "exit")
                        {
                            shell.Close();
                            return;
                        }

                        Console.WriteLine($"");
                        Console.WriteLine($"---- SendCommand:{command}");
                        Console.WriteLine($"");

                        var result = SendCommand(command, shell);
                        //Console.WriteLine(result);
                    });

                    string line;
                    //while ((line = shell.ReadLine(TimeSpan.FromMinutes(30))) != null)
                    //{
                    //    Console.WriteLine(line);
                    //    shell.Flush();

                    //    // if a termination pattern is known, check it here and break to exit immediately
                    //}

                    //while (vclient.IsConnected) { shell.Flush(); }
                    while (IsScriptRunning) { shell.Flush(); }
                    
                    //Console.WriteLine(SendCommand("comand 1", shell));

                    shell.Close();
                }
                vclient.Disconnect();
            }
        }

        private static void Shell_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            Console.WriteLine("Shell_ErrorOccurred >>" + e.Exception.Message);
        }

        private static void Shell_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
        {
            var text = ASCIIEncoding.ASCII.GetString(e.Data);
            if (text != null && text.Length > 0)
            {
                if (text.Contains("\r            \r"))
                {
                    var test = true;
                }
                Console.WriteLine(text);
                if (text.Contains("exit"))
                {
                    IsScriptRunning = false;
                    //var client = sender as ShellStream;
                    //client.Close();
                }
            }
        }

        protected static void RunSSHCommands(SshClient client, string[] commands)
        {
            using (client)// var client = new SshClient("192.168.1.149", "pi", "raspberry"))
            {
                client.ErrorOccurred += Client_ErrorOccurred;
                //client.HostKeyReceived += Client_HostKeyReceived;
                client.Connect();

                foreach (var cmd in commands)
                {
                    //Task.Run(() => {

                    var command = client.CreateCommand(cmd);// client.RunCommand(cmd);//.Execute();
                    var result = command.Execute();

                    Console.Write(result);

                    var reader = new StreamReader(command.ExtendedOutputStream);
                    Console.WriteLine("DEBUG:");
                    Console.Write(reader.ReadToEnd());

                    var cmdResult = command.Result;

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

                    //});
                }
                client.Disconnect();
            }
        }

        private static void Client_HostKeyReceived(object sender, Renci.SshNet.Common.HostKeyEventArgs e)
        {
            Console.WriteLine($"HostKeyReceived: {e.HostKeyName} id:{e.FingerPrint}");
        }

        private static void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            Console.WriteLine($"error: {e.Exception.Message}");
        }

        protected static void ExecuteSSHCommands(string ipClient, string[] commands)
        {
            using (var session = new WinSCP.Session())
            {
                session.Timeout = TimeSpan.FromSeconds(5);
                //session.AddRawConfiguration("ExitCode1IsError", "1");

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
                            if (result.Output != null && result.Output.Contains("not supported"))
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

        protected static void FetchConfig(string ipAddressClient)
        {
            ///opt/retropie/configs/*.cfg
            try
            {
                using (var session = new WinSCP.Session())
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipAddressClient,//"192.168.1.117",//"192.168.1.149",
                        UserName = "pi",
                        Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                    });

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    TransferOperationResult transferResult = null;

                    if (isPushToHost) //Push
                    {
                        string remotePath = $"/opt/retropie/configs/";
                        string localPath = $@"D:\RetroPie\RetroPieSync\{ipAddressClient}\configs\*";

                        Directory.CreateDirectory(localPath.Replace("*", ""));

                        transferResult = session.PutFiles(localPath, remotePath, false, transferOptions);

                    }
                    else //Pull Sync
                    {
                        string remotePath = $"/opt/retropie/configs/*";
                        string localPath = $@"D:\RetroPie\RetroPieSync\{ipAddressClient}\configs\";

                        Directory.CreateDirectory(localPath.Replace("*", ""));

                        transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);
                    }

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);

            }
        }

        protected static void PutArtwork(string ipAddressClient)
        {
            try
            {
                List<string> artFolders = new List<string>() { "downloaded_images", "gamelists" };

                using (var session = new WinSCP.Session())
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipAddressClient,//"192.168.1.117",//"192.168.1.149",
                        UserName = "pi",
                        Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                    });

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    foreach (string folder in artFolders)
                    {
                        TransferOperationResult transferResult = null;

                        string remotePath = $"/home/pi/.emulationstation/{folder}/";
                        string localPath = $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\{folder}\*";

                        transferResult = session.PutFiles(localPath, remotePath, false, transferOptions);

                        // Throw on any error
                        transferResult.Check();

                        // Print results
                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                        {
                            Console.WriteLine("Transfer of {0} succeeded", transfer.FileName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);

            }
        }

        protected static void SyncArtwork(string ipAddressClient)
        {
            try
            {
                List<string> artFolders = new List<string>() { "downloaded_images", "gamelists" };

                using (var session = new WinSCP.Session())
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipAddressClient,//"192.168.1.117",//"192.168.1.149",
                        UserName = "pi",
                        Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                    });

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    foreach (string folder in artFolders)
                    {
                        SynchronizeClientToServerDirectories(ipAddressClient, session, $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\{folder}\", $"/home/pi/.emulationstation/{folder}/");
                        //TransferOperationResult transferResult = null;

                        //string remotePath = $"/home/pi/.emulationstation/{folder}/*";
                        //string localPath = $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\{folder}\";

                        //transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);

                        //// Throw on any error
                        //transferResult.Check();

                        //// Print results
                        //foreach (TransferEventArgs transfer in transferResult.Transfers)
                        //{
                        //    Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);

            }
        }

        protected static void GetBlacklistFile(string ipAddressClient)
        {
            try
            {
                using (var session = new WinSCP.Session())
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipAddressClient,//"192.168.1.117",//"192.168.1.149",
                        UserName = "pi",
                        Password = "raspberry"// ,SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                    });

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.AddRawSettings("FtpListAll", "0");

                    TransferOperationResult transferResult = null;

                    string remotePath = $"/home/pi/blacklist.txt";
                    string localPath = $@"D:\RetroPie\RetroPieSync\{ipAddressClient}\home\pi\blacklist.txt";

                    if (!Directory.Exists($@"D:\RetroPie\RetroPieSync\{ipAddressClient}\home\pi\")) Directory.CreateDirectory($@"D:\RetroPie\RetroPieSync\{ipAddressClient}\home\pi\");

                    //if (!File.Exists(localPath)) File.Create(localPath);

                    transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                    }

                }
            }
            catch (SessionRemoteException sre)
            {
                Console.WriteLine("SessionRemoteException: {0}", sre);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        protected static void ProcessBlacklist(string ipAddressClient)
        {
            GetBlacklistFile(ipAddressClient);
            string localPath = $@"D:\RetroPie\RetroPieSync\{ipAddressClient}\home\pi\blacklist.txt";
            if (!File.Exists(localPath))
            {
                Console.WriteLine($"Not found for processing: {localPath}");
                return;
            }

            var blacklistedUrls = File.ReadLines(localPath).Distinct().ToList();

            using (var session = new WinSCP.Session())
            {
                foreach (var filePath in blacklistedUrls)
                {
                    RemoveFile(ipAddressClient, session, filePath);
                }

                CacheBlacklistFile(ipAddressClient);
                string remotePath = $"/home/pi/blacklist.txt";
                RemoveFile(ipAddressClient, session, remotePath);
            }

            File.Delete(localPath);

        }

        protected static void CacheBlacklistFile(string ipAddressClient)
        {
            string localPath = $@"D:\RetroPie\RetroPieSync\{ipAddressClient}\home\pi\blacklist.txt";
            string localCachedPath = $@"D:\RetroPie\RetroPieSync\{cachingServerHost}\home\pi\blacklist.txt";

            if (!Directory.Exists($@"D:\RetroPie\RetroPieSync\{cachingServerHost}\home\pi\"))
                Directory.CreateDirectory($@"D:\RetroPie\RetroPieSync\{cachingServerHost}\home\pi\");

            if (!File.Exists(localCachedPath)) File.Create(localCachedPath);

            var blacklistUrls = File.ReadLines(localPath).Distinct().ToList();
            var cachedBlacklistUrls = File.ReadLines(localCachedPath).Distinct().ToList();

            foreach (var url in blacklistUrls)
            {
                if (!cachedBlacklistUrls.Contains(url))
                    cachedBlacklistUrls.Add(url);
            }

            File.WriteAllLines(localCachedPath, cachedBlacklistUrls);

        }


        protected static void RemoveFile(string ipAddressClient, WinSCP.Session session, string remotePath)
        {
            try
            {
                //using (var session = new WinSCP.Session())
                //{
                if (!session.Opened)
                {
                    session.FileTransferred += FileTransferred;
                    session.Failed += Session_Failed;
                    // Connect
                    session.Open(new SessionOptions
                    {
                        GiveUpSecurityAndAcceptAnySshHostKey = true,
                        Protocol = Protocol.Sftp,
                        HostName = ipAddressClient,
                        UserName = "pi",
                        Password = "raspberry"
                    });
                }

                // Download files
                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;
                transferOptions.AddRawSettings("FtpListAll", "0");

                RemovalEventArgs transferResult = null;

                if (session.FileExists(remotePath))
                {
                    transferResult = session.RemoveFile(remotePath);//.GetFiles(remotePath, localPath, false, transferOptions);

                    // Print results
                    if (transferResult.Error != null)
                    {
                        Console.WriteLine("Error removing {0} from {1} error:{2}", transferResult.FileName, ipAddressClient, transferResult.Error);
                    }
                    else
                    {
                        Console.WriteLine("Success removing {0} from {1}", transferResult.FileName, ipAddressClient);
                    }
                }
                else
                {
                    Console.WriteLine($"Couldnt find file {remotePath} to remove on {ipAddressClient}");
                }


                //}
            }
            catch (Exception e)
            {
                Console.WriteLine("Error removing {0} error:{1}", remotePath, e.Message);
                //Console.WriteLine("Error: {0}", e);

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
                            Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                        }
                    }

                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);

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
                Console.WriteLine("Upload of {0} succeeded", e.FileName);
            }
            else
            {
                Console.WriteLine("Upload of {0} failed: {1}", e.FileName, e.Error);
            }

            if (e.Chmod != null)
            {
                if (e.Chmod.Error == null)
                {
                    Console.WriteLine(
                        "Permissions of {0} set to {1}", e.Chmod.FileName, e.Chmod.FilePermissions);
                }
                else
                {
                    Console.WriteLine(
                        "Setting permissions of {0} failed: {1}", e.Chmod.FileName, e.Chmod.Error);
                }
            }
            else
            {
                Console.WriteLine("Permissions of {0} kept with their defaults", e.Destination);
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
                Console.WriteLine(
                    "Timestamp of {0} kept with its default (current time)", e.Destination);
            }
        }


        /**********************************************************************/
        /*********************** DAT File Processing **************************/
        /**********************************************************************/
        #region Region - DAT Files
        public static XmlDocument LoadDatFile(string datFile)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatFiles", $@"{datFile}");

            XmlDocument xmlDoc = new XmlDocument();

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Path missing");
                return null;
            }
            xmlDoc.Load(path);
            return xmlDoc;
        }

        public static string NewDatFromNeoGeoGames()
        {
            // sourcefile="neogeo.c" && !cloneof
            var datFile = "mame2003.xml";
            var xmlDoc = LoadDatFile(datFile);
            var xmlRootNodeNode = xmlDoc.ChildNodes[1].Name;

            var gameNeoGeoNodes = xmlDoc.SelectNodes($"//{xmlRootNodeNode}/game[not(@cloneof) and @sourcefile = 'neogeo.c']");//, nsmgr);
            var mameNode = xmlDoc.SelectSingleNode($"//{xmlRootNodeNode}");

            int ct = gameNeoGeoNodes.Count;

            Console.WriteLine($"Found {ct} NeoGeo Games");

            mameNode.RemoveAll();

            for (int i = 0; i < ct; i++)
            {
                var node = gameNeoGeoNodes.Item(i);
                mameNode.AppendChild(node);
            }

            int finalCt = xmlDoc.SelectNodes($"//{xmlRootNodeNode}/game").Count;

            Console.WriteLine($"Final XML has {ct} NeoGeo Games");

            var xml = xmlDoc.OuterXml.ToString();
            xmlDoc.Save($@"c:\temp\{datFile}.neoonly");
            //var finalDoc = new XmlDocument();
            //XDocument doc = XDocument.Parse(xml);

            //finalDoc.LoadXml(doc.ToString());

            return "";
        }

        public static string ProcessDATFile(string remoteIPClient = "192.168.1.117")
        {
            try
            {
                //string remoteIPClient = "192.168.1.117";

                XmlDocument xmlDoc = LoadDatFile("mame2003-lr-no-clones-no-neogeo.xml");

                var xmlRootNodeNode = "datafile";// mame

                var allGameNodes = xmlDoc.SelectNodes($"//{xmlRootNodeNode}/game");
                var gameNodes = xmlDoc.SelectNodes($"//{xmlRootNodeNode}/game[not(@cloneof)]");//, nsmgr);

                var mameNode = xmlDoc.SelectSingleNode($"//{xmlRootNodeNode}");
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

                var xml = xmlDoc.OuterXml.ToString();
                var finalDoc = new XmlDocument();
                XDocument doc = XDocument.Parse(xml);

                finalDoc.LoadXml(doc.ToString());
                var allGameNodesTest = finalDoc.SelectNodes($"//{xmlRootNodeNode}/game");
                int ctt = allGameNodesTest.Count;

                using (var session = new WinSCP.Session())
                {
                    foreach (var folder in distinctFolderList)
                    {
                        var folderNodes = xmlDoc.SelectNodes($"//{xmlRootNodeNode}/game/category[starts-with(text(),'{folder}')]");

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
            catch (Exception exc)
            {
                Console.WriteLine($"Exception: {exc}");
            }
            return "";

        }
        #endregion

        /**********************************************************************/
        /************** Gamelist.xml ******************************************/

        public static void BuildGamelistXml()
        {
            string path = Path.Combine($@"D:\RetroPie\RetroPieSync\{cachingServerHost}\gamelists\arcade\gamelist.xml");

            Dictionary<string, XmlNode> romXmlData = new Dictionary<string, XmlNode>();

            XmlDocument xmlDoc = new XmlDocument();

            if (!File.Exists(path))
            {
                Console.WriteLine("Path missing");
                return;
            }
            xmlDoc.Load(path);

            var allGameNodes = xmlDoc.SelectNodes($"//gameList/game");

            int ct = allGameNodes.Count;

            Console.WriteLine($"{ct} games found.");

            for (int i = 0; i < ct; i++)
            {
                var item = allGameNodes.Item(i);
                var node = item.SelectSingleNode("path");//.Attributes["name"].Value;
                var success = Uri.TryCreate(node.InnerText, UriKind.Relative, out Uri result);
                var fileName = Path.GetFileName(node.InnerText);

                if (!string.IsNullOrEmpty(fileName))
                {
                    if (node.InnerText.ToLower().Contains("working"))
                    {
                        if (romXmlData.ContainsKey(fileName))
                        {
                            var existingNode = romXmlData[fileName];
                        }
                        else
                        {
                            romXmlData.Add(fileName, node);
                        }
                       
                    }
                    else
                    {
                        if (node.InnerText.ToLower().Contains("working"))
                        {
                            var test = true;
                        }
                        Console.WriteLine($"Excluding {node.InnerText}");
                        xmlDoc.DocumentElement.RemoveChild(item);
                    }

                }
            }

            //processedXmlDoc.AppendChild(xmlDoc.ImportNode(xmlDoc.DocumentElement, false));

            //foreach (var key in romXmlData.Keys)
            //{
            //    processedXmlDoc.FirstChild.AppendChild(romXmlData[key]);
            //}

            xmlDoc.Save($@"D:\RetroPie\RetroPieSync\{cachingServerHost}\gamelists\arcade\gamelist-{DateTime.Now.Millisecond}.xml");
            
        }
    }

}