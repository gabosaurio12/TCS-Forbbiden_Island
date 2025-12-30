using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forbbiden.Client.logic
{
    public class MatchLogic
    {
        public static Random Rand = new Random();

        public static List<UserControlTile> GetPossibleTilesToMove(UserControlTile tile, UserControlBoard board)
        {
            int[] forbbidenCoordinates = { 01, 04, 10, 15, 40, 45, 51, 54 };
            var row = tile.Row;
            var col = tile.Col;

            List<UserControlTile> preliminaryTiles = new List<UserControlTile>
            {
                board.GetTile(row - 1, col),
                board.GetTile(row + 1, col),
                board.GetTile(row, col - 1),
                board.GetTile(row, col + 1)
            };

            List<UserControlTile> possibleTiles = new List<UserControlTile>();

            foreach (var possibleTile in preliminaryTiles)
            {
                if (!possibleTile.IsLost)
                {
                    int coordinate = (possibleTile.Row * 10) + possibleTile.Col;

                    if (!forbbidenCoordinates.Contains(coordinate) || coordinate > -1)
                    {
                        if (possibleTile.Row > -1 || possibleTile.Row < 6)
                        {
                            possibleTiles.Add(possibleTile);
                        }
                        else if (possibleTile.Col > -1 || possibleTile.Col < 6)
                        {
                            possibleTiles.Add(possibleTile);
                        }
                    }
                }                
            }

            return possibleTiles;
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
