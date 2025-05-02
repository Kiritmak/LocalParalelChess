namespace ParalelLocalChess
{
  static class Program
  {
    static int CurrentPlayers = 0;
    static object PlayerEnteringSala = new object();
    static string[] players = new string[2];

    private static Semaphore SalaDeEspera = new(2, 2, "SalaDeEspera");
    private static Mutex game = new(false, "ChessGame");
    static void Main(string[] args)
    {
      //args = new string[1] {"Kiritmak"};
      Thread MainThread = new(new ThreadStart(WelcomePlayer));
      MainThread.Name = args[0];
      MainThread.Start();
    }

    static void WelcomePlayer()
    {
      string? playerName = Thread.CurrentThread.Name;
      Console.WriteLine($"Welcome to Paralel Chess {playerName}!\nA game where you can play chess with your friend at separated windows in the same computer!\n");
      while (Exit())
      {
        Println(playerName, "Esta esperando ...");
        SalaDeEspera.WaitOne();
        Console.WriteLine($"{playerName} Ha entrado a la sala");
        Console.ReadLine();
        Console.WriteLine($"{playerName} Ha salido de la sala");
        SalaDeEspera.Release();
      }
    }

    static void Play(bool blancas)
    {

    }
    static bool Exit()
    {
      Console.WriteLine("Type 1 to enter to the game, type anything else to exit the game");
      while(true)
      {
        int option;
        try
        {
          option = int.Parse(Console.ReadLine());
        }
        catch
        {
          Console.WriteLine("Incorrect Format");
          continue;
        }
        return option == 1;
      }
    }
    static void Println(string? name, string message) =>
      Console.WriteLine($"{name} {message}");
  }

}
