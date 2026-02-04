using UnityEngine;
using System.Collections;
using System;

public static class PlayerReadyUtility
{
    public static IEnumerator WaitForLocalPlayer(Action<Player> OnReady)
    {
        Player player;

        while ((player = GameManager.Instance.GetPlayer()) == null || !player.IsReady || !player.IsLocal)
            yield return null;

        OnReady(player);             
    }
}
