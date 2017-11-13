public class Player
{
    private static Player _current;

    public static Player Current
    {
        get
        {
            if (_current == null)
                _current = new Player();
            return _current;
        }
    }

    public int KillCount { get; set; }
}
