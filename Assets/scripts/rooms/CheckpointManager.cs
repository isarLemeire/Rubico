using UnityEngine;
namespace Player
{
    public class CheckpointManager
    {

        public Vector3 lastCheckpoint { get; private set; }
        public Vector2Int lastCheckpointRoom { get; private set; }
        private PlayerController _controller;

        private readonly RoomScriptableStats _stats;
        private readonly PlayerScriptableStats _playerStats;

        public CheckpointManager(PlayerController controller, RoomScriptableStats stats, PlayerScriptableStats playerStats, Vector3 initialPosition)
        {
            _stats = stats;
            _playerStats = playerStats;

            // Initialize the first checkpoint and its room index
            lastCheckpoint = initialPosition;
            lastCheckpointRoom = GetRoomIndex(initialPosition);
            _controller = controller;
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

        public bool UpdateCheckPoint(Vector3 playerPosition, BoxCollider2D collider, bool isGrounded)
        {
            float playerHeight = collider.bounds.size.y;
            float offset = collider.offset.y;
            float skin = 0.01f;
            LayerMask groundMask = _playerStats.CollisionMask; // Assign in stats or inspector

            // --- Step 1: Find floor below player ---
            Vector2 rayOrigin = new Vector2(playerPosition.x, playerPosition.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, _stats.groundCheckDistance, groundMask);

            if (!hit.collider)
                return false; // No floor nearby, don't set checkpoint

            Vector3 groundedPosition = hit.point;

            // --- Step 2: Figure out what room the grounded position is in ---
            Vector2Int currentRoomIndex = GetRoomIndex(groundedPosition);

            if (currentRoomIndex == lastCheckpointRoom)
                return false; // Already set for this room

            // --- Step 3: Room bounds ---
            Vector3 roomOrigin = GetRoomPosition(currentRoomIndex);
            float minX = roomOrigin.x + _stats.CheckpointMargin;
            float maxX = roomOrigin.x + _stats.RoomWidth - _stats.CheckpointMargin;
            float minY = roomOrigin.y + _stats.CheckpointMargin;
            float maxY = roomOrigin.y + _stats.RoomHeight - _stats.CheckpointMargin;

            // --- Step 4: Clamp grounded position inside margin ---
            float checkpointX = Mathf.Clamp(groundedPosition.x, minX, maxX);
            float checkpointY = Mathf.Clamp(groundedPosition.y, minY, maxY);

            Vector3 newCheckpoint = new Vector3(checkpointX, checkpointY + playerHeight/2 + skin - offset, playerPosition.z);

            // --- Step 5: Assign checkpoint ---
            lastCheckpoint = newCheckpoint;
            lastCheckpointRoom = currentRoomIndex;

            _controller.ctx.HP = _controller.ctx.Stats.MaxHP;
            _controller.UIanimator.SetInteger("HP", _controller.ctx.HP);

            return true;
        }

    }
}