using System.Collections;
using System.Drawing;
using System.Text;

namespace ParalelLocalChess
{
  public class Position
  {
    public Position(string s)
    {
      if (string.IsNullOrEmpty(s) || s.Length != 2)
        throw new ArgumentException();
      s = s.ToUpper();
      TextColumn = s[0];
      TextRow = int.Parse(s.Substring(1));
      if(Row < 0 ||  Row > 8) throw new ArgumentException();
      if(TextColumn < 'A' || TextColumn > 'H' ) throw new ArgumentException();
    }
    public char TextColumn {  get; set; }
    public int TextRow { get; set; }
    public int Row { get => Row-1;  }
    public int Column { get => (int)(TextColumn - 'A'); }

    public char GetPieceAtPosition(ChessBoard chessBoard)
    {
      return chessBoard[Row, Column];
    }
    public void SetPieceAtPosition(char P, ChessBoard chessBoard)
    {
      chessBoard[Row, Column] = P;
    }
  }

  public class ChessBoard : IEnumerable<char[]>, IEnumerable
  {
    private List<char[]> chessBoard;
    public ChessBoard()
    {
      chessBoard = new List<char[]>();
      for(int i=0; i<8; i++)
      {
        char[] chars = new char[8];
        for (int j = 0; j < 8; j++)
        {
          chars[j] = (i + j) % 2 == 0 ? 'W' : 'B';
        }
        chessBoard.Add(chars);
      }
    }
    public char this[int row, int column]
    {
      get => chessBoard[row][column];
      set => chessBoard[row][column] = value;
    }

    public IEnumerator<char[]> GetEnumerator()
    {
      foreach (char[] chars in chessBoard) yield return chars;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public List<string> ToList()
    {
      List<string> list = new List<string>();
      foreach(char[] chars in chessBoard)
      {
        string s = "";
        foreach (char c in chars) s += c;
        list.Add(s);
      }
      return list;
    }
  }



  static class Program
  {
    static string ColorDataFilepath = @"E:\Code\C#\LocalParalelChess\TextFiles\ColorData.txt";
    static string chessBoardFilepath = @"E:\Code\C#\LocalParalelChess\TextFiles\ChessBoard.txt";
    static string? playerName;

    private static Semaphore SalaDeEspera = new(2, 2, "SalaDeEspera");
    private static Mutex game = new(false, "ChessGame");
    private static Mutex ColorSelection = new(false, "Color");
    static void Main(string[] args)
    {
      //args = new string[1] {"Kiritmak"};
      Thread MainThread = new(new ThreadStart(WelcomePlayer));
      MainThread.Name = args[0];
      playerName = MainThread.Name;
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
      string color = blancas ? "blancas" : "negras";
      string[] Positions;
      ChessBoard chessBoard = new ChessBoard();

      while(true) 
      {
        Println(playerName, "Esperando para elejir...");
        game.WaitOne();
        Println(playerName, "Es tu turno");

        GetChessBoard(chessBoard);
        if (Win(blancas)!= -1) break;
        showChessBoard(chessBoard, blancas);

        Println(playerName, "Esta pensando... ");
        ReadingPlayerInput(out Positions);
        //Leer el movimiento del jugador y, elejir si es valido o no
        //Transformar el tablero
        SaveChessBoard(chessBoard);
        Println(playerName, "Ha elejido un movimiento");

        game.ReleaseMutex();
        do Thread.Sleep(2000);
        while (File.ReadAllLines(ColorDataFilepath)[1].Split(',').ToList().Count == 1);
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
      int result = -1;
      if(result!=-1) //Reseting the board to original state
      {
        List<string> chessBoard = new List<string>();
        for (int i = 0; i < 8; i++)
        {
          string line = "";
          for (int j = 0; j < 8; j++)
          {
            if ((i + j) % 2 == 0) line += "W";
            else line += "B";
          }
          chessBoard.Add(line);
        }
        File.WriteAllLines(chessBoardFilepath, chessBoard);
      }
      if (result == 0) //Ganan las blancas
      {
        if (blancas) Println(playerName, "Ha ganado. Felicidades");
        else Println(playerName, "Ha perdido. Suerte Para la proxima");
      }
      else if (result == 1) //Ganan las negras
      {
        if (blancas) Println(playerName, "Ha perdido. Suerte Para la proxima");
        else Println(playerName, "Ha ganado. Felicidades");
      }
      return result;
    }
    static void showChessBoard(ChessBoard chessBoard, bool blancas)
    {
      List<string> showBoard = new List<string>();
      int line = 1;

      showBoard.Add("-------------------------------------------------");
      foreach (char[] chess in chessBoard)
      {
        string s = "";
        foreach (char c in chess)
        {
          string view;
          switch (c)
          {
            case 'B': view = "   "; break;
            case 'W': view = "///"; break;
            default: throw new NotImplementedException();
          }
          view = $"| {view} ";
          s+=(view);
        }
        s+="|";
        showBoard.Add(s);
        showBoard.Add("-------------------------------------------------");
      }

      string s2 = "";
      char colom = 'A';
      Func<char, bool> condition = (char c) => { return c<='H'; };
      if (!blancas)
      {
        condition = (char c) => { return c >= 'A'; };
        colom = 'H';
        line = 8;
      }

      for (char c = colom; condition(c);)
      {
        s2 += $"   {c}  ";
        if (blancas) c++;
        else c--;
      }
      showBoard.Add(s2);

      for(int i=0; i<showBoard.Count; i++) 
      {
        if (showBoard[i][0] != '-' && i!=showBoard.Count-1)
        {
          Console.Write($"{line} ");
          line += blancas ? 1 : -1;
        }
        
        else Console.Write("  ");
        Console.WriteLine(showBoard[i]);
      }
    }
    static void SaveChessBoard(ChessBoard chessBoard)
    {
      File.WriteAllLines(chessBoardFilepath, chessBoard.ToList());
    }
    static void ReadingPlayerInput(out string[] Positions)
    {
      string moove = Console.ReadLine();
      while(true)
      {
        if(string.IsNullOrEmpty(moove)) continue;
        Positions = moove.Split("=>");
        if (Positions.Count() != 2) continue;
        if (Positions[0].Length != 2 || Positions[1].Length != 2) continue;
        
      }
    }
    static void GetChessBoard(ChessBoard board)
    {
      List<string> text = File.ReadAllLines(chessBoardFilepath).ToList();
      for(int i=0; i<8; i++)
        for (int j = 0; j < 8; j++)
          board[i, j] = text[i][j];
    }
  }

}
