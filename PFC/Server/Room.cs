using System;

namespace Server
{
    public class Room
    {
        private static int _count;
        private int Id { get; }
        private Player Player1 { get; }
        private Player Player2 { get; }
        public RoomState State { get; private set; }

        public Room(Player player1, Player player2)
        {
            Id = ++_count;
            Player1 = player1;
            Player2 = player2;
            State = RoomState.Run;
            Console.WriteLine("Room " + Id + " created with players " + Player1.Name + " #" + Player1.Id + " and " + Player2.Name + " #" +Player2.Id);
            Player1.ConnectInfo.SendObject("message", "You face " + Player2.Name);
            Player2.ConnectInfo.SendObject("message", "You face " + Player1.Name);
            Player1.ConnectInfo.SendObject("message", "Type 'stone' or 'paper' or 'scissors'");
            Player2.ConnectInfo.SendObject("message", "Type 'stone' or 'paper' or 'scissors'");
            Player1.Room = this;
            Player2.Room = this;
        }
        
        public void Game()
        {
            if (Player1.Action != PlayerAction.Undefined && Player2.Action != PlayerAction.Undefined)
            {
                if (Player1.Action == Player2.Action)
                {
                    Player1.ConnectInfo.SendObject("message", "Try again");
                    Player2.ConnectInfo.SendObject("message", "Try again");
                    
                }
                
                if (Player1.Action == PlayerAction.Stone && Player2.Action == PlayerAction.Scissors)
                    Round(Player1, Player2);
                
                if (Player1.Action == PlayerAction.Paper && Player2.Action == PlayerAction.Stone)
                    Round(Player1, Player2);
                
                if (Player1.Action == PlayerAction.Scissors && Player2.Action == PlayerAction.Paper)
                    Round(Player1, Player2);
                
                if (Player1.Action == PlayerAction.Scissors && Player2.Action == PlayerAction.Stone)
                    Round(Player2, Player1);
                
                if (Player1.Action == PlayerAction.Stone && Player2.Action == PlayerAction.Paper)
                    Round(Player2, Player1);
                
                if (Player1.Action == PlayerAction.Paper && Player2.Action == PlayerAction.Scissors)
                    Round(Player2, Player1);
                
                Player1.Action = PlayerAction.Undefined;
                Player2.Action = PlayerAction.Undefined;
                
                Player1.AlreadyPlayed = false;
                Player2.AlreadyPlayed = false;
            }
        }

        private void Round(Player winner, Player loser)
        {
            winner.ConnectInfo.SendObject("message", loser.Name + " chose " + loser.Action);
            loser.ConnectInfo.SendObject("message", winner.Name + " chose " + winner.Action);
            winner.ConnectInfo.SendObject("message", "You win!");
            loser.ConnectInfo.SendObject("message", "You lose!");
            winner.ConnectInfo.SendObject("message", "Type 'Ready' for play again");
            loser.ConnectInfo.SendObject("message", "Type 'Ready' for play again");
            winner.State = PlayerState.Waiting;
            loser.State = PlayerState.Waiting;
            State = RoomState.Finished;
            Console.WriteLine("Room " + Id + " is finished. " + winner.Name + " #" + winner.Id + " win, " +  loser.Name + " #" + loser.Id + " lose");
        }

        public void DiconnectionClient(Player discPlayer)
        {
            Console.WriteLine("Room " + Id + " is finished. " + discPlayer.Name + " #" + discPlayer.Id + " disconnected");
            if (Player1 != discPlayer)
            {
                Player1.ConnectInfo.SendObject("message", "Player " + Player2.Name + " disconnected. Type 'Ready' for play again");
                Player1.State = PlayerState.Waiting;
                Player1.Action = PlayerAction.Undefined;
                Player1.AlreadyPlayed = false;
            }
            else if (Player2 != discPlayer)
            {
                Player2.ConnectInfo.SendObject("message", "Player " + Player1.Name + " disconnected. Type 'Ready' for play again");
                Player2.State = PlayerState.Waiting;
                Player2.Action = PlayerAction.Undefined;
                Player2.AlreadyPlayed = false;
            }
            State = RoomState.Finished;
        }
    }
}