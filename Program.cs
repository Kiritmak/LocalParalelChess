using System.Collections;
using static ParalelLocalChess.Program;
using System.Drawing;
using System.Text;
using ParalelLocalChess;
using static System.Net.Mime.MediaTypeNames;

namespace ParalelLocalChess
{
  public static class Extensions
  {
    public static string Reverso(this string s)
    {
      string r = "";
      foreach (char c in s.Reverse()) r += c;
      return r;
    }
  }

  public class Position
  {
    //DeltaPiece[<char>][<axis><ith moove>]
    public Dictionary<char, int[,]> DeltaPiece = new Dictionary<char, int[,]>();
    public Position(string s)
    {
      if (string.IsNullOrEmpty(s) || s.Length != 2)
        throw new ArgumentException();
      s = s.ToUpper();

      TextColumn = s[0];
      TextRow = int.Parse(s.Substring(1));

      if(TextRow < 1 ||  TextRow > 8) throw new ArgumentException();
      if(TextColumn < 'A' || TextColumn > 'H' ) throw new ArgumentException();

      DeltaPiece['K'] = new int[2, 8] { { -1, -1, 0, 1, 1, 1, 0, -1}, {0, 1, 1, 1, 0, -1, -1, -1} };
      DeltaPiece['Q'] = DeltaPiece['K'];
      DeltaPiece['A'] = new int[2, 4] { {-1, 1, 1, -1}, {1, 1, -1, -1} };
      DeltaPiece['T'] = new int[2, 4] { { -1, 0, 1, 0 }, { 0, 1, 0, -1 } };
      DeltaPiece['C'] = new int[2, 8] { { -2, -1, 1, 2, 2, 1, -1, -2 }, {1, 2, 2, 1, -1, -2, -2, -1} };
      DeltaPiece['P'] = new int[2, 4] { { -1, -2, -1, -1 }, {0, 0, 1, -1} };
    }

    public char Color { get => ((Row + Column) % 2 == 0 ? 'B' : 'W'); }
    public char TextColumn {  get; set; }
    public int TextRow { get; set; }
    public int Row { get => 7-(TextRow-1);  }
    public int Column { get => (int)(TextColumn - 'A'); }

