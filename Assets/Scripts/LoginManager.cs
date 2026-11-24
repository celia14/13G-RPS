using System;
using Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;

    private Server.MockServer server;
    private GameManager gameManager;

    public event Action<UserSession> OnLogin;

    private void Awake()
    {
        server = Server.MockServer.Instance;
        gameManager = GameManager.Instance;

        loginButton.onClick.AddListener(OnLoginButtonClicked);
        usernameInput.onValueChanged.AddListener(OnInputFieldChanged);
        passwordInput.onValueChanged.AddListener(OnInputFieldChanged);
    }

    private void Start()
    {
        // Subscribe the singleton game manager to my login event
        OnLogin += gameManager.HandleLogin;
    }

    private void OnDestroy()
    {
        OnLogin -= gameManager.HandleLogin;
    }

    private void OnInputFieldChanged(string _arg0)
    {
        loginButton.interactable = !string.IsNullOrEmpty(usernameInput.text) && !string.IsNullOrEmpty(passwordInput.text);
    }

    private void OnLoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        
        try
        {
            UserSession session = server.Login(username, password);
            Debug.Log($"Login successful: {session.playerId}");

            OnLogin.Invoke(session);
        }
        catch (Exception ex)
        {
            // Here we can show an error message to the user,
            // but not implemented at the moment
            Debug.LogError($"Login failed: {ex.Message}");
        }
    }
}
