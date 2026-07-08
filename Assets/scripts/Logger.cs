using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
/*
public class Logger : MonoBehaviour
{
    public static Logger Instance;

    [Header("Session Info")]
    public string participantID = "P01";
    public string conditionOrder = "A";
    public string deviceName = "Quest3";

    private List<string> _log = new List<string>();
    private float _trialStartTime;
    private int _trialIndex = 0;
    private string _currentCondition;
    private string _targetLetter;

    void Awake()
    {
        Instance = this;
        Log("timestamp,event,condition,trial,target,detail");
    }

    public void LogTrialStart(string condition, string target)
    {
        _trialStartTime = Time.time;
        _currentCondition = condition;
        _targetLetter = target;
        _trialIndex++;
        Log($"{Timestamp()},TRIAL_START,{condition},{_trialIndex},{target},");
    }

    public void LogTrialEnd(bool success)
    {
        float duration = Time.time - _trialStartTime;
        Log($"{Timestamp()},TRIAL_END,{_currentCondition},{_trialIndex},{_targetLetter},success={success},duration={duration:F2}");
    }

    public void LogGrasp(string letter, bool success)
    {
        Log($"{Timestamp()},GRASP,{_currentCondition},{_trialIndex},{_targetLetter},letter={letter},correct={success}");
    }

    public void LogBreakdown(BreakdownType type)
    {
        Log($"{Timestamp()},BREAKDOWN,{_currentCondition},{_trialIndex},{_targetLetter},type={type}");
    }

    public void LogPlacement(bool success)
    {
        Log($"{Timestamp()},PLACEMENT,{_currentCondition},{_trialIndex},{_targetLetter},success={success}");
    }

    void Log(string entry)
    {
        _log.Add(entry);
        Debug.Log(entry);
    }

    public void SaveLog()
    {
        string path = Path.Combine(Application.persistentDataPath,
            $"{participantID}_{conditionOrder}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllLines(path, _log);
        Debug.Log($"Log saved to {path}");
    }

    string Timestamp() => Time.time.ToString("F3");

    void OnApplicationQuit() => SaveLog();
}*/