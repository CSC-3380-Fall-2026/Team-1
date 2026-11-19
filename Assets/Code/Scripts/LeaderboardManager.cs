using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles operations involving data from the online leaderboard.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    // A LeaderboardManager singleton.
    // Makes it easier for the UI to work with this object.
    public static LeaderboardManager Instance {private set; get;}

    // This event is raised whenever this component begins a GET/POST request.
    // Those are asynchronous coroutines, so the UI and other interested parties need
    // to know when they start (e.g. to disable buttons until the request completes).
    public delegate void NetworkRequestStartedEvent();
    public NetworkRequestStartedEvent NetworkRequestStarted;

    // This event is raised whenever a GET/POST request made by this component concludes.
    // Those are asynchronous coroutines, so the UI and other interesed parties need to
    // know when they end (e.g. to enable relevant buttons or display request status).
    public delegate void NetworkRequestCompletedEvent(string requestStatusMessage);
    public NetworkRequestCompletedEvent NetworkRequestCompleted;

    // The IP address/hostname of the web server hosting the api that handles leaderboard requests
    [SerializeField, Tooltip("The IP/hostname of the leaderboard api web server.")]
    string _leaderboardAddress;

    // The string which the course name should be appended to when trying to access a course on the leaderboard.
    [SerializeField, Tooltip("The course name will be appended to this string when making a leaderboard request.")]
    string _courseResultsAccessPrefix = "/leaderboard/";

    // Previously retrieved leaderboards will be stored here to avoid having to request it whenever the UI changes.
    // Course leaderboards are stored with "<courseName>.<holeNumber>" as the key, where holeNumber 0 is the general
    // course leaderboard, holeNumber 1 is the leaderboard for hole 1, etc.
    public Dictionary<string, List<PlayerResult>> _cachedLeaderboardRecords = new Dictionary<string, List<PlayerResult>>();

    // A request that returns a code with this value will be treated as a successful request.
    const long _requestSuccessCode = 200;

    /// <summary>
    /// This function is called when the object is first instantiated, before any objects' Start() calls.
    /// Initializes singleton.
    /// </summary>
    void Awake()
    {
        // Singleton handling.
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    /// <summary>
    /// Generates a list of each player's cumulative score records across all holes in a course.
    /// The players' score records per hole are collected by the GameMaster, but not the cumulative.
    /// That will be generated here.
    /// </summary>
    /// <param name="playerResults">A list of the results of each player in a course in each hole.</param>
    /// <returns>A list of player results containing the combined scoring fields across all holes.</returns>
    public static List<PlayerResult> GetCourseResultsFromHoleResults(List<List<PlayerResult>> playerResults)
    {
        int playerCount = playerResults[0].Count;
        int holeCount = playerResults.Count;

        List<PlayerResult> courseResults = new List<PlayerResult>(playerCount);

        // Need to make a new PlayerResult that adds the score, hits and time spent across all the holes.
        for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
        {
            PlayerResult courseResult = new PlayerResult
            {
                PlayerName = playerResults[0][playerIndex].PlayerName,
                PlayerScore = 0,
                PlayerHits = 0,
                PlayerTimeSpent = 0
            };

            for (int holeIndex = 0; holeIndex < holeCount; holeIndex++)
            {
                courseResult.PlayerScore += playerResults[holeIndex][playerIndex].PlayerScore;
                courseResult.PlayerHits += playerResults[holeIndex][playerIndex].PlayerHits;
                courseResult.PlayerTimeSpent += playerResults[holeIndex][playerIndex].PlayerTimeSpent;
            }

            courseResults.Add(courseResult);
        }

        return courseResults;
    }

    /// <summary>
    /// Provides a way for other classes to instruct the LeaderboardMnager to get a given course's leaderboard.
    /// </summary>
    /// <param name="courseName">The name of the course whose leaderboard should be retrieved.</param>
    public void GetCourseLeaderboard(string courseName)
    {
        // Input validation.
        if (courseName == string.Empty || courseName == null)
        {
            Debug.LogError($"Supplied an invalid name (empty or null) when trying to get a course leaderboard.");
            return;
        }

        // This process runs asynchronously. Its state is reported via delegates.
        StartCoroutine(GetCourseRecordsFromLeaderboard(courseName));
    }

    /// <summary>
    /// Searches the leaderboard cache for a cumulative course/specific hole leaderboard and returns it.
    /// </summary>
    /// <param name="courseName">The name of the course related to the wanted leaderboard.</param>
    /// <param name="holeNumber">The hole number of the wanted leaderboard. 0 is for the cumulative course, 1+ for holes.</param>
    /// <returns>A list of the player results retrieved from the cache for the specified course/hole.</returns>
    public List<PlayerResult> GetCachedCourseLeaderboard(string courseName, int holeNumber)
    {
        // Input validation.
        // Rather than failing, an empty list is returned instead.
        if (courseName == string.Empty || courseName == null)
            return new List<PlayerResult>();

        // In the cache dictionary, the keys are formatted in a <courseName>.<holeNumber> format, where hole number 0 returns the
        // cumulative course stats (sum of stats across all holes in the course).
        string leaderboardKey = $"{courseName}.{holeNumber}";

        // Returns the list if found, empty list if not.
        if (_cachedLeaderboardRecords.ContainsKey(leaderboardKey))
            return _cachedLeaderboardRecords[leaderboardKey];
        else
            return new List<PlayerResult>();
    }

    /// <summary>
    /// Provides a way for other classes to instruct the LeaderboardManager to post given results to the leaderboard.
    /// </summary>
    /// <param name="playerResults">The list of player score stats for each hole.</param>
    /// <param name="courseName">The name of the course the results were achieved on.</param>
    public void SendResultsToLeaderboard(List<List<PlayerResult>> playerResults, string courseName)
    {
        StartCoroutine(PostResultsToLeaderboard(playerResults, courseName));
    }

    /// <summary>
    /// Retrieves the leaderboard of a course and all its holes from the database.
    /// The results are stored in the cache with the key being <courseName>.<holeNumber>, where holeNumber 0 is
    /// the player's cumulative stats across all the holes and and 1, 2, 3... is the player's stats for hole 1, 2, 3...
    /// </summary>
    /// <param name="courseName">The name of the course whose records will be retrieved.</param>
    /// <returns>An IEnumerator, as this is a coroutine. Null if there was an error.</returns>
    IEnumerator GetCourseRecordsFromLeaderboard(string courseName)
    {
        // Invalid input handling.
        if (courseName == string.Empty || courseName == null)
        {
            Debug.LogError("Tried to get the leaderboard, but supplied an empty courseName value. Should never happen.");
            yield return null;
        }

        // This URI tells the web server to send a json response with the course's leaderboards.
        Uri courseRecordsRequestURI = new Uri($"http://{_leaderboardAddress}{_courseResultsAccessPrefix}/{courseName}");

        using (UnityWebRequest courseRecordsGETRequest = UnityWebRequest.Get(courseRecordsRequestURI))
        {
            // Lets subscribed parties know that a network request is just about to begin.
            NetworkRequestStarted?.Invoke();

            // This is asynchronous, so normal execution of the game will continue until a response is received, at
            // which point the rest of the function will be executed.
            yield return courseRecordsGETRequest.SendWebRequest();

            // Anything other than the successful response code indicates an error.
            if (courseRecordsGETRequest.responseCode != _requestSuccessCode)
            {
                Debug.LogError(courseRecordsGETRequest.error);

                // Lets subscribed parties know that the GET request for the course's leaderboard failed.
                NetworkRequestCompleted?.Invoke($"Failed to retrieve {courseName}'s leaderboard.");
            }
            else
            {
                // The course's leaderboard will be sent as a JSON object in the following structure:
                //  The top-level elements indicate the leaderboard index, where 0 is the cumulative course leaderboard, 1, 2, 3... are holes 1, 2, 3...
                //  The value at each index is a list of the records in the leaderboard table (a list of key-value pairs).
                //  The 'key' in those pairs is the column of the record (name, score, hits, time-spent).
                //  The 'value' in those pairs is the value in the aformentioned columns.
                List<List<Dictionary<string, object>>> courseRecords = new List<List<Dictionary<string, object>>>();
                courseRecords = JsonUtility.FromJson<List<List<Dictionary<string, object>>>>(courseRecordsGETRequest.downloadHandler.text);

                // Need to translate the records' key-value pairs from raw objects into PlayerResult objects.
                for (int i = 0; i < courseRecords.Count; i++)
                {
                    // In the leaderboard cache dictionary, the key should be <courseName>.<holeNumber>, where holeNumber 0 is the cumulative course record.
                    string keyInCachedLeaderboardsDictionary = $"{courseName}.{i}";

                    List<PlayerResult> resultsList = new List<PlayerResult>(courseRecords[i].Count);
                    foreach (Dictionary<string, object> rawCourseRecord in courseRecords[i])
                        resultsList.Add(new PlayerResult
                        {
                            PlayerName = (string)rawCourseRecord["player_name"],
                            PlayerScore = (float)rawCourseRecord["score"],
                            PlayerHits = (int)rawCourseRecord["hits"],
                            PlayerTimeSpent = (float)rawCourseRecord["time_spent"]
                        });

                    _cachedLeaderboardRecords[keyInCachedLeaderboardsDictionary] = resultsList;
                }

                // Lets subscribed parties know that the GET request for the course's leaderboard succeeded.
                NetworkRequestCompleted?.Invoke($"Successfully retrieved {courseName}'s leaderboard.");
            }
        }
    }

    /// <summary>
    /// Uploads a new set of player results to the leaderboard database via the web server.
    /// Sends POST requests with the player result records as payloads.
    /// </summary>
    /// <param name="playerResults">A list of the results of each player in a course in each hole.</param>
    /// <param name="courseName">The name of the course whose leaderboard the results will be added to.</param>
    /// <returns>An IEnumerator, as this is a coroutine. Null if there was an error.</returns>
    IEnumerator PostResultsToLeaderboard(List<List<PlayerResult>> playerResults, string courseName)
    {
        // The first list in the supplied player results list is the cumulative course results.
        List<PlayerResult> courseResults = playerResults[0];
        string courseResultsPostRequestPayload = JsonUtility.ToJson(courseResults);

        // Can serialize the list of player hole results easily since PlayerResult only contains serializable values.
        List<string> holeResultsPostRequestPayloads = new List<string>(playerResults.Count - 1);

        // The first list of results is the cumulative course results list, so for hole-specific start at 1, not 0.
        for (int i = 1; i < playerResults.Count; i++)
            holeResultsPostRequestPayloads.Add(JsonUtility.ToJson(playerResults[i]));

        // This URI tells the web server to accept a json message with the additions to the leaderboards.
        Uri courseResultsURI = new Uri($"http://{_leaderboardAddress}{_courseResultsAccessPrefix}/{courseName}");

        // Lets subscribed parties know that a network request is just about to begin.
        NetworkRequestStarted?.Invoke();

        // This POST request will send the cumulative player results, not the per-hole ones.
        using (UnityWebRequest courseTotalScoresPostRequest = UnityWebRequest.Post(
            courseResultsURI,
            courseResultsPostRequestPayload,
            "application/json"))
        {
            // This is asynchronous, so normal execution of the game will continue until a response is received, at
            // which point the rest of the function will be executed.
            yield return courseTotalScoresPostRequest.SendWebRequest();

            if (courseTotalScoresPostRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(courseTotalScoresPostRequest.error);

                // Lets subscribed parties know that the POST request to the course's leaderboard failed.
                NetworkRequestCompleted?.Invoke($"Failed to upload cumulative results to {courseName}'s leaderboard.");

                yield return null;
            }
            else
                Debug.Log("Uploaded course total results to the leaderboard successfully.");
        }

        // Makes and sends POST requests for each hole's player results.
        for (int holeIndex = 0; holeIndex < holeResultsPostRequestPayloads.Count; holeIndex++)
        {
            Uri holeResultsURI = new Uri($"http://{_leaderboardAddress}{_courseResultsAccessPrefix}/{courseName}/{holeIndex}");

            using (UnityWebRequest holeScoresPostRequest = UnityWebRequest.Post(
                holeResultsURI,
                holeResultsPostRequestPayloads[holeIndex],
                "application/json"
            ))
            {
                // This is asynchronous, so normal execution of the game will continue until a response is received, at
                // which point the rest of the function will be executed.
                yield return holeScoresPostRequest.SendWebRequest();

                if (holeScoresPostRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(holeScoresPostRequest.error);

                    // Lets subscribed parties know that the POST request for the course's leaderboard failed.
                    NetworkRequestCompleted?.Invoke($"Failed to upload hole {holeIndex}'s results to {courseName}'s leaderboard.");

                    yield return null;
                }
                else
                    Debug.Log($"Uploaded hole {holeIndex} results to the leaderboard successfully.");
            }
        }

        // Lets subscribed parties know that the POST request for the course's leaderboard succeeded.
        NetworkRequestCompleted?.Invoke($"Successfully uploaded results to {courseName}'s leaderboard.");
    }
}
