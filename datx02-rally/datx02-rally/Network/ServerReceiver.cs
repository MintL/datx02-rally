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

        public void ReceiveMessages(IList<Player> Players)
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
                        break;
                }
            }
        }

        private void ParseDataPackage(IList<Player> PlayerList, NetIncomingMessage msg) 
        {
            MessageType type = (MessageType)msg.ReadByte();
            switch (type)
            {
                case MessageType.PlayerPos:
                    
                    break;
                case MessageType.Chat:
                    break;
                case MessageType.Debug:
                    break;
                case MessageType.LobbyUpdate:
                    break;
                default:
                    break;
            }
        }
    }
}
