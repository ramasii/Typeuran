using UnityEngine;
using UnityEngine.SceneManagement;

public class UIScript : MonoBehaviour
{
    private GameObject startButton;
    private GameObject firedStamp;
    // Start is called before the first frame update
    void Start()
    {
        UIStarting();
    }

    private void UIStarting()
    {
        startButton = GameObject.Find("Start Button");

        firedStamp = null;
        Transform temp = transform.Find("GameEndPanel");
        if (temp != null){
            firedStamp = temp.Find("Fired Letter").transform.Find("FiredStamp").gameObject;
            Debug.Log("Fired Stamp found: " + (firedStamp != null));
        }
        if (startButton != null) startButton.AddComponent<StartButton>();
    }

    // Update is called once per frame
    void Update()
    {
        // update animasi firedStamp jika game sedang pause
        if(Time.timeScale == 0f && firedStamp != null){
            if(firedStamp.TryGetComponent<Animator>(out var a))
            {
                if(firedStamp.activeInHierarchy){
                    a.Update(Time.unscaledDeltaTime);
                }
            }
        }
    }

    public void GoToScene(string sceneName)
    {
        Time.timeScale = 1f; // Reset time scale before changing scene
        SceneManager.LoadScene(sceneName);
    }

    public void DragStartButton()
    {
        if (startButton != null)
        {
            StartButton buttonScript = startButton.GetComponent<StartButton>();
            if (buttonScript != null)
            {
                buttonScript.StartDrag();
            }
        }
    }

    public void EndDragStartButton()
    {
        if (startButton != null)
        {
            StartButton buttonScript = startButton.GetComponent<StartButton>();
            if (buttonScript != null)
            {
                buttonScript.EndDrag();
            }
        }
    }
}

public class StartButton : MonoBehaviour
{
    private float startingYPosition;
    private bool isDragging = false;
    private Transform rollingDoor;
    private float rollingDoorStartingYPosition;
    private GameObject judul;
    private CanvasGroup judulCanvasGroup;

    private void Start()
    {
        startingYPosition = transform.position.y;
        rollingDoor = GameObject.Find("Rolling Door").transform;
        rollingDoorStartingYPosition = rollingDoor.position.y;
        judul = GameObject.Find("Judul");
        if (judul != null) judulCanvasGroup = judul.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        Dragging();
    }

    private void Dragging()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (isDragging)
        {
            if (mousePosition.y > startingYPosition)
            {
                transform.position = new Vector3(transform.position.x, mousePosition.y, transform.position.z);
                rollingDoor.position = new Vector3(rollingDoor.position.x, mousePosition.y + (rollingDoorStartingYPosition - startingYPosition), rollingDoor.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, startingYPosition, transform.position.z);
                rollingDoor.position = new Vector3(rollingDoor.position.x, rollingDoorStartingYPosition, rollingDoor.position.z);
            }

            judulCanvasGroup.alpha = transform.position.y / startingYPosition;
        }
        else
        {
            if (transform.position.y >= 3.5f)
            {
                transform.position += new Vector3(0f, 5f, 0f) * Time.deltaTime * 1;
                rollingDoor.position += new Vector3(0f, 5f, 0f) * Time.deltaTime * 1;
                if (transform.position.y >= 7f)
                {
                    SceneManager.LoadScene("GameLevel");
                }
            }else{
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, startingYPosition, transform.position.z), Time.deltaTime * 5f);
                rollingDoor.position = Vector3.Lerp(rollingDoor.position, new Vector3(rollingDoor.position.x, rollingDoorStartingYPosition, rollingDoor.position.z), Time.deltaTime * 5f);
                judulCanvasGroup.alpha = Mathf.Lerp(judulCanvasGroup.alpha, 1f, Time.deltaTime * 5f);
            }
        }
    }

    public void StartDrag()
    {
        isDragging = true;
    }

    public void EndDrag()
    {
        isDragging = false;

    }
}
