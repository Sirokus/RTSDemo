using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject UnitPrefab;

    public int RandomSpawnNum = 10;

    public Material[] UnitMaterials;

    public static GameManager Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i< RandomSpawnNum; i++)
        {
            float x = Random.Range(-5f, 5f);
            float z = Random.Range(-5f, 5f);
            Quaternion quat = Quaternion.identity;
            quat.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            GameObject unit = Instantiate(UnitPrefab, new Vector3(x, 0f, z), quat);
            unit.GetComponent<Unit>().ID = Random.Range(0, 2);
            unit.GetComponentInChildren<MeshRenderer>().material = UnitMaterials[unit.GetComponent<Unit>().ID];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
