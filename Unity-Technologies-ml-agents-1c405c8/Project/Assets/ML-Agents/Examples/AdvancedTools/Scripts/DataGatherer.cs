using System;
using System.IO;
using UnityEngine;
using TMPro;


public class DataGatherer : MonoBehaviour {
    private static int totalEpisodes = 0;
    private static int totalWins = 0;
    private static int totalSteps = 0;
    private static float totalRewards = 0f;
    private static float totalTime = 0f;


    private const string LOG_NAME = "Log";


    [SerializeField]
    private TextMeshPro _cumulativeEpisodeText = null;

    [SerializeField]
    private TextMeshPro _cumulativeWinText = null;

    [SerializeField]
    private TextMeshPro _cumulativeRewardText = null;

    [SerializeField]
    private TextMeshPro _cumulativeStepText = null;

    [SerializeField]
    private TextMeshPro _cumulativeTimeText = null;


    private int _episodesCompleted = 0;
    private int _wins = 0;
    private int _steps = 0;
    private float _timeSinceLastClock = 0f;
    private float _rewards = 0f;
    private float _time = 0f;



    private void Start() {
        Calculate();
        SendAverages( true );
    }


    private void OnApplicationQuit() {
        Calculate();
        SendAverages( true );
    }


    private void Update() {
        _timeSinceLastClock += Time.deltaTime;
    }


    public void Clock( float rewardAchieved, int stepsTaken, int episodesCompleted ) {
        _episodesCompleted = episodesCompleted;
        ++totalEpisodes;

        Calculate( rewardAchieved, stepsTaken, episodesCompleted );
        bool write = totalEpisodes % 5000 == 0;
        SendAverages( write );
    }


    private void Calculate( float rewardAchieved = 0f, int stepsTaken = 0, int episodesCompleted = 0 ) {
        float timeTaken = _timeSinceLastClock;
        _timeSinceLastClock = 0;

        if ( rewardAchieved > Mathf.Epsilon ) {
            ++_wins;
            ++totalWins;
        }
        _rewards += rewardAchieved;
        totalRewards += rewardAchieved;
        _steps += stepsTaken;
        totalSteps += stepsTaken;
        _time += timeTaken;
        totalTime += timeTaken;
    }


    private void SendAverages( bool write ) {
        string cumulativeEpisode = $"total amount of episodes: {totalEpisodes}";
        string cumulativeWins = $"average wins: {GetAverageWins()}";
        string cumulativeReward = $"average reward acquired: {GetAverageReward().ToString( "0.000" )}";
        string cumulativeSteps = $"average steps taken: {GetAverageSteps()}";
        string cumulativeTimeTaken = $"average time taken: {GetAverageTime().ToString( "0.00" )}";

        _cumulativeEpisodeText.text = cumulativeEpisode;
        _cumulativeWinText.text = cumulativeWins;
        _cumulativeRewardText.text = cumulativeReward;
        _cumulativeStepText.text = cumulativeSteps;
        _cumulativeTimeText.text = cumulativeTimeTaken;

        // Write to disk
        if ( write ) {
            Write( cumulativeEpisode, cumulativeWins, cumulativeReward, cumulativeSteps, cumulativeTimeTaken );
        }
    }


    private void Write( string episode, string wins, string rewards, string steps, string time ) {
        //Path of the file
        string path = Application.persistentDataPath + $"/{LOG_NAME}.txt";

        // Combine text
        DateTime now = System.DateTime.Now;
        string content = $"{now.ToString()}:\r\n{episode}\r\n{wins}\r\n{rewards}\r\n{steps}\r\n{time}\r\n\r\n";

        // Write
        StreamWriter writer = new StreamWriter( path, true );
        writer.Write( content );
    }


    public float GetAverageWins() {
        return totalWins / ( float )totalEpisodes;
    }


    public float GetAverageReward() {
        return totalRewards / ( float )totalEpisodes;
    }


    public float GetAverageSteps() {
        return totalSteps / ( float )totalEpisodes;
    }


    public float GetAverageTime() {
        return totalTime / ( float )totalEpisodes;
    }


    public float GetAgentAverageWins() {
        return _wins / ( float )_episodesCompleted;
    }


    public float GetAgentAverageReward() {
        return _rewards / ( float )_episodesCompleted;
    }


    public float GetAgentAverageSteps() {
        return _steps / ( float )_episodesCompleted;
    }


    public float GetAgentAverageTime() {
        return _time / ( float )_episodesCompleted;
    }
}
