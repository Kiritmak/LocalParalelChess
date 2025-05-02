namespace ParalelLocalChess
{
  static class Program
  {
    static string filepath = @"E:\Code\C#\LocalParalelChess\TextFiles\ColorData.txt";
    static int CurrentPlayers = 0;
    static object PlayerEnteringSala = new object();
    static string[] players = new string[2];

    private static Semaphore SalaDeEspera = new(2, 2, "SalaDeEspera");
    private static Mutex game = new(false, "ChessGame");
    private static Mutex ColorSelection = new(false, "Color");
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
        Println(playerName, "Esta esperando para entrar a la sala...");
        SalaDeEspera.WaitOne();
        Console.WriteLine($"{playerName} Ha entrado a la sala");
        Println(playerName, "Esta esperando para pedir un color...");
        ColorSelection.WaitOne();
        Println(playerName, "Esta eligiendo un color");
        Console.ReadLine();
        List<string> text = File.ReadAllLines(filepath).ToList();
        string[] Blancas = text[0].Split(",");
        if (Blancas.Length==1)
        {
          Blancas = ["Blancas", playerName] ;
          text[0] = $"Blancas, {playerName}";
          File.WriteAllLines(filepath, text);
        }
        else
        {
          text[1] = $"Negras, {playerName}";
          File.WriteAllLines(filepath, text);
        }
        ColorSelection.ReleaseMutex();
        Play(Blancas[1] == playerName);
        text[0] = "Blancas";
        text[1] = "Negras";
        File.WriteAllLines (filepath, text);
        Console.WriteLine($"{playerName} Ha salido de la sala");
        SalaDeEspera.Release();
      }
    }

    static void Play(bool blancas)
    {
      string? playerName = Thread.CurrentThread.Name;
      string color = blancas ? "blancas" : "negras";
      Console.WriteLine($"{playerName} esta jugando con las {color}");
      Console.ReadLine();
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
