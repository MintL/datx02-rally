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
        NetClient Client;

        public ServerReceiver(NetClient Client)
        {
            this.Client = Client;
        }

        public void ReceiveMessages(Dictionary<byte, Player> Players)
        {
            List<NetIncomingMessage> messages = new List<NetIncomingMessage>();
            Client.ReadMessages(messages);
            foreach (var message in messages)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        ParseDataPackage(Players, message);
                        break;
                    default:
                        Console.WriteLine("Received unknown network message: " + message.MessageType);
                        Console.WriteLine(message.ReadString());
                        break;
                }
            }
        }

        private void ParseDataPackage(Dictionary<byte, Player> PlayerList, NetIncomingMessage msg) 
        {
            MessageType type = (MessageType)msg.ReadByte();
            Player player = null;
            if (type != MessageType.LobbyUpdate && !PlayerList.TryGetValue(msg.ReadByte(), out player))
            {
                Console.WriteLine("Received message from unknown discoveredPlayer, discarding...");
                return;
            }
            switch (type)
            {
                case MessageType.PlayerPos:
                    player.SetPosition(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
                    break;
                case MessageType.Chat:
                    Console.WriteLine("{0}: {1}", player.PlayerName, msg.ReadString());
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
                default:
                    break;
            }
        }
    }
}
