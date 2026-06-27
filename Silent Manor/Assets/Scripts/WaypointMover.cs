using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class WaypointData
{
    public Transform point;
    public float waitSeconds = 1f;
}

public class WaypointMover : MonoBehaviour
{
    [Header("路径配置")]
    public List<WaypointData> waypointDatas;
    public float moveSpeed = 2f;
    public bool loopPingPong; // true=往返巡逻，false=走到终点停止
    public Transform waypointParent;

    private int currentIndex;
    private bool isWaiting;
    private bool isForward = true; // true正向 0→1→2；false反向 2→1→0
    private Animator anim;

    // 新增：对话锁定，对话期间禁止移动
    [HideInInspector] public bool isLockedByDialogue;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // 过滤空路径点
        waypointDatas.RemoveAll(data => data.point == null);
        if (waypointDatas.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name} 未配置任何有效路径点！");
            enabled = false;
        }
    }

    void Update()
    {
        // 新增判断：对话锁定 直接停止移动
        if (isLockedByDialogue)
            return;

        // 游戏暂停 / 正在等待 停止移动
        if (GameManager.Instance.IsGamePaused || isWaiting)
        {
            anim.SetBool("isWalking", false);
            return;
        }

        MoveToWayPoint();
    }

    private void MoveToWayPoint()
    {
        Transform target = waypointDatas[currentIndex].point;
        Vector2 dir = (target.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        anim.SetFloat("InputX", dir.x);
        anim.SetFloat("InputY", dir.y);
        anim.SetBool("isWalking", true);


        // 到达目标点
        if (Vector2.Distance(target.position, transform.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        // 使用当前路径点独立等待时间
        float pointWait = waypointDatas[currentIndex].waitSeconds;

        anim.SetBool("isWalking", false);
        yield return new WaitForSeconds(pointWait);

        // 计算下一个索引
        CalculateNextIndex();

        isWaiting = false;
    }

    // 核心：往返逻辑计算下一个点
    void CalculateNextIndex()
    {
        int totalCount = waypointDatas.Count;
        // 只有一个点，原地不动
        if (totalCount <= 1) return;

        // 正向走
        if (isForward)
        {
            currentIndex++;
            // 走到最后一个，切换反向
            if (currentIndex >= totalCount - 1)
            {
                isForward = false;
            }
        }
        // 反向走
        else
        {
            currentIndex--;
            // 走到第一个，切换正向
            if (currentIndex <= 0)
            {
                isForward = true;
            }
        }

        // 不循环模式：走到头直接停住
        if (!loopPingPong)
        {
            // 正向走到终点 / 反向走到起点，停止移动
            if ((isForward && currentIndex == totalCount - 1) || (!isForward && currentIndex == 0))
            {
                enabled = false;
            }
        }
    }

    [ContextMenu("批量导入子物体为路径点")]
    void ImportChildrenAsWaypoints()
    {
        Transform parent = waypointParent;
        waypointDatas.Clear();
        for (int i = 0; i < parent.childCount; i++)
        {
            WaypointData data = new WaypointData();
            data.point = parent.GetChild(i);
            data.waitSeconds = 1f;
            waypointDatas.Add(data);
        }
        Debug.Log("已导入全部子物体作为路径点");
    }
}
