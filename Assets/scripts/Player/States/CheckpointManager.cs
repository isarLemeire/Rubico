using UnityEngine;

public class CheckpointManager
{

    public Vector3 lastCheckpoint { get; private set; }
    public Vector2Int lastCheckpointRoom { get; private set; }


    private readonly RoomScriptableStats _stats;

    public CheckpointManager(RoomScriptableStats stats, Vector3 initialPosition)
    {
        _stats = stats;

        // Initialize the first checkpoint and its room index
        lastCheckpoint = initialPosition;
        lastCheckpointRoom = GetRoomIndex(initialPosition);
    }

    private Vector2Int GetRoomIndex(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / _stats.RoomWidth),
            Mathf.FloorToInt(position.y / _stats.RoomHeight)
        );
    }

    private Vector3 GetRoomPosition(Vector2Int roomCoord)
    {
        float xPos = roomCoord.x * _stats.RoomWidth;
        float yPos = roomCoord.y * _stats.RoomHeight;
        return new Vector3(xPos, yPos, 0);
    }


    public bool UpdateCheckPoint(Vector3 playerPosition, bool isGrounded)
    {
        // 1. Figure out what room I am
        Vector2Int currentRoomIndex = GetRoomIndex(playerPosition);

        // 2. If they do not match (i.e., we are in a new room), proceed.
        if (currentRoomIndex == lastCheckpointRoom)
        {
            return false; // Checkpoint is already set for this room. Do nothing.
        }

        // --- Logic for NEW Room ---

        // 3. Figure out the bounds of the new room.

        // Get the bottom-left world position of the new room (Room origin)
        Vector3 roomOrigin = GetRoomPosition(currentRoomIndex);

        // Define world-space boundaries (based on the room's origin)
        float minX = roomOrigin.x;
        float maxX = roomOrigin.x + _stats.RoomWidth;
        float minY = roomOrigin.y;
        float maxY = roomOrigin.y + _stats.RoomHeight;

        float margin = _stats.CheckpointMargin;

        // 4. See if I am at least checkpointMargin inside.

        // Player must be greater than the min boundary plus the margin
        bool isInMarginX = playerPosition.x >= (minX + margin) &&
                           playerPosition.x <= (maxX - margin);

        // Player must be greater than the min boundary plus the margin
        bool isInMarginY = playerPosition.y >= (minY + margin) &&
                           playerPosition.y <= (maxY - margin);

        if (isInMarginX && isInMarginY)
        {
            // 5. If all checks pass, set my current position to the checkpoint, and update the Vector2Int.
            lastCheckpoint = playerPosition;
            lastCheckpointRoom = currentRoomIndex;

            // Debug.Log($"New checkpoint set at: {lastCheckpoint} in Room: {lastCheckpointRoom}");

            return true;
        }

        // Conditions not met (not inside the margin).
        return false;
    }
}