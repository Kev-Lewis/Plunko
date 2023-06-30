using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int highScore;
    public int totalPegs;
    public int totalLevels;
    public int highestProjScore;
    public int shotsFired;

    //tutorial bool
    public bool tutorialBool;

    //pegs destroyed bool
    /*public bool pegsDestroyed1;
    public bool pegsDestroyed2;
    public bool pegsDestroyed3;*/

    //shots fired bool
    /*public bool shotsDestroyed1;
    public bool shotsDestroyed2;
    public bool shotsDestroyed3;*/

    //levels cleared bool
    //public bool levelsCleared1;
    //public bool levelsCleared2;
    //public bool levelsCleared3;

    //proj score bool
    /*public bool projScore1;
    public bool projScore2;
    public bool projScore3;*/


    public GameData(){
        highScore = 0;
        totalPegs = 0;
        totalLevels = 0;
        highestProjScore = 0;
        shotsFired = 0;

        tutorialBool = false;

    }
}
