
using Meta.XR.MRUtilityKit;

using System.Collections.Generic;

using UnityEngine;



public class ChestSpawner : Singleton<ChestSpawner>
{
    private GameManager gameManager => GameManager.Instance;
    private Settings settings => gameManager.Settings;
    private CollectablesManager collectablesManager => CollectablesManager.Instance;


    public GameObject ChestPrefab;

    public float ChestSize = 0.5f;

    private MRUKAnchor floorAnchor = null;
    private Vector2 floorSize;
    private List<Vector2> floorPlaneBoundry;
    private MRUKRoom mrukRoom;

    private MRUK mruk => MRUK.Instance;


    void Start()
    {
        gameManager.OnWave += OnWave;
        gameManager.OnMruk += MrukRoomCreatedEvent;

        if (ChestPrefab == null)
        {
            Debug.LogError("Chest prefab is not set in ChestSpawner script. Disabling script.");
            enabled = false;
        }
    }


    private void OnDestroy()
    {
        if (GameManager.InstanceExists)
        {
            gameManager.OnWave -= OnWave;
            gameManager.OnMruk -= MrukRoomCreatedEvent;
        }
    }



    private Vector3 GetRandomSpawnLocation()
    {
        if (floorAnchor == null)
        {
            Debug.LogError("No floor anchor in chest spawner! Have you connected the MRUK events?");
            return Vector3.zero;
        }

        Vector3 randomPoint = new(1000.0f, 1000.0f, 1000.0f);

        for (int attempts = 0; attempts < 20; attempts++)
        {
            mrukRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP, ChestSize, LabelFilter.Excluded(MRUKAnchor.SceneLabels.CEILING), out randomPoint, out Vector3 normal);

            randomPoint += normal * 0.2f;

            if (mrukRoom.IsPositionInRoom(randomPoint) && !mrukRoom.IsPositionInSceneVolume(randomPoint))
            {
                return randomPoint;
            }
        }

        return floorAnchor.GetAnchorCenter();
    }



    private GameObject SpawnChest(int numTorpedos = 0, int health = 0)
    {
        GameObject newChest = Instantiate(ChestPrefab, GetRandomSpawnLocation(), Quaternion.AngleAxis(360.0f * Random.value, Vector3.up));

        newChest.GetComponent<Inventory>().Initialize(numTorpedos, health, settings.ChestMaxTorpedoes, settings.ChestMaxHealth);

        collectablesManager.AddChest(newChest);

        return newChest;
    }



    private List<GameObject> SpawnChests(int numChests, int numTorpedos, int health)
    {
        List<GameObject> newChests = new(numChests);

        for (int i = 0; i < numChests && i + collectablesManager.NumChests < settings.ChestMax; i++)
        {
            newChests.Add(SpawnChest());
        }

        return newChests;
    }



    private void DistributeTorpedoesAndHealth(int health, int torpedoes, List<GameObject> chests)
    {
        int numChests = chests.Count;

        if (numChests == 0) return;

        // Ensure that each chest has at least one item
        foreach (GameObject chest in chests)
        {
            AddRandomToChest(chest);
        }

        // Distribute the remaining items randomly
        while (health > 0 || torpedoes > 0)
        {
            int randomChestIndex = Random.Range(0, numChests);
            AddRandomToChest(chests[randomChestIndex]);
        }

        void AddRandomToChest(GameObject chest)
        {
            if (health > 0 && torpedoes > 0)
            {
                if (Random.value < 0.5f)
                {
                    chest.GetComponent<Inventory>().AddTorpedoes(1);
                    torpedoes--;
                }
                else
                {
                    chest.GetComponent<Inventory>().AddHealth(1);
                    health--;
                }
            }
            else if (health > 0)
            {
                chest.GetComponent<Inventory>().AddHealth(1);
                health--;
            }
            else if (torpedoes > 0)
            {
                chest.GetComponent<Inventory>().AddTorpedoes(1);
                torpedoes--;
            }
        }
    }



    private void MrukRoomCreatedEvent(object sender, GameManager.OnMrukCreatedArgs args)
    {
        mrukRoom = mruk.GetCurrentRoom();

        if (!mrukRoom.HasAllLabels(MRUKAnchor.SceneLabels.FLOOR)) return;

        floorAnchor = mrukRoom.FloorAnchor;

        floorSize = floorAnchor.PlaneRect.Value.size;

        floorPlaneBoundry = floorAnchor.PlaneBoundary2D;
    }



    public void MrukRoomRemovedEvent()
    {
        floorAnchor = null;
    }



    private void OnWave(object sender, GameManager.OnWaveArgs waveArgs)
    {
        if (collectablesManager.NumChests >= settings.ChestMax) return;

        float difficultyQuota = settings.ChestMaxWaveContribution * waveArgs.DifficultyDelta;
        float maxSingleItemChestDifficultyValue = settings.ChestDifficultyValue - Mathf.Min(settings.ChestTorpedoDifficultyValue, settings.ChestHealthDifficultyValue);
        int maxNumChestsToSpawn = Mathf.Clamp(Mathf.RoundToInt(difficultyQuota / maxSingleItemChestDifficultyValue), 0, settings.ChestMaxSpawnPerWave);
        int numChestsToSpawn = Random.Range(0, maxNumChestsToSpawn + 1);

        float baseChestDifficultyValue = settings.ChestDifficultyValue * numChestsToSpawn;
        difficultyQuota -= baseChestDifficultyValue;
        float healthDifficultyRatio = Random.Range(0.0f, 1.0f);

        int totalHealth = Mathf.Max(Mathf.CeilToInt(healthDifficultyRatio * numChestsToSpawn), Mathf.FloorToInt(difficultyQuota * healthDifficultyRatio / settings.ChestHealthDifficultyValue));
        int totalTorpedoes = Mathf.Max(Mathf.CeilToInt((1.0f - healthDifficultyRatio) * numChestsToSpawn), Mathf.FloorToInt(difficultyQuota * (1.0f - healthDifficultyRatio) / settings.ChestTorpedoDifficultyValue));

        List<GameObject> newChests = SpawnChests(numChestsToSpawn, totalTorpedoes, totalHealth);
        DistributeTorpedoesAndHealth(totalHealth, totalTorpedoes, newChests);

        if (newChests.Count <= 0) return;

        Debug.Log($"Wave {waveArgs.DifficultyDelta} spawned {newChests.Count} chests with {totalTorpedoes} torpedoes and {totalHealth} health. Total collectables difficulty: {collectablesManager.GetDifficulty()}");
    }
}
