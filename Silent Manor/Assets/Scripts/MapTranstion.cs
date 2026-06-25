using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class MapTranstion : MonoBehaviour
{
    [SerializeField] private BoxCollider2D targetMapBound;
    private CinemachineConfiner2D confiner;
    private UI_MapTranstionFade mapTranstionFade;
    [SerializeField] private Direction dir;
    public float offset = 1.5f;

    enum Direction{ Up, Down, Left, Right }

    void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }

    void Start()
    {
        mapTranstionFade = UI.Instance.mapTranstion;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(MapTranstionRoutine(collision.gameObject));
        }
    }

    private IEnumerator MapTranstionRoutine(GameObject player)
    {
        InputManager.input.Disable();
        mapTranstionFade.FadeOut();
        // 等待淡出完成
        yield return new WaitForSeconds(mapTranstionFade.fadeDuration);
        
        confiner.BoundingShape2D = targetMapBound;
        UpdatePlayerPos(player); // 淡出后再移动玩家
        
        yield return new WaitForSeconds(1f); // 可选：短暂停留
        mapTranstionFade.FadeIn();
        // 等待淡入完成后再开启输入
        yield return new WaitForSeconds(mapTranstionFade.fadeDuration);
        InputManager.input.Enable();
    }

    private void UpdatePlayerPos(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        switch(dir)
        {
            case Direction.Up:
                newPos.y += offset;
                break;
            case Direction.Down:
                newPos.y -= offset;
                break;
            case Direction.Left:
                newPos.x -= offset;
                break;
            case Direction.Right:
                newPos.x += offset;
                break;
        }

        player.transform.position = newPos;
    }
}
