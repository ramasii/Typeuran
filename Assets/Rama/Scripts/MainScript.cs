using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    [Header("Word Input")]
    [SerializeField] private List<string> sentenceList = new List<string>();
    [SerializeField] private string currSentence;
    [SerializeField] private int currSentcIndex = 0;
    [SerializeField] private int currCharIndex = 0;
    [SerializeField] private TextMeshProUGUI currSentcText;
    [SerializeField] private TextMeshProUGUI inputText;
    [SerializeField] private float timeRemaining;
    [SerializeField] private float givenTime;
    [SerializeField] private Image timerImage;
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject WinImg;
    [SerializeField] private GameObject LoseImg;
    [Header("Game Settings")]
    [SerializeField] private bool paused = false;
    [SerializeField] private bool gameEnd = false;

    void Start()
    {
        // khusus debug dan testing
        currSentence = sentenceList[currSentcIndex];
        currSentcText.text = currSentence;
        inputText.text = ""; // Reset input text
        givenTime = currSentence.Length * 0.2f;
        timeRemaining = givenTime; 
        // akhir debug dan testing
    }

    void Update()
    {
        InputFromKeyboard();
        TimerUpdate();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(); // Toggle pause state when Escape is pressed
        }
    }

    void TimerUpdate(){
        if (paused) return; // jika game sedang pause, tidak perlu update timer

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timerImage.fillAmount = timeRemaining / givenTime;
        }
        else // waktu habis maka gagal
        {
            GameEnd(false);
        }
    }

    void InputFromKeyboard(){
        if (paused) return; // jika game sedang pause, tidak perlu input

        if(Input.anyKeyDown){
            string inputChar = new string(Input.inputString
                .Where(c => char.IsLetter(c) || c == ' ') // filter khusus huruf dan spasi
                .ToArray()) // gabung char menjadi string
                .ToLower(); // huruf insensitif 

            if (inputChar.Length > 0)
            {
                if (currCharIndex < currSentence.Length)
                {
                    if (inputChar[0] == currSentence[currCharIndex])
                    {
                        inputText.text += inputChar; // Tambahkan karakter yang benar
                        currCharIndex++;

                        // Update teks yang ditampilkan dengan warna hijau untuk karakter yang sudah benar
                        currSentcText.text = $"<color=green>{currSentence.Substring(0, currCharIndex)}</color>" + currSentence.Substring(currCharIndex);

                        if (currCharIndex >= currSentence.Length)
                        {
                            Debug.Log("Kata selesai: " + currSentence);
                            
                            NextWord(); // Pindah ke kata berikutnya

                            givenTime = timeRemaining + currSentence.Length * 0.2f;
                            timeRemaining = givenTime;
                        }
                    }
                    else
                    {
                        Debug.Log("Karakter salah, coba lagi.");
                    }
                }
            }
        }
    }

    void NextWord()
    {
        if (currSentcIndex < sentenceList.Count - 1)
        {
            currSentcIndex++;
            currSentence = sentenceList[currSentcIndex];
            currSentcText.text = currSentence;
            currCharIndex = 0;
            inputText.text = ""; // Reset input text
        }
        else
        {
            GameEnd(); // Panggil fungsi untuk mengakhiri permainan
            Debug.Log("No more words in the list.");
        }
    }

    void GameEnd(bool isWin = true)
    {
        Debug.Log("Game Over! You have completed all sentences.");
        paused = true; // Set game to paused state
        gameEnd = true; // Set game end state
        // Implement game over logic here, such as showing a UI panel or restarting the game
        gameOverPanel.SetActive(true);
        if (isWin)
        {
            WinImg.SetActive(true);
        }
        else
        {
            LoseImg.SetActive(true);
        }
    }

    void PauseGame(){
        if(gameEnd) return; // Jika game sudah berakhir, tidak perlu pause
        paused = !paused;
        if(paused)
        {
            pausePanel.SetActive(true); 
        }
        else
        {
            pausePanel.SetActive(false); 
        }
    }
}
