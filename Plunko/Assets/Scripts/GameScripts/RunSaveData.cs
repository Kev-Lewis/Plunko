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
    public List<PlanetSaveData> planets = new List<PlanetSaveData>();

    public List<PegWeightSaveData> pegWeightBonuses = new List<PegWeightSaveData>();

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

[Serializable]
public class PlanetSaveData
{
    public string saveId;
    public Vector3 position;
    public Vector3 scale;
    public float moveSpeed;
}

[Serializable]
public class PegWeightSaveData
{
    public string saveId;
    public int bonusWeight;
}