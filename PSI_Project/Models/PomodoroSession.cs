﻿using System;

namespace PSI_Project.Models;

public class PomodoroSession
{
    public string UserEmail { get; private set; }
    public bool IsActive { get; private set; }
    private DateTime _startTime;
    private PomodoroIntensity _currentIntensity;
    private int _phaseCounter = 0; // Keeps track of completed study/break phases
    private string _currentMode; // Current mode: "Study", "Short Break", "Long Break"

    public PomodoroSession(string userEmail, string intensity)
    {
        UserEmail = userEmail;
        SetIntensity(intensity);
        IsActive = false;
        _currentMode = "Study"; // Start with Study mode
    }

    public void Start(string intensity)
    {
        if (!IsActive)
        {
            SetIntensity(intensity);
            _startTime = DateTime.Now;
            IsActive = true;
            _currentMode = "Study"; // Reset to Study mode when timer starts
            _phaseCounter = 0;
        }
    }

    public void Stop()
    {
        IsActive = false;
    }

    public (int RemainingTime, string Mode, bool IsActive) GetState()
    {
        if (!IsActive)
        {
            return (0, "Inactive", false);
        }

        UpdateMode();

        var elapsedTime = (int)(DateTime.Now - _startTime).TotalSeconds;
        int duration = GetDurationForCurrentMode();
        var remainingTime = Math.Max(0, duration * 60 - elapsedTime);

        return (remainingTime, _currentMode, IsActive);
    }

    private void SetIntensity(string intensity)
    {
        _currentIntensity = intensity switch
        {
            "Low" => new PomodoroIntensity("Study", 15, 3, 10),
            "Medium" => new PomodoroIntensity("Study", 25, 5, 15),
            "High" => new PomodoroIntensity("Study", 30, 5, 20),
            _ => throw new ArgumentException("Invalid intensity level.")
        };
    }

    private int GetDurationForCurrentMode()
    {
        return _currentMode switch
        {
            "Study" => _currentIntensity.StudyDuration,
            "Short Break" => _currentIntensity.ShortBreakDuration,
            "Long Break" => _currentIntensity.LongBreakDuration,
            _ => 0
        };
    }

    private void UpdateMode()
    {
        var elapsedTime = (int)(DateTime.Now - _startTime).TotalSeconds;
        int duration = GetDurationForCurrentMode() * 60;

        if (elapsedTime >= duration)
        {
            _phaseCounter++;
            _startTime = DateTime.Now; // Reset start time for the next phase

            if (_currentMode == "Study" && _phaseCounter % 3 != 0)
            {
                _currentMode = "Short Break";
            }
            else if (_currentMode == "Study" && _phaseCounter % 3 == 0)
            {
                _currentMode = "Long Break";
            }
            else
            {
                _currentMode = "Study";
            }
        }
    }
}
