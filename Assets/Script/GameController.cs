using System.Collections.Generic;
using System.Linq;
using Script;
using UnityEngine;

public class GameController
{
    private List<List<Tile>> _boardTiles;
    private List<int> _tilesTypes;
    private int _tileCount;
    
    private int _boardWidth => _boardTiles[0].Count;
    private int _boardHeight => _boardTiles.Count;

    public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
    {
        _tilesTypes = new List<int> { 0, 1, 2, 3 };
        _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);
        return _boardTiles;
    }

    public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = SwappedBoard(fromX, fromY, toX, toY);

        //Swap in Y axis
        if (fromX == toX)
        {
            if (VerticalValidation(fromX, fromY, toY, newBoard)) return true;

            return HorizontalValidation(fromX, fromY, fromX, newBoard) || HorizontalValidation(toX, toY, toX, newBoard);
        }
        
        //Swap in X axis
        if (HorizontalValidation(fromX, fromY, toX, newBoard)) return true;

        return VerticalValidation(fromX, fromY, fromY, newBoard) || VerticalValidation(toX, toY, toY, newBoard);
    }
    
    public MoveResult SwapTile(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = SwappedBoard(fromX, fromY, toX, toY);
        List<BoardSequence> boardSequences = new List<BoardSequence>();
        List<List<bool>> matchedTiles;
        
        while (HasMatch(matchedTiles = FindMatches(newBoard)))
        {
            Dictionary<Vector2Int, int> matchedPosition = CleanMatchedPositions(newBoard, matchedTiles);
            List<Vector2Int> matchedPositionList = matchedPosition.Keys.ToList();

            BoardSequence sequence = new BoardSequence
            {
                matchedPosition = matchedPositionList,
                movedTiles = DropMatchTiles(matchedPositionList, newBoard),
                addedTiles = FillBoard(newBoard)
            };
            boardSequences.Add(sequence);
            
            if (matchedPosition.Count < 4) continue;

            foreach (int type in _tilesTypes)
            {
                var typeCount = matchedPosition.Values.Count(value => value == type);
                
                if (typeCount < 4) continue;

                switch (typeCount)
                {
                    case 4:
                        boardSequences.Add(ClearLineBonus(newBoard, matchedPosition.Where(pair => pair.Value == type).Select(pair => pair.Key).ToList()));
                        break;
                    case 5:
                        boardSequences.Add(ExplodeBonus(newBoard, matchedPosition.Where(pair => pair.Value == type).Select(pair => pair.Key).ToList()));
                        break;
                    case 7:
                        boardSequences.Add(ClearColorBonus(newBoard, matchedPosition.Where(pair => pair.Value == type).Select(pair => pair.Key).ToList()));
                        break;
                    default:
                        continue;
                }
            }
        }

        _boardTiles = newBoard;
        return new MoveResult(boardSequences, boardSequences.Count * 10);
    }
    
    private BoardSequence ClearLineBonus(List<List<Tile>> newBoard, List<Vector2Int> matchedPosition)
    {
        List<List<bool>> matchedTiles = CreateMatchBoard(newBoard);
        
        if (matchedPosition[0].y == matchedPosition[1].y)
        {
            for (int x = 0; x < _boardWidth; x++)
            {
                matchedTiles[matchedPosition[0].y][x] = true;
            }
        }
        else
        {
            for (int y = 0; y < _boardHeight; y++)
            {
                matchedTiles[y][matchedPosition[0].x] = true;
            }
        }

        Dictionary<Vector2Int, int> bonusPositions = CleanMatchedPositions(newBoard, matchedTiles);
        List<Vector2Int> bonusPositionsList = bonusPositions.Keys.ToList();

        return new BoardSequence
        {
            matchedPosition = bonusPositionsList,
            movedTiles = DropMatchTiles(bonusPositionsList, newBoard),
            addedTiles = FillBoard(newBoard)
        };
    }


    private BoardSequence ExplodeBonus(List<List<Tile>> newBoard, List<Vector2Int> matchedPosition)
    {
        List<List<bool>> matchedTiles = CreateMatchBoard(newBoard);

        foreach (Vector2Int position in matchedPosition)
        {
            for (int x = position.x - 1; x <= position.x + 1; x++)
            {
                for (int y = position.y - 1; y <= position.y + 1; y++)
                {
                    if (x >= 0 && x < _boardWidth && y >= 0 && y < _boardHeight)
                    {
                        matchedTiles[y][x] = true;
                    }
                }
            }
        }

        Dictionary<Vector2Int, int> bonusPositions = CleanMatchedPositions(newBoard, matchedTiles);
        List<Vector2Int> bonusPositionsList = bonusPositions.Keys.ToList();
        
        return new BoardSequence
        {
            matchedPosition = bonusPositionsList,
            movedTiles = DropMatchTiles(bonusPositionsList, newBoard),
            addedTiles = FillBoard(newBoard)
        };
    }


    private BoardSequence ClearColorBonus(List<List<Tile>> newBoard, List<Vector2Int> matchedPosition)
    {
        List<List<bool>> matchedTiles = CreateMatchBoard(newBoard);
        
        int matchedColor = newBoard[matchedPosition[0].x][matchedPosition[0].y].type;
        
        for (int x = 0; x < _boardWidth; x++)
        {
            for (int y = 0; y < _boardHeight; y++)
            {
                if (newBoard[y][x] != null && newBoard[y][x].type == matchedColor)
                {
                    matchedTiles[y][x] = true;
                }
            }
        }
        
        Dictionary<Vector2Int, int> bonusPositions = CleanMatchedPositions(newBoard, matchedTiles);
        List<Vector2Int> bonusPositionsList = bonusPositions.Keys.ToList();

        return new BoardSequence
        {
            matchedPosition = bonusPositionsList,
            movedTiles = DropMatchTiles(bonusPositionsList, newBoard),
            addedTiles = FillBoard(newBoard)
        };
    }

    private List<AddedTileInfo> FillBoard(List<List<Tile>> newBoard)
    {
        List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
        
        for (int y = newBoard.Count - 1; y > -1; y--)
        {
            for (int x = newBoard[y].Count - 1; x > -1; x--)
            {
                if (newBoard[y][x].type != -1) continue;
                
                int tileType = Random.Range(0, _tilesTypes.Count);
                Tile tile = newBoard[y][x];
                tile.id = _tileCount++;
                tile.type = _tilesTypes[tileType];
                
                addedTiles.Add(new AddedTileInfo
                {
                    position = new Vector2Int(x, y),
                    type = tile.type
                });
            }
        }
        
        return addedTiles;
    }

    private static List<MovedTileInfo> DropMatchTiles(List<Vector2Int> matchedPosition, List<List<Tile>> newBoard)
    {
        Dictionary<int, MovedTileInfo> movedTiles = new Dictionary<int, MovedTileInfo>();
        List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();
        
        foreach (var t in matchedPosition)
        {
            int x = t.x;
            int y = t.y;
            if (y <= 0) continue;
            
            for (int j = y; j > 0; j--)
            {
                Tile movedTile = newBoard[j - 1][x];
                newBoard[j][x] = movedTile;
                if (movedTile.type <= -1) continue;
                
                if (movedTiles.ContainsKey(movedTile.id))
                {
                    movedTiles[movedTile.id].to = new Vector2Int(x, j);
                }
                else
                {
                    MovedTileInfo movedTileInfo = new MovedTileInfo
                    {
                        from = new Vector2Int(x, j - 1),
                        to = new Vector2Int(x, j)
                    };
                    
                    movedTiles.Add(movedTile.id, movedTileInfo);
                    movedTilesList.Add(movedTileInfo);
                }
            }

            newBoard[0][x] = new Tile
            {
                id = -1,
                type = -1
            };
        }
        
        return movedTilesList;
    }

    private static Dictionary<Vector2Int, int> CleanMatchedPositions(List<List<Tile>> board, List<List<bool>> matchedTiles)
    {
        Dictionary<Vector2Int, int> matchedPositions = new Dictionary<Vector2Int, int>();
        
        for (int y = 0; y < board.Count; y++)
        {
            for (int x = 0; x < board[y].Count; x++)
            {
                if (!matchedTiles[y][x]) continue;
                
                matchedPositions.Add(new Vector2Int(x, y), board[y][x].type);
                board[y][x] = new Tile { id = -1, type = -1 };
            }
        }
        
        return matchedPositions;
    }

    private static bool HasMatch(List<List<bool>> list)
    {
        for (int y = 0; y < list.Count; y++)
            for (int x = 0; x < list[y].Count; x++)
                if (list[y][x])
                    return true;
        return false;
    }
    
    private static List<List<bool>> FindMatches(List<List<Tile>> newBoard)
    {
        List<List<bool>> matchedTiles = CreateMatchBoard(newBoard);

        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (x > 1
                    && newBoard[y][x].type == newBoard[y][x - 1].type
                    && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
                {
                    matchedTiles[y][x] = true;
                    matchedTiles[y][x - 1] = true;
                    matchedTiles[y][x - 2] = true;
                }
                
                if (y > 1
                    && newBoard[y][x].type == newBoard[y - 1][x].type
                    && newBoard[y - 1][x].type == newBoard[y - 2][x].type)
                {
                    matchedTiles[y][x] = true;
                    matchedTiles[y - 1][x] = true;
                    matchedTiles[y - 2][x] = true;
                }
            }
        }
        
        return matchedTiles;
    }

    private static List<List<bool>> CreateMatchBoard(List<List<Tile>> newBoard)
    {
        List<List<bool>> matchedTiles = new List<List<bool>>();
        
        for (int y = 0; y < newBoard.Count; y++)
        {
            matchedTiles.Add(new List<bool>(newBoard[y].Count));
            for (int x = 0; x < newBoard.Count; x++)
            {
                matchedTiles[y].Add(false);
            }
        }

        return matchedTiles;
    }

    private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
    {
        List<List<Tile>> board = new List<List<Tile>>(height);
        _tileCount = 0;
        for (int y = 0; y < height; y++)
        {
            board.Add(new List<Tile>(width));
            for (int x = 0; x < width; x++)
            {
                board[y].Add(new Tile { id = -1, type = -1 });
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                List<int> noMatchTypes = new List<int>(tileTypes.Count);
                for (int i = 0; i < tileTypes.Count; i++)
                {
                    noMatchTypes.Add(_tilesTypes[i]);
                }

                if (x > 1
                    && board[y][x - 1].type == board[y][x - 2].type)
                {
                    noMatchTypes.Remove(board[y][x - 1].type);
                }
                if (y > 1
                    && board[y - 1][x].type == board[y - 2][x].type)
                {
                    noMatchTypes.Remove(board[y - 1][x].type);
                }

                board[y][x].id = _tileCount++;
                board[y][x].type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
            }
        }

        return board;
    }
    
    private List<List<Tile>> SwappedBoard(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        (newBoard[fromY][fromX], newBoard[toY][toX]) = (newBoard[toY][toX], newBoard[fromY][fromX]);
        return newBoard;
    }
    
    private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
    {
        List<List<Tile>> newBoard = new List<List<Tile>>(boardToCopy.Count);
        for (int y = 0; y < boardToCopy.Count; y++)
        {
            newBoard.Add(new List<Tile>(boardToCopy[y].Count));
            for (int x = 0; x < boardToCopy[y].Count; x++)
            {
                Tile tile = boardToCopy[y][x];
                newBoard[y].Add(new Tile { id = tile.id, type = tile.type });
            }
        }

        return newBoard;
    }

    private static bool VerticalValidation(int fromX, int fromY, int toY, List<List<Tile>> board)
    {
        int boardLimit = board.Count - 1;
        int bottomY = Mathf.Clamp( Mathf.Min(fromY-2, toY-2), 0, boardLimit);
        int topY = Mathf.Clamp(Mathf.Max(fromY+2, toY+2), 0, boardLimit);

        int matches = 0;
        for (int i = bottomY; i < topY; i++)
        {
            matches = board[i][fromX].type == board[i + 1][fromX].type ? matches + 1 : 0;

            if (matches == 2) return true;
        }

        return false;
    }
    
    private static bool HorizontalValidation(int fromX, int fromY, int toX, List<List<Tile>> board)
    {
        int boardLimit = board[0].Count - 1;
        int bottomX = Mathf.Clamp(Mathf.Min(fromX-2, toX-2), 0, boardLimit);
        int topX = Mathf.Clamp(Mathf.Max(fromX+2, toX+2), 0, boardLimit);

        int matches = 0;
        for (int i = bottomX; i < topX; i++)
        {
            matches = board[fromY][i].type == board[fromY][i + 1].type ? matches + 1 : 0;

            if (matches == 2) return true;
        }

        return false;
    }
}