    public char GetPieceAtPosition(ChessBoard chessBoard)
    {
      char r = chessBoard[Row, Column];
      if (r == 'W' || r == 'B') return '?';
      return r;
    }
    public void SetPieceAtPosition(char P, ChessBoard chessBoard)
    {
      chessBoard[Row, Column] = P;
    }
    public int CanMooveTo(Position target, ChessBoard chessBoard)
    {
      char pieza = this.GetPieceAtPosition(chessBoard);
      if(char.ToLower(pieza) == 'p')
      {

        int mult = pieza == 'p' ? 1 : -1;
        int newRow;
        int newColumn;
        pieza = char.ToUpper(pieza);
        string s;

        //Avanzar una casilla
        if(target.GetPieceAtPosition(chessBoard) == '?')
        {
          newRow = Row + DeltaPiece[pieza][0, 0] * mult;
          newColumn = Column + DeltaPiece[pieza][1, 0] * mult;
          s = $"{(char)('A' + newColumn)}{7 - newRow + 1}";
          try { if (target.Equals(new Position(s))) return 1; }
          catch { }
        }

        //Avanzar dos casillas
        if ( (mult == 1 && this.TextRow == 2) || (mult==-1 && this.TextRow == 7))
        {
          try 
          {
            char passedC = mult == 1 ? 'x' : 'X';

            newRow = Row + (-1) * mult;
            newColumn = Column;
            s = $"{(char)('A' + newColumn)}{7 - newRow + 1}";
            Position newPos1 = new Position(s);

            newRow += (-1) * mult;
            s = $"{(char)('A' + newColumn)}{7 - newRow + 1}";
            Position newPos2 = new Position(s);

            if(newPos1.GetPieceAtPosition(chessBoard) == '?' && target.Equals(newPos2) && isCharEmpty(newPos2.GetPieceAtPosition(chessBoard)))
            {
              newPos1.SetPieceAtPosition(passedC, chessBoard);
              return -1;
            }
          }
          catch { }
        }

        //Capturar alguna pieza
        for(int i=1; i<=2; i++)
        {
          try
          {
            newRow = Row + DeltaPiece[pieza][0, i + 1] * mult;
            newColumn = Column + DeltaPiece[pieza][1, i + 1] * mult;
            s = $"{(char)('A' + newColumn)}{7 - newRow + 1}";

            Position newPos = new(s);
            if (target.Equals(newPos) && newPos.GetPieceAtPosition(chessBoard) != '?') return 1;
          }
          catch { }
        }

      }
      else if(char.ToUpper(pieza) == 'K' || char.ToUpper(pieza) == 'C')
      {
        pieza = char.ToUpper(pieza);
        for(int i=0; i < DeltaPiece[pieza].GetLength(1); i++)
        {
          int newRow = Row + DeltaPiece[pieza][0, i];
          int newColumn = Column + DeltaPiece[pieza][1, i];
          string s = $"{(char)('A' + newColumn)}{7 - newRow+1}";
          try
          { 
            Position newPos = new(s);
            if(newPos.Equals(target)) return 1;
          }
          catch {}
        }
      }
      else
      {
        pieza = char.ToUpper(pieza);
        for (int i = 0; i < DeltaPiece[pieza].GetLength(1); i++)
        {
          int k = 1;
          while (true)
          {
            int newRow = Row + DeltaPiece[pieza][0, i]*k;
            int newColumn = Column + DeltaPiece[pieza][1, i]*k;
            string s = $"{(char)('A' + newColumn)}{7 - newRow + 1}";
            try
            {
              Position newPos = new(s);
              if (newPos.Equals(target)) return 1;
              else if (newPos.GetPieceAtPosition(chessBoard) != '?') break;
              k++;
            }
            catch { break; }
          }
        }
      }
      return 0;
    }
    public override string ToString()
    {
      return $"{TextColumn}{TextRow}: {Row};{Column}";
    }
    public override bool Equals(object? obj)
    {
      Position? other= obj as Position;
      if (other == null) throw new ArgumentNullException();
      return (this.Row == other.Row && this.Column == other.Column) ;
    }
  }

  public class ChessBoard : IEnumerable<char[]>, IEnumerable
  {
    private List<char[]> chessBoard;
    public ChessBoard()
    {
      chessBoard = new List<char[]>();
      chessBoard.Add(['T', 'C','A','Q', 'K', 'A', 'C', 'T']);
      chessBoard.Add(['P', 'P', 'P', 'P', 'P', 'P', 'P', 'P']);
      for(int i=3; i<=6; i++)
      {
        char[] chars = new char[8];
        for (int j = 0; j < 8; j++)
        {
          chars[j] = (i + j) % 2 == 0 ? 'B' : 'W';
        }
        chessBoard.Add(chars);
      }
      chessBoard.Add(['p', 'p', 'p', 'p', 'p', 'p', 'p', 'p']);
      chessBoard.Add(['t', 'c', 'a', 'q', 'k', 'a', 'c', 't']);
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
    public ChessBoard Clone()
    {
      ChessBoard result = new ChessBoard();
      for (int i = 0; i < 8; i++)
        for (int j = 0; j < 8; j++)
          result[i, j] = this[i, j];
      return result;
    }
  }

  public static class Program
  {
    static Dictionary<string, Func<ChessBoard, int>> SpecialComands = new();

    static string ExectuableFilepath = Environment.CurrentDirectory;
    static string ColorDataFilepath = @$"{ExectuableFilepath}\..\TextFiles\ColorData.txt";
    static string chessBoardFilepath = @$"{ExectuableFilepath}\..\TextFiles\ChessBoard.txt";
    static string? playerName;
    static bool hasAPassedPawn = false;

