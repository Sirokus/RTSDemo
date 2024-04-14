using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Unit : MonoBehaviour
{
    #region Components
    public Rigidbody rb { get; private set; }
    public CapsuleCollider cd { get; private set; }
    #endregion

    [Header("组件")]
    public GameObject SelectCircle;
    public Vector3 EndPos;

    [Header("属性")]
    public float Speed = 5;
    public float SpeedRandomRange = 0.5f;

    [Header("战斗")]
    public int ID = 0;
    public float FindRange = 10f;
    public float AttackRange = 0.8f;
    public float Damage = 20;
    public float Health = 100;

    public bool Attacked = false;
    public bool IsMoving = false;

    public float StopCoolDown = 0.5f;
    public float StopTimer = 0;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cd = GetComponent<CapsuleCollider>();

        SelectCircle.SetActive(false);

        EndPos  = transform.position;

        gameObject.GetComponentInChildren<MeshRenderer>().material = GameManager.Instance.UnitMaterials[ID];
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActiveAndEnabled) return;

        if(StopTimer < 0)
        {
            StopTimer = StopCoolDown;
            EndPos = transform.position;
        }

        if(GetVelocity() < 0.5)
        {
            StopTimer -= Time.deltaTime;
        }
        else
        {
            StopTimer = StopCoolDown;
        }

        if(!IsMoving && !Attacked)
        {
            RaycastHit[] Hits = Physics.SphereCastAll(transform.position, FindRange, Vector3.up, 1000);
            foreach(RaycastHit h in Hits)
            {
                Unit unit = h.collider.GetComponent<Unit>();
                if(unit && unit.isActiveAndEnabled && unit.ID != ID)
                {
                    MoveTo(unit.transform.position);
                    break;
                }
            }
        } 

        if(!Attacked)
        {
            RaycastHit[] Hits = Physics.SphereCastAll(transform.position, AttackRange, Vector3.up, 1000);
            foreach(RaycastHit h in Hits)
            {
                Unit unit = h.collider.GetComponent<Unit>();
                if(unit && unit.isActiveAndEnabled && unit.ID != ID)
                {
                    StartCoroutine("Attack", unit);
                    break;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (Health <= 0) Destroy(gameObject);
    }

    IEnumerator Attack(Unit unit)
    {
        Attacked = true;

        unit.Health -= Damage;

        if(unit.Health <= 0)
        {
            unit.gameObject.SetActive(false);
            EndPos = transform.position;
        }

        yield return new WaitForSeconds(0.5f);

        Attacked = false;
    }

    private void FixedUpdate()
    {
        if(!Mathf.Approximately((EndPos - transform.position).magnitude, 0) && Mathf.Abs(EndPos.y - transform.position.y) <= 1f && !Attacked)
        {
            transform.position = Vector3.MoveTowards(transform.position, EndPos, Speed * Time.fixedDeltaTime);
            IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }
    }

    public void Select(bool isSelected)
    {
        SelectCircle.SetActive(isSelected);
    }

    public void MoveTo(Vector3 pos)
    {
        Speed += Random.Range(-SpeedRandomRange, SpeedRandomRange);
        EndPos = pos;
        Attacked = false;
        transform.LookAt(pos);
    }

    public virtual float GetVelocity() { return rb.velocity.magnitude; }
    public virtual Vector3 GetVelocityV() { return rb.velocity; }
    public virtual void SetVelocity(Vector3 velocity) { rb.velocity = velocity; }
}
