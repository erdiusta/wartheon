using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MaterializeEffect))]
public class ChestItem : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    TextMeshPro textTMP;
    MaterializeEffect materializeEffect;

    [HideInInspector] public bool isItemMaterialized;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        textTMP = GetComponentInChildren<TextMeshPro>();
        materializeEffect = GetComponent<MaterializeEffect>();
    }

<<<<<<< Updated upstream
    public void Initialize(Sprite sprite, string text, Vector3 spawnPosition, Color materializeColor)
=======
    private void OnEnable()
    {
        if (transform.parent != null)
        {
            if (transform.parent.tag == Settings.enemyTag)
            {
                enemy = GetComponentInParent<Enemy>();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.meteor ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            Player player = collision.GetComponent<Player>();

            if (chest == null)
            {
                try
                {
                    if(isColliding) return;

                    // Calculate the distance between the chest item and the player
                    float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

                    // Check if there's currently a nearest chest item and if it's valid
                    if (nearestChestItem == null || nearestChestItem == this || (nearestChestItem != null && Vector2.Distance(player.transform.position, nearestChestItem.transform.position) 
                        > distanceToPlayer))
                    {
                        nearestChestItem = this;
                    }

                    // Only allow the nearest chest item to be interacted with
                    if (nearestChestItem == this)
                    {
                        animator.SetBool(Settings.hovered, true);

                        if (hasWeaponDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                Counter counter = GetComponentInParent<Counter>();

                                if (counter != null)
                                {
                                    if (weaponDetails != null)
                                    {
                                        if (!player.mainHandSlotFilled)
                                        {
                                            if (GameManager.Instance.GetPlayer().coins.coinAmount >= weaponDetails.price && !isPurchasing)
                                            {
                                                isPurchasing = true;
                                                CollectWeaponItem(player);
                                            }
                                            else
                                            {
                                                StaticDialogueHandler.CallInsufficientFundsEvent();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    CollectWeaponItem(player);
                                }
                            }
                        }
                        else if (hasActiveDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {

                                if (!InputManager.Instance.isPressedPreviousFrame)
                                {
                                    // Drop process
                                    if (player.selectedActiveItem.GetCurrentActiveItem() != null && !isPickedUp)
                                    {
                                        Debug.Log("Dropping item from ChestItem.");
                                        player.playerControl.DropProcess(toBeDroppedChestItem, DropType.ActiveItem);
                                    }

                                    // Pick up process
                                    if (player.selectedActiveItem.GetCurrentActiveItem() == null)
                                    {
                                        isColliding = false;
                                        CollectActiveItem(player, this);
                                    }
                                }

                                if (isPickedUp)
                                {
                                    InputManager.Instance.isPressedPreviousFrame = true;
                                }
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasSecondaryPassiveDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                if (!InputManager.Instance.isPressedPreviousFrame)
                                {
                                    if (!isPickedUp)
                                    {
                                        CollectPassiveItem(player);
                                    }
                                }

                                if (isPickedUp)
                                {
                                    InputManager.Instance.isPressedPreviousFrame = true;
                                }
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasPrimaryPassiveDrop)
                        {
                            CollectPassiveItem(player);
                        }
                    }
                    else
                    {
                        animator.SetBool(Settings.hovered, false);
                    }
                }
                catch (InvalidOperationException)
                {
                    Destroy(gameObject);
                }
            }
            else if (chest != null && chest.dropCompleted && chest.chestState == ChestState.weaponItem)
            {
                hasWeaponDrop = true;

                try
                {
                // Calculate the distance between the chest item and the player
                float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

                    // Check if there's currently a nearest chest item and if it's valid
                    if (nearestChestItem == null || nearestChestItem == this || (nearestChestItem != null && Vector2.Distance(player.transform.position, nearestChestItem.transform.position)
                        > distanceToPlayer))
                    {
                        nearestChestItem = this;
                    }

                    // Only allow the nearest chest item to be interacted with
                    if (nearestChestItem == this)
                    {
                        animator.SetBool(Settings.hovered, true);

                        if (hasWeaponDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                CollectWeaponItem(player);
                                chest.chestState = ChestState.empty;
                            }
                        }
                        else if (hasActiveDrop)
                        {
                            CollectActiveItem(player, this);
                            chest.chestState = ChestState.empty;
                        }
                        else if (hasSecondaryPassiveDrop)
                        {
                            CollectPassiveItem(player);
                            chest.chestState = ChestState.empty;
                        }
                    }
                    else
                    {
                        animator.SetBool(Settings.hovered, false);
                    }
                }
                catch (InvalidOperationException)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.meteor ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            animator.SetBool(Settings.hovered, false);

            // Reset nearestChestItem when the player exits the trigger
            if (nearestChestItem == this)
            {
                nearestChestItem = null;
            }
        }
    }

    /// <summary>
    /// Initialize for enemy drops
    /// </summary>
    public void Initialize(IReceivable receivable, Sprite sprite, string text, Vector3 spawnPosition)
>>>>>>> Stashed changes
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;

        StartCoroutine(MaterializeItem(materializeColor, text));
    }

    /// <summary>
    /// Materialize the chest item
    /// </summary>
    IEnumerator MaterializeItem(Color materializeColor, string text)
    {
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] { spriteRenderer };

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(GameResources.Instance.materializeShader, materializeColor, 1f, 
            spriteRendererArray, GameResources.Instance.litMaterial));

        isItemMaterialized = true;
        textTMP.text = text;
    }
}
