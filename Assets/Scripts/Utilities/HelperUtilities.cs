using Mirror;
using System.Collections;
using UnityEngine;

public static class HelperUtilities
{
    /// <summary>
    /// Get the mouse world position.
    /// </summary>
    public static Vector3 GetMouseWorldPosition(Player player)
    {
        if (player == null || !player.IsLocal || player.cameraManager == null) return Vector3.zero;

        Camera cam = player.cameraManager.GetGameplayCamera();

        if (cam == null) return Vector3.zero;

        Vector2 mouse = InputManager.Instance.pointerPosition.action.ReadValue<Vector2>();

        // Clamp to camera pixel rect (Not screen!)
        Rect rect = cam.pixelRect; // Prevents frustum error

        if (!rect.Contains(mouse)) return Vector3.zero;

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, cam.nearClipPlane));

        worldPos.z = 0f;

        return worldPos;
    }

    /// <summary>
    /// Get the camera viewport lower and upper bounds
    /// </summary>
    public static void CameraWorldPositionBounds(out Vector2Int cameraWorldPositionLowerBounds, out Vector2Int cameraWorldPositionUpperBounds, Camera camera)
    {
        Vector3 worldPositionViewportBottomLeft = camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 worldPositionViewportTopRight = camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        cameraWorldPositionLowerBounds = new Vector2Int((int)worldPositionViewportBottomLeft.x, (int)worldPositionViewportBottomLeft.y);
        cameraWorldPositionUpperBounds = new Vector2Int((int)worldPositionViewportTopRight.x, (int)worldPositionViewportTopRight.y);
    }

    /// <summary>
    /// Get the angle in degrees from a direction vector
    /// </summary>
    public static float GetAngleFromVector(Vector3 vector)
    {
        float radians = Mathf.Atan2(vector.y, vector.x);
        float degrees = radians * Mathf.Rad2Deg;

        return degrees;
    }

    /// <summary>
    /// Get the direction vector from an angle in degrees
    /// </summary>
    public static Vector3 GetDirectionVectorFromAngle(float angle)
    {
        Vector3 directionVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0f);
        return directionVector;
    }

    /// <summary>
    /// Get AimDirection enum value from the pased in angleDegrees
    /// </summary>
    public static AimDirection GetAimDirection(float angleDegrees)
    {
        AimDirection aimDirection;

        // Set player direction
        // Up
        if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            aimDirection = AimDirection.Up;
        }
        // UpRight
        else if (angleDegrees >= 22f && angleDegrees <= 67f)
        {
            aimDirection = AimDirection.UpRight;
        }
        // Right
        else if ((angleDegrees >= -22f && angleDegrees <= 0f) || (angleDegrees > 0 && angleDegrees < 22f))
        {
            aimDirection = AimDirection.Right;
        }
        // DownRight
        else if (angleDegrees > -67f && angleDegrees < -22f)
        {
            aimDirection = AimDirection.DownRight;
        }
        // Down
        else if (angleDegrees > -112f && angleDegrees <= -67f)
        {
            aimDirection = AimDirection.Down;
        }
        // DownLeft
        else if (angleDegrees > -158f && angleDegrees <= -112f)
        {
            aimDirection = AimDirection.DownLeft;
        }
        // Left
        else if ((angleDegrees >= 158f && angleDegrees < 180f) || (angleDegrees <= -158f && angleDegrees >= -180f))
        {
            aimDirection = AimDirection.Left;
        }
        // UpLeft
        else if ((angleDegrees > 112f && angleDegrees < 158f))
        {
            aimDirection = AimDirection.UpLeft;
        }
        else
        {
            aimDirection = AimDirection.Right;
        }

        return aimDirection;
    }

    /// <summary>
    /// Get AttackDirection enum value from the pased in angleDegrees
    /// </summary>
    public static AttackDirection GetAttackDirection(float angleDegrees)
    {
        AttackDirection attackDirection;

        // Set player direction
        // Up
        if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            attackDirection = AttackDirection.Up;
        }
        // Up Right
        else if (angleDegrees >= 22f && angleDegrees <= 67f)
        {
            attackDirection = AttackDirection.UpRight;
        }
        // Right
        else if ((angleDegrees >= -22f && angleDegrees <= 0f) || (angleDegrees > 0 && angleDegrees < 22f))
        {
            attackDirection = AttackDirection.Right;
        }
        // Down Right
        else if (angleDegrees > -67f && angleDegrees < -22f)
        {
            attackDirection = AttackDirection.DownRight;
        }
        // Down
        else if (angleDegrees > -112f && angleDegrees <= -67f)
        {
            attackDirection = AttackDirection.Down;
        }
        // Down Left
        else if (angleDegrees > -158f && angleDegrees <= -112f)
        {
            attackDirection = AttackDirection.DownLeft;
        }
        // Left
        else if ((angleDegrees >= 158f && angleDegrees < 180f) || (angleDegrees <= -158f && angleDegrees >= -180f))
        {
            attackDirection = AttackDirection.Left;
        }
        // Up Left
        else if ((angleDegrees > 112f && angleDegrees < 158f))
        {
            attackDirection = AttackDirection.UpLeft;
        }
        else
        {
            attackDirection = AttackDirection.Right;
        }

        return attackDirection;
    }

    public static RollDirection GetRollDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) return dir.x > 0 ? RollDirection.Right : RollDirection.Left;
        else return dir.y > 0 ? RollDirection.Up : RollDirection.Down;
    }

    /// <summary>
    /// Convert the linear volume scale to decibels
    /// </summary>
    public static float LinearToDecibels(int linear)
    {
        float linearScaleRange = 20f;

        // Formula to convert from the linear scale to the logarithmic decibel scale
        return Mathf.Log10((float)linear / linearScaleRange) * 20f;
    }

    /// <summary>
    /// Empty string debug check
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " is empty and must contain a value in object " + thisObject.name.ToString());
            return true;
        }

        return false;
    }

    /// <summary>
    /// null value debug check
    /// </summary>
    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, Object objectToCheck)
    {
        if (objectToCheck == null)
        {
            Debug.Log(fieldName + " is null and must contain a value in object " + thisObject.name.ToString());
            return true;
        }

        return false;
    }

    /// <summary>
    /// List empty or contains null value check - returns true if there is an error
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableObjectToCheck == null)
        {
            Debug.Log(fieldName + " is null in object " + thisObject.name.ToString());
            return true;
        }

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " has null values in object " + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(fieldName + " has null values in object " + thisObject.name.ToString());
            error = true;
        }

        return error;
    }

    /// <summary>
    /// Positive value debug check- if zero is allowed set isZeroAllowed to true. Returns true if there is an error
    /// </summary>
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, int valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain a positive value or zero in object " + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + " must contain a positive value in object " + thisObject.name.ToString());
                error = true;
            }
        }

        return error;
    }

    /// <summary>
    /// positive value debug check - if zero is allowed set isZeroAllowed to true. Returns true if there is an error
    /// </summary>
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain a positive value or zero in object " + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + " must contain a positive value in object " + thisObject.name.ToString());
                error = true;
            }
        }

        return error;
    }

    /// <summary>
    /// positive range debug check - set isZeroAllowed to true if the min and max range values can both be zero. Returns true if there is an error
    /// </summary>
    public static bool ValidateCheckPositiveRange(Object thisObject, string fieldNameMinimum, float valueToCheckMinimum, string fieldNameMaximum, 
        float valueToCheckMaximum, bool isZeroAllowed)
    {
        bool error = false;

        if (valueToCheckMinimum > valueToCheckMaximum)
        {
            Debug.Log(fieldNameMinimum + " must be less than or equal to " + fieldNameMaximum + " in object " + thisObject.name.ToString());
            error = true;
        }

        if (ValidateCheckPositiveValue(thisObject, fieldNameMinimum, valueToCheckMinimum, isZeroAllowed)) error = true;

        if (ValidateCheckPositiveValue(thisObject, fieldNameMaximum, valueToCheckMaximum, isZeroAllowed)) error = true;

        return error;
    }

    /// <summary>
    /// positive range debug check - set isZeroAllowed to true if the min and max range values can both be zero. Returns true if there is an error
    /// </summary>
    public static bool ValidateCheckPositiveRange(Object thisObject, string fieldNameMinimum, int valueToCheckMinimum, string fieldNameMaximum, 
        int valueToCheckMaximum, bool isZeroAllowed)
    {
        bool error = false;
        if (valueToCheckMinimum > valueToCheckMaximum)
        {
            Debug.Log(fieldNameMinimum + " must be less than or equal to " + fieldNameMaximum + " in object " + thisObject.name.ToString());
            error = true;
        }

        if (ValidateCheckPositiveValue(thisObject, fieldNameMinimum, valueToCheckMinimum, isZeroAllowed)) error = true;

        if (ValidateCheckPositiveValue(thisObject, fieldNameMaximum, valueToCheckMaximum, isZeroAllowed)) error = true;

        return error;
    }

    /// <summary>
    /// Get the nearest spawn position to the player
    /// </summary>
    public static Vector3 GetSpawnPositionNearestToPlayer(Vector3 playerPosition)
    {
        Grid grid;
        Vector2Int[] spawnPositions;

        if (!NetworkServer.active && !NetworkClient.active)
        {
            // Single Player
            Room currentRoom = GameManager.Instance.GetCurrentRoom();

            grid = currentRoom.instantiatedRoom.grid;
            spawnPositions = currentRoom.spawnPositionArray;
        }
        else
        {
            // Multiplayer
            RoomNetData currentRoomNetData = GameSessionManager.Instance.GetCurrentRoomNetData();

            InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);

            if (instantiatedRoom == null)
            {
                Debug.LogError($"Instantiated room not ready for room Id {currentRoomNetData.roomId}");
                return playerPosition;
            }

            grid = instantiatedRoom.grid;
            spawnPositions = instantiatedRoom.roomNetData.spawnPositions;
        }

        Vector3 nearestSpawnPosition = new Vector3(10000f, 10000f, 0f);


        // Loop through room spawn positions
        foreach (Vector2Int spawnPositionGrid in spawnPositions)
        {
            // Convert the spawn grid positions to world positions
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPositionGrid);

            if (Vector3.Distance(spawnPositionWorld, playerPosition) < Vector3.Distance(nearestSpawnPosition, playerPosition))
            {
                nearestSpawnPosition = spawnPositionWorld;
            }
        }

        return nearestSpawnPosition;
    }

    /// <summary>
    /// Get closest player's based on specified position
    /// </summary>
    public static Player GetClosestPlayer(Vector3 fromPosition)
    {
        if (!NetworkServer.active) return null;

        Player closestPlayer = null;
        float closestSqrDistance = float.MaxValue;

        foreach (Player player in GameSessionManager.Instance.ServerPlayers)
        {
            if (player == null) continue;
            if (player.health.hasDied) continue;

            float sqrDistance = (player.transform.position - fromPosition).sqrMagnitude;

            if (sqrDistance < closestSqrDistance && !player.isStealthActive)
            {
                closestSqrDistance = sqrDistance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }
}