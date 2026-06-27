using UnityEngine;

public interface IInteractable
{
    bool CanInteract();

    void Interact();

    //提供图标生成高度偏移
    Vector3 GetIconSpawnPos();
}
