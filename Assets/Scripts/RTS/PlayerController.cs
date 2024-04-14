using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    [Header("选区")]
    public GameObject SelectionArea;    //框选网格
    private Vector3 StartPos, EndPos;    //框选位置
    public LayerMask groundLayer;   //地面层（用于框选）

    [Header("单位")]
    public List<Unit> SelectedUnits;    //选择的单位
    public LayerMask unitLayer;     //单位层（用于选择）

    [Header("阵列")]
    public float Space = 1;     //阵列的间隔
    public int Cols = 5;        //阵列的行数

    [Header("视角")]
    public float CameraMoveSpeed = 20;

    [Header("单位")]
    public int ID = 0;

    void Start()
    {
        
    }

    

    // Update is called once per frame
    void Update()
    {
        //保证选择的单位有效
        for(int i = 0; i < SelectedUnits.Count;)
        {
            if (!SelectedUnits[i].isActiveAndEnabled)
            {
                SelectedUnits.RemoveAt(i);
            }
            else i++;
        }

        CameraMove();

        Selection();

        MoveAction();
    }

    void CameraMove()
    {
        float InputX = Input.GetAxis("Horizontal");
        float InputZ = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += InputX * CameraMoveSpeed * Time.deltaTime;
        pos.z += InputZ * CameraMoveSpeed * Time.deltaTime;

        transform.position = pos;
    }

    void Selection()
    {
        //按下鼠标
        if (Input.GetMouseButtonDown(0))
        {
            StartPos = GetMouseGamePos();
        }
        //按住鼠标
        else if (Input.GetMouseButton(0))
        {
            //设置框选结束坐标
            EndPos = GetMouseGamePos();

            //计算和鼠标按下时的距离差值
            Vector3 delta = EndPos - StartPos;

            //设置区域位置
            SelectionArea.transform.position = delta / 2 + StartPos;

            //设置区域缩放
            SelectionArea.transform.localScale = new Vector3(delta.x, 0.5f, delta.z);

            //显示盒体延迟到第一次计算完
            SelectionArea.SetActive(true);
        }
        //释放鼠标
        else if (Input.GetMouseButtonUp(0))
        {
            //取消之前的选择
            foreach (Unit unit in SelectedUnits) unit.Select(false);
            SelectedUnits.Clear();

            //盒体大小以及追踪
            Vector3 size = SelectionArea.transform.localScale;
            size.x = Mathf.Abs(size.x / 2);
            size.z = Mathf.Abs(size.z / 2);

            //多选
            if (size.x * size.z > 0.1)  
            {
                RaycastHit[] hitInfos = Physics.BoxCastAll(SelectionArea.transform.position, size, Vector3.up, Quaternion.identity, 0, unitLayer);

                foreach (var hit in hitInfos)
                {
                    Unit unit = hit.collider.GetComponentInParent<Unit>();
                    if (hit.collider.gameObject.activeSelf && unit && unit.ID == ID)
                    {
                        SelectedUnits.Add(unit);
                        unit.Select(true);
                    }
                }
            }
            //单选
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit, 1000, unitLayer))
                {
                    Unit unit = hit.collider.GetComponent<Unit>();
                    if(hit.collider.gameObject.activeSelf && unit && unit.ID == ID)
                    {
                        SelectedUnits.Add(unit);
                        unit.Select(true);
                    }
                }
            }

            //取消和初始化选择区域
            SelectionArea.SetActive(false);
            SelectionArea.transform.localScale = Vector3.zero;
        }
    }

    private void MoveAction()
    {
        if (Input.GetMouseButtonDown(1) && SelectedUnits.Count > 0) //按下右键且操控角色大于0
        {
            //获取鼠标位置和上次位置，并正确初始化
            Vector3 mousePos = GetMouseGamePos();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, 1000, unitLayer))
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if(unit &&unit.isActiveAndEnabled && unit.ID != ID)
                {
                    for (int i = 0; i < SelectedUnits.Count; i++) SelectedUnits[i].MoveTo(unit.transform.position);
                    return;
                }
            }

            //计算阵列中心点
            Vector3 center = Vector3.zero;
            foreach (Unit unit in SelectedUnits) center += unit.transform.position;
            center /= SelectedUnits.Count;

            //按下shfit进行阵列摆布，否则原阵列行进
            if (Input.GetKey(KeyCode.LeftShift))
            {
                float Angle = Quaternion.LookRotation(mousePos - center, center).eulerAngles.y;           //计算行进方向的Y旋转角度（减180度保证少的人的行在后面）
                Vector3[] position = CalculateTargetPos(mousePos, Space, Cols, SelectedUnits.Count, Angle); //计算所有位置（方形阵列，可设置行数，间隔大小）
                AssignPosToMove(SelectedUnits, position);                                                               //为单位分配位置（按照离点最近的单位优先分配）
            }
            else
            {
                //简单计算偏移进行原阵列位移
                Vector3 Offset = mousePos - center;
                for (int i = 0; i < SelectedUnits.Count; i++) SelectedUnits[i].MoveTo(SelectedUnits[i].transform.position + Offset);
            }
        }
    }

    Vector3 GetMouseGamePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000, groundLayer))
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }

    Vector3[] CalculateTargetPos(Vector3 TargetPos, float Space, int cols, int num, float Angle)
    {
        Vector3[] arr = new Vector3[num];

        int Xflag = 1, OffsetX = 0;
        int Zflag = 1, OffsetZ = 0;
        for (int i = 0; i < num; i++)
        {
            arr[i] = Vector3.zero;

            if(i % cols != 0)
            {
                if (((i % cols) - 1) % 2 == 0) OffsetX++;
                if (i > 1 || cols % 2 == 0) Xflag *= -1;
            }
            else
            {
                OffsetX = 0;
                Xflag = 1;
            }
            arr[i].x += OffsetX * Xflag;
            
            if(i % cols == 0 && i != 0)
            {
                if(((i / cols) - 1) % 2 == 0) OffsetZ++;
                if (i / cols > 1 || cols % 2 == 0) Zflag *= -1;
            }
            arr[i].z += OffsetZ * Zflag;

            arr[i] *= Space;
            arr[i] += TargetPos;

            arr[i] = RotateRoundPoint(arr[i], TargetPos, Vector3.up, Angle);

            Debug.DrawLine(arr[i], arr[i] + Vector3.up * 10, Color.red, 10f);
        }

        return arr;
    }

    public Vector3 RotateRoundPoint(Vector3 Position, Vector3 Point, Vector3 Axis, float Angle)
    {
        return Quaternion.AngleAxis(Angle, Axis) * (Position - Point) + Point;
    }

    private void AssignPosToMove(List<Unit> Units, Vector3[] position)
    {
        int End = Units.Count - 1;          //记录尾部
        foreach(Vector3 pos in position)    //遍历每个点
        {
            //寻找该点最远的单位
            int i = 0;
            float MaxDis = 0;
            for(int j = 0; j <= End; j++)
            {
                float dis = Vector3.Distance(Units[j].transform.position, pos);
                if(dis > MaxDis)
                {
                    i = j;
                    MaxDis = dis;
                }
            }

            //控制单位移动到位置
            Units[i].MoveTo(pos);

            //交换当前单位与尾部单位
            Unit temp = Units[i];
            Units[i] = Units[End];
            Units[End] = temp;

            //尾部收缩
            End--;
        }
    }
}
