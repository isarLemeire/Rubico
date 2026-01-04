using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomScriptableStats _stats;
    [SerializeField] private GameObject player;

    [Header("Room Data (Set in Inspector)")]
    [SerializeField] private List<Vector2Int> roomKeys = new List<Vector2Int>();
    [SerializeField] private List<GameObject> roomValues = new List<GameObject>();

    private Dictionary<Vector2Int, GameObject> _prefabMap = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> rooms = new Dictionary<Vector2Int, GameObject>();
    private HashSet<Vector2Int> activeRooms;

    // Tracks rooms currently being loaded asynchronously
    private HashSet<Vector2Int> _loadingRooms = new HashSet<Vector2Int>();

    private Vector2Int currentRoom;

    void Awake()
    {
        InitializeRoomDictionary();
    }

    void Start()
    {
        activeRooms = new HashSet<Vector2Int>();
        currentRoom = GetPlayerRoom();
        UpdateRooms();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2Int newRoom = GetPlayerRoom();

        if (newRoom != currentRoom)
        {
            currentRoom = newRoom;
            UpdateRooms();
        }
    }

    Vector2Int GetPlayerRoom()
    {
        var p = player.transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(p.x / _stats.RoomWidth),
            Mathf.FloorToInt(p.y / _stats.RoomHeight)
        );
    }

    // Updated: Returns bottom-left corner position
    Vector3 GetRoomPosition(Vector2Int roomCoord)
    {
        float xPos = roomCoord.x * _stats.RoomWidth;
        float yPos = roomCoord.y * _stats.RoomHeight;
        return new Vector3(xPos, yPos, 0);
    }

    private void InitializeRoomDictionary()
    {
        _prefabMap.Clear();

        if (roomKeys.Count != roomValues.Count)
        {
            Debug.LogError("Room Manager Error: Key list count does not match Value list count! Cannot initialize rooms.");
            return;
        }

        for (int i = 0; i < roomKeys.Count; i++)
        {
            if (_prefabMap.ContainsKey(roomKeys[i]))
            {
                Debug.LogWarning($"Room Manager Warning: Duplicate room key {roomKeys[i]} found. Skipping.");
                continue;
            }
            _prefabMap.Add(roomKeys[i], roomValues[i]);
        }
    }

    /// <summary>
    /// Loads and Instantiates a room over multiple frames to avoid stuttering.
    /// This is the simplified, frame-splitting version.
    /// </summary>
    private IEnumerator LoadRoomAsync(Vector2Int roomCoord, GameObject roomPrefab)
    {
        if (_loadingRooms.Contains(roomCoord)) yield break;
        _loadingRooms.Add(roomCoord);

        if (rooms.ContainsKey(roomCoord))
        {
            _loadingRooms.Remove(roomCoord);
            yield break;
        }

        Vector3 spawnPosition = GetRoomPosition(roomCoord);

        // --- Frame Splitting: Yield before and after the heavy Instantiate call ---
        yield return null;

        // The heaviest operation
        GameObject newRoomInstance = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);

        yield return null;

        // Finalize
        newRoomInstance.transform.position = spawnPosition;
        newRoomInstance.SetActive(false); // Keep it inactive for the 5x5 buffer zone

        rooms.Add(roomCoord, newRoomInstance);
        _loadingRooms.Remove(roomCoord);
    }

    /// <summary>
    /// Destroys the existing room instance at the coordinate and instantly
    /// re-instantiates a new copy using the synchronous method.
    /// </summary>
    public void ReloadRoom()
    {
        Vector2Int roomCoord = GetPlayerRoom();
        if (rooms.ContainsKey(roomCoord))
        {
            // 1. Destroy the old instance
            rooms[roomCoord].SetActive(false);
            Destroy(rooms[roomCoord]);
            rooms.Remove(roomCoord);

            // If it was loading asynchronously, stop tracking it
            if (_loadingRooms.Contains(roomCoord))
            {
                _loadingRooms.Remove(roomCoord);
            }
        }

        // 2. Instantiate the new copy immediately (Synchronous)
        if (_prefabMap.ContainsKey(roomCoord))
        {
            GameObject roomPrefab = _prefabMap[roomCoord];
            Vector3 spawnPosition = GetRoomPosition(roomCoord);

            // This is a synchronous call and WILL cause a hitch, but ensures immediate reset.
            GameObject newRoomInstance = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);

            rooms.Add(roomCoord, newRoomInstance);

            // Ensure the room is active if it's the current room
            if (roomCoord == currentRoom)
            {
                newRoomInstance.SetActive(true);
            }
            else
            {
                newRoomInstance.SetActive(false);
            }
        }

        // Ensure the activeRooms set is updated after this manual operation
        UpdateRooms();
    }

    void UpdateRooms()
    {
        // 1. Define 3x3 (needed) and 5x5 (buffer) areas
        var needed3x3 = new HashSet<Vector2Int>();
        var needed5x5 = new HashSet<Vector2Int>();

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector2Int coord = currentRoom + new Vector2Int(x, y);
                needed5x5.Add(coord);

                if (Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1)
                {
                    needed3x3.Add(coord);
                }
            }
        }

        // --- 2. DESTROY rooms outside the 5x5 zone ---
        var roomsToDestroy = new List<Vector2Int>();
        foreach (var r in rooms.Keys)
        {
            if (!needed5x5.Contains(r) && !_loadingRooms.Contains(r))
            {
                Destroy(rooms[r]);
                roomsToDestroy.Add(r);
            }
        }
        foreach (var r in roomsToDestroy)
        {
            rooms.Remove(r);
        }

        // --- 3. ACTIVATE/FORCE LOAD 3x3 area (Synchronous Priority) ---
        foreach (var roomCoord in needed3x3)
        {
            if (_prefabMap.ContainsKey(roomCoord))
            {
                // A. Room is not loaded yet (not in 'rooms')
                if (!rooms.ContainsKey(roomCoord))
                {
                    // Force synchronous load
                    GameObject roomPrefab = _prefabMap[roomCoord];
                    Vector3 spawnPosition = GetRoomPosition(roomCoord);

                    GameObject newRoomInstance = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);

                    rooms.Add(roomCoord, newRoomInstance);

                    if (_loadingRooms.Contains(roomCoord))
                    {
                        _loadingRooms.Remove(roomCoord);
                    }
                }

                // B. Room is loaded: ensure it is active and positioned
                GameObject roomObject = rooms[roomCoord];

                // Position update
                roomObject.transform.position = GetRoomPosition(roomCoord);

                // Activation
                if (!roomObject.activeSelf)
                {
                    roomObject.SetActive(true); // <--- Set to ACTIVE for the 3x3 area
                }
            }
        }

        // --- 4. START ASYNC LOAD for 5x5 area (Buffer Zone) ---
        foreach (var roomCoord in needed5x5)
        {
            // If the room exists in the prefab map
            if (_prefabMap.ContainsKey(roomCoord))
            {
                // If it's not loaded AND not already loading AND is outside the currently active 3x3 zone
                if (!rooms.ContainsKey(roomCoord) && !_loadingRooms.Contains(roomCoord) && !needed3x3.Contains(roomCoord))
                {
                    GameObject roomPrefab = _prefabMap[roomCoord];
                    StartCoroutine(LoadRoomAsync(roomCoord, roomPrefab));
                }
            }
            // For rooms already loaded in the buffer zone (5x5 but not 3x3), ensure they remain INACTIVE.
            else if (rooms.ContainsKey(roomCoord) && !needed3x3.Contains(roomCoord))
            {
                rooms[roomCoord].SetActive(false); // Explicitly ensure buffer rooms are inactive
            }
        }

        // --- 5. DEACTIVATE old 3x3 rooms ---
        // Iterate through ALL loaded rooms (in 5x5 zone)
        foreach (var r in rooms.Keys)
        {
            // If a loaded room is outside the current 3x3 area, set it inactive (Buffer zone status)
            if (!needed3x3.Contains(r) && rooms[r].activeSelf)
            {
                rooms[r].SetActive(false);
            }
        }

        // --- 6. Finalize activeRooms ---
        activeRooms.Clear();
        foreach (var roomCoord in needed3x3)
        {
            if (rooms.ContainsKey(roomCoord) && rooms[roomCoord].activeSelf)
            {
                activeRooms.Add(roomCoord);
            }
        }
    }
}