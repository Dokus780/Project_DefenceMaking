using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Battler : MonoBehaviour
{
    public int damage;
    public int curHp;
    public int maxHp;
    public int armor;
    public float attackSpeed;
    public float moveSpeed;

    public UnitType unitType = UnitType.Enemy;

    private HpBar hpBar;

    [SerializeField]
    private Transform rotatonAxis;
    [SerializeField]
    private Transform hpPivot;

    public bool battleState = false;
    public Battler curTarget;

    public bool isDead = false;

    protected List<TileNode> crossedNodes = new List<TileNode>();
    protected TileNode prevTile;
    protected TileNode curTile;
    protected TileNode lastCrossRoad;
    protected List<TileNode> afterCrossPath = new List<TileNode>();
    protected Coroutine moveCoroutine = null;

    protected Animator animator;

    [SerializeField]
    private Transform attackZone;

    protected List<Battler> rangedTargets = new List<Battler>();

    private int prevRotLevel = -1;
    private Coroutine rotLevelCoroutine = null;

    [SerializeField]
    private AudioClip attackSound;
    [SerializeField]
    private AudioClip deadSound;

    [SerializeField]
    protected string battlerID;

    private void RemoveBody()
    {
        gameObject.SetActive(false);
    }

    public virtual void Dead()
    {
        hpBar?.UpdateHp();
        isDead = true;
        StopAllCoroutines();
        animator?.SetBool("Die", true);
        Invoke("RemoveBody", 2.5f);
        if(deadSound != null)
            AudioManager.Instance.Play2DSound(deadSound, SettingManager.Instance.fxVolume);
    }

    public virtual void GetDamage(int damage, Battler attacker)
    {
        if (isDead)
            return;

        int finalDamage = damage - armor;
        if (finalDamage <= 0)
            finalDamage = 1;

        curHp -= finalDamage;
        if (curHp <= 0)
            Dead();
    }

    public void RotateCharacter(Vector3 direction)
    {
        if (isDead)
            return;

        Vector3 targetDirection = direction - transform.position;
        if (attackZone != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            attackZone.rotation = targetRotation;
        }

        if (rotatonAxis != null)
        {
            float rotationX = UtilHelper.NormalizeAngle(rotatonAxis.rotation.eulerAngles.x);

            if (targetDirection.x < 0f)
            {
                if (rotationX > 0)
                    rotationX *= -1f;
                rotatonAxis.rotation = Quaternion.Euler(rotationX, 180, rotatonAxis.rotation.eulerAngles.z);
            }
            else
            {
                if (rotationX < 0)
                    rotationX *= -1f;
                rotatonAxis.rotation = Quaternion.Euler(rotationX, 0, rotatonAxis.rotation.eulerAngles.z);
            }
        }
    }

    protected IEnumerator Move(Vector3 nextPos, System.Action callback = null)
    {
        float distance = Vector3.Distance(transform.position, nextPos);
        //다음 노드로 이동
        while (distance > 0.001f)
        {
            //전투상태 진입 시 움직임멈춤
            if (battleState)
            {
                yield return null;
                continue;
            }
            RotateCharacter(nextPos);
            // 다음 위치로 이동
            transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime * GameManager.Instance.timeScale);
            // 현재 위치와 목표 위치 간의 거리 갱신
            distance = Vector3.Distance(transform.position, nextPos);
            yield return null;
        }

        callback?.Invoke();
    }

    protected virtual TileNode FindNextNode(TileNode curNode)
    {
        //startNode에서 roomDirection이나 pathDirection이 있는 방향의 이웃노드를 받아옴
        List<TileNode> nextNodes = UtilHelper.GetConnectedNodes(curNode);
        //해당 노드에서 전에 갔던 노드는 제외
        nextNodes.Remove(prevTile);
        if (crossedNodes != null)
        {
            foreach (TileNode node in crossedNodes)
                nextNodes.Remove(node);
        }

        if (nextNodes.Count == 0)
            return null;

        //갈림길일경우 저장
        if (nextNodes.Count > 1)
        {
            afterCrossPath = new List<TileNode>();
            lastCrossRoad = curNode;
            afterCrossPath.Add(lastCrossRoad);
        }

        TileNode nextNode = nextNodes[UnityEngine.Random.Range(0, nextNodes.Count)];
        return nextNode;
    }

    protected virtual void DeadLock_Logic_Move()
    {

    }

    protected virtual void NodeAction(TileNode nextNode)
    {
        prevTile = curTile;
        curTile = nextNode;
        if (!crossedNodes.Contains(curTile))
            crossedNodes.Add(curTile);

        if(!afterCrossPath.Contains(curTile))
            afterCrossPath.Add(curTile);
    }

    protected IEnumerator MoveLogic()
    {
        if(curTile == null)
            curTile = NodeManager.Instance.startPoint;
        transform.position = curTile.transform.position;
        while (true)
        {
            if (curTile == null) break;
            TileNode nextNode = FindNextNode(curTile);

            if (nextNode == null) // 막다른길일 경우
            {
                //갈림길에 도달할때까지 되돌아감
                if (lastCrossRoad != null)
                {
                    afterCrossPath.Reverse();
                    for (int i = 0; i < afterCrossPath.Count; i++)
                    {
                        nextNode = afterCrossPath[i];
                        if (moveCoroutine != null)
                            StopCoroutine(moveCoroutine);
                        yield return moveCoroutine = StartCoroutine(Move(nextNode.transform.position,
                            () => { NodeAction(nextNode); }));
                    }

                    lastCrossRoad = null;
                    afterCrossPath = new List<TileNode>();
                    continue;
                }
                else
                {
                    //도착지까지 길찾아서 최단루트로 바로이동
                    DeadLock_Logic_Move();
                    yield break;
                }
            }

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            yield return moveCoroutine = StartCoroutine(Move(nextNode.transform.position,
                () => { NodeAction(nextNode); }));

            yield return null;
        }
    }

    protected Battler FindNextTarget(Battler prevTarget = null)
    {
        rangedTargets.Remove(prevTarget);
        if (rangedTargets.Count == 0)
            return null;
        else
        {
            Battler closestTarget = rangedTargets[0];
            float dist = (transform.position - closestTarget.transform.position).magnitude;
            foreach(Battler target in rangedTargets)
            {
                float targetDist = (transform.position - target.transform.position).magnitude;
                if(targetDist < dist)
                {
                    dist = targetDist;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }
    }

    protected void ExcuteBattle()
    {
        if (curTarget == null || curTarget.isDead)
            curTarget = FindNextTarget(curTarget);

        if(curTarget == null)
        {
            if (animator != null)
            {
                animator.SetBool("Attack", false);
                animator.SetFloat("AttackSpeed", attackSpeed * GameManager.Instance.timeScale);
                if (animator.GetCurrentAnimatorStateInfo(0).IsTag("IDLE"))
                    battleState = false;
            }
            return;
        }

        RotateCharacter(curTarget.transform.position);

        //공격속도에 따라 애니메이션 속도 제어
        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", attackSpeed * GameManager.Instance.timeScale);
            animator.SetBool("Attack", true);
        }
    }

    //애니메이션 이벤트에서 작동
    public void Attack()
    {
        if (curTarget == null || curTarget.isDead)
        {
            curTarget = FindNextTarget(curTarget);
            if (curTarget == null)
                return;
        }

        if (attackSound != null)
            AudioManager.Instance.Play2DSound(attackSound, SettingManager.Instance.fxVolume);

        curTarget.GetDamage(damage, this);
    }

    private void ATTACK()
    {
        if (curTarget == null || curTarget.isDead)
        {
            curTarget = FindNextTarget(curTarget);
            if (curTarget == null)
                return;
        }

        if (attackSound != null)
            AudioManager.Instance.Play2DSound(attackSound, SettingManager.Instance.fxVolume);

        curTarget.GetDamage(damage, this);
    }

    public void SetRotation()
    {
        if (rotatonAxis != null)
        {
            rotatonAxis.rotation = TargetRoation(GameManager.Instance.cameraController.Camera_Level);
            prevRotLevel = GameManager.Instance.cameraController.Camera_Level;
        }
    }

    public virtual void Init()
    {
        if(this.hpBar == null)
        {
            string resourcePath = "";
            if (unitType == UnitType.Enemy)
                resourcePath = "Prefab/UI/Adventure_hp_bar";
            else if (unitType == UnitType.Player)
                resourcePath = "Prefab/UI/Monster_hp_bar";


            HpBar hpBar = Resources.Load<HpBar>(resourcePath);
            hpBar = Instantiate(hpBar, GameManager.Instance.cameraCanvas.transform);
            hpBar.Init(this);

            this.hpBar = hpBar;
        }

        SetRotation();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private Quaternion TargetRoation(int camera_Level)
    {
        Vector3 characterRotation = UtilHelper.NormalizeEulerAngles(rotatonAxis.eulerAngles);

        switch (camera_Level)
        {
            case 0:
                characterRotation.x = 70f;
                break;
            case 1:
                characterRotation.x = 70f;
                break;
            case 2:
                characterRotation.x = 50f;
                break;
            case 3:
                characterRotation.x = 20f;
                break;
        }

        if (characterRotation.y > 90f)
        {
            characterRotation.x *= -1f;
            characterRotation.y = 180f;
        }

        return Quaternion.Euler(characterRotation);
    }

    private Quaternion UpdateStartRotation(Quaternion originRotation)
    {
        Vector3 startRotation = UtilHelper.NormalizeEulerAngles(originRotation.eulerAngles);
        Vector3 curRotation = UtilHelper.NormalizeEulerAngles(rotatonAxis.rotation.eulerAngles);

        if (curRotation.x < 0 && startRotation.x < 0)
            return originRotation;
        else if (curRotation.x >= 0 && startRotation.x >= 0)
            return originRotation;

        startRotation.x *= -1f;
        return Quaternion.Euler(startRotation);
    }

    private IEnumerator IUpdateRotation()
    {
        if (rotatonAxis != null)
        {
            int camera_Level = GameManager.Instance.cameraController.Camera_Level;
            Vector3 startRotation = UtilHelper.NormalizeEulerAngles(rotatonAxis.rotation.eulerAngles);
            Vector3 targetRotation = UtilHelper.NormalizeEulerAngles(TargetRoation(camera_Level).eulerAngles);

            float elapsedTime = 0f;
            float lerpTime = 1f;

            while (elapsedTime < lerpTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / lerpTime);

                targetRotation = UtilHelper.NormalizeEulerAngles(TargetRoation(camera_Level).eulerAngles);
                //startRotation = UpdateStartRotation(startRotation);

                Vector3 nextRotation = Vector3.Lerp(startRotation, targetRotation, t);
                rotatonAxis.rotation = Quaternion.Euler(nextRotation);
                yield return null;
            }

            //rotatonAxis.rotation = Quaternion.Euler(targetRotation);
        }
    }

    private void UpdateRotation()
    {
        if (rotLevelCoroutine != null)
            StopCoroutine(rotLevelCoroutine);

        rotLevelCoroutine = StartCoroutine(IUpdateRotation());
    }


    public virtual void Update()
    {
        if (hpBar != null)
            hpBar.UpdateHpBar(hpPivot.position);

        if(prevRotLevel != GameManager.Instance.cameraController.Camera_Level)
        {
            prevRotLevel = GameManager.Instance.cameraController.Camera_Level;
            UpdateRotation();
        }
    }
}
