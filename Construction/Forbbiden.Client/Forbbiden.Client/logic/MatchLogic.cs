using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forbbiden.Client.logic
{
    public static class MatchLogic
    {
        public static Random Rand { get; } = new Random();

        public static List<UserControlTile> GetAvatarsBeginningTiles(UserControlBoard board, int numberOfPlayers)
        {
            var boardTiles = board.GetAllTilesFromGrid();
            var beginningTiles = new List<UserControlTile>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                beginningTiles.Add(boardTiles[Rand.Next(0, boardTiles.Count)]);
            }

            return beginningTiles;
        }

        private static bool ValidateTileToMove(UserControlTile possibleTile)
        {
            HashSet<int> forbiddenCoordinates = new HashSet<int>() { 1, 4, 10, 15, 40, 45, 51, 54 };

            bool isValid = true;
            if (possibleTile == null)
            {
                isValid = false;
            }
            if (possibleTile.IsLost)
            {
                isValid = false;
            }
            int coordinate = (possibleTile.Row * 10) + possibleTile.Col;
            if (forbiddenCoordinates.Contains(coordinate))
            {
                isValid = false;
            }
            if (possibleTile.Row < 0 || possibleTile.Row > 5)
            {
                isValid = false;
            }
            if (possibleTile.Col < 0 || possibleTile.Col > 5)
            {
                isValid = false;
            }

            return isValid;
        }

        private static List<UserControlTile> CleanPreliminaryTiles(UserControlTile[] preliminaryTiles)
        {
            List<UserControlTile> possibleTiles = new List<UserControlTile>();

            foreach (var possibleTile in preliminaryTiles)
            {
                if (ValidateTileToMove(possibleTile))
                {
                    possibleTiles.Add(possibleTile);
                }
            }

            return possibleTiles;
        }

        public static List<UserControlTile> GetPossibleTilesToMove(UserControlTile tile, UserControlBoard board)
        {            
            var row = tile.Row;
            var col = tile.Col;

            var neighbours = new []
            {
                board.GetTile(row - 1, col),
                board.GetTile(row + 1, col),
                board.GetTile(row, col - 1),
                board.GetTile(row, col + 1)
            };

            var possibleTilesToMove = CleanPreliminaryTiles(neighbours);
            return possibleTilesToMove;
        }

        public static List<UserControlTile> GetPossibleTilesToShore(UserControlTile currentTile, UserControlBoard board)
        {
            var preliminaryTiles = GetPossibleTilesToMove(currentTile, board);
            preliminaryTiles.Add(currentTile);
            List<UserControlTile> possibleTiles = new List<UserControlTile>();

            foreach(var tile in preliminaryTiles)
            {
                if (tile.IsFlood)
                {
                    possibleTiles.Add(tile);
                }
            }

            return possibleTiles;
        }

        public static List<UserControlTile> GetPossibleTilesToMitigate(UserControlBoard board)
        {
            var preliminaryTiles = board.GetAllTilesFromGrid();
            List<UserControlTile> possibleTiles = new List<UserControlTile>();

            foreach(var tile in preliminaryTiles)
            {
                if (tile.IsFlood)
                {
                    possibleTiles.Add(tile);
                }
            }

            return possibleTiles;
        }

        public static void ResetTiles(List<UserControlTile> tiles)
        {
            foreach (var resetTile in tiles)
            {
                resetTile.DeactivateMovement();
                resetTile.ResetBorder();
            }
        }

        public static List<Card> ShuffleCards(List<Card> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = Rand.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }

            return cards;
        }
    }
}
