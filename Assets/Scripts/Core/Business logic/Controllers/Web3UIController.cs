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

  private bool isConnected = false;

  private void Awake()
  {
  }

  private void OnDestroy()
  {
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
  /// Resets the connection status to disconnected.
  /// </summary>
  public void ResetConnection()
  {
    isConnected = false;
    UpdateConnectionStatus("Disconnected", Color.red);
    connectButton.interactable = true;
  }
}
