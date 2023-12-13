using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMeshRendOnStart : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
