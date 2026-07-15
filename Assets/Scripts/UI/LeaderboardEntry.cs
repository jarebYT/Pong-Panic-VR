using TMPro;
using UnityEngine;

public class LeaderboardEntry : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    public void SetData(int rank, string name, int score)
    {
        rankText.text = rank + ".";
        nameText.text = name;
        scoreText.text = score.ToString();
    }
}