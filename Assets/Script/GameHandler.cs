using UnityEngine;
using DG.Tweening;
using Script;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private int boardWidth = 10;
    [SerializeField] private int boardHeight = 10;
    [SerializeField] private int MaxMoves = 30;
    [SerializeField] private BoardView boardView;
    [SerializeField] private HudView hudView;
    [SerializeField] private EndGamePopupController endGamePopupController;
    
    private GameController gameController;
    private int selectedX, selectedY = -1;
    private bool isAnimating;
    private int movesLeft;
    private int totalScore;
    
    private bool isSecondClick => selectedX > -1 && selectedY > -1;

    private void Awake()
    {
        gameController = new GameController();
        boardView.onTileClick += OnTileClick;
    }

    private void Start()
    {
        movesLeft = MaxMoves;
        hudView.UpdateHud(0, movesLeft);
        boardView.CreateBoard(gameController.StartGame(boardWidth, boardHeight));
    }

    private void OnTileClick(int x, int y)
    {
        if (isAnimating) return;

        if (!CanSwap(x, y))
        {
            selectedX = x;
            selectedY = y;
            return;
        }
        
        isAnimating = true;
        bool isValid = gameController.IsValidMovement(selectedX, selectedY, x, y);
        
        boardView.AnimatePlayerMove(selectedX, selectedY, x, y, isValid).OnComplete(() =>
        {
            if (isValid)
            {
                MoveResult swapResult = gameController.SwapTile(selectedX, selectedY, x, y);
                totalScore += swapResult.Score;
                boardView.AnimateBoard(swapResult.BoardResult, 0, OnAnimationEnds);
                hudView.UpdateHud(totalScore, --movesLeft);
                return;
            }
            
            OnAnimationEnds();
        });
    }

    private void OnAnimationEnds()
    {
        isAnimating = false;
        selectedX = -1;
        selectedY = -1;

        if (movesLeft <= 0)
        {
            endGamePopupController.Show(totalScore);
        }
    }

    private bool CanSwap(int x, int y)
    {
        return isSecondClick && Mathf.Abs(selectedX - x) + Mathf.Abs(selectedY - y) == 1;
    }
}
