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
    // The IP address/hostname of the web server hosting the api that handles leaderboard requests
    [SerializeField, Tooltip("The IP/hostname of the leaderboard api web server.")]
    string _leaderboardAddress;

    // The string which the course name should be appended to when trying to access a course on the leaderboard.
    [SerializeField, Tooltip("The course name will be appended to this string when making a leaderboard request.")]
    string _courseResultsAccessPrefix = "/leaderboard/";

    // Previously retrieved leaderboards will be stored here to avoid having to request it whenever the UI changes.
    // Course leaderboards are stored with "<courseName>.<holeNumber>" as the key, where holeNumber 0 is the general
    // course leaderboard, holeNumber 1 is the leaderboard for hole 1, etc.
    Dictionary<string, List<PlayerResult>> cachedLeaderboardRecords;

    // A request that returns a code with this value will be treated as a successful request.
    const long _requestSuccessCode = 200;

    /// <summary>
    /// Generates a list of each player's cumulative score records across all holes in a course.
    /// The players' score records per hole are collected by the GameMaster, but not the cumulative.
    /// That will be generated here.
    /// </summary>
    /// <param name="playerResults">A list of the results of each player in a course in each hole.</param>
    /// <returns>A list of player results containing the combined scoring fields across all holes.</returns>
    List<PlayerResult> GetCourseResultsFromHoleResults(List<List<PlayerResult>> playerResults)
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

    void GetCourseLeaderboard(string courseName, bool overwrite = false)
    {
        if (!overwrite && cachedLeaderboardRecords.ContainsKey(courseName))
            return;

        StartCoroutine(GetCourseRecordsFromLeaderboard(courseName));
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
            // This is asynchronous, so normal execution of the game will continue until a response is received, at
            // which point the rest of the function will be executed.
            yield return courseRecordsGETRequest.SendWebRequest();

            // Anything other than the successful response code indicates an error.
            if (courseRecordsGETRequest.responseCode != _requestSuccessCode)
                Debug.LogError(courseRecordsGETRequest.error);
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

                    cachedLeaderboardRecords[keyInCachedLeaderboardsDictionary] = resultsList;
                }
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
        // The supplied list of hole results doesn't include the cumulative course score, so need to make one.
        List<PlayerResult> courseResults = GetCourseResultsFromHoleResults(playerResults);
        string courseResultsPostRequestPayload = JsonUtility.ToJson(courseResults);

        // Can serialize the list of player hole results easily since PlayerResult only contains serializable values.
        List<string> holeResultsPostRequestPayloads = new List<string>(playerResults.Count);
        foreach (List<PlayerResult> holeResults in playerResults)
            holeResultsPostRequestPayloads.Add(JsonUtility.ToJson(holeResults));

        // This URI tells the web server to accept a json message with the additions to the leaderboards.
        Uri courseResultsURI = new Uri($"http://{_leaderboardAddress}{_courseResultsAccessPrefix}/{courseName}");

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
                    Debug.LogError(holeScoresPostRequest.error);
                else
                    Debug.Log($"Uploaded hole {holeIndex} results to the leaderboard successfully.");
            }
        }
    }
}
