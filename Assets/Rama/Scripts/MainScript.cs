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
    private int currCharInSentcIndex = 0;
    [SerializeField] private TextMeshProUGUI currSentcText;
    [SerializeField] private TextMeshProUGUI inputText;
    private List<string> currWordList = new List<string>();
    private int currWordIndex = 0;

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
    [SerializeField] private Image clockImage;
    [SerializeField] private GameObject upgradePanel;
    [Header("Game Summary")]
    private int todayScore = 0;
    private int totalScore = 0;
    private int totalDay = 0;
    private int totalCustomer = 0;
    private int todayCustomer = 0;
    private List<int> todayState = new List<int>();
    private int todayFailedOrder = 0;
    private int totalFailedOrder = 0;
    public int hp = 3;
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
    [SerializeField] private float dayTime = 60f;
    [SerializeField] private float dayTimeReductionSpeed = 1f;
    [SerializeField] private int dayPerUpgrade = 3; // Setiap berapa hari upgrade bisa dilakukan
    public int maxHP = 3;
    private float tempDayTime;
    int tempScore = 0; // skor sementara, jika order selesai, akan ditambahkan ke todayScore
    public string customerSpriteFolder;
    private bool alreadySeeUpgradePanel = false; // untuk menghindari upgrade panel muncul terus menerus
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
    [Header("Player Upgrades")]
    public float additionalDayTime = 0f;
    public float additionalPatience = 0f;
    public int autoCorrectLevel = 0; // Level auto correct, semakin tinggi semakin banyak kesalahan yang bisa diperbaiki per kata
    public int maxAutoCorrectLevel = 2;
    public int autoCorrectAvailable = 0;
    public bool autoSpaceUpgrade = false;
    public bool reverseModeUpgrade = false;
    private string targetReversedWord = ""; // Untuk menyimpan kata yang sedang diketik
    private string targetWord = ""; // Untuk menyimpan kata target yang harus diketik
    private int currCharInWordIndex = 0; // Untuk menyimpan index karakter dalam kata yang sedang diketik
    private string typedReversedWord = ""; // Untuk menyimpan kata yang sudah diketik dalam reverse mode
    private int addReversedProgress = 0; // Untuk menyimpan jumlah karakter yang sudah ditambahkan dalam reverse mode
    [Header("Audio")]
    public AudioClip mainMenuBGM;
    public AudioClip inGameBGM;

    void Start()
    {
        AudioManager.Instance.PlayBGM(inGameBGM);
        // khusus debug dan testing
        tempDayTime = dayTime; // Simpan waktu hari ini untuk reset
        customerAsking = false;
        ShuffleList(customerPrefabs); // Acak urutan customer
        // NextCustomer(); // Spawn customer pertama
        StartCoroutine(DelaySpawnCustomer()); // Spawn customer berikutnya setelah delay

        currSentence = GenerateOrder(); // Ambil kalimat dari customer
        currSentcText.text = currSentence;
        currWordList = currSentence.Split(' ').ToList(); // Pisahkan kalimat menjadi kata-kata
        autoCorrectAvailable = autoCorrectLevel; // Set jumlah auto correct yang tersedia sesuai level
        // inputText.text = ""; // Reset input text
        givenTime = currSentence.Length * timePerChar;
        givenTime += givenTime * additionalPatience; // Tambahkan waktu ekstra berdasarkan tambahan kesabaran
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(); // Toggle pause state when Escape is pressed
        }
    }

    void TimerUpdate()
    {
        if (paused || customerAsking == false) return; // jika game sedang pause atau ga ada customer, tidak perlu update timer

        if (tempDayTime > 0)
        {
            tempDayTime -= Time.deltaTime * dayTimeReductionSpeed; // Kurangi waktu hari ini
        }

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else // waktu habis maka kasih penalty atau bahkan gagal
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.failSFX);
            hp--; // Kurangi HP
            ShowSentencePanel(false); // Sembunyikan panel kalimat
            currCharInSentcIndex = 0; // Reset index karakter
            inputText.text = ""; // Kosongkan teks kalimat

            todayFailedOrder++; // Tambah jumlah order gagal hari ini

            if (hp > 0)
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
        todayScoreText.text = "Score: " + todayScore.ToString();
        totalDayText.text = "Day: " + totalDay.ToString();

        timerImage.fillAmount = timeRemaining / givenTime; // Update fill amount dari timer

        totalCustomerText.text = "Total Customers: " + totalCustomer.ToString();
        todayCustomerText.text = "Today's Order: " + todayCustomer.ToString();

        hpText.text = "HP: " + hp.ToString();

        todayHappyText.text = "Happy: " + todayState[0].ToString();
        todayNeutralText.text = "Neutral: " + todayState[1].ToString();
        todayUpsetText.text = "Upset: " + todayState[2].ToString();

        todayFailedOrderText.text = "Failed Orders: " + todayFailedOrder.ToString();
        totalFailedOrderText.text = "Total Failed Orders: " + totalFailedOrder.ToString();

        clockImage.fillAmount = tempDayTime / dayTime; // Update fill amount dari clockImage
    }

    void InputFromKeyboard()
    {
        if (paused) return; // jika game sedang pause, tidak perlu input

        if (Input.anyKeyDown)
        {
            string inputChar = new string(Input.inputString
                .Where(c => char.IsLetter(c) || c == ' ') // filter khusus huruf dan spasi
                .ToArray()) // gabung char menjadi string
                .ToLower(); // huruf insensitif 

            if (inputChar.Length > 0)
            {
                if (currCharInSentcIndex < currSentence.Length)
                {
                    if (inputChar[0] == currSentence[currCharInSentcIndex])
                    {
                        if (inputChar[0] == ' ')
                        {
                            AudioManager.Instance.PlaySpacebarSFX();
                            autoCorrectAvailable = autoCorrectLevel; // Reset jumlah auto correct 
                            currWordIndex++; // Pindah ke kata berikutnya
                            addReversedProgress = 0; // Reset jumlah karakter yang sudah ditambahkan dalam reverse mode
                            currCharInWordIndex = 0; // Reset index karakter dalam kata yang sedang diketik
                            Debug.Log("current word " + currWordList[currWordIndex]);
                        }
                        else
                        {
                            AudioManager.Instance.PlayRandomKeyTypeSFX();
                        }

                        currCharInSentcIndex++;
                        addReversedProgress = addReversedProgress > 0 ? addReversedProgress - 1 : 0; ;
                        tempScore += scorePerChar; // tambah skor

                        if (inputChar[0] != ' ') currCharInWordIndex++; // Tambah index karakter dalam kata yang sedang diketik

                        ReverseMode(inputChar); // Cek reverse mode

                        if (currCharInSentcIndex < currSentence.Length)
                        { // jika masih ada karakter berikutnya
                            if (autoSpaceUpgrade && currSentence[currCharInSentcIndex] == ' ')
                            {
                                currCharInSentcIndex++; // jika auto space, langsung skip spasi
                                tempScore += scorePerChar; // tambah skor
                                autoCorrectAvailable = autoCorrectLevel; // Reset jumlah auto correct
                                currWordIndex++;
                                addReversedProgress = 0; // Reset jumlah karakter yang sudah ditambahkan dalam reverse mode
                                currCharInWordIndex = 0; // Reset index karakter dalam kata yang sedang diketik
                                Debug.Log("current word " + currWordList[currWordIndex]);
                            }
                        }

                        inputText.text = currCharInSentcIndex == 0 ? $"<color=green>|</color><color=#C7C7C8>{currSentence}</color>" : $"<color=green>{currSentence.Substring(0, currCharInSentcIndex)}|</color>" + $"<color=#C7C7C8>{currSentence.Substring(currCharInSentcIndex)}</color>";

                        if (currCharInSentcIndex >= currSentence.Length)
                        {
                            OrderCompleted();
                        }
                    }
                    else // ini untuk hurufnya salah, sekaligus pengecekan auto correct dan reverse mode
                    {
                        ReverseMode(inputChar); // Cek reverse mode

                        // jika auto correct tersedia dan karakter yang salah bukan spasi maka perbaiki
                        if (autoCorrectAvailable > 0 && currSentence[currCharInSentcIndex] != ' ')
                        {
                            autoCorrectAvailable--; // Kurangi jumlah auto correct yang tersedia
                            currCharInSentcIndex++; // Lewati karakter yang salah
                            currCharInWordIndex++; // Tambah index karakter dalam kata yang sedang diketik
                            addReversedProgress = addReversedProgress > 0 ? addReversedProgress - 1 : 0; // Kurangi jumlah karakter yang sudah ditambahkan dalam reverse mode
                            inputText.text = $"<color=green>{currSentence.Substring(0, currCharInSentcIndex)}|</color>" + $"<color=#C7C7C8>{currSentence.Substring(currCharInSentcIndex)}</color>";
                            // AudioManager.Instance.PlayAutoCorrectSFX(); // Mainkan efek suara auto correct
                            Debug.Log("Huruf salah, tapi auto correct berhasil memperbaiki!");
                        }
                        else
                        {
                            Debug.Log("huruf salah");
                        }
                    }
                }
            }
        }
    }

    private void OrderCompleted()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.successSFX);
        Debug.Log("Kalimat selesai: " + currSentence);

        currCharInSentcIndex = 0; // Reset index karakter
        inputText.text = ""; // Kosongkan teks kalimat
        todayCustomer++; // Tambah customer hari ini

        ShowSentencePanel(false); // Sembunyikan panel kalimat

        todayState[currentCustomer.GetComponent<CustomerBehaviour>().customerState]++; // Tambah total state sesuai dengan state customer

        todayScore += tempScore; // Tambahkan skor sementara ke skor hari ini
        tempScore = 0; // Reset skor sementara

        //spawn objek makanan (testing)
        Instantiate(foodPrefabs[Random.Range(0, foodPrefabs.Count)], currentCustomer.transform.position, Quaternion.identity, currentCustomer.transform); // Spawn makanan di posisi customer

        // ini cara panggil nextCustomer dengan rapih
        StartCoroutine(DelaySpawnCustomer()); // Spawn customer berikutnya setelah delay
    }

    void ReverseMode(string inputChar)
    {
        if (!reverseModeUpgrade) return;

        // Ambil kata saat ini jika belum ada target
        targetWord = currWordList[currWordIndex];
        targetReversedWord = Reverse(targetWord);

        // Cek apakah input sesuai dengan urutan reversed word
        if (typedReversedWord.Length < targetReversedWord.Length)
        {
            // Cek karakter per karakter
            if (inputChar[0] == targetReversedWord[typedReversedWord.Length])
            {
                typedReversedWord += inputChar[0];
                addReversedProgress++;

                Debug.Log($"[Reverse Mode] progress: {typedReversedWord} vs {targetReversedWord}");

                // Jika sudah selesai mengetik reversed word
                if (typedReversedWord.Length == targetReversedWord.Length)
                {
                    Debug.Log($"[Reverse Mode] SELESAI: {typedReversedWord} == {targetReversedWord}");
                    ResetPatience();

                    // Update progress dan tampilan
                    currCharInSentcIndex += addReversedProgress;
                    if (currCharInSentcIndex >= currSentence.Length)
                    {
                        OrderCompleted();
                    }

                    // Reset variabel
                    typedReversedWord = "";
                    targetWord = "";
                    targetReversedWord = "";
                    addReversedProgress = 0;
                    currCharInWordIndex = 0;

                    if (currCharInSentcIndex >= currSentence.Length) return;
                    inputText.text = currCharInSentcIndex == 0 ? $"<color=green>|</color><color=#C7C7C8>{currSentence}</color>" : $"<color=green>{currSentence.Substring(0, currCharInSentcIndex)}|</color>" + $"<color=#C7C7C8>{currSentence.Substring(currCharInSentcIndex)}</color>";
                    // inputText.text = $"<color=green>{currSentence.Substring(0, currCharInSentcIndex)}|</color>" + $"<color=#C7C7C8>{currSentence.Substring(currCharInSentcIndex)}</color>";
                }
            }
            else
            {
                // Reset jika salah ketik
                if (typedReversedWord.Length > 0)
                {
                    Debug.Log($"[Reverse Mode] GAGAL: {typedReversedWord} vs {targetReversedWord}");
                }
                typedReversedWord = "";
                addReversedProgress = 0;
            }
        }
    }

    public void ResetPatience()
    {
        timeRemaining = givenTime;
        Debug.Log("Patience reset! Time remaining: " + timeRemaining);
    }

    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }

    void NextCustomer()
    {

        if (customerIndex < customerPrefabs.Count)
        {
            if (currentCustomer) Destroy(currentCustomer); // Hapus customer sebelumnya
            if (tempDayTime <= 0) GameEnd();

            if (tempDayTime > 0)
            {
                givenTime = 1;
                timeRemaining = givenTime;

                currentCustomer = Instantiate(customerPrefabs[customerIndex], CustomerSpawnPoint.position, Quaternion.identity); // Spawn customer berikutnya
                StartCoroutine(PlayCustomerArrivesWithDelay(1f));
                currentCustomer.AddComponent<CustomerBehaviour>().GetIn(); // Tambahkan komponen CustomerBehaviour dan masukkan (apanya? 🤨)

                customerIndex++;
                StartCoroutine(DelayShowSentencePanel());
            }
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

    IEnumerator DelaySpawnCustomer()
    {
        if (currentCustomer) currentCustomer.GetComponent<CustomerBehaviour>().GetOut(); // kick pada customer sebelumnya >:3
        yield return new WaitForSeconds(customerSpawnDelay);
        NextCustomer(); // Spawn customer berikutnya
    }

    void ShowSentencePanel(bool show = true)
    {
        Transform bubble = sentcPanel.transform.Find("FadeInBubble");
        if (bubble != null)
        {
            bubble.gameObject.SetActive(show);
        }
        sentcPanel.GetComponent<CanvasGroup>().alpha = show ? 1f : 0f;

        if (show)
        {
            customerAsking = true;

            sentcPanel.GetComponent<CanvasGroup>().alpha = 1f; // Tampilkan panel kalimat
            currSentence = GenerateOrder(); // Ambil kalimat dari customer berikutnya
            currSentcText.text = currSentence;
            currWordList = currSentence.Split(' ').ToList(); // Pisahkan kalimat menjadi kata-kata
            currWordIndex = 0; // Reset index kata
            currCharInSentcIndex = 0;
            addReversedProgress = 0; // Reset jumlah karakter yang sudah ditambahkan dalam reverse mode
            inputText.text = $"<color=green>|</color><color=#C7C7C8>{currSentence}</color>"; // Reset input text
            givenTime = timeRemaining + currSentence.Length * timePerChar;
            givenTime += givenTime * additionalPatience; // Tambahkan waktu ekstra berdasarkan tambahan kesabaran
            timeRemaining = givenTime;
            typedReversedWord = ""; // Reset kata yang sudah diketik dalam reverse mode
            currCharInWordIndex = 0; // Reset index karakter dalam kata yang sedang diketik
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
        bool extra = Random.Range(0, 100) % 2 == 0; // Random true/false untuk apakah ada extra
        bool wio = Random.Range(0, 100) % 2 == 0; // Random true/false untuk with/without
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

    /// <summary>
    /// Melanjutkan ke hari berikutnya, reset semua variabel yang diperlukan.
    /// Jika hari ini adalah hari upgrade, tampilkan panel upgrade.
    /// Jika tidak, lanjutkan ke hari berikutnya.
    /// </summary>
    public void NextDay()
    {
        // Implementasi logika untuk melanjutkan ke hari berikutnya
        Debug.Log("Next Day!");
        gameOverPanel.SetActive(false); // Sembunyikan panel game over

        gameEnd = false;
        paused = false;

        if (totalDay % dayPerUpgrade == 0 && alreadySeeUpgradePanel == false)
        {// Jika hari ini adalah hari upgrade
            ShowUpgradePanel(true); // Tampilkan panel upgrade
            alreadySeeUpgradePanel = true; // Tandai bahwa panel upgrade sudah ditampilkan
            return;
        }

        alreadySeeUpgradePanel = false; // Reset tanda sudah melihat upgrade panel
        ShowUpgradePanel(false); // Sembunyikan panel upgrade

        timePerChar -= timePerChar * timeReductionPerDay; // Kurangi waktu per karakter untuk meningkatkan kesulitan
        dayTime -= dayTime * 0.1f; // Kurangi waktu hari ini untuk meningkatkan kesulitan
        dayTime += dayTime * additionalDayTime; // Tambahkan waktu ekstra berdasarkan tambahan waktu hari
        if (totalDay % 3 == 0) scorePerChar++;

        todayScore = 0; // Reset skor hari ini
        todayCustomer = 0; // Reset jumlah customer hari ini

        todayFailedOrder = 0; // Reset jumlah order gagal hari ini

        todayState[0] = 0; // Reset total state happy
        todayState[1] = 0; // Reset total state neutral
        todayState[2] = 0; // Reset total state upset

        tempDayTime = dayTime; // Reset waktu hari ini

        customerIndex = 0; // Reset index customer
        ShuffleList(customerPrefabs); // Acak ulang urutan customer
        StartCoroutine(DelaySpawnCustomer()); // Spawn customer pertama setelah delay
    }

    void ShowUpgradePanel(bool show = true)
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(show);
        }
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

    public void PauseGame()
    {
        if (gameEnd) return; // Jika game sudah berakhir, tidak perlu pause
        paused = !paused;
        if (paused)
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
public class UpgradeCardData
{
    public string title;
    public string description;
    public int cost;
    public Sprite icon;
}

public class UpgradeCardBehaviour : MonoBehaviour
{
    public UpgradeCardData upgradeData;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private Image iconImage;

    void Awake()
    {
        titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        descriptionText = transform.Find("Description").GetComponent<TextMeshProUGUI>();
        iconImage = transform.Find("Icon").GetComponent<Image>();
    }
    void Start()
    {
        if (upgradeData != null)
        {
            titleText.text = upgradeData.title;
            descriptionText.text = upgradeData.description;
            iconImage.sprite = upgradeData.icon;
        }
    }

    public virtual void Upgrade()
    {
        // Implement upgrade logic here
    }
}

public class HPUpgradeCard : UpgradeCardBehaviour
{
    public int healthIncrease;

    public override void Upgrade()
    {
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.hp += healthIncrease;
            if (mainScript.hp > mainScript.maxHP)
            {
                mainScript.hp = mainScript.maxHP;
            }
            Debug.Log($"HP increased by {healthIncrease}. New HP: {mainScript.hp}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}

public class MorePatienceUpgradeCard : UpgradeCardBehaviour
{
    public float patienceIncrease;

    public override void Upgrade()
    {
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.additionalPatience += patienceIncrease;
            Debug.Log($"Patience increased by {patienceIncrease}. New Patience: {mainScript.additionalPatience}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}

public class MoreDayTimeUpgradeCard : UpgradeCardBehaviour
{
    public float dayTimeIncrease;

    public override void Upgrade()
    {
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.additionalDayTime += dayTimeIncrease;
            Debug.Log($"Day Time increased by {dayTimeIncrease}. New Day Time: {mainScript.additionalDayTime}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}

public class HealUpgradeCard : UpgradeCardBehaviour
{
    public int healAmount;

    public override void Upgrade()
    {
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.hp += healAmount;
            if (mainScript.hp > mainScript.maxHP)
            {
                mainScript.hp = mainScript.maxHP;
            }
            Debug.Log($"Healed by {healAmount}. New HP: {mainScript.hp}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}

public class AutoCorrectUpgradeCard : UpgradeCardBehaviour
{
    public int autoCorrectIncrease;

    public override void Upgrade()
    {
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.autoCorrectLevel += autoCorrectIncrease;
            if (mainScript.autoCorrectLevel > mainScript.maxAutoCorrectLevel)
            {
                mainScript.autoCorrectLevel = mainScript.maxAutoCorrectLevel; // Batasi level auto correct
            }
            mainScript.autoCorrectAvailable = mainScript.autoCorrectLevel; // Set jumlah auto correct yang tersedia sesuai level

            Debug.Log($"Auto Correct Level increased by {autoCorrectIncrease}. New Auto Correct Level: {mainScript.autoCorrectLevel}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}

public class AutoSpaceUpgradeCard : UpgradeCardBehaviour
{
    public override void Upgrade()
    {
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.autoSpaceUpgrade = true; // Aktifkan auto space
            Debug.Log("Auto Space activated.");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}

public class ReverseModeUpgradeCard : UpgradeCardBehaviour
{
    public override void Upgrade()
    {
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.reverseModeUpgrade = true; // Aktifkan reverse mode
            Debug.Log("Reverse Mode activated.");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}

[System.Serializable]
public class CustomerData
{
    public string name;
    public Sprite customerImage;
    public CustomerData(string name, Sprite customerImage)
    {
        this.name = name;
        this.customerImage = customerImage;
    }
}