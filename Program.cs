//Adding the libraries
using System;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TicTacToe_Game
{
    /*
     * Applying Abstract Factory Design Pattern
     * Abstract Factory class Player 
     */
    public abstract class Player
    {
        //creating common player attributes 
        protected int playerID;
        protected char symbol;

        //Factory methods for Player
        public abstract char getSymbol();
        public abstract int getID();

        public abstract bool giveCommand(GameState gameState, CommandHandler commandHandler);
    }

    //Create concrete product for Player
    public class HumanPlayer : Player
    {
        //HumanPlayer constructor includes ID and symbol
        public HumanPlayer(int playerID, char symbol)
        {
            this.symbol = symbol;
            this.playerID = playerID;
        }

        public override char getSymbol() => symbol;
        public override int getID() => playerID;

        //User enter the command
        public override bool giveCommand(GameState gameState, CommandHandler commandHandler)
        {
            char symbol = gameState.currentPlayerID == 1 ? 'X' : 'O';
            WriteLine($"Player {gameState.currentPlayerID} ({symbol}), enter your command (ex: type HELP for a list of commands): ");
            //ensure command in the correct format
            string command = ReadLine().Trim().ToLower();

            string[] parts = command.Split(' ');
            return commandHandler.handleCommand(parts, gameState);
        }
    }

    //Create concrete product for Player
    public class ComputerPlayer : Player
    {
        //ComputerPlayer constructor includes ID and symbol
        public ComputerPlayer(int playerID, char symbol)
        {
            this.symbol = symbol;
            this.playerID = playerID;
        }

        public override char getSymbol() => symbol;
        public override int getID() => playerID;

        public override bool giveCommand(GameState gameState, CommandHandler commandHandler)
        {
            // Simple logic for computer player that randomly generate a move from 1 to 9
            // However, a move must be checked until it is valid
            Random rand = new Random();
            int position = 0;
            do
            {
                position = rand.Next(1, 10);// create a number between 1 to 10
            } while (!gameState.isValidMove(position));

            WriteLine($"Computer ({symbol}) places at the position {position}");
            /*
             * ComputerPlayer will only make a move, can not giveCommand.
             */
            gameState.setMove(position, symbol, playerID);
            commandHandler.saveState(gameState); 
            return true;
        }
    }

    public class GameState
    {
        private char[] board = new char[9]; //Tracking the board 
        private int[] playerMoves = new int[9]; //Tracking which player move made in turn
        public int turn { get; set; }
        public int currentPlayerID { get; set; }

        public GameState()
        {
            for (int i = 0; i < board.Length; i++)
            {
                board[i] = ' ';
            }
            // Game started with default to player 1 and turn = 1
            turn = 1;   
            currentPlayerID = 1; 
        }

        /* The valid move is true if position on board is empty in the range 1 to 9
         * */
        public bool isValidMove(int position)
        {
            return position >= 1 && position <= 9 && board[position - 1] == ' ';
        }

        public void setMove(int position, char symbol, int playerID)
        {
            if (isValidMove(position))
            {
                board[position - 1] = symbol; // board filled with 'X' and 'O'
                playerMoves[position - 1] = turn; //Tracking the player move according it's turn
                currentPlayerID = playerID; // Update currentPlayerID when move is set
            }
        }

        /*
         * The board is not empty meaning that it is fully filled => game is over
         */
        public bool isBoardFull()
        {
            return !board.Contains(' ');
        }

        public char[] getBoard()
        {
            return board;
        }

        public int[] getPlayerMoves()
        {
            return playerMoves;
        }


        public void loadState(char[] savedBoard, int savedTurn, int savedPlayerID, int[] savedPlayerMoves)
        {
            Array.Copy(savedBoard, board, board.Length);
            Array.Copy(savedPlayerMoves, playerMoves, playerMoves.Length);
            turn = savedTurn;
            currentPlayerID = savedPlayerID;
        }

        public void printBoard()
        {
            WriteLine("\nWelcome to Wild Tic-Tac-Toe, each player takes a turn placing a X or a O in a cell \n in attempts to be the last to connect 3 in a row and win the game. \n Please take a look to check the detailed guide below for positioning: ");
            WriteLine("----+---+---");
            WriteLine("| 1 | 2 | 3 |");
            WriteLine("----+---+---");
            WriteLine("| 4 | 5 | 6 |");
            WriteLine("----+---+---");
            WriteLine("| 7 | 8 | 9 |");
            WriteLine("----+---+---\n");
            WriteLine("----+---+---");
            WriteLine($"| {board[0]} | {board[1]} | {board[2]} |");
            WriteLine("----+---+---");
            WriteLine($"| {board[3]} | {board[4]} | {board[5]} |");
            WriteLine("----+---+---");
            WriteLine($"| {board[6]} | {board[7]} | {board[8]} |");
            WriteLine("----+---+---\n");
        }
    }




    public class CommandHandler
    {
        //CommandHandler attributes
        private History history = new History();
        private GameState gameState;

        //CommandHandler constructor
        public CommandHandler(GameState gameState)
        {
            this.gameState = gameState;
            history.pushState(gameState.getBoard(), gameState.turn, gameState.currentPlayerID, gameState.getPlayerMoves());
        }

        public bool handleCommand(string[] commandParts, GameState gameState)
        {
            /*
             * According to each command, there are some according functions 
             */
            switch (commandParts[0])
            {
                case "move":
                    makeMove(gameState);
                    gameState.printBoard(); // Display the board after a valid move
                    return true;
                case "save":
                    saveGame(gameState);
                    return false;
                case "load":
                    loadGame(gameState);
                    gameState.printBoard(); // Display the board after loading
                    return false;
                case "undo":
                    undoMove();
                    gameState.printBoard(); // Display the board after undo
                    return false;
                case "redo":
                    redoMove();
                    gameState.printBoard(); // Display the board after redo
                    return false;
                case "exit":
                    exitGame();
                    return false;                 
                case "help":
                    showHelp();
                    return false;
                default:
                    WriteLine("Unknown command. Type HELP for a list of commands.\n");
                    return false;
        
            }
        }

        private void makeMove(GameState gameState)
        {
            char symbol = gameState.currentPlayerID == 1 ? 'X' : 'O';
            WriteLine("Please enter the position (1 to 9): ");
            string positionInput = ReadLine().Trim();

            try { 
            int position = Convert.ToInt32(positionInput);
            if (gameState.isValidMove(position))
            {
                /*
                 * If user enter a valid position, will make a setMove()
                 * otherwise game will let user know the position is occupied
                 * */
                gameState.setMove(position, symbol, gameState.currentPlayerID);
                saveState(gameState);
            }
            else
            {
                WriteLine("Invalid move. Cell already occupied, please try again.");
            }
            }
            catch (Exception)
            {
                WriteLine("Invalid move. Cell already occupied, please try again.");
            }
}
        //save the Game State
        private void saveGame(GameState gameState)
        {
            history.save(gameState.getBoard(), gameState.turn, gameState.currentPlayerID, gameState.getPlayerMoves());
            WriteLine("Game state saved.\n");
        }

        //load the Game State
        private void loadGame(GameState gameState)
        {
            (char[] savedBoard, int savedTurn, int savedPlayerID, int[] savedPlayerMoves) = history.load();
            if (savedBoard != null)
            {
                gameState.loadState(savedBoard, savedTurn, savedPlayerID, savedPlayerMoves);
                WriteLine("Game state loaded.\n");
            }
            else
            {
                WriteLine("No saved game found.\n");
            }
        }

        //undo a Move
        private void undoMove()
        {
            (char[] previousState, int previousTurn, int previousPlayerID, int[] savedPlayerMoves) = history.undo();
            //checking if the previous move exists
            if (previousState != null)
            {
                gameState.loadState(previousState, previousTurn, previousPlayerID, savedPlayerMoves);

                gameState.currentPlayerID = previousPlayerID == 1 ? 2 : 1;
                WriteLine("Last move undone.\n");
            }
            else
            {
                WriteLine("No previous move to undo.\n");
            }
        }

        private void redoMove()
        {
            (char[] nextState, int nextTurn, int nextPlayerID, int[] nextPlayerMoves) = history.redo();
            //checking if the next move exists
            if (nextState != null)
            {
                gameState.loadState(nextState, nextTurn, nextPlayerID, nextPlayerMoves);
                gameState.currentPlayerID = nextPlayerID;
                WriteLine("Move redone.\n");
            }
            else
            {
                WriteLine("No move to redo.\n");
            }
        }

        private void exitGame()
        {
            WriteLine("Exiting the game\n");
            Environment.Exit(0);
        }
        //call showHelp from Help class
        private void showHelp()
        {
            Help.showHelp();
        }
        //tracking Game State into History
        public void saveState(GameState gameState)
        {
            history.pushState(gameState.getBoard(), gameState.turn, gameState.currentPlayerID, gameState.getPlayerMoves());
        }
    }

    public class History
    {
        //History attributes
        private readonly Stack<(char[], int, int, int[])> undoStack = new Stack<(char[], int, int, int[])>();
        private readonly Stack<(char[], int, int, int[])> redoStack = new Stack<(char[], int, int, int[])>();

        public void pushState(char[] board, int turn, int playerID, int[] playerMoves)
        {
            undoStack.Push(((char[])board.Clone(), turn, playerID, (int[])playerMoves.Clone()));
            redoStack.Clear();
        }

        public (char[], int, int, int[]) undo()
        {
            if (undoStack.Count > 1)
            {
                var currentState = undoStack.Pop();
                redoStack.Push(currentState);
                return undoStack.Peek();
            }
            return (null, 0, 0, null);
        }

        public (char[], int, int, int[]) redo()
        {
            if (redoStack.Count > 0)
            {
                var nextState = redoStack.Pop();
                undoStack.Push(nextState);
                return nextState;
            }
            return (null, 0, 0, null);
        }

        /*
         * save board, turn, currentplayerID and playerMoves in separate line into TicTacToe.txt
         */
        public void save(char[] board, int turn, int playerID, int[] playerMoves)
        {
            //create TicTacToe.txt file and write on it
            using (FileStream fs = new FileStream("TicTacToe.txt", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine(new string(board));
                    writer.WriteLine(turn);
                    writer.WriteLine(playerID);
                    writer.WriteLine(string.Join(",", playerMoves));
                }
            }
        }

        public (char[], int, int, int[]) load()
        {
            //check if the file exists before it is opened and readable
            if (File.Exists("TicTacToe.txt"))
            {
                using (FileStream fs = new FileStream("TicTacToe.txt", FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {

                        char[] board = reader.ReadLine().ToCharArray();
                        int turn = Convert.ToInt32(reader.ReadLine());
                        int playerID = Convert.ToInt32(reader.ReadLine());
                        int[] playerMoves = reader.ReadLine().Split(',').Select(int.Parse).ToArray();
                        return (board, turn, playerID, playerMoves);
                    }
                }
            }
            WriteLine("Saved file not found.");
            return (null, 0, 0, null);
        }
    }

    public static class Help
    {

        //Help command to assist users to know all commands provided
        public static void showHelp()
        {
            WriteLine("Commands:");
            WriteLine("MOVE - Play a turn");
            WriteLine("UNDO - Undo the last turn");
            WriteLine("REDO - Redo the last turn");
            WriteLine("SAVE - Saves the game state to file");
            WriteLine("LOAD - Loads the game state from file");
            WriteLine("EXIT - exits the game");
        }
    }

    //Applying Template Method Design pattern
    public abstract class GameController
    {
        protected GameState gameState = new GameState();
        protected Player player1;
        protected Player player2;
        protected CommandHandler commandHandler;


        /*
         * Abstract methods to be implemented by subclass : WTTTGameController
         */
        protected abstract void initializeGame();
        protected abstract void makePlayers(int playerOption);
        protected abstract bool endOfGame();
        protected abstract void printWinner();
        protected abstract void changeTurn();

        /*
         * Template method
         * */
        public void playGame(int playerOption)
        {
            initializeGame();
            //Creating 2 Human players if option is 1
            //Creating 1 Human player & 1 Computer player if option is 2
            makePlayers(playerOption);
            commandHandler = new CommandHandler(gameState);
            while (!endOfGame())
            {
                //Show user a detailed instruction and game board
                gameState.printBoard();
                Player currentPlayer = (gameState.currentPlayerID == 1) ? player1 : player2;
                bool moveMade = currentPlayer.giveCommand(gameState, commandHandler);
                //The turn only change when user makes a valid move
                if(moveMade)
                {
                    gameState.turn++; //increase the game turn
                    changeTurn();
                }
            }
            gameState.printBoard();
            //every time after a move, winning patterns need to be check and inform if any player wins
            printWinner();
        }

    }

    public class WTTTGameController : GameController // Changed to public
    {
        private int currentPlayerIndex = 0;
        protected override void initializeGame()
        {
            /*
             * The game must start with a new gameState 
             * */
            commandHandler = new CommandHandler(gameState);
            WriteLine("Tic Tac Toe game initialized");
        }

        protected override void makePlayers(int playerOption)
        {
            /*
             * If the user choose 1, the game creates 2 Human players
             * If the user choose 2, the game creates 1 Human and 1 Computer players
             * The game must start with player 1
             * */
            if (playerOption == 1)
            {
                //Players created with IDs and symbols
                player1 = new HumanPlayer(1, 'X');
                player2 = new HumanPlayer(2, 'O');
                gameState.currentPlayerID = player1.getID(); // in default, game always start with player1
                WriteLine("Starting Human vs Human Wild Tic-Tac-Toe game...");
            }
            else if (playerOption == 2)
            {
                //Players created with IDs and symbols
                player1 = new HumanPlayer(1, 'X');
                player2 = new ComputerPlayer(2, 'O');
                gameState.currentPlayerID = player1.getID(); // in default, game always start with player1
                WriteLine("Starting Human vs Computer Wild Tic-Tac-Toe game...");
            }
        }


        protected override void changeTurn()
        {
            currentPlayerIndex++;
            gameState.currentPlayerID = (currentPlayerIndex % 2 == 0) ? player1.getID() : player2.getID();
        }

        protected override bool endOfGame()
        {
            /*
             * The game is over when one player wins 
             * OR when the board is full
             */
            if(checkWinner(player1.getSymbol()) || checkWinner(player2.getSymbol()) || gameState.isBoardFull())
            {
                WriteLine("The game is over");
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void printWinner()
        {
            /* check which player wins
             * if the board is fully filled, no one wins
             */
            if (checkWinner(player1.getSymbol()))
            {
                WriteLine($"The game is over and player {player1.getID()} is the WINNER, well done");
            }
            else if (checkWinner(player2.getSymbol()))
            {
                WriteLine($"The game is over and player {player2.getID()} is the WINNER, well done");
            }
            else
            {
                WriteLine("The game is a draw!");
            }
        }

        private bool checkWinner(char symbol)
        {
            char[] board = gameState.getBoard();

            //Applying Wild Tic Tac Toe game rule to win 
            int[,] winPatterns = new int[,]
            {
                { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, // checking 3 rows to win
                { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, // checking 3 columns to win
                { 0, 4, 8 }, { 2, 4, 6 }              // checking 2 diagonals to wins
            };

            for (int i = 0; i < winPatterns.GetLength(0); i++)
            {
                if (board[winPatterns[i, 0]] == symbol &&
                    board[winPatterns[i, 1]] == symbol &&
                    board[winPatterns[i, 2]] == symbol)
                {
                    return true;
                }
            }
            return false;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {

            /*
             * 1. Show instructions to user to choose one mode of play
             * 2. Game will be started if user confirm by typing 'y'
             * */
            //declare some variables
            char gameStart = 'n';
            int gameOption = 0;
            /*only if user type any key except for 'y', the game will be restarted.
             * It means that when user types 'y', a Wild Tic-Tac-Toe game will be created with 2 players and a new GameState
             * */
            while (gameStart != 'y')
            {
                Clear();
                //Show the starting point of game
                WriteLine("Welcome to the board Game Tic Tac Toe \nPlease choose one option from the list provided to start the game");
                WriteLine("1. Human vs Human");
                WriteLine("2. Human vs Computer");
                WriteLine("Please enter number 1 or 2 only");


                bool validOption = false;
                while (!validOption)
                {
                    try
                    {
                        gameOption = Convert.ToInt32(ReadLine());
                        //checking if the user enter correct option number only 1 or 2
                        if (gameOption == 1 || gameOption == 2)
                        {
                            validOption = true;
                        }
                        else
                        {
                           WriteLine("Invalid option selected. Please choose one mode of play 1 or 2 ");
                        }
                    }
                    catch (FormatException)
                    {
                        WriteLine("Invalid option selected. Please choose one mode of play 1 or 2 ");
                    }
                }

                string gameChoice = (gameOption == 1) ? "Human vs Human" : "Human vs Computer";
                WriteLine($"You confirmed to select {gameChoice} \nPlease type y to start or any key to choose your option again");
                gameStart = ReadLine().ToLower()[0];
            }

            /* 
             * 1. Proceed to start the game by creating an object of WTTTGameController class
             * 2. the selected option of Human vs Human or Human vs Computer as a parameter of playGame function
             * */
            GameController WTTTboardGame = new WTTTGameController();
            WTTTboardGame.playGame(gameOption);
            ReadLine();
        }


    }
}
