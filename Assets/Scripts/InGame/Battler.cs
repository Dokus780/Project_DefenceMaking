using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void RemoveBody()
    {
        gameObject.SetActive(false);
    }

    public virtual void Dead()
    {
        hpBar.UpdateHp();
        isDead = true;
        StopAllCoroutines();
        animator.SetBool("Die", true);
        Invoke("RemoveBody", 2.5f);
    }

    public virtual void GetDamage(int damage, Battler attacker)
    {
        int finalDamage = damage - armor;
        if (finalDamage <= 0)
            finalDamage = 1;

        curHp -= finalDamage;
        if (curHp <= 0)
            Dead();
    }

    public void RotateCharacter(Vector3 direction)
    {
        Vector3 targetDirection = direction - transform.position;
        if (attackZone != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            attackZone.rotation = targetRotation;
        }

        if (rotatonAxis != null)
        {
            float rotationY = (targetDirection.x > 0f) ? 0f : 180f;
            rotatonAxis.transform.rotation = Quaternion.Euler(rotatonAxis.rotation.eulerAngles.x, rotationY, rotatonAxis.rotation.eulerAngles.z);
        }
    }

    protected IEnumerator Move(Vector3 nextPos, System.Action callback = null)
    {
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

            // ���� ��ġ�� �̵�
            transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime * GameManager.Instance.timeScale);

            // ���� ��ġ�� ��ǥ ��ġ ���� �Ÿ� ����
            distance = Vector3.Distance(transform.position, nextPos);

            RotateCharacter(nextPos);

            yield return null;
        }

        callback?.Invoke();
    }

    protected TileNode FindNextNode(TileNode curNode)
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
                        yield return moveCoroutine = StartCoroutine(Move(nextNode.transform.position,
                            () => { NodeAction(nextNode); }));
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
                if (animator.GetCurrentAnimatorStateInfo(0).IsTag("IDLE"))
                    battleState = false;
            }
            return;
        }

        RotateCharacter(curTarget.transform.position);

        //���ݼӵ��� ���� �ִϸ��̼� �ӵ� ����
        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", attackSpeed * GameManager.Instance.timeScale);
            animator.SetBool("Attack", true);
        }
    }

    //�ִϸ��̼� �̺�Ʈ���� �۵�
    private void Attack()
    {
        if (curTarget == null || curTarget.isDead)
        {
            curTarget = FindNextTarget(curTarget);
            if (curTarget == null)
                return;
        }

        curTarget.GetDamage(damage, this);
    }

    private void LookAtCamera()
    {
        Vector3 cameraPosition = Camera.main.transform.position;

        // ��������Ʈ ������Ʈ�� ��ġ�� ī�޶��� ��ġ�� �������� ���� ���͸� ����մϴ�.
        Vector3 direction = cameraPosition - transform.position;

        // ���� ������ ������ ���մϴ�.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // ������Ʈ�� rotation ���� �����Ͽ� ī�޶� ������ �ٶ󺸵��� �մϴ�.
        float rotationX = rotatonAxis.rotation.eulerAngles.x;
        float rotationY = rotatonAxis.rotation.eulerAngles.y;
        float rotationZ = angle += 70f;
        if (Mathf.Abs(rotationY) > 90f)
            rotationZ *= -1;
        rotatonAxis.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
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

        if(animator == null)
            animator = GetComponentInChildren<Animator>();
    }


    public virtual void Update()
    {
        if (hpBar != null)
            hpBar.UpdateHpBar(hpPivot.position);
    }
}
