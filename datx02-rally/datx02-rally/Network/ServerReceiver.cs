﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using datx02_rally.Components;

namespace datx02_rally
{
    class ServerReceiver
    {
        NetClient ServerThread;
        ServerClient ServerHandler;
        bool DEBUG_MODE = true;

        public ServerReceiver(NetClient serverThread, ServerClient handler)
        {
            this.ServerThread = serverThread;
            this.ServerHandler = handler;
        }

        public void ReceiveMessages()
        {
            List<NetIncomingMessage> messages = new List<NetIncomingMessage>();
            ServerThread.ReadMessages(messages);
            foreach (var message in messages)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        ParseDataPackage(message);
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Console.WriteLine("WARNING message: " + message.ReadString());
                        break;
                    default:
                        Console.WriteLine("Received unknown network message: " + message.MessageType);
                        break;
                }
            }
        }

        private Boolean IsPlayerMessage(MessageType type)
        {
            return type != MessageType.LobbyUpdate &&
                type != MessageType.Countdown &&
                type != MessageType.OK &&
                type != MessageType.Debug;
        }

        private void ParseDataPackage(NetIncomingMessage msg) 
        {
            MessageType type = (MessageType)msg.ReadByte();
            Dictionary<byte, Player> PlayerList = ServerHandler.Players;
            Player player = null;
            if (IsPlayerMessage(type) && !PlayerList.TryGetValue(msg.ReadByte(), out player))
            {
                //Console.WriteLine("Received message from unknown player, discarding...");
                return;
            }
            switch (type)
            {
                case MessageType.PlayerPos:
                    byte sequenceNo = msg.ReadByte();
                    float x = msg.ReadFloat(); float y = msg.ReadFloat(); float z = msg.ReadFloat();
                    float rotation = msg.ReadFloat(); float velocity = msg.ReadFloat();
                    DateTime sentTime = new DateTime(msg.ReadInt64());
                    player.SetPosition(x, y, z, rotation, velocity, sequenceNo, sentTime);
                    break;
                case MessageType.Chat:
                    string chatMsg = msg.ReadString();
                    Console.WriteLine("CHAT {0}: {1}", player.PlayerName, chatMsg);
                    var chatTuple = new Tuple<string, string, DateTime>(player.PlayerName, chatMsg, DateTime.Now);
                    ServerHandler.ChatHistory.AddLast(chatTuple);
                    break;
                case MessageType.Debug:
                    break;
                case MessageType.LobbyUpdate:
                    if (DEBUG_MODE) Console.Write("Received LobbyUpdate message: ");
                    Dictionary<byte, Player> newPlayerList = new Dictionary<byte, Player>();

                    byte playerCount = msg.ReadByte();
                    for (int i = 0; i < playerCount; i++)
                    {
                        byte discoveredPlayerId = msg.ReadByte();
                        string discoveredPlayerName = msg.ReadString();
                        bool discoveredPlayerReady = msg.ReadBoolean();
                        Vector3 discoveredPlayerColor = new Color(msg.ReadByte(), msg.ReadByte(), msg.ReadByte()).ToVector3();

                        if (DEBUG_MODE) Console.Write(discoveredPlayerName + ", ");

                        if (ServerHandler.LocalPlayer.ID != discoveredPlayerId) // ignore info of local player
                        {
                            Player discoveredPlayer;
                            if (!PlayerList.TryGetValue(discoveredPlayerId, out discoveredPlayer))
                            {
                                ServerHandler.Game.GetService<HUDConsoleComponent>().WriteOutput("New remote player "+discoveredPlayerName+" discovered!");
                                var newPlayer = new Player(GameManager.GetInstance(), discoveredPlayerId, discoveredPlayerName);
                                newPlayer.State = discoveredPlayerReady ? PlayerState.Game : PlayerState.Lobby;
                                newPlayer.CarColor = discoveredPlayerColor;
                                newPlayerList[discoveredPlayerId] = newPlayer;
                            }
                            else
                            {
                                discoveredPlayer.PlayerName = discoveredPlayerName; // maybe has changed name
                                discoveredPlayer.CarColor = discoveredPlayerColor; // or color
                                discoveredPlayer.State = discoveredPlayerReady ? PlayerState.Game : PlayerState.Lobby;
                                newPlayerList[discoveredPlayerId] = discoveredPlayer;
                                PlayerList.Remove(discoveredPlayerId);
                            }
                        }
                        ServerHandler.Players = newPlayerList;
                    }
                    // Remaining players in PlayerList are no longer connected to server.
                    foreach (var disconnectedPlayer in PlayerList.Values)
                    {
                        ServerHandler.Game.GetService<HUDConsoleComponent>().WriteOutput("Player "+disconnectedPlayer.PlayerName+" disconnected!");
                        var ccc = ServerHandler.Game.GetService<CarControlComponent>();
                        if (ccc != null) ccc.RemoveCar(disconnectedPlayer);
                    }
                    if (DEBUG_MODE) Console.WriteLine();
                    break;
                case MessageType.OK:
                    byte assignedID = msg.ReadByte();
                    int gameSeed = msg.ReadInt32();
                    if (DEBUG_MODE) Console.WriteLine("Received OK handshake from server with ID: "+assignedID+", seed: "+gameSeed);
                    ServerHandler.LocalPlayer.ID = assignedID;
                    ServerHandler.Seed = gameSeed;
                    ServerHandler.connected = true;
                    ServerHandler.Game.GetService<HUDConsoleComponent>().WriteOutput("Connected! (id "+assignedID+")");
                    break;
                case MessageType.Countdown:
                    byte countdown = msg.ReadByte();
                    if (DEBUG_MODE) Console.WriteLine("Received Countdown from server: " + countdown);
                    string[] countdownStr = { "3", "2", "1", "Go!" };
                    ServerHandler.Game.GetService<HUDComponent>().ShowTextNotification(Color.AliceBlue, countdownStr[countdown], TimeSpan.FromSeconds(0.8));
                    ServerHandler.GamePlay.mode.CountDownState = countdown;
                    if (countdown == 3)
                        ServerHandler.GamePlay.mode.GameStarted = true;
                    break;
                case MessageType.RaceTime:
                    player.RaceTime = new TimeSpan(msg.ReadInt64());
                    break;
                default:
                    break;
            }
        }
    }
}
