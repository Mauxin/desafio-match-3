using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [SerializeField] private TileSpotView tileSpotPrefab;
    [SerializeField] private TilePrefabRepository tilePrefabRepository;
    [SerializeField] private GridLayoutGroup boardContainer;

    private TileSpotView[][] _tileSpots;
    private TileView[][] _tiles;

    public event Action<int, int> onTileClick;

    public void CreateBoard(List<List<Tile>> board)
    {
        boardContainer.constraintCount = board[0].Count;

        _tileSpots = new TileSpotView[board.Count][];
        _tiles = new TileView[board.Count][];

        for (int y = 0; y < board.Count; y++)
        {
            _tileSpots[y] = new TileSpotView[board[0].Count];
            _tiles[y] = new TileView[board[0].Count];

            for (int x = 0; x < board[0].Count; x++)
            {
                TileSpotView tileSpot = Instantiate(tileSpotPrefab);
                tileSpot.transform.SetParent(boardContainer.transform, false);
                tileSpot.SetPosition(x, y);
                tileSpot.onClick += OnTileSpotClick;

                _tileSpots[y][x] = tileSpot;

                int tileTypeIndex = board[y][x].type;
                if (tileTypeIndex <= -1) continue;
                
                TileView tile = tilePrefabRepository.CreateTile(tileTypeIndex);
                tileSpot.SetTile(tile);

                _tiles[y][x] = tile;
            }
        }
    }
    
    public void AnimateBoard(List<BoardSequence> boardSequences, int i, Action onComplete)
    {
        BoardSequence boardSequence = boardSequences[i];
        DOTween.Sequence().Append(DestroyTiles(boardSequence.matchedPosition))
            .Append(MoveTiles(boardSequence.movedTiles))
            .Append(CreateTile(boardSequence.addedTiles))
            .OnComplete(() =>
            {
                i++;
                if (i < boardSequences.Count)
                {
                    AnimateBoard(boardSequences, i, onComplete);
                } 
                else
                {
                    onComplete();
                }
            });
    }

    public Tween AnimatePlayerMove(int fromX, int fromY, int toX, int toY, bool validPlay)
    {
        if (!validPlay)
        {
            return DOTween.Sequence()
                .Append(SwapTiles(fromX, fromY, toX, toY))
                .Append(SwapTiles(toX, toY, fromX, fromY));
        }

        return SwapTiles(fromX, fromY, toX, toY);
    }

    private Tween SwapTiles(int fromX, int fromY, int toX, int toY)
    {
        Sequence swapSequence = DOTween.Sequence()
            .Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX]))
            .Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX]));

        (_tiles[fromY][fromX], _tiles[toY][toX]) = (_tiles[toY][toX], _tiles[fromY][fromX]);

        return swapSequence;
    }

    private Tween DestroyTiles(List<Vector2Int> matchedPosition)
    {
        for (int i = 0; i < matchedPosition.Count; i++)
        {
            Vector2Int position = matchedPosition[i];
            Destroy(_tiles[position.y][position.x].gameObject);
            _tiles[position.y][position.x] = null;
        }
        return DOVirtual.DelayedCall(0.2f, () => { });
    }

    private Tween MoveTiles(List<MovedTileInfo> movedTiles)
    {
        TileView[][] tiles = new TileView[_tiles.Length][];
        for (int y = 0; y < _tiles.Length; y++)
        {
            tiles[y] = new TileView[_tiles[y].Length];
            for (int x = 0; x < _tiles[y].Length; x++)
            {
                tiles[y][x] = _tiles[y][x];
            }
        }

        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < movedTiles.Count; i++)
        {
            MovedTileInfo movedTileInfo = movedTiles[i];

            Vector2Int from = movedTileInfo.from;
            Vector2Int to = movedTileInfo.to;

            sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x]));

            tiles[to.y][to.x] = _tiles[from.y][from.x];
        }

        _tiles = tiles;
        return sequence;
    }

    private Tween CreateTile(List<AddedTileInfo> addedTiles)
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < addedTiles.Count; i++)
        {
            AddedTileInfo addedTileInfo = addedTiles[i];
            Vector2Int position = addedTileInfo.position;

            TileSpotView tileSpot = _tileSpots[position.y][position.x];

            TileView tilePrefab = tilePrefabRepository.tileTypePrefabList[addedTileInfo.type];
            TileView tile = Instantiate(tilePrefab);
            tileSpot.SetTile(tile);

            _tiles[position.y][position.x] = tile;

            tile.transform.localScale = Vector2.zero;
            seq.Join(tile.transform.DOScale(1.0f, 0.2f));
        }

        return seq;
    }

    private void OnTileSpotClick(int x, int y)
    {
        onTileClick(x, y);
    }
}