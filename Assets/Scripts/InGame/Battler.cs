using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public enum AttackType
{
    Melee,
    Ranged,
}

public class Battler : MonoBehaviour
{
    public int damage;
    public int curHp;
    public int maxHp;
    public int armor;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;

    public UnitType unitType = UnitType.Enemy;

    [SerializeField]
    private AttackType attackType = AttackType.Melee;

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

    private float MoveSpeed
    {
        get
        {
            float slowRate = Mathf.Clamp(1 - PassiveManager.Instance.GetSlowRate(curTile), 0, Mathf.Infinity);
            return moveSpeed * slowRate;
        }
    }

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

    private void TileMoveCheck(TileNode nextNode, float distance)
    {
        if (curTile == nextNode)
            return;

        if(distance < Vector3.Distance(curTile.transform.position, transform.position))
        {
            prevTile = curTile;
            curTile = nextNode;
        }
    }

    

    protected IEnumerator Move(TileNode nextNode, System.Action callback = null)
    {
        Vector3 nextPos = nextNode.transform.position;
        float distance = Vector3.Distance(transform.position, nextPos);
        //���� ���� �̵�
        while (distance > 0.001f)
        {
            //�������� ���� �� �����Ӹ���
            if (battleState)
            {
                yield return null;
                continue;
            }

            if (nextNode.curTile == null)
                yield break;

            RotateCharacter(nextPos);
            // ���� ��ġ�� �̵�
            transform.position = Vector3.MoveTowards(transform.position, nextPos, MoveSpeed * Time.deltaTime * GameManager.Instance.timeScale);
            // ���� ��ġ�� ��ǥ ��ġ ���� �Ÿ� ����
            distance = Vector3.Distance(transform.position, nextPos);
            TileMoveCheck(nextNode, distance);
            yield return null;
        }

        callback?.Invoke();
    }

    protected virtual TileNode FindNextNode(TileNode curNode)
    {
        //startNode���� roomDirection�̳� pathDirection�� �ִ� ������ �̿���带 �޾ƿ�
        List<TileNode> nextNodes = UtilHelper.GetConnectedNodes(curNode);
        //�ش� ��忡�� ���� ���� ���� ����
        nextNodes.Remove(prevTile);
        if (crossedNodes != null)
        {
            foreach (TileNode node in crossedNodes)
                nextNodes.Remove(node);
        }

        if (nextNodes.Count == 0)
            return null;

        //�������ϰ�� ����
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
        //prevTile = curTile;
        //curTile = nextNode;
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

            if (nextNode == null) // ���ٸ����� ���
            {
                //�����濡 �����Ҷ����� �ǵ��ư�
                if (lastCrossRoad != null)
                {
                    afterCrossPath.Reverse();
                    for (int i = 0; i < afterCrossPath.Count; i++)
                    {
                        nextNode = afterCrossPath[i];
                        if (moveCoroutine != null)
                            StopCoroutine(moveCoroutine);
                        yield return moveCoroutine = StartCoroutine(Move(nextNode, () => { NodeAction(nextNode); }));
                    }

                    lastCrossRoad = null;
                    afterCrossPath = new List<TileNode>();
                    continue;
                }
                else
                {
                    //���������� ��ã�Ƽ� �ִܷ�Ʈ�� �ٷ��̵�
                    DeadLock_Logic_Move();
                    yield break;
                }
            }

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            yield return moveCoroutine = StartCoroutine(Move(nextNode, () => { NodeAction(nextNode); }));

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

    private bool IsTargetEscaped()
    {
        Collider collider = curTarget.GetComponent<Collider>();
        float dist = Vector3.Distance(collider.ClosestPoint(curTarget.transform.position), transform.position);
        if (dist > attackRange + 0.1f)
            return true;

        return false;
    }

    protected void AttackEndCheck()
    {
        if (curTarget == null || curTarget.isDead)
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
    }

    protected void ExcuteBattle()
    {
        battleState = true;
        //RotateCharacter(curTarget.transform.position);

        //���ݼӵ��� ���� �ִϸ��̼� �ӵ� ����
        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", attackSpeed * GameManager.Instance.timeScale);
            animator.SetBool("Attack", true);
        }
    }

    //�ִϸ��̼� �̺�Ʈ���� �۵�
    public void Attack()
    {
        if (curTarget == null || curTarget.isDead)
            return;

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

    private bool IsTargetValid(Battler target)
    {
        float attackRange = this.attackRange;
        AttackType attackType = this.attackType;
        
        if (target.tag == "King")
        {
            //Ÿ���� �����ϰ�� : ��Ÿ��� 0.5f�� �ٰŸ� �������� ó��
            attackRange = 0.5f;
            attackType = AttackType.Melee;
        }
        
        if (attackType == AttackType.Melee)
        {
            //�ٰŸ��ϰ�� : ������ Ÿ���� ����Ÿ������ Ȯ��
            if (!this.curTile.neighborNodeDic.ContainsValue(target.curTile))
                return false;
        }
        else if(attackType == AttackType.Ranged)
        {
            //���Ÿ��ϰ�� : ������ Ÿ�� ���̿� ���� ������ �������� ������ Ȯ��

        }

        return true;
    }

    protected bool BattleCheck()
    {
        curTarget = null;
        //���� �ֺ� attackRange��ŭ spherecastAll����
        Collider[] colliders = new Collider[10];
        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, LayerMask.GetMask("Character"));
        for (int i = 0; i < colliderCount; i++)
        {
            Battler battle = colliders[i].GetComponent<Battler>();
            if (battle == null || battle == this)
                continue;
            if (battle.unitType == unitType || battle.isDead || !IsTargetValid(battle))
                continue;
            if (curTarget == null) //��Ÿ� ���� ���� ������ Ÿ���ϰ�쿡�� ���������ϵ����Ѵ�.
                curTarget = battle;
            else if (Vector3.Distance(transform.position, battle.transform.position) <
                Vector3.Distance(transform.position, curTarget.transform.position) && battle.tag != "King")
                curTarget = battle;
        }

        if (curTarget == null)
            return false;
        else
            return true;
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
