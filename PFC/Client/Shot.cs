namespace Client
{
    public class Shot
    {
        public int Id { get; }
        public PlayerAction Action { get; }

        
        public Shot(int id, PlayerAction action)
        {
            Id = id;
            Action = action;
        }
    }
}