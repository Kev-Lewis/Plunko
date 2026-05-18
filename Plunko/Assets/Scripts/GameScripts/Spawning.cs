using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawning : MonoBehaviour
{
    // Spawning Patterns
    public enum BoardPatterns 
    {
        HardSlide,
        DownwardsArc,
        UpwardsArc,
        Wave,
        Bucket,
        Spiral,
        Triangle,
        DenseField,
        WallBounce,
        Diamond,
    }
    
    // L/R + Slide settings
    private enum SpawnSide { Left = -1, Right = 1}
    private enum SlideMode { Both, LeftOnly, RightOnly, None}

    [Header("Seed Settings")]
    [SerializeField] private bool useSeededGeneration = true;

    // Seed Format: PL + 8 hexadecimal characters.
    [SerializeField] private string seed = "PL00001A3F";

    [Header("Peg Prefabs")]
    [SerializeField] private GameObject normalPeg;
    [SerializeField] private GameObject multiHitPeg;
    [SerializeField] private GameObject pyramidPeg;
    [SerializeField] private GameObject blackholePeg;
    [SerializeField] private GameObject ammoPlusPeg;
    [SerializeField] private GameObject ammoMinusPeg;
    [SerializeField] private GameObject x2Peg;
    [SerializeField] private GameObject arrowPegLeft;
    [SerializeField] private GameObject arrowPegUpLeft;
    [SerializeField] private GameObject arrowPegRight;
    [SerializeField] private GameObject arrowPegUpRight;
    [SerializeField] private GameObject twoHitPeg;

    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject sliderPeg;
    [SerializeField] private GameObject sliderPegShort;
    [SerializeField] private GameObject slideCornerLeft;
    [SerializeField] private GameObject slideCornerRight;
    [SerializeField] private GameObject paddle1;
    [SerializeField] private GameObject paddle2;
    [SerializeField] private GameObject paddle3;
    [SerializeField] private GameObject paddle4;

    [Header("Generation Settings")]
    [SerializeField] private int minRange = 12;
    [SerializeField] private int maxRange = 24;
    [SerializeField] private List<BoardPatterns> enabledPatterns = new List<BoardPatterns>();

    [Header("Grid Settings")]
    [SerializeField] private int xGridSize = 8;
    [SerializeField] private int yGridSize = 7;
    [SerializeField] private float zeroX = -7.5f;
    [SerializeField] private float zeroY = 2f;

    public int spawnCount;

    private bool[,] grid;
    private int[] slidePos;
    private Shooter shoot;
    private int levelsCleared;
    private SpawnSide nextNormalSide = SpawnSide.Left;
    private System.Random seededRandom;

    private readonly List<GameObject> unlockedPegs = new List<GameObject>();
    private readonly List<GameObject> unlockPool = new List<GameObject>();

    private static readonly BoardPatterns[] defaultPatterns =
    {
        BoardPatterns.HardSlide,
        BoardPatterns.DownwardsArc,
        BoardPatterns.UpwardsArc,
        BoardPatterns.Wave,
        BoardPatterns.Bucket,
        BoardPatterns.Spiral,
        BoardPatterns.Triangle,
        BoardPatterns.DenseField,
        BoardPatterns.WallBounce,
        BoardPatterns.Diamond,
    };

    private void Start() {
        grid = new bool[xGridSize, yGridSize];
        slidePos = new int[xGridSize];
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();

        ResetRunState();
        SpawnObjects();
    }

    // Main starting point
    public void SpawnObjects() {
        ResetBoardState();
        SpawnRandomPaddles();
        SpawnBoardPattern(PickRandomPattern());
    }

    // Use this later when starting a new run from a menu, seed input field, or daily challenge.
    public void StartNewSeededRun(string newSeed) {
        seed = NormalizeSeed(newSeed);
        ResetRunState();
        SpawnObjects();
    }

    private void InitializeRandom() {
        if (useSeededGeneration) {
            seededRandom = new System.Random(SeedStringToInt(seed));
        }
        else {
            seededRandom = null;
        }
    }

     private string NormalizeSeed(string denormalized_seed) {
        if (string.IsNullOrWhiteSpace(denormalized_seed)) {
            return "PL00000000";
        }

        denormalized_seed = denormalized_seed.Trim().ToUpper();

        if (!denormalized_seed.StartsWith("PL")) {
            denormalized_seed = "PL" + denormalized_seed;
        }

        string hexPart = denormalized_seed.Substring(2);

        if (hexPart.Length > 8) {
                        hexPart = hexPart.Substring(0, 8);
        }

        while (hexPart.Length < 8) {
            hexPart = "0" + hexPart;
        }

        for (int i = 0; i < hexPart.Length; i++) {
            bool isDigit = hexPart[i] >= '0' && hexPart[i] <= '9';
            bool isHexLetter = hexPart[i] >= 'A' && hexPart[i] <= 'F';

            if (!isDigit && !isHexLetter) {
                return "PL00000000";
            }
        }

        return "PL" + hexPart;
    }

    private int SeedStringToInt(string denormalized_seed) {
        string normalizedSeed = NormalizeSeed(denormalized_seed);
        string hexPart = normalizedSeed.Substring(2, 8);

        try {
            return System.Convert.ToInt32(hexPart, 16);
        }
        catch {
            return 0;
        }
    }

    private int Range(int minInclusive, int maxExclusive) {
        if (maxExclusive <= minInclusive) {
            return minInclusive;
        }

        if (useSeededGeneration && seededRandom != null) {
            return seededRandom.Next(minInclusive, maxExclusive);
        }

        return UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    private float Range(float minInclusive, float maxInclusive) {
        if (maxInclusive <= minInclusive) {
            return minInclusive;
        }

        if (useSeededGeneration && seededRandom != null) {
            return minInclusive + (float)seededRandom.NextDouble() * (maxInclusive - minInclusive);
        }

        return UnityEngine.Random.Range(minInclusive, maxInclusive);
    }

    private float Value() {
        if (useSeededGeneration && seededRandom != null) {
            return (float)seededRandom.NextDouble();
        }

        return UnityEngine.Random.value;
    }

    private bool Chance(float probability) {
        return Value() < probability;
    }

    private BoardPatterns PickRandomPattern() {
        if (enabledPatterns != null && enabledPatterns.Count > 0) {
            return enabledPatterns[Range(0, enabledPatterns.Count)];
        }

        return defaultPatterns[Range(0, defaultPatterns.Length)];
    }

    private void SpawnBoardPattern(BoardPatterns pattern) {
        switch (pattern) {
            case BoardPatterns.HardSlide:
                SpawnHard();
                break;
            case BoardPatterns.DownwardsArc:
                SpawnDownwardsArc();
                break;
            case BoardPatterns.UpwardsArc:
                SpawnUpwardsArc();
                break;
            case BoardPatterns.Wave:
                SpawnWave();
                break;
            case BoardPatterns.Bucket:
                SpawnBucket();
                break;
            case BoardPatterns.Spiral:
                SpawnSpiral();
                break;
            case BoardPatterns.Triangle:
                SpawnTriangle();
                break;
            case BoardPatterns.DenseField:
                SpawnDenseField();
                break;
            case BoardPatterns.WallBounce:
                SpawnWallBounce();
                break;
            case BoardPatterns.Diamond:
                SpawnDiamond();
                break;
        }
    }

    private void ResetRunState() {
        InitializeRandom();
        levelsCleared = 0;
        nextNormalSide = SpawnSide.Left;
        resetUnlockedPegs();
    }

    private void ResetBoardState() {
        if (grid == null || grid.GetLength(0) != xGridSize || grid.GetLength(1) != yGridSize) {
            grid = new bool[xGridSize, yGridSize];
        }

        if (slidePos == null || slidePos.Length != xGridSize) {
            slidePos = new int[xGridSize];
        }

        for (int x = 0; x < xGridSize; x++) {
            for (int y = 0; y < yGridSize; y++) {
                grid[x, y] = false;
            }

            slidePos[x] = yGridSize + 1;
        }

        spawnCount = 0;
        SetPaddles(false, false, false, false);
    }

    public void resetList() {
        ResetBoardState();
    }

    public void resetUnlockedPegs() {
        unlockedPegs.Clear();
        AddUnlockedPeg(ammoPlusPeg);
        AddUnlockedPeg(ammoMinusPeg);
    }

    public void unlockNewPeg() {
        BuildUnlockPool();

        if (unlockPool.Count == 0) {
            return;
        }

        GameObject chosenPeg = unlockPool[Range(0, unlockPool.Count)];
        AddUnlockedPeg(chosenPeg);

        if (shoot != null) {
            shoot.startUnlockedPopUp(chosenPeg);
        }
    }

    private void BuildUnlockPool(){
        unlockPool.Clear();
        AddToUnlockPool(x2Peg);
        AddToUnlockPool(multiHitPeg);
        AddToUnlockPool(blackholePeg);
        AddToUnlockPool(pyramidPeg);
        AddToUnlockPool(arrowPegLeft);
        AddToUnlockPool(twoHitPeg);
    }

    private void AddToUnlockPool(GameObject peg) {
        if (peg != null && !unlockedPegs.Contains(peg) && !unlockPool.Contains(peg)) {
            unlockPool.Add(peg);
        }
    }

    private void AddUnlockedPeg(GameObject peg) {
        if (peg != null && !unlockedPegs.Contains(peg)) {
            unlockedPegs.Add(peg);
        }
    }

    private void SpawnRandomPaddles()
    {
        int roll = Range(0, 100);

        if (roll < 15) {
            SetPaddles(true, false, false, false);
        }
        else if (roll < 30) {
            SetPaddles(false, true, false, false);
        }
        else if (roll < 65) {
            SetPaddles(false, false, true, true);
        }
        else {
            SetPaddles(false, false, false, false);
        }
    }

    private void SetPaddles(bool p1, bool p2, bool p3, bool p4) {
        if (paddle1 != null) paddle1.SetActive(p1);
        if (paddle2 != null) paddle2.SetActive(p2);
        if (paddle3 != null) paddle3.SetActive(p3);
        if (paddle4 != null) paddle4.SetActive(p4);
    }

    public void SpawnHard() {
        SpawnSlideRamp();

        int targetPegCount = Range(minRange, maxRange + 1);
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = targetPegCount * 30;

        while (spawned < targetPegCount && attempts < maxAttempts) {
            attempts++;

            int x = Range(0, xGridSize);
            int y = Range(0, yGridSize - 1);

            if (y < slidePos[x] && TrySpawnCell(x, y)) {
                spawned += 2;
            }
        }
    }

    private int SpawnSlideRamp() {
        int y = Range(0, 2);
        int roll = Range(0, 100);
        bool cornerSpawned = false;

        SlideMode mode;
        if (roll < 35) mode = SlideMode.Both;
        else if (roll < 60) mode = SlideMode.LeftOnly;
        else if (roll < 85) mode = SlideMode.RightOnly;
        else mode = SlideMode.None;

        if (mode == SlideMode.None) {
            return y;
        }

        for (int x = 0; x < xGridSize; x++) {
            slidePos[x] = y;

            if (y <= yGridSize - 3) {
                SpawnSliderByMode(x, y, mode, false);
                y++;
            }
            else if (!cornerSpawned) {
                SpawnSliderByMode(x, y, mode, true);
                cornerSpawned = true;
            }
            else {
                SpawnSliderByMode(x, y, mode, false);
            }
        }

        return y;
    }

    public void SpawnDownwardsArc() {
        SpawnArc(true);
    }

    public void SpawnUpwardsArc() {
        SpawnArc(false);
    }

    private void SpawnArc(bool downward) {
        int startX = Range(0, 2);
        int y = downward ? Range(0, 2) : Range(4, 6);
        bool staggered = Chance(0.5f);
        int staggerCounter = 0;

        for (int x = startX; x < xGridSize; x++) {
            SpawnBand(x, y, 3, downward ? 1 : -1);

            if (!staggered || staggerCounter == 1) {
                if (downward && y < yGridSize - 4) y++;
                if (!downward && y > 2) y--;
                staggerCounter = 0;
            }
            else {
                staggerCounter++;
            }
        }
    }

    public void SpawnWave() {
        int centerY = Range(2, yGridSize - 2);
        int direction = Chance(0.5f) ? -1 : 1;

        for (int x = 0; x < xGridSize; x++) {
            SpawnBand(x, centerY - 1, 3, 1);

            centerY += direction;

            if (centerY <= 1 || centerY >= yGridSize - 3) {
                direction *= -1;
            }
        }
    }

    public void SpawnTriangle() {
    int startingX = Range(2, 4);
    int startingY = 4;

    int level = 1;
    int prevY = 1;
    int currentY = 1;

    SpawnSliderPair(startingX - 1, startingY + 1);

    while (startingX < xGridSize) {
        for (int i = 0; i < level; i++) {
            TrySpawnCell(startingX, startingY - i);
        }

        currentY = startingY + level;

        if (currentY != prevY) {
            SpawnTriangleSlider(startingX - 1, startingY - level);
        }

        SpawnSliderPair(startingX, startingY + 1);

        if (level <= startingY) {
            level++;
        }

        startingX++;
        prevY = currentY;
    }
}

    public void SpawnBucket() {
        int startX = 1;
        int bottomY = yGridSize - 2;

        for (int y = 0; y < bottomY; y++) {
            SpawnBucketWall(startX - 1, y - 0.5f);

            if (y < bottomY - 2) {
                SpawnBucketWall(startX + 2, y - 0.5f);
            }
        }

        for (int x = startX; x < xGridSize; x++) {
            if (x < startX + 2) {
                for (int y = 0; y < bottomY; y++) {
                    TrySpawnCell(x, y);
                }
            }
            else {
                TrySpawnCell(x, yGridSize - 3);
                TrySpawnCell(x, yGridSize - 4);
            }

            SpawnSliderPair(x, bottomY);

            if (x > startX + 2) {
                SpawnInsideBucketRail(x, bottomY - 3.25f);
            }
        }
    }

    public void SpawnSpiral() {
        int temp = 0;

        for (int x = 0; x < xGridSize; x++) {
            if (x <= 3) {
                TrySpawnCell(x, x);
                TrySpawnCell(x, yGridSize - 2 - x);
            }
            else if (x == 4 && x < yGridSize - 1) {
                for (int i = 0; i < x; i++) {
                    TrySpawnCell(x, x - i);
                }
            }
            else if (x == 5 && x < yGridSize - 1) {
                for (int i = 0; i < x + 1; i++) {
                    TrySpawnCell(x, x - i);
                }
            }
            else {
                int amount = yGridSize - 2 - temp;

                for (int i = 0; i < amount; i++) {
                    TrySpawnCell(x, yGridSize - 3 - temp - i);
                }

                temp++;
            }
        }
    }

    // SpawnDenseField Helper

    private void SpawnDenseCenterPeg(GameObject prefab, float worldX, float y) {
        if (prefab == null) {
            return;
        }

        float worldY = zeroY - y;

        Instantiate(prefab, new Vector3(worldX, worldY, 0f), Quaternion.identity);
        spawnCount++;
    }

    public void SpawnDenseField() {
        float centerX = 0f;
        float centerY = 2.5f;

        float xSpacing = 0.75f;
        float ySpacing = 0.55f;

        int rows = 6;
        int maxCols = 15;

        for (int row = 0; row < rows; row++) {
            int pegsInRow;

            if (row == 0 || row == rows - 1) {
                pegsInRow = 9;
            }
            else if (row == 1 || row == rows - 2) {
                pegsInRow = 13;
            }
            else {
                pegsInRow = maxCols;
            }

            float rowOffset = row % 2 == 0 ? 0f : xSpacing * 0.5f;
            float startX = centerX - ((pegsInRow - 1) * xSpacing * 0.5f);

            float y = centerY - ((rows - 1) * ySpacing * 0.5f) + row * ySpacing;

            for (int i = 0; i < pegsInRow; i++) {
                float x = startX + i * xSpacing + rowOffset;
                SpawnDenseCenterPeg(normalPeg, x, y);
            }
        }
    }

    public void SpawnWallBounce() {
        for (int x = 0; x < xGridSize; x += 2) {
            SpawnSliderPair(x, Range(1, yGridSize - 2));
            TrySpawnCell(x, Range(0, yGridSize - 1));
        }

        for (int x = 1; x < xGridSize; x += 2) {
            TrySpawnCell(x, 1);
            TrySpawnCell(x, yGridSize - 3);
        }
    }

    public void SpawnDiamond() {
        int centerX = xGridSize / 2;
        int centerY = yGridSize / 2;
        int radius = 3;

        bool centerAlwaysSpecial = true;

        for (int x = 0; x < xGridSize; x++) {
            for (int y = 0; y < yGridSize; y++) {
                int distance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);

                if (distance <= radius) {
                    if (distance == 0 && centerAlwaysSpecial && unlockedPegs.Count > 0) {
                        GameObject specialPeg = unlockedPegs[Range(0, unlockedPegs.Count)];

                        SpawnSingleSpecial(specialPeg, x, y, SpawnSide.Left);
                        SpawnSinglePeg(normalPeg, x, y, SpawnSide.Right, Quaternion.identity);

                        spawnCount += 1 + ExtraSpawnCountForSpecial(specialPeg);
                        continue;
                    }

                    TrySpawnCell(x, y);
                }
            }
        }
    }

    private void SpawnBand(int x, int startY, int amount, int yStep) {
        for (int i = 0; i < amount; i++) {
            TrySpawnCell(x, startY + i * yStep);
        }
    }

    private bool TrySpawnCell(int x, int y) {
        if (x < 0 || x >= xGridSize || y < 0 || y >= yGridSize) {
            return false;
        }

        if (grid[x, y]) {
            return false;
        }

        grid[x, y] = true;
        spawnPegs(x, y);
        return true;
    }

    private void spawnPegs(int x, int y) {
        int specialChance = Mathf.Clamp(20 + levelsCleared * 2, 20, 50);
        bool useNormalPegs = Range(0, 100) >= specialChance || unlockedPegs.Count == 0;

        if (useNormalPegs) {
            SpawnNormalPair(x, y);
            spawnCount += 2;
            return;
        }

        GameObject specialPeg = unlockedPegs[Range(0, unlockedPegs.Count)];
        SpawnSide specialSide = Opposite(nextNormalSide);

        SpawnSingleSpecial(specialPeg, x, y, specialSide);
        SpawnSinglePeg(normalPeg, x, y, nextNormalSide, Quaternion.identity);

        spawnCount += 1 + ExtraSpawnCountForSpecial(specialPeg);
        nextNormalSide = Opposite(nextNormalSide);
    }

    public void spawnSpecial(GameObject obj, int xPos, int yPos, int which) {
        SpawnSide specialSide = which == -1 ? SpawnSide.Right : SpawnSide.Left;
        SpawnSingleSpecial(obj, xPos, yPos, specialSide);
    }

    private void SpawnSingleSpecial(GameObject peg, int x, int y, SpawnSide side) {
        if (peg == null) {
            return;
        }

        if (peg == arrowPegLeft) {
            SpawnRandomArrowPeg(x, y, side);
            return;
        }

        SpawnSinglePeg(peg, x, y, side, peg.transform.rotation);
    }

    private void SpawnRandomArrowPeg(int x, int y, SpawnSide side) {
        GameObject[] arrows = { arrowPegLeft, arrowPegUpLeft, arrowPegRight, arrowPegUpRight };
        GameObject chosenArrow = arrows[Range(0, arrows.Length)];

        if (chosenArrow != null) {
            SpawnSinglePeg(chosenArrow, x, y, side, chosenArrow.transform.rotation);
        }
    }

    private int ExtraSpawnCountForSpecial(GameObject peg) {
        if (peg == multiHitPeg || peg == pyramidPeg || peg == twoHitPeg) {
            return 1;
        }

        return 0;
    }

    private void SpawnNormalPair(int x, int y) {
        SpawnSinglePeg(normalPeg, x, y, SpawnSide.Left, Quaternion.identity);
        SpawnSinglePeg(normalPeg, x, y, SpawnSide.Right, Quaternion.identity);
    }

    private void SpawnSinglePeg(GameObject prefab, int x, int y, SpawnSide side, Quaternion rotation) {
        if (prefab == null) {
            return;
        }

        Instantiate(prefab, WorldPos(x, y, side), rotation);
    }
    
    private Vector3 WorldPos(int x, float y, SpawnSide side) {
        float worldX = side == SpawnSide.Left ? zeroX + x : -zeroX - x;
        float worldY = zeroY - y;
        return new Vector3(worldX, worldY, 0f);
    }

    private SpawnSide Opposite(SpawnSide side) {
        return side == SpawnSide.Left ? SpawnSide.Right : SpawnSide.Left;
    }

    private void SpawnSliderByMode(int x, int y, SlideMode mode, bool corner) {
        if (mode == SlideMode.Both || mode == SlideMode.LeftOnly) {
            if (corner) SpawnSlideCorner(SpawnSide.Left, x, y);
            else SpawnSliderRail(SpawnSide.Left, x, y, -45f);
        }

        if (mode == SlideMode.Both || mode == SlideMode.RightOnly) {
            if (corner) SpawnSlideCorner(SpawnSide.Right, x, y);
            else SpawnSliderRail(SpawnSide.Right, x, y, 45f);
        }
    }

    private void SpawnSliderPair(int x, float y) {
        SpawnSliderRail(SpawnSide.Left, x, y, -45f);
        SpawnSliderRail(SpawnSide.Right, x, y, 45f);
    }

    private void SpawnTriangleSlider(int x, float y) {
        SpawnSliderRail(SpawnSide.Left, x, y, 45f);
        SpawnSliderRail(SpawnSide.Right, x, y, -45f);
    }

    private void SpawnSliderRail(SpawnSide side, int x, float y, float zRotation) {
        if (sliderPeg == null || sliderPegShort == null) {
            return;
        }

        if (y < yGridSize - 2) {
            GameObject rail = Instantiate(sliderPeg, WorldPos(x, y + 0.5f, side), Quaternion.identity);
            rail.transform.Rotate(new Vector3(0f, 0f, zRotation));
        }
        else {
            GameObject rail = Instantiate(sliderPegShort, WorldPos(x, y + 0.05f, side), Quaternion.identity);
            rail.transform.Rotate(Vector3.zero);
        }
    }

    private void SpawnSlideCorner(SpawnSide side, int x, float y) {
        GameObject cornerPrefab = side == SpawnSide.Left ? slideCornerLeft : slideCornerRight;

        if (cornerPrefab == null) {
            return;
        }

        Vector3 offset = side == SpawnSide.Left ? new Vector3(0.1f, -0.05f, 0f) : new Vector3(-0.1f, -0.05f, 0f);
        Instantiate(cornerPrefab, WorldPos(x, y, side) + offset, Quaternion.identity);
    }

    private void SpawnBucketWall(int x, float y) {
        SpawnShortRail(SpawnSide.Left, x, y + 0.5f, 90f);
        SpawnShortRail(SpawnSide.Right, x, y + 0.5f, 90f);
    }

    private void SpawnInsideBucketRail(int x, float y) {
        SpawnShortRail(SpawnSide.Left, x, y + 0.5f, 0f);
        SpawnShortRail(SpawnSide.Right, x, y + 0.5f, 0f);
    }

    private void SpawnShortRail(SpawnSide side, int x, float y, float zRotation) {
        if (sliderPegShort == null) {
            return;
        }

        GameObject rail = Instantiate(sliderPegShort, WorldPos(x, y, side), Quaternion.identity);
        rail.transform.Rotate(new Vector3(0f, 0f, zRotation));
    }

    public void reduceCount() {
        spawnCount--;
    }

    public void addLevelsCleared() {
        levelsCleared++;
    }
}
