using System;
using UnityEngine;

public static class StaticLogHandler
{
    public static void LogValidationFailure(string reason, uint playerNetId, ItemSlotStatus itemSlotStatus, int fromIndexNum, int toIndexNum, SlotType toSlot)
    {
        Debug.LogWarning($"[CmdMoveItem Validation Failed] {reason} | Player: {playerNetId}, FromStatus: {itemSlotStatus}," +
            $" ToSlot: {toSlot}, FromIndex: {fromIndexNum}, ToIndex: {toIndexNum}");
    }
}
