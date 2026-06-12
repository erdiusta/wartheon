using Mirror;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public NpcType npcType;
    public SoundEffectSO gambleWinSoundEffect;
    public SoundEffectSO gambleLostSoundEffect;

    [HideInInspector] public bool tradeDone;
    [HideInInspector] public bool hintGiven;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.aoeSkill ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            Player player = collision.GetComponent<Player>();

            uint netID = (!NetworkServer.active && !NetworkClient.active) ? 0 : player.NetAuth.netId;
            StaticEventHandler.CallNPCInteractionStartedEvent(npcType, netID); // NPC camera trigger event
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.aoeSkill ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            Player player = collision.GetComponent<Player>();

            StaticEventHandler.CallNPCInteractionEndedEvent(); // NPC camera close event
        }
    }
}