    private static Semaphore SalaDeEspera = new(2, 2, "SalaDeEspera");
    private static Mutex game = new(false, "ChessGame");
    private static Mutex ColorSelection = new(false, "Color");
    static void Main(string[] args)
    {
      if (args.Count() != 1)
      {
        Console.WriteLine("Ingresa tu nombre!");
        string? name = "";
        while(string.IsNullOrEmpty(name)) name = Console.ReadLine();
        args = new string[] { name };
        Main(args);
      }
      else
      {
        SpecialComands["close"] = (chessBoard) =>
        {
          SaveChessBoard(chessBoard);
          return -1;
        };
        SpecialComands["reset"] = (chessBoard) =>
        {
          chessBoard = new();
          return SpecialComands["close"](chessBoard);
        };
        Thread MainThread = new(new ThreadStart(WelcomePlayer));
        MainThread.Name = args[0];
        playerName = MainThread.Name;
        MainThread.Start();
      }
    }
    static void WelcomePlayer()
    {
      string? playerName = Thread.CurrentThread.Name;

      Console.WriteLine($"Bienvenido a Paralel Chess {playerName}!\nUn juego de ajedrez en donde juegas con tu amigo en ventanas separadas!\n");
      Console.WriteLine("Cada celda en un tablero de ajedrez tiene unas coordenadas\nPor ejemplo: e4, a6, b8, h2");
      Console.WriteLine("Para realizar un movimiento, escribe las coordenadas de la celda inicial, seguido de una felcha \"=>\", y luego las otras coordenadas");
      Console.WriteLine("Por ejemplo: e4=>e5, a6=>c4");
      Console.WriteLine("Para jugar con tu amigo, abre otro ejecutable del juego");
      Console.WriteLine("Dicho esto, espero que disfrutes del juego!");
      while (Exit())
      {
        Println(playerName, "Esta esperando para entrar a la sala...");
        SalaDeEspera.WaitOne();
        Console.WriteLine($"{playerName} Ha entrado a la sala");
        Println(playerName, "Esta esperando para pedir un color...");
        ColorSelection.WaitOne();
        List<string> text = File.ReadAllLines(ColorDataFilepath).ToList();
        string[] Blancas = text[0].Split(",");
        if (Blancas.Count() == 1 || Blancas[1] == playerName)
        {
          Blancas = ["Blancas", playerName] ;
          text[0] = $"Blancas,{playerName}";
          File.WriteAllLines(ColorDataFilepath, text);
        }
        else
        {
          text[1] = $"Negras,{playerName}";
          File.WriteAllLines(ColorDataFilepath, text);
        }
        Println(playerName, "Ha elejido un color");
        ColorSelection.ReleaseMutex();
        Play(Blancas[1] == playerName);
        Console.WriteLine($"{playerName} Ha salido de la sala");
        SalaDeEspera.Release();
      }
    }
    static void Play(bool blancas)
    {
      string color = blancas ? "blancas" : "negras";
      Position[] positions = new Position[2];
      ChessBoard chessBoard = new ChessBoard();

      while(true) 
      {
        Println(playerName, "Esperando para elejir...");
        game.WaitOne();
        Println(playerName, "Es tu turno");

        if(hasAPassedPawn) RemovePassedPawn(blancas);
        GetChessBoard(chessBoard);
        int w = Win(blancas, chessBoard);
        if ( w != -1) //Comprobando si esta en jaque mate algun rey
        {
          List<string> text = File.ReadAllLines(ColorDataFilepath).ToList();
          if((!blancas && w==1) || (blancas && w == 0))
          {
            chessBoard = new ChessBoard();
            text[0] = "Blancas";
            text[1] = "Negras";
            File.WriteAllLines(ColorDataFilepath, text);
            File.WriteAllLines(chessBoardFilepath, chessBoard.ToList());
          }
          game.ReleaseMutex();
          break;
        }
        showChessBoard(chessBoard, blancas);

        Println(playerName, "Esta pensando... ");
        if (ReadingPlayerInput(chessBoard, positions, blancas) == -1) //Leer el movimiento del jugador y, elejir si es valido o no
        {
          game.ReleaseMutex();
          break;
        }

        SaveChessBoard(chessBoard);//Transformar el tablero

        Println(playerName, "Ha elejido un movimiento");

        game.ReleaseMutex();
        do Thread.Sleep(2000);
        while (File.ReadAllLines(ColorDataFilepath)[1].Split(',').ToList().Count == 1);
      } 

    }
    static bool Exit()
    {
      Console.WriteLine("Escribe 1 para empezar un juego, cualquier otra entrada cerrara el programa");
      while(true)
      {
        string? option;
        option = Console.ReadLine();
        return option == "1";
      }
    }
    static void Println(string? name, string message) =>
      Console.WriteLine($"{name} {message}");
    static int Win(bool blancas, ChessBoard chessBoard)
    {
      // 0 Ganan las blancas
      // 1 Ganan las negras
      // -1 No gana nadie aun
      int result = -1;

      int Brow=-1, Bcol=-1, Wrow=-1, Wcol=-1;

      //Getting both kings positions
      for(int i=0; i<8; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          if (chessBoard[i, j] == 'k')
          {
            Wrow = i;
            Wcol = j;
          }
          if (chessBoard[i, j] == 'K')
          {
            Brow = i; Bcol = j;
          }
        }
      }


