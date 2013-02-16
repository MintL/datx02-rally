using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace datx02_rally
{
    class ServerReceiver
    {
        NetClient ServerThread;
        ServerClient ServerHandler;

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

        private Boolean isPlayerMessage(MessageType type)
        {
            return MessageType.Debug > type;
        }

        private void ParseDataPackage(NetIncomingMessage msg) 
        {
            MessageType type = (MessageType)msg.ReadByte();
            Dictionary<byte, Player> PlayerList = ServerHandler.Players;
            Player player = null;
            if (isPlayerMessage(type) && !PlayerList.TryGetValue(msg.ReadByte(), out player))
            {
                Console.WriteLine("Received message from unknown player, discarding...");
                return;
            }
            switch (type)
            {
                case MessageType.PlayerPos:
                    player.SetPosition(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
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
                    byte playerCount = msg.ReadByte();
                    for (int i = 0; i < playerCount; i++)
                    {
                        byte discoveredPlayerId = msg.ReadByte();
                        string discoveredPlayerName = msg.ReadString();
                        Player discoveredPlayer;
                        if (!PlayerList.TryGetValue(discoveredPlayerId, out discoveredPlayer)) 
                        {
                            PlayerList[discoveredPlayerId] = new Player(Game1.GetInstance(), discoveredPlayerId, discoveredPlayerName);
                        }
                        else if (discoveredPlayer.PlayerName != discoveredPlayerName) //changed player name
                        {
                            discoveredPlayer.PlayerName = discoveredPlayerName;
                        }
                    }
                    break;
                case MessageType.OK:
                    Console.WriteLine("Received OK handshake from server");
                    ServerHandler.connected = true;
                    ServerHandler.Game.GetService<HUDConsoleComponent>().WriteOutput("Connected!");
                    break;
                default:
                    break;
            }
        }
    }
}
