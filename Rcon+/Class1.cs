using System;
using UnityEngine;
using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Plugins;
using Rocket.Unturned;
using Rocket.Core.Plugins;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SDG.Unturned;
using System.Xml.Serialization;
using Rocket.Core.Logging;

namespace Rcon
{
    public class RconConfiguration : IRocketPluginConfiguration
    {
        [XmlArrayItem(ElementName = "Rcon")] 
        public int Port;
        public String IP;
        public String password;
        

        public void LoadDefaults()
        {
            Port = 27017;
            IP = "0.0.0.0";
            password = "edit me";
        }
    }

    public class Rcon : RocketPlugin<RconConfiguration>
    {
        public static Rcon instance = null; 
        int last = DateTime.Now.Second;
        static Dictionary<String, String> config = new Dictionary<string,string>();
        static TcpListener server;
        protected override void Load()
        {
            instance = this;
            server = new TcpListener(IPAddress.Parse(this.Configuration.Instance.IP), this.Configuration.Instance.Port);
            server.Start();
            Logger.Log("TCP Linstening server started! online: " + server.Server.Connected.ToString() + ", port: " + this.Configuration.Instance.Port.ToString());
            Thread ac = new Thread(accept_client);
            ac.Start();
        }
        protected override void Unload()
        {
            base.Unload();
            server.Stop();
            server = null;
        }
        public static void accept_client()
        {
            while (true)
            {
                TcpClient client2 = server.AcceptTcpClient();
                Logger.Log(((IPEndPoint)client2.Client.LocalEndPoint).Address.ToString() + " connected to Rcon+ protocol");
                Thread thr = new Thread(() => reg_client(client2));
                thr.Start();
                StreamWriter SW = new StreamWriter(client2.GetStream());
                SW.WriteLine("Welcome to the Rcon+ Socket of this Unturned server! please use login <password>");
                SW.Flush();
               
            }
        }
        public static void reg_client(TcpClient client)
        {
            StreamReader SR = new StreamReader(client.GetStream());
            StreamWriter SW = new StreamWriter(client.GetStream());
            String ip = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
            Boolean loggedin = false;
            Logger.Log(ip+ " registered as client");
            while (true)
            {
                String received = SR.ReadLine();
                Logger.Log(ip + "> " + received);
                if (!received.Equals(""))
                {
                    if (received.StartsWith("login"))
                    {
                        String pass = received.Replace("login ", "");
                        loggedin = pass.Equals(instance.Configuration.Instance.password);
                        if (loggedin)
                        {
                            SW.WriteLine("You loggedin succesfully");
                            SW.Flush();
                        }
                    }else if (!loggedin)
                    {
                        SW.WriteLine("You need to login first!");
                        SW.Flush();
                    }
                    else
                    {
                        CommandWindow.ConsoleInput.onInputText.Invoke(received);
                    }
                }
            }
        }
        public void FixedUpdate()
        {
        }
    }
}