using System.Collections.Generic;
using UnityEngine;

public class LocalGameData : MonoBehaviour
{
    public static LocalGameData Instance {private set; get;}

    [SerializeField]
    public List<Player> PlayerProfiles;

    public string GUID;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Player testPlayer1 = new Player();
        Player testPlayer2 = new Player();
        Player testPlayer3 = new Player();
        Player testPlayer4 = new Player();

        testPlayer1.PlayerName = "Test Player 1";
        testPlayer2.PlayerName = "Test Player 2";
        testPlayer3.PlayerName = "Test Player 3";
        testPlayer4.PlayerName = "Test Player 4";

        PlayerProfiles.Add(testPlayer1);
        PlayerProfiles.Add(testPlayer2);
        PlayerProfiles.Add(testPlayer3);
        PlayerProfiles.Add(testPlayer4);

        GUID = System.Guid.NewGuid().ToString();
    }
}
