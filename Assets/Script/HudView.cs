using TMPro;
using UnityEngine;

namespace Script
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _movesText;
        
        public void UpdateHud(int score, int moves)
        {
            SetScore(score);
            SetMoves(moves);
        }
        
        private void SetScore(int score)
        {
            _scoreText.text = $"Score: {score}";
        }
        
        private void SetMoves(int moves)
        {
            _movesText.text = $"Moves: {moves}";
        }
    }
}