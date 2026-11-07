using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameModeUIManager : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset _gameModeUIAsset;

    // A reference to the component on this GameObject that handles displaying UXML files on the screen.
    UIDocument _UIDocumentComponent;

    void Awake()
    {
         // Gets a reference to the UIDocument component used for displaying UXML files.
        _UIDocumentComponent = GetComponent<UIDocument>();

        // Tells the UIDocument component to display the given gameUI.
        _UIDocumentComponent.visualTreeAsset = _gameModeUIAsset;

        // Links the HostGame button to the function that handles hosting the game.
        Button hostGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("HostGameButton");
        hostGameButton.RegisterCallback<ClickEvent>(HostGameButtonClicked);

        // Links the JoinGame button to the function that handles joining a game.
        Button joinGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("JoinGameButton");
        joinGameButton.RegisterCallback<ClickEvent>(JoinGameButtonClicked);
    }


    /// <summary>
    /// Implements behavior to start hosting a game when a relevant button is clicked.
    /// This function is subscribed to the button's click event in the Awake function.
    /// </summary>
    /// <param name="clickEvent">Contains information regarding the click event.</param>
    void HostGameButtonClicked(ClickEvent clickEvent)
    {
        // Gets a reference to the group of radio buttons that let the player indicate whether their hosted game should be local or LAN.
        // Determines whether LAN is used based on whether the value is 0 (first element, local) or 1 (second element, LAN).
        RadioButtonGroup localOrLANRadioButton = _UIDocumentComponent.rootVisualElement.Query<RadioButtonGroup>("LocalOrLanRadioButtonGroup");
        bool dontUseLAN = localOrLANRadioButton.value == 0;

        // Gets a reference to the integer input box that lets the player input what port they want the game server listening on.
        IntegerField hostPortIntegerField = _UIDocumentComponent.rootVisualElement.Query<IntegerField>("HostPortIntegerField");
        int hostPort = hostPortIntegerField.value;

        // Tells the lobby manager to start hosting (acting as a server and a client simultaneously) using the gathered information.
        LobbyManager.Instance.BeginHosting(dontUseLAN, (ushort)hostPort);

        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    /// <summary>
    /// Implements behavior to join an existing game session when a relevant button is clicked.
    /// This function is subscibed to the button's click event in the Awake function.
    /// </summary>
    /// <param name="clickEvent">Contains information regarding the click event.</param>
    void JoinGameButtonClicked(ClickEvent clickEvent)
    {
        // Gets a reference to the text box in which the user has input the host's IP address and stores its value.
        TextField hostIPTextField = _UIDocumentComponent.rootVisualElement.Query<TextField>("IPAddressTextField");
        string hostIPAddress = hostIPTextField.value;

        // Gets a reference to the integer input box indicating the port that the host's server is listening on and stores its value.
        IntegerField hostPortIntegerField = _UIDocumentComponent.rootVisualElement.Query<IntegerField>("JoinPortIntegerField");
        int hostPort = hostPortIntegerField.value;

        // Tells the lobby manager to connect to the host/server using the gathered information.
        LobbyManager.Instance.ConnectToHost(hostIPAddress, hostPort.ToString());
    }
}
