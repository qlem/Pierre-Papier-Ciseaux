using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using Newtonsoft.Json;

namespace Server
{
    public class Server
    {
        private  List<Player> _players = new List<Player>();
        private  List<Room> _rooms = new List<Room>();

        public Server()
        {
            // Handlers
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Name", NewCientConnection);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("Ready", ReadyToPlay);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("disconnected", DisconnectionClient);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Shot", ShotClient);
            
            // Open connection
            Connection.StartListening(ConnectionType.UDP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));
            Console.WriteLine("Server listening for UDP connection on:");
            foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.UDP))
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
        }

        public void Run()
        {
            while (true)
            {
                if (_players.Count > 0)
                    CheckPlayers();
                if (_rooms.Count > 0)
                    CheckRooms();
                Thread.Sleep(1000);
            }
            
            // TODO : Shut down NetworkComms
            NetworkComms.Shutdown();
        }
        
        private  void NewCientConnection(PacketHeader header, Connection connection, string name)
        {
            _players.Add(new Player(name, connection));
            connection.SendObject("clientId", _players.Last().Id);
            connection.SendObject("message", "You are connected! Please type 'Ready' for start playing.");
            Console.WriteLine("Player " + _players.Last().Name + " #" + _players.Last().Id + " connected");
        }

        private void DisconnectionClient(PacketHeader header, Connection client, int id)
        {
            var discPlayer = _players.Find(player => player.Id == id);
            discPlayer.Disconnection();
            _players.Remove(discPlayer);
        }

        private  void ReadyToPlay(PacketHeader header, Connection client, int id)
        {   
            foreach (var player in _players)
            {
                if (player.Id != id) continue;
                player.State = PlayerState.Ready;
                break;
            }
        }

        private  void ShotClient(PacketHeader header, Connection connection, string data)
        {
            var shot = JsonConvert.DeserializeObject<Shot>(data);
            foreach (var player in _players)
            {
                if (player.Id == shot.Id && player.State == PlayerState.Playing)
                {
                    if (!player.AlreadyPlayed)
                        player.Shot(shot.Action);
                    else
                        player.ConnectInfo.SendObject("message", "You already chosen your action");
                    break;
                }
            }
        }
        
        private  void CheckPlayers()
        {
            var nb = _players.Count(player => player.State == PlayerState.Ready);

            if (nb < 2) return;
            {
                var player1 = _players.Find(player => player.State == PlayerState.Ready);
                player1.State = PlayerState.Playing;
                var player2 = _players.Find(player => player.State == PlayerState.Ready);
                player2.State = PlayerState.Playing;
                _rooms.Add(new Room(player1, player2));
            }
        }

        private void CheckRooms()
        {
            foreach (var room in _rooms)
            {
                if (room.State == RoomState.Run)
                    room.Game();
                if (room.State != RoomState.Finished) continue;
                _rooms.Remove(room);
                break;
            }
        }
    }
}