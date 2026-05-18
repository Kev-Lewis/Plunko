using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RunSaveData
{
    public bool hasActiveRun;

    public string seed;
    public int levelNumber;
    public int levelsCleared;
    public string currentBoardPattern;

    public int totalScore;
    public int ammoCount;
    public int previousScore;
    public int totalPegsToSave;
    public int totalLevelsToSave;
    public bool gameOver;

    public List<string> unlockedPegNames = new List<string>();
    public List<string> activeUpgradeIds = new List<string>();
    public List<BoardObjectSaveData> boardObjects = new List<BoardObjectSaveData>();

    public string savedAt;
}

[Serializable]
public class BoardObjectSaveData
{
    public string saveId;
    public string objectTag;
    public Vector3 position;
    public Vector3 rotation;
}