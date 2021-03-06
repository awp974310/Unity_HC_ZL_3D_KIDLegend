﻿using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("速度"), Range(0, 1500)]
    public float speed = 1.5f;
    [Header("玩家資料")]
    public PlayerData data;

    private Rigidbody rig;
    private FixedJoystick joystick;
    private Animator ani;                   // 動畫控制器元件
    private Transform target;               // 目標物件
    private LevelManager levelManager;      // 關卡管理器
    private HpValueManager hpValueManager;  // 血條數值管理器
    private Vector3 posBullet;              // 子彈座標
    private float timer;                    // 計時器

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        ani = GetComponent<Animator>();  // 動畫控制器 = 取得元件<動畫控制器>()
        joystick = GameObject.Find("虛擬搖桿").GetComponent<FixedJoystick>();
        
        //target = GameObject.Find("目標").GetComponent<Transform>();
        target = GameObject.Find("目標").transform;

        levelManager = FindObjectOfType<LevelManager>();                // 透過類行尋找物件 (場景上只有一個)
        hpValueManager = GetComponentInChildren<HpValueManager>();      // 取得子物件元件
    }

    // 固定更新：一秒執行 50 次 - 處理物理行為
    private void FixedUpdate()
    {
        Move();
    }

    //  碰到物件身上有 IsTrigger 碰撞器執行一次
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "傳送區域")
        {
            StartCoroutine(levelManager.NextLevel());   // 協程方法，必須要用 啟動協程
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        float v = joystick.Vertical;
        float h = joystick.Horizontal;

        rig.AddForce(h * speed, 0, v * speed);

        ani.SetBool("跑步開關", v != 0 || h != 0);  // 動畫控制器.設定布林值("參數名稱"，布林值)

        Vector3 pos = transform.position;                               // 玩家做標 = 變形.座標
        target.position = new Vector3(pos.x + h, 0.3f, pos.z + v);      // 目標.座標 = 新 三維向量(玩家.X + 水平，0.3f，玩家.Z + 垂直)

        //transform.LookAt(target); // 這會吃土

        Vector3 posTarget = new Vector3(target.position.x, transform.position.y, target.position.z);    // 目標座標 = 新 三維向量(目標.X，玩家.Y，目標.Z)
        transform.LookAt(posTarget);                                                                    // 玩家變形.看著(目標座標)

        if (v == 0 && h == 0) Attack();
    }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage">接收的傷害值</param>
    public void Hit(float damage)
    {
        if (ani.GetBool("死亡開關")) return;                                // 如果 死亡開關 是勾選 跳出
        data.hp -= damage;
        hpValueManager.SetHp(data.hp, data.hpMax);                          // 更新血量(目前，最大)
        StartCoroutine(hpValueManager.ShowValue(damage, "-", Color.white)); // 啟動協程
        if (data.hp <= 0) Dead();
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        ani.SetBool("死亡開關", true);                   // 死亡動畫
        enabled = false;                                // 關閉此腳本 (this 可省略)
        StartCoroutine(levelManager.ShowRevival());     // 開啟協程(關卡管理器.顯示復活畫面)
    }

    /// <summary>
    /// 復活
    /// </summary>
    public void Revival()
    {
        enabled = true;                                     // 開啟此腳本 (this 可省略)
        ani.SetBool("死亡開關", false);                      // 死亡動畫
        data.hp = data.hpMax;                               // 恢復血量
        hpValueManager.SetHp(data.hp, data.hpMax);          // 更新血量(目前，最大)
        levelManager.HideRevival();                         // 關卡管理器.關閉復活畫面
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (timer < data.cd)            // 如果 計時器 < 冷卻時間
        {
            timer += Time.deltaTime;    // 計時器 累加
        }
        else
        {
            timer = 0;                  // 計時器 歸零
            ani.SetTrigger("攻擊觸發");  // 攻擊動畫
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;                                                                           // 圖示.顏色 = 顏色
        posBullet = transform.position + transform.forward * data.attackZ + transform.up * data.attackY;    // 子彈座標 = 飛龍.座標 + 飛龍前方 * Z + 飛龍上方 * Y
        Gizmos.DrawSphere(posBullet, 0.1f);                                                                 // 圖示.繪製球體(中心點，半徑)
    }
}
