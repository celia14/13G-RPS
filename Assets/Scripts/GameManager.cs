using Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance { get; private set; }

    private UserSession session;
    public UserSession Session => session;

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Destroy this instance if another one already exists
            Destroy(gameObject);
            return;
        }

        // Keep this instance alive across scene loads
        DontDestroyOnLoad(gameObject);
    }

    public void HandleLogin(UserSession userSession)
    {
        session = userSession;

        // Proceed to a match
        StartMatch();
    }

    public void StartMatch()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void ShowTitle()
    {
        SceneManager.LoadScene("LoginScene", LoadSceneMode.Single);
    }
}
