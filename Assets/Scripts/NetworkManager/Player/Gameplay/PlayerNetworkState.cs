using Mirror;
using System;

public class PlayerNetworkState : NetworkBehaviour
{
    public bool HasCharacter => characterIndex >= 0;

    public event Action<int> CharacterAssigned;

    // Pure replicated data
    [SyncVar(hook = nameof(OnCharacterIndexChanged))]
    public int characterIndex = -1;

    private void OnCharacterIndexChanged(int oldValue, int newValue)
    {
        if (newValue >= 0) CharacterAssigned?.Invoke(newValue);
    }

    // Server-only setter
    [Server]
    public void SetCharacterIndex(int index)
    {
        characterIndex = index;
    }
}
