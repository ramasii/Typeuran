using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class MainScript : MonoBehaviour
{
    [Header("Word Input")]
    // [SerializeField] private List<string> sentenceList = new List<string>();
    private string currSentence;
    // [SerializeField] private int currSentcIndex = 0;
    private int currCharIndex = 0;
    [SerializeField] private TextMeshProUGUI currSentcText;
    [SerializeField] private TextMeshProUGUI inputText;
    private float timeRemaining;
    private float givenTime;
    [SerializeField] private Image timerImage;
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject WinImg;
    [SerializeField] private GameObject LoseImg;
    [SerializeField] private GameObject sentcPanel;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI todayScoreText;
    [SerializeField] private TextMeshProUGUI totalDayText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject nextDayButton;
    [Header("Game Summary")]
    private int todayScore = 0;
    private int totalScore = 0;
    private int totalDay = 0;
    [Header("Game Settings")]
    [SerializeField] private bool paused = false;
    [SerializeField] private bool gameEnd = false;
    [SerializeField] private float timePerChar = 0.3f;
    [SerializeField] private int scorePerChar = 1;
    [SerializeField] private List<string> countList = new List<string>();
    [SerializeField] private List<string> menuList = new List<string>();
    [SerializeField] private List<string> modifierList = new List<string>();
    [SerializeField] private Transform CustomerSpawnPoint;
    [Header("Customer Settings")]
    [SerializeField] private List<GameObject> customerPrefabs = new List<GameObject>();
    private int customerIndex = 0;
    [SerializeField] private bool customerAsking = false;
    private GameObject currentCustomer;

    void Start()
    {
        // khusus debug dan testing
        ShuffleList(customerPrefabs); // Acak urutan customer
        NextCustomer(); // Spawn customer pertama

        currSentence = GenerateOrder(); // Ambil kalimat dari customer
        currSentcText.text = currSentence;
        inputText.text = ""; // Reset input text
        givenTime = currSentence.Length * timePerChar; 
        timeRemaining = givenTime; 
        sentcPanel.GetComponent<CanvasGroup>().alpha = 0f; // Sembunyikan panel kalimat
        // akhir debug dan testing
    }

    void Update()
    {
        InputFromKeyboard();
        TimerUpdate();
        UIUpdate();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(); // Toggle pause state when Escape is pressed
        }
    }

    void TimerUpdate(){
        if (paused || customerAsking == false) return; // jika game sedang pause atau ga ada customer, tidak perlu update timer

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else // waktu habis maka gagal
        {
            GameEnd(false);
        }
    }

    void UIUpdate()
    {
        totalScoreText.text = "Total Score: " + totalScore.ToString();
        todayScoreText.text = "Today's Score: " + todayScore.ToString();
        totalDayText.text = "Day: " + totalDay.ToString();

        timerImage.fillAmount = timeRemaining / givenTime; // Update fill amount dari timer
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
                        // inputText.text += inputChar; // Tambahkan karakter yang benar
                        currCharIndex++;
                        todayScore += scorePerChar; // tambah skor

                        currSentcText.text = currSentence; 
                        inputText.text = currCharIndex == 0 ? "|" + $"<color=grey>{currSentence}</color>" : $"<color=green>{currSentence.Substring(0, currCharIndex)}</color>" + "|" + $"<color=grey>{currSentence.Substring(currCharIndex)}</color>";

                        if (currCharIndex >= currSentence.Length)
                        {
                            Debug.Log("Kata selesai: " + currSentence);

                            currCharIndex = 0; // Reset index karakter
                            inputText.text = ""; // Kosongkan teks kalimat
                            
                            // NextSentence(); // Pindah ke kalimat berikutnya
                            ShowSentencePanel(false); // Sembunyikan panel kalimat
                            currentCustomer.GetComponent<CustomerBehaviour>().GetOut(); // kick pada customer sebelumnya >:3
                            StartCoroutine(DelaySpawnCustomer()); // Spawn customer berikutnya setelah delay

                            // givenTime = timeRemaining + currSentence.Length * timePerChar;
                            // timeRemaining = givenTime;
                        }
                    }
                    else
                    {
                        Debug.Log("huruf salah, coba lagi.");
                    }
                }
            }
        }
    }

    void NextCustomer()
    {
        if (customerIndex < customerPrefabs.Count)
        {
            if(currentCustomer) Destroy(currentCustomer); // Hapus customer sebelumnya

            currentCustomer = Instantiate(customerPrefabs[customerIndex], CustomerSpawnPoint.position, Quaternion.identity); // Spawn customer berikutnya
            currentCustomer.AddComponent<CustomerBehaviour>().GetIn(); // Tambahkan komponen CustomerBehaviour dan masukkan (apanya? 🤨)

            customerIndex++;
            StartCoroutine(DelayShowSentencePanel());
        }
        else
        {
            GameEnd(); // Panggil fungsi untuk mengakhiri permainan
            Debug.Log("No more customers in the list.");
        }
    }

    IEnumerator DelayShowSentencePanel()
    {
        yield return new WaitForSeconds(2f);
        ShowSentencePanel(true); // atau false kalau ingin menyembunyikan
    }

    IEnumerator DelaySpawnCustomer(){
        yield return new WaitForSeconds(2f);
        NextCustomer(); // Spawn customer berikutnya
    }

    void ShowSentencePanel(bool show = true)
    {
        if (show)
        {
            customerAsking = true;

            sentcPanel.GetComponent<CanvasGroup>().alpha = 1f; // Tampilkan panel kalimat
            currSentence = GenerateOrder(); // Ambil kalimat dari customer berikutnya
            currSentcText.text = currSentence;
            currCharIndex = 0;
            inputText.text = "|" + $"<color=grey>{currSentence}</color>"; // Reset input text
            givenTime = timeRemaining + currSentence.Length * timePerChar;
            timeRemaining = givenTime;
        }
        else
        {
            customerAsking = false;
            sentcPanel.GetComponent<CanvasGroup>().alpha = 0f; // Sembunyikan panel kalimat
        }
    }

    string GenerateOrder()
    {
        string count = countList[Random.Range(0, countList.Count)];
        string menu = menuList[Random.Range(0, menuList.Count)];
        string modifier = modifierList[Random.Range(0, modifierList.Count)];
        bool extra = Random.Range(0, 100)%2 == 0; // Random true/false untuk apakah ada extra
        bool wio = Random.Range(0, 100)%2 == 0; // Random true/false untuk with/without
        string wioText = wio ? "with" : "without";
        string extraText = extra ? wioText + " " + modifier : " ";

        return $"{count} {menu} {extraText}".Trim(); // Trim untuk menghapus spasi berlebih
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void NextDay()
    {
        // Implementasi logika untuk melanjutkan ke hari berikutnya
        Debug.Log("Next Day!");
        gameOverPanel.SetActive(false); // Sembunyikan panel game over

        gameEnd = false;
        paused = false;

        todayScore = 0; // Reset skor hari ini
        customerIndex = 0; // Reset index customer
        currentCustomer.GetComponent<CustomerBehaviour>().GetOut(); // Kick customer terakhir
        Destroy(currentCustomer); // Hapus customer terakhir
        ShuffleList(customerPrefabs); // Acak ulang urutan customer
        StartCoroutine(DelaySpawnCustomer()); // Spawn customer pertama setelah delay
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
            totalDay++; // Tambah hari
            totalScore += todayScore; // Tambah total skor

            WinImg.SetActive(true);
            LoseImg.SetActive(false);
            nextDayButton.SetActive(true);
            restartButton.SetActive(false);
        }
        else
        {
            WinImg.SetActive(false);
            LoseImg.SetActive(true);
            nextDayButton.SetActive(false);
            restartButton.SetActive(true);
        }
    }

    public void PauseGame(){
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

public class CustomerBehaviour : MonoBehaviour
{
    [SerializeField] private Animator customerAnimator;

    void Awake()
    {
        customerAnimator = GetComponent<Animator>();
    }

    public void GetOut()
    {
        customerAnimator.SetTrigger("GetOut");
    }

    public void GetIn()
    {
        customerAnimator.SetTrigger("GetIn");
    }
}

[System.Serializable]
public class CustomerData{
    public string name;
    public Sprite customerImage;
    public CustomerData(string name, Sprite customerImage)
    {
        this.name = name;
        this.customerImage = customerImage;
    }
}