      //Cada color verificara dos cosas: Primero si su rey esta siendo atacado por otra pieza.
      //Segundo, si existe un rey enemigo
      if(blancas)
      {
        //Comprobando si el rey blanco esta en jaque, y si las blancas pueden hacer algun movimiento que lo saque del jaque
        bool WhiteChecked = KingIsChecked(true, chessBoard);

        if (Brow == -1) result = 0; 
        else if (WhiteChecked)
        {
          List<string> mooves = GetAllPossibleMooves(true, chessBoard);
          if (mooves.Count > 0)
          {
            for(int i=0; i<mooves.Count;i++)
            {
              string transition = mooves[i];
              ChessBoard aux = chessBoard.Clone();
              if (KingSafeAfterTransition(blancas, aux, transition))
              {
                Console.WriteLine(transition);
                result = -1;
                break;
              }
              if (i == mooves.Count - 1)
              {
                result = 1;
                chessBoard[Wrow, Wcol] = (new Position(GetFormat(Wrow, Wcol))).Color;
                File.WriteAllLines(chessBoardFilepath, chessBoard.ToList());
              }
            }
          }
          else
          {
            result = 1;
            chessBoard[Wrow, Wcol] = (new Position(GetFormat(Wrow, Wcol))).Color;
            File.WriteAllLines(chessBoardFilepath, chessBoard.ToList());
          }
        }
      }
      else
      {
        //Comprobando si el rey negro esta en jaque, y si las negras pueden hacer algun movimiento que lo saque del jaque

        bool BlackChecked = KingIsChecked(false, chessBoard);

        if (Wrow == -1) result = 1;
        else if (BlackChecked)
        {
          List<string> mooves = GetAllPossibleMooves(false, chessBoard);
          if (mooves.Count > 0)
          {
            for (int i = 0; i < mooves.Count; i++)
            {
              string transition = mooves[i];
              ChessBoard aux = chessBoard.Clone();
              if (KingSafeAfterTransition(blancas, aux, transition))
              {
                Console.WriteLine(transition);
                result = -1;
                break;
              }
              if (i == mooves.Count - 1)
              {
                result = 0;
                chessBoard[Brow, Bcol] = (new Position(GetFormat(Brow, Bcol))).Color;
                File.WriteAllLines(chessBoardFilepath, chessBoard.ToList());
              }
            }
          }
          else
          {
            result = 0;
            chessBoard[Brow, Bcol] = (new Position(GetFormat(Brow, Bcol))).Color;
            File.WriteAllLines(chessBoardFilepath, chessBoard.ToList());
          }
        }

      }

