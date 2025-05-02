namespace ParalelLocalChess
{
  static class Program
  {
    static string ColorDataFilepath = @"E:\Code\C#\LocalParalelChess\TextFiles\ColorData.txt";
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
        List<string> text = File.ReadAllLines(ColorDataFilepath).ToList();
        string[] Blancas = text[0].Split(",");
        if (Blancas.Length==1)
        {
          Blancas = ["Blancas", playerName] ;
          text[0] = $"Blancas, {playerName}";
          File.WriteAllLines(ColorDataFilepath, text);
        }
        else
        {
          text[1] = $"Negras, {playerName}";
          File.WriteAllLines(ColorDataFilepath, text);
        }
        Println(playerName, "Ha elejido un color");
        ColorSelection.ReleaseMutex();
        Play(Blancas[1] == playerName);
        text[0] = "Blancas";
        text[1] = "Negras";
        File.WriteAllLines (ColorDataFilepath, text);
        Console.WriteLine($"{playerName} Ha salido de la sala");
        SalaDeEspera.Release();
      }
    }

    static void Play(bool blancas)
    {
      string? playerName = Thread.CurrentThread.Name;
      string color = blancas ? "blancas" : "negras";
      List<string> chessBoard = new List<string>();
      for(int i=1; i<=8; i++)
      {
        string? line = "";
        for(int j=1; j<=8; j++)
        {
          if ((i + j) % 2 == 0) line += " /// ";
          else line += "     ";
          line += "|";
        }
        chessBoard.Add(line);
        chessBoard.Add("--------------------------------------------------");
      }

      while(true) 
      {
        Println(playerName, "Esperando para elejir...");
        game.WaitOne();
        Println(playerName, "Es tu turno");
        int w = Win(blancas);
        if (w != -1) break;
        showChessBoard(chessBoard);
        Println(playerName, "Esta pensando... ");
        Console.ReadLine();
        Println(playerName, "Ha elejido un movimiento");
        game.ReleaseMutex();
        Thread.Sleep(2000);
      } 

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
    static int Win(bool blancas)
    {
      return -1;
    }
    static void showChessBoard(List<string> chessBoard)
    {
      foreach (string chess in chessBoard)
      {
        Console.WriteLine(chess);
      }
    }
  }

}
