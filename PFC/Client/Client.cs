using System;
using System.Linq;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.UDP;
using Newtonsoft.Json;

namespace Client
{
    public class Client
    {
        private int Id { get; set; }
        private Connection ClientConnection;

        public Client()
        {

            // Set connection to server
            Console.WriteLine("Please enter the server IP:port");
            var serverInfo = Console.ReadLine();
            var serverIp = serverInfo.Split(':').First();
            var serverPort = int.Parse(serverInfo.Split(':').Last());
            var connInfo = new ConnectionInfo(serverIp, serverPort);
            ClientConnection = UDPConnection.GetConnection(connInfo, UDPOptions.None);

            // Handlers
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("message", WriteMessage);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("clientId", SetId);

            // Send my name to server
            Console.WriteLine("Please enter your name:");
            var name = Console.ReadLine();
            ClientConnection.SendObject("Name", name);
        }

        public void Run()
        {
            while (true)
            {
                var command = Console.ReadLine();
                if (string.CompareOrdinal(command, "quit") == 0)
                {
                    ClientConnection.SendObject("disconnected", Id);
                    break;
                }
                if (string.CompareOrdinal(command, "Ready") == 0)
                    ClientConnection.SendObject("Ready", Id);

                if (string.CompareOrdinal(command, "stone") == 0)
                {
                    var data = JsonConvert.SerializeObject(new Shot(Id, PlayerAction.Stone));
                    ClientConnection.SendObject("Shot", data);
                }

                if (string.CompareOrdinal(command, "paper") == 0)
                {
                    var data = JsonConvert.SerializeObject(new Shot(Id, PlayerAction.Paper));
                    ClientConnection.SendObject("Shot", data);
                }

                if (string.CompareOrdinal(command, "scissors") == 0)
                {
                    var data = JsonConvert.SerializeObject(new Shot(Id, PlayerAction.Scissors));
                    ClientConnection.SendObject("Shot", data);
                }
            }
            NetworkComms.Shutdown();
        }

        private void SetId(PacketHeader header, Connection connection, int id)
        {
            Id = id;
        }

        private void WriteMessage(PacketHeader header, Connection connection, string message)
        {
            Console.WriteLine(message);
        }

    }
}