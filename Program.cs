namespace ParalelLocalChess
{
  static class Program
  {
    static int CurrentPlayers = 0;
    static void Main(string playerName)
    {
      Mutex game = new Mutex(false, "ChessGame");
      Console.WriteLine($"Welcome to Paralel Chess!\nA game where you can play chess with your friend at separated windows in the same computer!");
      while (Exit())
      {
        //Joing the game
      }
    }

    static bool Exit()
    {
      Console.WriteLine("Type 1 to enter to the game, type anything else to exit the game");
      int option = Console.Read();
      return option == 1;
    }
  }

}
