public class InventoryManager : SingletonMonobehaviour<InventoryManager> 
{
    public void MoveItemToWeaponSet(int setIndex, DraggableItem item)
    {
        // Get weapon info from draggable item
        Weapon weapon = GetCurrentWeapon(item);

        // Check if desired set slot is available

        // Update the UI accordingly
        UpdateWeaponSetUI(setIndex);
    }

    private Weapon GetCurrentWeapon(DraggableItem item) => item.GetDraggedWeapon();

    private int GetWeaponSetNumber(DraggableItem item) => item.GetSetNumber();


    private void UpdateWeaponSetUI(int setIndex)
    {
        // Update the UI to reflect the changes in the weapon sets
    }
}
