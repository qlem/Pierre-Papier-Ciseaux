using System;
using NetworkCommsDotNet.Connections;

namespace Server
{ 
    public class Player
    {
        private static int _count;
        public int Id { get; }
        public string Name { get; }
        public Connection ConnectInfo { get; }
        public PlayerAction Action { get; set; }
        public PlayerState State { get; set; }
        public bool AlreadyPlayed { get; set; }
        public Room Room { private get; set; }

        public Player(string name, Connection connectInfo)
        {
            Id = ++_count;
            Name = name;
            ConnectInfo = connectInfo;
            Action = PlayerAction.Undefined;
            State = PlayerState.Waiting;
            AlreadyPlayed = false;
        }

        public void Shot(PlayerAction action)
        {
            Action = action;
            AlreadyPlayed = true;
            ConnectInfo.SendObject("message", "You chose " + Action);
            Console.WriteLine("Player " + Name + " #" + Id + " chose " + action);
        }

        public void Disconnection()
        {
            Console.WriteLine("Player " + Name + " #" + Id + " disconnected");
            if (State == PlayerState.Playing)
                Room.DiconnectionClient(this);
        }
    }
}