using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private bool isGameActive = false;
    private bool canPlayLoopingVideo = true;
    private bool preloopVideoFinished = false;
    private bool preloopVideoRequested = false;
    private bool waitingForClickToStart = false;
    private float splashScreenDelayTimeout = 0.5f;
    private float preloopStallTimeout = 10f;

    public bool isDebugging;

    public Button startGameButton;
    public Button quitButton;
    public Button returnButton;
    public Button replayButton;
    public GameObject titleScreen;
    public SpriteRenderer cheeseSprite;
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip eatingSound;
    public GetDistanceFromTarget getDistanceFromTarget;
    public GameObject preloopVideoCanvas;
    public GameObject loopingVideoCanvas;
    public GameObject preloopVideo;
    public GameObject loopingVideo;
    public UnityEngine.Video.VideoPlayer preloopVideoPlayer;
    public UnityEngine.Video.VideoPlayer loopingVideoPlayer;
    public GameObject blackScreen;


    AudioSource gameManagerAudio;

    void Start()
    {
        Cursor.visible = false;

        gameManagerAudio = GetComponent<AudioSource>();

        loopingVideoPlayer = loopingVideo.GetComponent<UnityEngine.Video.VideoPlayer>();
        preloopVideoPlayer = preloopVideo.GetComponent<UnityEngine.Video.VideoPlayer>();

        // WebGL can't play imported VideoClips, so the videos are loaded by URL from StreamingAssets
        SetVideoUrl(preloopVideoPlayer, "Pre-Loop.mp4");
        SetVideoUrl(loopingVideoPlayer, "Loopings.mp4");

        // URL videos take a moment to load, so wait for the real end instead of checking isPlaying
        preloopVideoPlayer.loopPointReached += source => preloopVideoFinished = true;
        preloopVideoPlayer.errorReceived += (source, message) => preloopVideoFinished = true;

        if (isDebugging)
            StartGame();
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Browsers block videos with sound until the player clicks, so wait for a click first
            Cursor.visible = true;
            waitingForClickToStart = true;
        }
        else
            PlayPreloopVideo();
    }

    private void Update()
    {
        if (waitingForClickToStart)
        {
            if (Input.anyKeyDown)
            {
                waitingForClickToStart = false;
                PlayPreloopVideo();
            }
            return;
        }

        if (!isGameActive)
        {
            // Skip to the menu if the intro video never manages to play, instead of staying on a black screen
            if (preloopVideoRequested && !preloopVideoFinished && !preloopVideoPlayer.isPlaying)
            {
                preloopStallTimeout -= Time.deltaTime;

                if (preloopStallTimeout < 0)
                    preloopVideoFinished = true;
            }

            splashScreenDelayTimeout -= Time.deltaTime;

            if (splashScreenDelayTimeout < 0)
                blackScreen.SetActive(false);

            if (preloopVideoFinished && splashScreenDelayTimeout < 0 && canPlayLoopingVideo)
            {
                Cursor.visible = true;

                gameManagerAudio.clip = menuMusic;
                gameManagerAudio.Play();
                //preloopVideoCanvas.SetActive(false);
                loopingVideoCanvas.SetActive(true);
                loopingVideoPlayer.Play();
                startGameButton.gameObject.SetActive(true);
                quitButton.gameObject.SetActive(true);
                canPlayLoopingVideo = false;
            }
        }
    }

    public void StartGame()
    {
        isGameActive = true;
        Cursor.visible = false;
        loopingVideoPlayer.Stop();
        titleScreen.SetActive(false);
        replayButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        cheeseSprite.enabled = true;
        getDistanceFromTarget.enabled = true;
    }

    public void GameOver()
    {
        gameManagerAudio.Stop();
        gameManagerAudio.PlayOneShot(eatingSound, 1.3f);
        Cursor.visible = true;
        replayButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(true);
        cheeseSprite.enabled = false;
        getDistanceFromTarget.enabled = false;
    }

    public void ReturnToTitleScreen()
    {
        returnButton.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
        titleScreen.SetActive(true);
        loopingVideoPlayer.Play();
    }

    public void PlayGameMusic()
    {
        gameManagerAudio.Stop();
        gameManagerAudio.loop = true;
        gameManagerAudio.clip = gameMusic;
        gameManagerAudio.volume = 0.75f;
        gameManagerAudio.Play();
    }

    public void PlayMenuMusic()
    {
        gameManagerAudio.Stop();
        gameManagerAudio.loop = true;
        gameManagerAudio.clip = menuMusic;
        gameManagerAudio.volume = 1.0f;
        gameManagerAudio.Play();
    }

    public void QuitTheGame()
    {
        // A browser page can't close itself, so reload the scene to get back to "Click to start"
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            Application.Quit();
    }

    private void OnGUI()
    {
        if (!waitingForClickToStart)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Screen.height / 12 };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Click to start", style);
    }

    private void PlayPreloopVideo()
    {
        Cursor.visible = false;
        preloopVideoPlayer.Play();
        preloopVideoRequested = true;
    }

    private void SetVideoUrl(UnityEngine.Video.VideoPlayer videoPlayer, string fileName)
    {
        videoPlayer.source = UnityEngine.Video.VideoSource.Url;
        videoPlayer.url = Application.streamingAssetsPath + "/" + fileName;
    }
}
