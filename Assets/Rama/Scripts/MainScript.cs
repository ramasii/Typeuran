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
    private float timeRemaining = 1; // jangan diubah jadi 0, nanti gamenya langsung tamat x_X
    private float givenTime = 1; // jangan diubah jadi 0, nanti gamenya langsung tamat x_X
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
    [SerializeField] private TextMeshProUGUI totalCustomerText;
    [SerializeField] private TextMeshProUGUI todayCustomerText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI todayHappyText;
    [SerializeField] private TextMeshProUGUI todayNeutralText;
    [SerializeField] private TextMeshProUGUI todayUpsetText;
    [SerializeField] private TextMeshProUGUI todayFailedOrderText;
    [SerializeField] private TextMeshProUGUI totalFailedOrderText;
    [Header("Game Summary")]
    private int todayScore = 0;
    private int totalScore = 0;
    private int totalDay = 0;
    private int totalCustomer = 0;
    private int todayCustomer = 0;
    private List<int> todayState = new List<int>();
    private int todayFailedOrder = 0;
    private int totalFailedOrder = 0;
    [SerializeField] private int hp = 3;
    [Header("Game Settings")]
    [SerializeField] private bool paused = false;
    [SerializeField] private bool gameEnd = false;
    [SerializeField] private float timePerChar = 0.3f;
    [SerializeField] private int scorePerChar = 1;
    [SerializeField] private float timeReductionPerDay = 0.1f; // Waktu per karakter berkurang setiap hari
    [SerializeField] private List<string> countList = new List<string>();
    [SerializeField] private List<string> menuList = new List<string>();
    [SerializeField] private List<string> modifierList = new List<string>();
    [SerializeField] private List<GameObject> foodPrefabs = new List<GameObject>();
    [SerializeField] private Transform CustomerSpawnPoint;
    int tempScore = 0; // skor sementara, jika order selesai, akan ditambahkan ke todayScore
    public string customerSpriteFolder;
    [Header("Customer Settings")]
    [SerializeField] private List<GameObject> customerPrefabs = new List<GameObject>();
    private int customerIndex = 0;
    [SerializeField] private bool customerAsking = false;
    [Range(0f, 1f)]
    public float happyThreshold = 0.75f; // Threshold untuk customer bahagia
    [Range(0f, 1f)]
    public float neutralThreshold = 0.25f;
    private GameObject currentCustomer;
    [SerializeField] private float customerSpawnDelay = 2f; // Delay sebelum spawn customer berikutnya
    [Header("Audio")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip inGameBGM;

    void Start()
    {
        AudioManager.Instance.PlayBGM(inGameBGM);
        // khusus debug dan testing
        ShuffleList(customerPrefabs); // Acak urutan customer
        NextCustomer(); // Spawn customer pertama

        currSentence = GenerateOrder(); // Ambil kalimat dari customer
        currSentcText.text = currSentence;
        inputText.text = ""; // Reset input text
        givenTime = currSentence.Length * timePerChar; 
        timeRemaining = givenTime; 
        sentcPanel.GetComponent<CanvasGroup>().alpha = 0f; // Sembunyikan panel kalimat

        todayState.Add(0); // Inisialisasi totalState dengan 0
        todayState.Add(0); // Inisialisasi totalState dengan 0
        todayState.Add(0); // Inisialisasi totalState dengan 0
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
        else // waktu habis maka kasih penalty atau bahkan gagal
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.failSFX);
            hp--; // Kurangi HP
            ShowSentencePanel(false); // Sembunyikan panel kalimat
            currCharIndex = 0; // Reset index karakter
            inputText.text = ""; // Kosongkan teks kalimat

            todayFailedOrder++; // Tambah jumlah order gagal hari ini

            if( hp > 0)
            {
                Debug.Log("Waktu habis! HP tersisa: " + hp);
                
                StartCoroutine(DelaySpawnCustomer()); // Spawn customer berikutnya setelah delay
            }
            else
            {
                GameEnd(false); // Panggil fungsi GameEnd dengan isWin = false
            }
        }
    }

    void UIUpdate()
    {
        totalScoreText.text = "Total Score: " + totalScore.ToString();
        todayScoreText.text = "Today's Score: " + todayScore.ToString();
        totalDayText.text = "Day: " + totalDay.ToString();

        timerImage.fillAmount = timeRemaining / givenTime; // Update fill amount dari timer

        totalCustomerText.text = "Total Customers: " + totalCustomer.ToString();
        todayCustomerText.text = "Today's Customers: " + todayCustomer.ToString() + " / " + customerPrefabs.Count.ToString();

        hpText.text = "HP: " + hp.ToString();

        todayHappyText.text = "Today Happy: " + todayState[0].ToString();
        todayNeutralText.text = "Today Neutral: " + todayState[1].ToString();
        todayUpsetText.text = "Today Upset: " + todayState[2].ToString();

        todayFailedOrderText.text = "Failed Orders: " + todayFailedOrder.ToString();
        totalFailedOrderText.text = "Total Failed Orders: " + totalFailedOrder.ToString();
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
                        if (inputChar[0] == ' ')
                        {
                            AudioManager.Instance.PlaySpacebarSFX();
                        }
                        else
                        {
                            AudioManager.Instance.PlayRandomKeyTypeSFX();
                        }
                        currCharIndex++;
                        tempScore += scorePerChar; // tambah skor

                        currSentcText.text = currSentence; 
                        inputText.text = currCharIndex == 0 ? $"<color=green>|</color><color=#C7C7C8>{currSentence}</color>" : $"<color=green>{currSentence.Substring(0, currCharIndex)}|</color>" + $"<color=#C7C7C8>{currSentence.Substring(currCharIndex)}</color>";

                        if (currCharIndex >= currSentence.Length)
                        {
                            AudioManager.Instance.PlaySFX(AudioManager.Instance.successSFX);
                            Debug.Log("Kalimat selesai: " + currSentence);

                            currCharIndex = 0; // Reset index karakter
                            inputText.text = ""; // Kosongkan teks kalimat
                            todayCustomer++; // Tambah customer hari ini
                            
                            ShowSentencePanel(false); // Sembunyikan panel kalimat

                            todayState[currentCustomer.GetComponent<CustomerBehaviour>().customerState]++; // Tambah total state sesuai dengan state customer

                            todayScore += tempScore; // Tambahkan skor sementara ke skor hari ini
                            tempScore = 0; // Reset skor sementara

                            //spawn objek makanan (testing)
                            Instantiate(foodPrefabs[0], currentCustomer.transform.position, Quaternion.identity, currentCustomer.transform); // Spawn makanan di posisi customer

                            // ini cara panggil nextCustomer dengan rapih
                            StartCoroutine(DelaySpawnCustomer()); // Spawn customer berikutnya setelah delay

                            // givenTime = timeRemaining + currSentence.Length * timePerChar;
                            // timeRemaining = givenTime;
                        }
                    }
                    else // ini untuk hurufnya salah
                    {
                        Debug.Log("huruf salah");
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

            givenTime = 1;
            timeRemaining = givenTime;

            currentCustomer = Instantiate(customerPrefabs[customerIndex], CustomerSpawnPoint.position, Quaternion.identity); // Spawn customer berikutnya
            StartCoroutine(PlayCustomerArrivesWithDelay(0.2f));
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
    private IEnumerator PlayCustomerArrivesWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance.PlayCustomerArrivesSFX();
    }

    IEnumerator DelayShowSentencePanel()
    {
        yield return new WaitForSeconds(2f);
        ShowSentencePanel(true); // atau false kalau ingin menyembunyikan
    }

    IEnumerator DelaySpawnCustomer(){
        currentCustomer.GetComponent<CustomerBehaviour>().GetOut(); // kick pada customer sebelumnya >:3
        yield return new WaitForSeconds(customerSpawnDelay);
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
            inputText.text = $"<color=green>|</color><color=#C7C7C8>{currSentence}</color>"; // Reset input text
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

        timePerChar -= timePerChar * timeReductionPerDay; // Kurangi waktu per karakter untuk meningkatkan kesulitan
        if(totalDay % 3 == 0) scorePerChar++;

        todayScore = 0; // Reset skor hari ini
        todayCustomer = 0; // Reset jumlah customer hari ini

        todayFailedOrder = 0; // Reset jumlah order gagal hari ini

        todayState[0] = 0; // Reset total state happy
        todayState[1] = 0; // Reset total state neutral
        todayState[2] = 0; // Reset total state upset

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

        totalScore += todayScore; // Tambah total skor meskipun kalah
        totalCustomer += todayCustomer; // Tambah total customer meskipun kalah
        totalFailedOrder += todayFailedOrder; // Tambah total order gagal

        // Implement game over logic here, such as showing a UI panel or restarting the game
        gameOverPanel.SetActive(true);
        AudioManager.Instance.PlayDayAlmostEndedSFX();
        if (isWin)
        {
            totalDay++; // Tambah hari

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

    public float GetTimeRemaining(bool percent = false)
    {
        return percent ? (timeRemaining / givenTime) : timeRemaining;
    }
}

public class CustomerBehaviour : MonoBehaviour
{
    [SerializeField] private Animator customerAnimator;
    [SerializeField] private SpriteRenderer customerSprite;
    private MainScript mainScript;
    private string customerName;
    public int customerState = 0; // 0: happy, 1: neutral, 2: upset

    void Awake()
    {
        customerAnimator = GetComponent<Animator>();
        customerSprite = GetComponent<SpriteRenderer>();
        mainScript = FindObjectOfType<MainScript>();
        customerName = gameObject.name; // Ambil nama customer dari GameObject
    }

    void Update()
    {
        if (mainScript == null) return; // Jika mainScript belum diinisialisasi, keluar dari update

        if (mainScript.GetTimeRemaining(true) > mainScript.happyThreshold)
        {
            customerState = 0; // Happy
            // customerSprite.sprite = GetCustomerImage("happy");
        }
        else if (mainScript.GetTimeRemaining(true) > mainScript.neutralThreshold)
        {
            customerState = 1; // Neutral
            customerSprite.sprite = GetCustomerImage("neutral");
        }
        else
        {
            customerState = 2; // Upset
            customerSprite.sprite = GetCustomerImage("upset");
        }
    }

    Sprite GetCustomerImage(string s)
    {
        customerName = customerName.Replace("(Clone)", ""); // hapus "(Clone)" dari nama customer jika ada
        // Ambil sprite customer berdasarkan nama
        Sprite sprite = Resources.Load<Sprite>(mainScript.customerSpriteFolder + "/" + customerName + "-" + s);

        if (sprite == null)
        {
            Debug.LogWarning($"Sprite {customerName} state '{s}' ga ada! Menggunakan sprite default. path: " + mainScript.customerSpriteFolder + "/" + customerName + "-" + s);
        }

        return sprite != null ? sprite : customerSprite.sprite; // Kembalikan sprite atau sprite default jika tidak ditemukan
    }

    public void GetOut()
    {
        if (mainScript.GetTimeRemaining(true) > mainScript.happyThreshold)
        {
            customerState = 0; // Happy
            customerSprite.sprite = GetCustomerImage("happy");
        }

        StartCoroutine(DelayGetOut()); // Delay sebelum customer keluar
    }

    IEnumerator DelayGetOut()
    {
        yield return new WaitForSeconds(1f); // Delay sebelum customer keluar
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