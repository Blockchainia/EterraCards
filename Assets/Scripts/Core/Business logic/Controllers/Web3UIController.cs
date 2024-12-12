using System.Collections;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI;
using Assets.Scripts;

public class Web3ConnectionController : MonoBehaviour
{
  public static Web3ConnectionController Instance { get; private set; }

  [Header("UI Elements")]
  [SerializeField] private TextMeshProUGUI connectionStatusLabel; // TextMeshPro for connection status
  [SerializeField] private Button connectButton;                 // Button to initiate Web3 connection
  [SerializeField] private Web3NetworkManager web3NetworkManager;

  private bool isConnected = false;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
      return;
    }

    // Ensure UI elements are assigned
    if (connectionStatusLabel == null)
    {
      Debug.LogError("ConnectionStatusLabel is not assigned in the inspector.");
    }
    if (connectButton == null)
    {
      Debug.LogError("ConnectButton is not assigned in the inspector.");
    }

    // Add listener for the Connect button
    connectButton.onClick.AddListener(OnConnectButtonClicked);

    // Initialize UI
    UpdateConnectionStatus("Disconnected", Color.red);
  }

  private void OnDestroy()
  {
    // Remove listener to avoid memory leaks
    connectButton.onClick.RemoveListener(OnConnectButtonClicked);
  }

  /// <summary>
  /// Updates the connection status text and color.
  /// </summary>
  /// <param name="status">The status text to display.</param>
  /// <param name="color">The color of the status text.</param>
  private void UpdateConnectionStatus(string status, Color color)
  {
    if (connectionStatusLabel != null)
    {
      connectionStatusLabel.text = status;
      connectionStatusLabel.color = color;
    }

    Debug.Log($"Web3 Connection Status: {status}");
  }

  /// <summary>
  /// Handles the Connect button click.
  /// </summary>
  public void OnConnectButtonClicked()
  {
    if (!isConnected)
    {
      StartCoroutine(AttemptConnection());
    }
    else
    {
      Debug.Log("Already connected to Web3.");
    }
  }

  /// <summary>
  /// Attempts to connect to Web3 with proper error handling.
  /// </summary>
  private IEnumerator AttemptConnection()
  {
    Debug.Log("Attempting to connect to Web3...");

    // Update UI to indicate connecting state
    UpdateConnectionStatus("Connecting...", Color.yellow);

    // Simulate a brief delay to mimic connection time
    yield return new WaitForSeconds(1f);

    try
    {
      // Attempt to initialize the Web3 client
      web3NetworkManager.InitializeClient();

      // If no exception is thrown, mark as connected
      isConnected = true;
      UpdateConnectionStatus("Connected", Color.green);

      // Disable connect button after successful connection
      connectButton.interactable = false;

      Debug.Log("Successfully connected to Web3.");
    }
    catch (System.Exception ex)
    {
      // Log the error and update UI to show failure
      Debug.LogError($"Error connecting to Web3: {ex.Message}");
      UpdateConnectionStatus("Error Connecting", Color.red);
    }

    // Ensure the coroutine completes properly
    yield break;
  }



  /// <summary>
  /// Resets the connection status to disconnected.
  /// </summary>
  public void ResetConnection()
  {
    isConnected = false;
    UpdateConnectionStatus("Disconnected", Color.red);
    connectButton.interactable = true;
  }
}
