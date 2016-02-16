﻿namespace Ballz.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Messages;
    using Microsoft.Xna.Framework;
    using System.Net.Sockets;

    class Server
    {
        private static int nextId = 1;
        TcpListener listener = null;
        private readonly Network network = null;
        private readonly List<Connection> connections = new List<Connection>();
        
        public Server(Network net)
        {
            network = net;
        }

        public void GameStarted()
        {
            Broadcast(new NetworkMessage(NetworkMessage.MessageType.NumberOfPlayers, this.GetNumberOfClients()));
            Broadcast(new NetworkMessage(NetworkMessage.MessageType.GameStarted));
        }

	    int GetNumberOfClients()
	    {
	        return connections.Count;
	    }

        public void Listen(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            // start lobby first
            network.GameState = Network.GameStateT.InLobby;
            UpdateLobbyList();
        }

        public void UpdateLobbyList()
        {
            Ballz.The().NetworkLobbyConnectedClients.Name = "Myself";
            foreach (var c in connections)
            {
                Ballz.The().NetworkLobbyConnectedClients.Name += ", " + c.Id;
            }
        }

        public void Update(GameTime time)
        {
            // new clients
            if (listener.Pending())
            {
                if (network.GameState == Network.GameStateT.InLobby)
                {
                    // add new player in lobby
                    var client = listener.AcceptTcpClient();
                    connections.Add(new Connection(client, nextId++));
                    network.RaiseMessageEvent(NetworkMessage.MessageType.NewClient);
                    // update lobby list
                    UpdateLobbyList();
                }
                else
                {
                    //TODO: Re-add Players that lost connection
                }
            }
            // receive data
            foreach (var c in connections)
            {
                if (c.DataAvailable())
                {
                    var data = c.ReceiveData();
                    OnData(data, c.Id);
                }
            }

            // TEST
            {
                //Broadcast(Ballz.The().World.Entities);
            }
            //TODO: Implement
        }

        private void OnData(object data, int sender)
        {
            //Console.WriteLine("Received data from " + sender + ": " + data.ToString()); // Debug
            // Input Message
            if (data.GetType() == typeof(InputMessage))
            {
            }
        }

        public void Broadcast(object data)
        {
            foreach (var c in connections)
            {
                c.Send(data);
            }
        }

        public void HandleMessage(object sender, Message message)
        {
            //TODO: handle Messages
        }
    }
}
