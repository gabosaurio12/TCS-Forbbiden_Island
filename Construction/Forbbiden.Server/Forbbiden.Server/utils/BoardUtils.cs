using Forbbiden.Server.logic;
using Forbbiden.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Server.Utils
{
    public class BoardUtils
    {
        public static List<Contracts.Tile> GetContractsTilesFromBoard(List<Model.Board> board, Forbidden_FEIEntities db)
        {
            var tilesIds = board.Select(b => b.tile_id).ToList();

            var modelTiles = db.Tile.Where(t => tilesIds.Contains(t.tile_id)).ToList();

            return modelTiles.Select(GetContractTile).ToList();
        }

        public static Contracts.Tile GetContractTile(Model.Tile modelTile)
        {
            return new Contracts.Tile()
            {
                TileId = modelTile.tile_id,
                Column = modelTile.col,
                Row = modelTile.row,
                IsFlood = modelTile.is_flood == 1,
                IsTreasure = modelTile.is_treasure == 1,
                IsEscape = modelTile.is_escape == 1,
                IsLost = modelTile.is_lost == 1,
                ImageFileName = modelTile.image_file_name,
                TreasureCard = modelTile.treasure_card_id.HasValue ?
                    GetCardById((int)modelTile.treasure_card_id) : null
            };
        }

        private static Contracts.Card GetCardById(int cardId)
        {
            return new BoardManager().GetCardById(cardId);
        }

        public static void AddTilesToDatabase(Forbidden_FEIEntities db, List<Model.Tile> modelTiles, List<Contracts.Tile> boardTiles)
        {
            foreach (var boardTile in boardTiles)
            {
                Model.Tile tile = GetModelTile(boardTile);
                modelTiles.Add(tile);
                db.Tile.Add(tile);
            }
        }

        public static Model.Tile GetModelTile(Contracts.Tile contractsTile)
        {
            return new Model.Tile()
            {
                col = contractsTile.Column,
                row = contractsTile.Row,
                is_treasure = contractsTile.IsTreasure ? 1 : 0,
                is_escape = contractsTile.IsEscape ? 1 : 0,
                is_flood = contractsTile.IsFlood ? 1 : 0,
                is_lost = contractsTile.IsLost ? 1 : 0,
                image_file_name = contractsTile.ImageFileName,
                treasure_card_id = contractsTile.TreasureCard?.CardId
            };
        }

        public static List<Contracts.Tile> AssignTilesIDs(List<Model.Tile> modelTiles, List<Contracts.Tile> boardTiles)
        {
            List<Contracts.Tile> contractTiles = boardTiles.ToList();

            for (int i = 0; i < modelTiles.Count; i++)
            {
                contractTiles[i].TileId = modelTiles[i].tile_id;
            }
            return contractTiles;
        }
    }
}
