using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DataGatherer : MonoBehaviour {
    private static int totalEpisodes = 0;
    private static int totalWins = 0;
    private static int totalSteps = 0;
    private static float totalRewards = 0f;
    private static float totalTime = 0f;


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



    private void Update() {
        _timeSinceLastClock += Time.deltaTime;
    }


    public void Clock( float rewardAchieved, int stepsTaken, int episodesCompleted ) {
        _episodesCompleted = episodesCompleted;
        ++totalEpisodes;

        float timeTaken = _timeSinceLastClock;
        _timeSinceLastClock = 0;

        if ( rewardAchieved > 0f ) {
            ++_wins;
            ++totalWins;
        }
        _rewards += rewardAchieved;
        totalRewards += rewardAchieved;
        _steps += stepsTaken;
        totalSteps += stepsTaken;
        _time += timeTaken;
        totalTime += timeTaken;

        SendAverages();
    }


    private void SendAverages() {
        _cumulativeEpisodeText.text = $"Total amount of episodes: {totalEpisodes}";
        _cumulativeWinText.text = $"average wins: {GetAverageWins()}";
        _cumulativeRewardText.text = $"average reward acquired: {GetAverageReward().ToString( "0.000" )}";
        _cumulativeStepText.text = $"average steps taken: {GetAverageSteps()}";
        _cumulativeTimeText.text = $"average time taken: {GetAverageTime().ToString( "0.00" )}";
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
