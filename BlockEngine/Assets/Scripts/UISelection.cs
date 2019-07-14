using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISelection : MonoBehaviour
{

    [SerializeField]
    BlockPlacement player = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            this.transform.position = new Vector3(player.currentBlock * 55, 50f, 0f);
        }
    }
}
