using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawning : MonoBehaviour
{
    [SerializeField] private GameObject normalPeg, multiHitPeg, pyramidPeg, blackholePeg, ammoPlusPeg, ammoMinusPeg, sliderPeg, sliderPegShort;
    [SerializeField] private GameObject paddle1, paddle2, paddle3, paddle4, slideCornerLeft, slideCornerRight, x2Peg;
    [SerializeField] private GameObject arrowPegLeft, arrowPegUpLeft, arrowPegRight, arrowPegUpRight, twoHitPeg;
    [SerializeField] private int minRange, maxRange;
    [SerializeField] private Camera cam;
    private Vector3 cameraViewPort;
    private Shooter shoot;

    public int spawnCount;   //eventually change to not be static
    private int xGridSize = 8;
    private int yGridSize = 7;
    private bool[,] grid;
    private float zeroX = -7.5f;
    private float zeroY = 2f;
    private int[] slidePos;
    private GameObject[] usablePegs;
    private int amt_of_pegs = 8;
    private int which_side_to_spawn;
    private int levels_cleared;

    // Start is called before the first frame update
    void Start()
    {   
        cameraViewPort = cam.ViewportToWorldPoint(new Vector3(1,1,cam.nearClipPlane));
        //print(cameraViewPort);
        grid = new bool[xGridSize+1, yGridSize];
        slidePos = new int[xGridSize+1];
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
        // for spawning
        usablePegs = new GameObject[amt_of_pegs];
        resetUnlockedPegs();
        which_side_to_spawn = 1;
        levels_cleared = 0;
        resetList();
        SpawnObjects();
    }

    public int spawnSlide()
    {
        int initialPos = Random.Range(0, 2);
        int howToSpawn = Random.Range(0, 14);
        bool cornerSpawned = false;
        for (int i = 0; i < xGridSize; i++)
        {
            if (howToSpawn < 15)
            {
                if (initialPos <= yGridSize - 3)
                {
                    slidePos[i] = initialPos;
                    spawnSliderPegs(i, initialPos);
                    initialPos++;
                }
                else if (!cornerSpawned)
                {
                    slidePos[i] = initialPos;
                    spawnSliderPegsCorner(i, initialPos);
                    cornerSpawned = true;
                }
                else
                {
                    slidePos[i] = initialPos;
                    spawnSliderPegs(i, initialPos);
                }
            }
            else if (howToSpawn <= 45)
            {
                if (initialPos <= yGridSize - 3)
                {
                    slidePos[i] = initialPos;
                    spawnLeftSliderPegs(i, initialPos);
                    initialPos++;
                }
                else if (!cornerSpawned)
                {
                    slidePos[i] = initialPos;
                    spawnLeftSliderPegsCorner(i, initialPos);
                    cornerSpawned = true;
                }
                else
                {
                    slidePos[i] = initialPos;
                    spawnLeftSliderPegs(i, initialPos);
                }
            }
            else if (howToSpawn <= 75)
            {
                if (initialPos <= yGridSize - 3)
                {
                    slidePos[i] = initialPos;
                    spawnRightSliderPegs(i, initialPos);
                    initialPos++;
                }
                else if (!cornerSpawned)
                {
                    slidePos[i] = initialPos;
                    spawnRightSliderPegsCorner(i, initialPos);
                    cornerSpawned = true;
                }
                else
                {
                    slidePos[i] = initialPos;
                    spawnRightSliderPegs(i, initialPos);
                }
            }
            else
            {
                return initialPos;
            }
        }
        return initialPos;
    }

    public void SpawnHard()
    {
        int range = spawnSlide();
        int count = Random.Range(minRange, maxRange);
        int amtSpawned = 0;

        int whichPaddles = Random.Range(0, 100);
        if (whichPaddles <= 15)
        {
            paddle1.SetActive(true);
        }
        else if (whichPaddles <= 30)
        {
            paddle2.SetActive(true);
        }
        else if (whichPaddles <= 65)
        {
            paddle3.SetActive(true);
            paddle4.SetActive(true);
        }

        while (amtSpawned < count)
        {
            int xPos = Random.Range(0, xGridSize);
            int yPos = Random.Range(0, yGridSize - 1);
            if (grid[xPos, yPos] == false && yPos < slidePos[xPos])
            {
                grid[xPos, yPos] = true;
                spawnPegs(xPos, yPos);
                amtSpawned += 2;
            }
        }
    }

    public void SpawnObjects()
    {
        int which_style = Random.Range(0, 200);
        //int which_style = 160;
        if (which_style <= 50)
        {
            SpawnHard();
        }
        else if (which_style <= 75)
        {
            SpawnDownwardsArc();
        }
        else if (which_style <= 100)
        {
            SpawnWave();
        }
        else if (which_style <= 125)
        {
            SpawnBucket();
        }
        else if (which_style <= 150)
        {
            SpawnSpiral();
        }
        else if (which_style <= 175)
        {
            SpawnTriangle();
        }
        else
        {
            SpawnUpwardsArc();
        }  
    }

    public void SpawnDownwardsArc()
    {
        // paddles
        int whichPaddles = Random.Range(0, 100);
        if (whichPaddles <= 15)
        {
            paddle1.SetActive(true);
        }
        else if (whichPaddles <= 30)
        {
            paddle2.SetActive(true);
        }
        else if (whichPaddles <= 65)
        {
            paddle3.SetActive(true);
            paddle4.SetActive(true);
        }

        int startingX = Random.Range(0, 2);
        int startingY = Random.Range(0, 1);
        int which_style = Random.Range(0, 50);
        if (which_style < 25)
        {
            while (startingX < xGridSize)
            {
                int amt_spawned = 0;
                while (amt_spawned < 3)
                {
                    spawnPegs(startingX, startingY + amt_spawned);
                    amt_spawned++;
                }
                if (startingY < yGridSize-4)
                {
                    startingY++;
                }
                startingX++;
            }
        }
        else
        {
            int alternate = 0;
            while (startingX < xGridSize)
            {
                int amt_spawned = 0;
                while (amt_spawned < 3)
                {
                    spawnPegs(startingX, startingY + amt_spawned);
                    amt_spawned++;
                }
                if (startingY < yGridSize - 4 && alternate == 1)
                {
                    alternate--;
                    startingY++;
                }
                else
                {
                    alternate++;
                }
                startingX++;
            }
        }
    }

    public void SpawnUpwardsArc()
    {
        // paddles
        int whichPaddles = Random.Range(0, 100);
        if (whichPaddles <= 15)
        {
            paddle1.SetActive(true);
        }
        else if (whichPaddles <= 30)
        {
            paddle2.SetActive(true);
        }
        else if (whichPaddles <= 65)
        {
            paddle3.SetActive(true);
            paddle4.SetActive(true);
        }

        int startingX = Random.Range(0, 1);
        int startingY = Random.Range(5, 6);
        int which_style = Random.Range(0, 50);
        if (which_style < 25)
        {
            while (startingX < xGridSize)
            {
                int amt_spawned = 0;
                while (amt_spawned < 3)
                {
                    spawnPegs(startingX, startingY - amt_spawned);
                    amt_spawned++;
                }
                if (startingY > 2)
                {
                    startingY--;
                }
                startingX++;
            }
        }
        else
        {
            int alternate = 0;
            while (startingX < xGridSize)
            {
                int amt_spawned = 0;
                while (amt_spawned < 3)
                {
                    spawnPegs(startingX, startingY - amt_spawned);
                    amt_spawned++;
                }
                if (startingY > 0 && alternate == 1)
                {
                    alternate--;
                    startingY--;
                }
                else
                {
                    alternate++;
                }
                startingX++;
            }
        }
    }

    public void SpawnTriangle()
    {
        // paddles
        int whichPaddles = Random.Range(0, 100);
        if (whichPaddles <= 15)
        {
            paddle1.SetActive(true);
        }
        else if (whichPaddles <= 30)
        {
            paddle2.SetActive(true);
        }
        else if (whichPaddles <= 65)
        {
            paddle3.SetActive(true);
            paddle4.SetActive(true);
        }

        int startingX = Random.Range(2, 4);
        int startingY = Random.Range(4, 5);
        int level = 1;
        int prevY = 1;
        int currentY = 1;
        spawnSliderPegs(startingX - 1, startingY + 1);
        while (startingX < xGridSize)
        {
            int amt_spawned = 0;
            while (amt_spawned < level)
            {
                spawnPegs(startingX, startingY - amt_spawned);
                amt_spawned++;
            }
            currentY = startingY + level;
            if (currentY != prevY)
            {
                spawnSliderTrianglePegs(startingX - 1, startingY - level);
            }
            spawnSliderPegs(startingX, startingY + 1);
            if (level <= startingY)
            {
                level++;
            }
            startingX++;
            prevY = currentY;
        }
    }

    public void SpawnBucket()
    {
        // paddles
        int whichPaddles = Random.Range(0, 100);
        if (whichPaddles <= 15)
        {
            paddle1.SetActive(true);
        }
        else if (whichPaddles <= 30)
        {
            paddle2.SetActive(true);
        }
        else if (whichPaddles <= 65)
        {
            paddle3.SetActive(true);
            paddle4.SetActive(true);
        }

        int startingX = Random.Range(1, 2);
        int startingY = Random.Range(0, 1);
        int initialY = yGridSize - 2;
        int first_two = 0;
        int level = 0;
        for (int i = startingY; i < initialY; i++)
        {
            spawnSlideBucketPegs(startingX - 1, startingY + i - 0.5f);
            if (startingY + i < initialY - 2)
            {
                spawnSlideBucketPegs(startingX + 2, startingY + i - 0.5f);
            }
        }
        while (startingX < xGridSize)
        {
            int amt_spawned = 0;
            if (first_two < 2)
            {
                level = yGridSize - 2;
            }
            else
            {
                startingY = yGridSize - 3;
                level = 2;
            }
            while (amt_spawned < level)
            {
                if (first_two < 2)
                {
                    spawnPegs(startingX, startingY + amt_spawned);
                    amt_spawned++;
                }
                else
                {
                    spawnPegs(startingX, startingY - amt_spawned);
                    amt_spawned++;
                }
            }
            spawnSliderPegs(startingX, initialY);
            if (first_two > 2)
            {
                spawnInsideSlideBucketPegs(startingX, initialY - 3.25f);
            }
            first_two++;
            startingX++;
        }
    }

    public void SpawnSpiral()
    {
        // paddles
        int whichPaddles = Random.Range(0, 100);
        if (whichPaddles <= 15)
        {
            paddle1.SetActive(true);
        }
        else if (whichPaddles <= 30)
        {
            paddle2.SetActive(true);
        }
        else if (whichPaddles <= 65)
        {
            paddle3.SetActive(true);
            paddle4.SetActive(true);
        }

        int startingX = Random.Range(0, 1);
        int temp = 0;
        while (startingX < xGridSize)
        {
            if (startingX < 3)
            {
                spawnPegs(startingX, startingX);
                spawnPegs(startingX, yGridSize - 2 - startingX);
            }
            else if (startingX == 3)
            {
                spawnPegs(startingX, startingX);
                spawnPegs(startingX, yGridSize - 2 - startingX);
            }
            else if (startingX == 4 && startingX < yGridSize-1)
            {
                int amt_spawned = 0;
                while (amt_spawned < startingX)
                {
                    spawnPegs(startingX, startingX-amt_spawned);
                    amt_spawned++;
                }
            }
            else if (startingX == 5 && startingX < yGridSize - 1)
            {
                int amt_spawned = 0;
                while (amt_spawned < startingX+1)
                {
                    spawnPegs(startingX, startingX - amt_spawned);
                    amt_spawned++;
                }
            }
            else
            {
                int amt_spawned = 0;
                while (amt_spawned < yGridSize-2 - temp)
                {
                    spawnPegs(startingX, yGridSize-3 - temp - amt_spawned);
                    amt_spawned++;
                }
                temp++;
            }
            startingX++;
        }
    }

    public void SpawnWave()
    {
        // paddles
        int whichPaddles = Random.Range(0, 100);
        if (whichPaddles <= 15)
        {
            paddle1.SetActive(true);
        }
        else if (whichPaddles <= 30)
        {
            paddle2.SetActive(true);
        }
        else if (whichPaddles <= 65)
        {
            paddle3.SetActive(true);
            paddle4.SetActive(true);
        }

        int startingX = Random.Range(0, 1);
        int startingY = Random.Range(2, 3);
        int direction = Random.Range(0, 50);
        if (direction < 25)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }
        while (startingX < xGridSize)
        {
            int amt_spawned = 0;
            while (amt_spawned < 3)
            {
                spawnPegs(startingX, startingY - 1 + amt_spawned);
                amt_spawned++;
            }
            if (direction == 1 && startingY <= 1)
            {
                direction = -1;
            }
            else if (direction == -1 && startingY >= yGridSize - 3)
            {
                direction = 1;
            }
            if (direction == 1)
            {
                startingY--;
            }
            else if (direction == -1)
            {
                startingY++;
            }
            startingX++;
        }
    }

    public void resetList()
    {
        for (int i = 0; i < xGridSize+1; i++)
        {
            for (int j = 0; j < yGridSize; j++)
            {
                grid[i, j] = false;
            }
            slidePos[i] = xGridSize + 1;
        }
        spawnCount = 0;
        levels_cleared = 0;
        paddle1.SetActive(false);
        paddle2.SetActive(false);
        paddle3.SetActive(false);
        paddle4.SetActive(false);
    }

    public void resetUnlockedPegs()
    {
        for (int i = 0; i < usablePegs.Length; i++)
        {
            if (i == 0)
            {
                usablePegs[i] = ammoPlusPeg;
            }
            else if (i == 1)
            {
                usablePegs[i] = ammoMinusPeg;
            }
            else
            {
                usablePegs[i] = null;
            }
        }
    }

    public void unlockNewPeg()
    {
        int how_many_unlocked = 0;
        for (int i = 0; i < usablePegs.Length; i++)
        {
            if (usablePegs[i] != null)
            {
                how_many_unlocked++;
            }
        }
        if (how_many_unlocked < amt_of_pegs)
        {
            bool new_inserted = false;
            while (!new_inserted)
            {
                int which = Random.Range(0, 25 * amt_of_pegs);
                if (which <= 25)
                {
                    // x2
                    bool currently_inserted = false;
                    for (int i = 0; i < usablePegs.Length; i++)
                    {
                        if (usablePegs[i] != null)
                        {
                            if (usablePegs[i].name == "x2Peg")
                            {
                                currently_inserted = true;
                            }
                        }
                    }
                    if (!currently_inserted)
                    {
                        usablePegs[how_many_unlocked] = x2Peg;
                        shoot.startUnlockedPopUp(x2Peg);
                        new_inserted = true;
                    }

                }
                else if (which <= 50)
                {
                    // multiHit
                    bool currently_inserted = false;
                    for (int i = 0; i < usablePegs.Length; i++)
                    {
                        if (usablePegs[i] != null)
                        {
                            if (usablePegs[i].name == "multiHitPeg")
                            {
                                currently_inserted = true;
                            }
                        }
                    }
                    if (!currently_inserted)
                    {
                        usablePegs[how_many_unlocked] = multiHitPeg;
                        shoot.startUnlockedPopUp(multiHitPeg);
                        new_inserted = true;
                    }
                }
                else if (which <= 75)
                {
                    // blackHole
                    bool currently_inserted = false;
                    for (int i = 0; i < usablePegs.Length; i++)
                    {
                        if (usablePegs[i] != null)
                        {
                            if (usablePegs[i].name == "blackholePeg")
                            {
                                currently_inserted = true;
                            }
                        }
                    }
                    if (!currently_inserted)
                    {
                        usablePegs[how_many_unlocked] = blackholePeg;
                        shoot.startUnlockedPopUp(blackholePeg);
                        new_inserted = true;
                    }
                }
                else if (which <= 100)
                {
                    // pyramid
                    bool currently_inserted = false;
                    for (int i = 0; i < usablePegs.Length; i++)
                    {
                        if (usablePegs[i] != null)
                        {
                            if (usablePegs[i].name == "pyramidPeg")
                            {
                                currently_inserted = true;
                            }
                        }
                    }
                    if (!currently_inserted)
                    {
                        usablePegs[how_many_unlocked] = pyramidPeg;
                        shoot.startUnlockedPopUp(pyramidPeg);
                        new_inserted = true;
                    }
                }
                else if (which <= 125)
                {
                    // arrow
                    bool currently_inserted = false;
                    for (int i = 0; i < usablePegs.Length; i++)
                    {
                        if (usablePegs[i] != null)
                        {
                            if (usablePegs[i].name == "arrowPegLeft")
                            {
                                currently_inserted = true;
                            }
                        }
                    }
                    if (!currently_inserted)
                    {
                        usablePegs[how_many_unlocked] = arrowPegLeft;
                        shoot.startUnlockedPopUp(arrowPegLeft);
                        new_inserted = true;
                    }
                }
                else if (which <= 150)
                {
                    // twoHitPeg
                    bool currently_inserted = false;
                    for (int i = 0; i < usablePegs.Length; i++)
                    {
                        if (usablePegs[i] != null)
                        {
                            if (usablePegs[i].name == "twoHitPeg")
                            {
                                currently_inserted = true;
                            }
                        }
                    }
                    if (!currently_inserted)
                    {
                        usablePegs[how_many_unlocked] = twoHitPeg;
                        shoot.startUnlockedPopUp(twoHitPeg);
                        new_inserted = true;
                    }
                }
            }
        }
    }

    void spawnPegs(int xPos, int yPos)
    {
        int how_many_unlocked = 0;
        for (int i = 0; i < usablePegs.Length; i++)
        {
            if (usablePegs[i] != null)
            {
                how_many_unlocked++;
            }
        }

        int normal = Random.Range(0, 100);
        int upper_bound = (2 * levels_cleared);
        if (upper_bound > 30)
        {
            upper_bound = 30;
        }
        if (normal <= 80 - upper_bound)
        {
            spawnNormalPegs(xPos, yPos);
            spawnCount += 2;
        }
        else
        {
            int range = 25 * how_many_unlocked;
            int which_random = Random.Range(0, range);
            if (which_random <= 25)
            {
                spawnSpecial(usablePegs[0], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
            else if (which_random <= 50)
            {
                spawnSpecial(usablePegs[1], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
            else if (which_random <= 75)
            {
                spawnSpecial(usablePegs[2], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
            else if (which_random <= 100)
            {
                spawnSpecial(usablePegs[3], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
            else if (which_random <= 125)
            {
                spawnSpecial(usablePegs[4], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
            else if (which_random <= 150)
            {
                spawnSpecial(usablePegs[5], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
            else if (which_random <= 175)
            {
                spawnSpecial(usablePegs[6], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
            else if (which_random <= 200)
            {
                spawnSpecial(usablePegs[7], xPos, yPos, which_side_to_spawn);
                spawnSingleNormalPeg(xPos, yPos, which_side_to_spawn);
                spawnCount++;
                which_side_to_spawn *= -1;
            }
        }
    }

    public void spawnSpecial(GameObject obj, int xPos, int yPos, int which)
    {
        if (obj.name == "x2Peg")
        {
            spawnx2Peg(xPos, yPos, which);
        }
        else if (obj.name == "ammoPlusPeg")
        {
            spawnAmmoPlus(xPos, yPos, which);
        }
        else if (obj.name == "ammoMinusPeg")
        {
            spawnAmmoMinus(xPos, yPos, which);
        }
        else if (obj.name == "multiHitPeg")
        {
            spawnMultiHitPeg(xPos, yPos, which);
            spawnCount++;
        }
        else if (obj.name == "blackholePeg")
        {
            spawnBlackHole(xPos, yPos, which);
        }
        else if (obj.name == "pyramidPeg")
        {
            spawnPyramidPeg(xPos, yPos, which);
            spawnCount++;
        }
        else if (obj.name == "arrowPegLeft")
        {
            int which_arrow = Random.Range(0, 100);
            if (which_arrow <= 25)
            {
                spawnArrowRight(xPos, yPos, which);
            }
            else if (which_arrow <= 50)
            {
                spawnArrowUpRight(xPos, yPos, which);
            }
            else if (which_arrow <= 75)
            {
                spawnArrowLeft(xPos, yPos, which);
            }
            else
            {
                spawnArrowUpLeft(xPos, yPos, which);
            }
        }
        else if (obj.name == "twoHitPeg")
        {
            spawnTwoHitPeg(xPos, yPos, which);
            spawnCount++;
        }
    }

    void spawnNormalPegs(int xPos, int yPos)
    {
        Instantiate(normalPeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        Instantiate(normalPeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
    }

    void spawnSliderPegs(int xPos, int yPos)
    {
        if (yPos < yGridSize - 2)
        {
            Instantiate(sliderPeg, new Vector3(zeroX + xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, -45));
            Instantiate(sliderPeg, new Vector3(-zeroX - xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0,0,45));
        }
        else
        {
            Instantiate(sliderPegShort, new Vector3(zeroX + xPos, zeroY - yPos - 0.05f, 0), Quaternion.identity);
            Instantiate(sliderPegShort, new Vector3(-zeroX - xPos, zeroY - yPos - 0.05f, 0), Quaternion.identity);
        }    
    }

    void spawnSliderPegsCorner(int xPos, int yPos)
    {
        Instantiate(slideCornerLeft, new Vector3(zeroX + xPos + 0.1f, zeroY - yPos - 0.05f, 0), Quaternion.identity);
        Instantiate(slideCornerRight, new Vector3(-zeroX - xPos - 0.1f, zeroY - yPos - 0.05f, 0), Quaternion.identity);
    }

    void spawnLeftSliderPegs(int xPos, int yPos)
    {
        if (yPos < yGridSize - 2)
        {
            Instantiate(sliderPeg, new Vector3(zeroX + xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, -45));
        }
        else
        {
            Instantiate(sliderPegShort, new Vector3(zeroX + xPos, zeroY - yPos - 0.05f, 0), Quaternion.identity);
        }
    }

    void spawnLeftSliderPegsCorner(int xPos, int yPos)
    {
        Instantiate(slideCornerLeft, new Vector3(zeroX + xPos + 0.1f, zeroY - yPos - 0.05f, 0), Quaternion.identity);
    }

    void spawnRightSliderPegs(int xPos, int yPos)
    {
        if (yPos < yGridSize - 2)
        {
            Instantiate(sliderPeg, new Vector3(-zeroX - xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, 45));
        }
        else
        {
            Instantiate(sliderPegShort, new Vector3(-zeroX - xPos, zeroY - yPos - 0.05f, 0), Quaternion.identity);
        }
    }

    void spawnSliderTrianglePegs(int xPos, int yPos)
    {
        Instantiate(sliderPeg, new Vector3(-zeroX - xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, -45));
        Instantiate(sliderPeg, new Vector3(zeroX + xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, 45));
    }

    void spawnSlideBucketPegs(int xPos, float yPos)
    {
        Instantiate(sliderPegShort, new Vector3(-zeroX - xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, 90));
        Instantiate(sliderPegShort, new Vector3(zeroX + xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, 90));
    }
    void spawnInsideSlideBucketPegs(int xPos, float yPos)
    {
        Instantiate(sliderPegShort, new Vector3(-zeroX - xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, 0));
        Instantiate(sliderPegShort, new Vector3(zeroX + xPos, zeroY - yPos - 0.5f, 0), Quaternion.identity).gameObject.transform.Rotate(new Vector3(0, 0, 0));
    }

    void spawnRightSliderPegsCorner(int xPos, int yPos)
    {
       Instantiate(slideCornerRight, new Vector3(-zeroX - xPos - 0.1f, zeroY - yPos - 0.05f, 0), Quaternion.identity);
    }

    void spawnSingleNormalPeg(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(normalPeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(normalPeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }

    void spawnPyramidPeg(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(pyramidPeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(pyramidPeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }

    void spawnMultiHitPeg(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(multiHitPeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(multiHitPeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }

    void spawnTwoHitPeg(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(twoHitPeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(twoHitPeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }

    void spawnBlackHole(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(blackholePeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(blackholePeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }

    void spawnAmmoMinus(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(ammoMinusPeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(ammoMinusPeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }

    void spawnAmmoPlus(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(ammoPlusPeg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(ammoPlusPeg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }
    void spawnx2Peg(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(x2Peg, new Vector3(-zeroX - xPos, zeroY - yPos, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(x2Peg, new Vector3(zeroX + xPos, zeroY - yPos, 0), Quaternion.identity);
        }
    }

    void spawnArrowLeft(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(arrowPegLeft, new Vector3(-zeroX - xPos, zeroY - yPos, 0), arrowPegLeft.transform.rotation);
        }
        else
        {
            Instantiate(arrowPegLeft, new Vector3(zeroX + xPos, zeroY - yPos, 0), arrowPegLeft.transform.rotation);
        }
    }

    void spawnArrowUpLeft(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(arrowPegUpLeft, new Vector3(-zeroX - xPos, zeroY - yPos, 0), arrowPegUpLeft.transform.rotation);
        }
        else
        {
            Instantiate(arrowPegUpLeft, new Vector3(zeroX + xPos, zeroY - yPos, 0), arrowPegUpLeft.transform.rotation);
        }
    }

    void spawnArrowRight(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(arrowPegRight, new Vector3(-zeroX - xPos, zeroY - yPos, 0), arrowPegRight.transform.rotation);
        }
        else
        {
            Instantiate(arrowPegRight, new Vector3(zeroX + xPos, zeroY - yPos, 0), arrowPegRight.transform.rotation);
        }
    }

    void spawnArrowUpRight(int xPos, int yPos, int which)
    {
        if (which == -1)
        {
            Instantiate(arrowPegUpRight, new Vector3(-zeroX - xPos, zeroY - yPos, 0), arrowPegUpRight.transform.rotation);
        }
        else
        {
            Instantiate(arrowPegUpRight, new Vector3(zeroX + xPos, zeroY - yPos, 0), arrowPegUpRight.transform.rotation);
        }
    }

    public void reduceCount()
    {
        spawnCount--;
    }

    public void addLevelsCleared()
    {
        levels_cleared++;
    }
}