      #region Retornando el resultado 
      if (result == 0)
      {
        if (blancas) Println(playerName, "Ha ganado. Felicidades");
        else Println(playerName, "Ha perdido. Suerte Para la proxima");
      }
      else if (result == 1)
      {
        if (blancas) Println(playerName, "Ha perdido. Suerte Para la proxima");
        else Println(playerName, "Ha ganado. Felicidades");
      }
      return result;
      #endregion 
    }
    static void showChessBoard(ChessBoard chessBoard, bool blancas)
    {
      List<string> showBoard = new List<string>();
      int line = 8;

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

            case 'P': view = "BPw"; break;
            case 'T': view = "BTw"; break;
            case 'C': view = "BKn"; break;
            case 'A': view = "BBs"; break;
            case 'Q': view = "BQn"; break;
            case 'K': view = "BKg"; break;
            case 'X': view = "BPP"; break; 

            case 't': view = "WTw"; break;
            case 'p': view = "WPw"; break;
            case 'c': view = "WKn"; break;
            case 'a': view = "WBs"; break;
            case 'q': view = "WQn"; break;
            case 'k': view = "WKg"; break;
            case 'x': view = "WPP"; break;

            default: throw new NotImplementedException();
          }
          view = blancas ? view : view.Reverso();
          view = $"| {view} ";
          s += view;
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
        line = 1;
      }

