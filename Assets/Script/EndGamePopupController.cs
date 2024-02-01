using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGamePopupController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _bestText;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private GameObject _newHighScoreTag;
    
    private const string BestScoreKey = "BestScore";
    
    private bool _isNewHighScore => _score > _bestScore;
    private int _bestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
    private int _score;
    
    private void Awake()
    {
        _playAgainButton.onClick.AddListener(PlayAgain);
        gameObject.SetActive(false);
    }

    public void Show(int score)
    {
        _score = score;
        SetScore(score);
        SetBest();
        gameObject.SetActive(true);
        _newHighScoreTag.SetActive(_isNewHighScore);

        if (_isNewHighScore) PlayerPrefs.SetInt(BestScoreKey, score);
    }
    
    private void SetScore(int score)
    {
        _scoreText.text = $"Score: {score}";
    }
    
    private void SetBest()
    {
        var best = _isNewHighScore ? _score : _bestScore;
        _bestText.text = $"Best: {best}";
    }
    
    private void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