      for (char c = colom; condition(c);)
      {
        s2 += $"   {c}  ";
        if (blancas) c++;
        else c--;
      }
      int startIndex = blancas ? 0 : showBoard.Count-1;
      int endIndex = blancas ? showBoard.Count - 1 : 0;
      Func<int, int, bool> check = blancas ? (a, b) => { return a <= b; } : (a, b) => { return a >= b; };
      for (int i=startIndex; check(i, endIndex); i+= blancas ? 1 : -1) 
      {
        if (showBoard[i][0] != '-' && i!=endIndex)
        {
          Console.Write($"{line} ");
          line += blancas ? -1 : 1;
        }
        else Console.Write("  ");
        if(!blancas) Console.WriteLine(showBoard[i].Reverso());
        else Console.WriteLine(showBoard[i]);
      }
      Console.Write("  ");
      Console.WriteLine(s2);
    }
    static void SaveChessBoard(ChessBoard chessBoard)
    {
      File.WriteAllLines(chessBoardFilepath, chessBoard.ToList());
    }
    static int ReadingPlayerInput(ChessBoard chessBoard, Position[] positions, bool blancas)
    {
      while (true)
      {
        try
        {
          ChessBoard aux = chessBoard.Clone();
          string moove = Console.ReadLine();
          if(SpecialComands.ContainsKey(moove.ToLower()))
          {
            return SpecialComands[moove.ToLower()](chessBoard);
          }
          string[] Positions = moove.Split("=>");
          if (Positions.Count() != 2) throw new Exception();
          positions[0] = new Position(Positions[0]);
          positions[1] = new Position(Positions[1]);

          char pieza = positions[0].GetPieceAtPosition(chessBoard);
          char nextPos = positions[1].GetPieceAtPosition(chessBoard);

          if (pieza == '?' || char.ToLower(pieza) == 'x') throw new Exception(); //Valida que haya una pieza en la primera posicion
          Console.WriteLine("Pasa la barrera de la existencia");

          //Valida que un jugador escoja la pieza de su color
          if((blancas && char.IsAsciiLetterUpper(pieza) ) || (char.IsAsciiLetterLower(pieza) && !blancas)) throw new Exception();
          Console.WriteLine("Pasa la barrera del color");

          //Valida que sean posiciones diferentes
          if (positions[0].Equals(positions[1])) throw new Exception();

          //Valida que, de haber una pieza en la siguiente posicion, que sea una de diferente color 
          if ((char.IsAsciiLetterLower(nextPos) == char.IsAsciiLetterLower(pieza)) && !char.IsPunctuation(nextPos)) throw new Exception();
          Console.WriteLine("Pasa la barrera del ataque");

          int r = positions[0].CanMooveTo(positions[1], chessBoard);
          if (r ==0 ) throw new Exception();
          if (r == -1) hasAPassedPawn = true; //Comprueba si hay un peon pasado

          if(char.ToLower(pieza)=='p')
          {
            Console.WriteLine("Se movio un peon");
            char OtherPieza = positions[1].GetPieceAtPosition(chessBoard);
            if(char.ToLower(OtherPieza) == 'x')
            {
              Console.WriteLine("El peon ataco a un peon pasado");
              int pieceRow = positions[1].Row + (blancas ? 1 : -1);
              int pieceColumn = positions[1].Column;
              Position piecePos = new(GetFormat(pieceRow, pieceColumn));
              piecePos.SetPieceAtPosition(piecePos.Color, chessBoard);
            }
            if (positions[1].Row == (blancas ? 0 : 7))
            {
              Console.WriteLine("El peon se ha convertido en reina");
              if (blancas) pieza = 'q';
              else pieza = 'Q';
            }
          }
          positions[0].SetPieceAtPosition(positions[0].Color, chessBoard);
          positions[1].SetPieceAtPosition(pieza, chessBoard);

          //Check if king is checked after the moove
          if(KingIsChecked(blancas, chessBoard))
          {
            Console.WriteLine("El rey no puede quedar al descubierto");
            chessBoard = aux.Clone();
            throw new Exception();
          }

          Console.WriteLine("El movimiento es valido");

          break;
        }
        catch
        {
          Console.WriteLine("Invalid Input");
        }
      }
      return 1;
    }
    static void GetChessBoard(ChessBoard board)
    {
      List<string> text = File.ReadAllLines(chessBoardFilepath).ToList();
      for(int i=0; i<8; i++)
        for (int j = 0; j < 8; j++)
          board[i, j] = text[i][j];
    }
    static void RemovePassedPawn(bool blancas)
    {
      hasAPassedPawn = false;
      ChessBoard board = new ChessBoard();
      char target = blancas ? 'x' : 'X';
      GetChessBoard (board);
      for(int i=0; i<8; ++i)
      {
        for(int j = 0;j < 8; j++)
          if(board[i, j] == target)
          {
            board[i, j] = (i+j)%2==0 ? 'B' : 'W';
            SaveChessBoard (board);
            return;
          }
      }
    }
    static string GetFormat(int Row, int Col)
    {
      return $"{(char)('A' + Col)}{7 - Row + 1}";
    }
    static bool KingIsChecked(bool blancas, ChessBoard chessBoard)
    {
      char King = blancas ? 'k' : 'K';
      int Row=0, Col=0, mult = blancas ? 1 : -1;
      Position dummy = new(GetFormat(Row, Col));
      var DeltaPiece = dummy.DeltaPiece;

      //Buscando la posicion del rey
      for(int i=0; i<8; i++)
      {
        for(int j=0; j<8; j++)
        {
          Position pos = new(GetFormat(i, j));
          char c = pos.GetPieceAtPosition(chessBoard);
          if(King== c)
          {
            Row = i;
            Col = j;
            i = 8;
            break;
          }
        }
      }

      //Buscando algun Caballo o Rey que lo ataque
      for(int i=0; i<8; i++)
      {
        int atkRow = Row;
        int atkCol = Col;
        char atkKnight = blancas ? 'C' : 'c';
        char atkKing = blancas ? 'K' : 'k';

        //Caballo atacante
        atkRow = Row + DeltaPiece['C'][0, i];
        atkCol = Col + DeltaPiece['C'][1, i];
        try
        {
          if (chessBoard[atkRow, atkCol] == atkKnight) return true;
        }
        catch { }

        //Rey atacante
        atkRow = Row + DeltaPiece['K'][0, i];
        atkCol = Col + DeltaPiece['K'][1, i];
        try
        {
          if (chessBoard[atkRow, atkCol] == atkKing) return true;
        }
        catch { }
      }

      //Buscando si un peon puede atacar al rey
      for(int i=1; i<=2; i++)
      {
        int atkRow = Row + DeltaPiece['P'][0, i + 1] * mult;
        int atkCol = Col + DeltaPiece['P'][1, i + 1] * mult;
        char atkPawn = blancas ? 'P' : 'p';
        try
        {
          if (chessBoard[atkRow, atkCol] == atkPawn) return true;
        }
        catch { }
      }

      //Buscando si el alfil o la torre pueden atacar al rey
      for(int i=0; i<4; i++)
      {
        int atkRow = Row;
        int atkCol = Col;
        char atkBishop = blancas ? 'A' : 'a';
        char atkRook = blancas ? 'T' : 't';

        int k = 1;
        while(true)
        {
          try
          {
            atkRow = Row + DeltaPiece['A'][0, i]*k;
            atkCol = Col + DeltaPiece['A'][1, i]*k;
            Position pos = new(GetFormat(atkRow, atkCol));
            if (chessBoard[atkRow, atkCol] == atkBishop) return true;
            if (chessBoard[atkRow, atkCol] != pos.Color) break;
            k++;
          }
          catch
          {
            break;
          }
        }
        k = 1;
        while(true)
        {
          try
          {
            atkRow = Row + DeltaPiece['T'][0, i] * k;
            atkCol = Col + DeltaPiece['T'][1, i] * k;
            Position pos = new(GetFormat(atkRow, atkCol));
            if (chessBoard[atkRow, atkCol] == atkRook) return true;
            if (chessBoard[atkRow, atkCol] != pos.Color) break;
            k++;
          }
          catch
          {
            break;
          }
        }
      }

      //Buscando si una dama puede atacar al rey
      for(int i=0; i<8; i++)
      {
        int atkRow = Row;
        int atkCol = Col;
        char atkQueen = blancas ? 'Q' : 'q';

        int k = 1;
        while (true)
        {
          try
          {
            atkRow = Row + DeltaPiece['Q'][0, i] * k;
            atkCol = Col + DeltaPiece['Q'][1, i] * k;
            Position pos = new(GetFormat(atkRow, atkCol));
            if (chessBoard[atkRow, atkCol] == atkQueen) return true;
            if (chessBoard[atkRow, atkCol] != pos.Color) break;
            k++;
          }
          catch
          {
            break;
          }
        }
      }

      return false;
    }
    static List<string> GetAllPossibleMooves(bool blancas, ChessBoard chessBoard)
    {
      List<string> mooves = new();
      string nextMoove = "";
      //Buscando posicion de una pieza
      for(int i=0; i<8; i++)
        for(int j=0; j<8; j++)
        {
          char piece = chessBoard[i, j];
          Position startPos = new(GetFormat(i, j));
          //Comprobando si la pieza es del mismo color
          if(char.IsAsciiLetterLower(piece) == blancas)
          {
            string startPosition = $"{startPos.Column}{startPos.Row}";
            piece = char.ToUpper(piece);
            int nextRow = i, nextCol = j;

            //Moviendo un Peon
            if(piece == 'P')
            {
              int mult = blancas ? 1 : -1;
              ChessBoard aux = chessBoard.Clone();

              for (int k = 0; k < 4; k++)
              {
                nextRow = i + startPos.DeltaPiece[piece][0, k]*mult;
                nextCol = j + startPos.DeltaPiece[piece][1, k]*mult;
                try
                {
                  Position nextPos = new(GetFormat(nextRow, nextCol));
                  char targetPiece = nextPos.GetPieceAtPosition(chessBoard);
                  if (nextPos.Equals(startPos)) throw new Exception();
                  else if (isCharEmpty(targetPiece) && SameColor(startPos, nextPos, chessBoard)) throw new Exception();
                  if (startPos.CanMooveTo(nextPos, aux) != 0)
                  {
                    string transition = $"{startPos.TextColumn}{startPos.TextRow}=>{nextPos.TextColumn}{nextPos.TextRow}";
                    mooves.Add(transition);
                  }
                }
                catch { }
              }

            }
            //Moviendo una Caballo o a un rey
            else if(piece == 'C' || piece == 'K')
            {
              for(int k=0; k<8; k++)
              {
                nextRow = i + startPos.DeltaPiece[piece][0, k];
                nextCol = j + startPos.DeltaPiece[piece][1, k];
                try
                {
                  Position nextPos = new(GetFormat(nextRow, nextCol));
                  char targetPiece = nextPos.GetPieceAtPosition(chessBoard);
                  if (nextPos.Equals(startPos)) throw new Exception();
                  else if(targetPiece != '?' && SameColor(startPos, nextPos, chessBoard)) throw new Exception();
                  if (startPos.CanMooveTo(nextPos, chessBoard) != 0)
                  {
                    string transition = $"{startPos.TextColumn}{startPos.TextRow}=>{nextPos.TextColumn}{nextPos.TextRow}";
                    mooves.Add(transition);
                  }
                }
                catch { }
              }
            }
            //Moviendo a otra pieza
            else
            {
              for(int k=0; k < startPos.DeltaPiece[piece].GetLength(1); k++)
              {
                int steps = 1;
                while(true)
                {
                  nextRow = i + startPos.DeltaPiece[piece][0, k]*steps;
                  nextCol = j + startPos.DeltaPiece[piece][1, k]*steps;
                  try
                  {
                    Position nextPos = new(GetFormat(nextRow, nextCol));
                    char targetPiece = nextPos.GetPieceAtPosition(chessBoard);

                    if (nextPos.Equals(startPos)) throw new Exception();
                    else if (!isCharEmpty(targetPiece) && SameColor(startPos, nextPos, chessBoard)) throw new Exception();

                    if (startPos.CanMooveTo(nextPos, chessBoard) != 0)
                    {
                      string transition = $"{startPos.TextColumn}{startPos.TextRow}=>{nextPos.TextColumn}{nextPos.TextRow}";
                      mooves.Add(transition);
                    }
                    else if (!isCharEmpty(targetPiece)) break;
                    steps++;
                  }
                  catch { break;  }
                }
              }
            }
          }
        }
      /*
      foreach(string moove in mooves)
        Console.WriteLine(moove);
      */
      return mooves;
    }
    static bool KingSafeAfterTransition(bool blancas, ChessBoard aux, string transition)
    {
      string[] s = transition.Split("=>");
      Position[] positions = [new(s[0]), new(s[1])];
      positions[1].SetPieceAtPosition(positions[0].GetPieceAtPosition(aux), aux);
      positions[0].SetPieceAtPosition(positions[0].Color, aux);
      return !KingIsChecked(blancas, aux);
    }
    static bool SameColor(Position p1, Position p2, ChessBoard chessBoard) 
    {
      char c1 = p1.GetPieceAtPosition(chessBoard);
      char c2 = p2.GetPieceAtPosition(chessBoard);
      return char.IsAsciiLetterLower(c1) == char.IsAsciiLetterLower(c2);
    }
    public static bool isCharEmpty(char c)
    {
      return c == '?' || char.ToLower(c) == 'x';
    }
  }

}
