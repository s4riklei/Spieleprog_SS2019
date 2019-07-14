using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacement : MonoBehaviour
{

    [SerializeField]
    GameObject blockIndicator = null;

    [SerializeField]
    GameObject grid = null;

    [SerializeField]
    public GameObject[] blockSelectionList = null;

    [SerializeField]
    public int currentBlock = 0;
	
	[SerializeField]
	GameObject player = null;

    private Vector3 currentBlockPosition;

    public List<GameObject> blockPlacementList = new List<GameObject>();
	
	private ChatControl chatControlInstance = null;



    // Start is called before the first frame update
    void Start()
    {
		this.chatControlInstance = player.GetComponent<ChatControl>();
    }

    // Update is called once per frame
    void Update()
    {
        toggleIndicator();
        updateIndicatorPosition();
        toggleGrid();
        updateGridPosition();
        cycleThroughMaterials();
        blockAction();
    }

    void toggleIndicator()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (blockIndicator != null)
            {
                if (Input.GetKeyDown(Constants.indicatorKey))
                {
                    blockIndicator.SetActive(!blockIndicator.activeInHierarchy);
                }
            }
        }
    }

    void updateIndicatorPosition()
    {
        Vector3 avatarBlockCenter = new Vector3(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y), Mathf.Floor(transform.position.z)) + new Vector3(.5f, .5f, .5f);

        if (transform.forward.x >= .7f)
        {
            blockIndicator.transform.position = avatarBlockCenter + new Vector3(1f, 0, 0);
        }
        if (transform.forward.x <= -.7f)
        {
            blockIndicator.transform.position = avatarBlockCenter + new Vector3(-1f, 0, 0);
        }
        if (transform.forward.y >= .7f)
        {
            blockIndicator.transform.position = avatarBlockCenter + new Vector3(0, 1f, 0);
        }
        if (transform.forward.y <= -.7f)
        {
            blockIndicator.transform.position = avatarBlockCenter + new Vector3(0, -1f, 0);
        }
        if (transform.forward.z >= .7f)
        {
            blockIndicator.transform.position = avatarBlockCenter + new Vector3(0, 0, 1f);
        }
        if (transform.forward.z <= -.7f)
        {
            blockIndicator.transform.position = avatarBlockCenter + new Vector3(0, 0, -1f);
        }

        currentBlockPosition = blockIndicator.transform.position;
    }

    void toggleGrid()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (grid != null)
            {
                if (Input.GetKeyDown(Constants.gridKey))
                {
                    grid.SetActive(!grid.activeInHierarchy);
                }
            }
        }
    }

    void updateGridPosition()
    {
        Vector3 avatarBlockPosition = new Vector3(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y), Mathf.Floor(transform.position.z));
        grid.transform.position = avatarBlockPosition;
    }

    void cycleThroughMaterials()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (blockSelectionList != null)
            {
                if (Input.GetAxis(Constants.mouseWheel) < 0)
                {
                    currentBlock = currentBlock == blockSelectionList.Length - 1 ? 0 : ++currentBlock;
                }
                if (Input.GetAxis(Constants.mouseWheel) > 0)
                {
                    currentBlock = currentBlock == 0 ? blockSelectionList.Length - 1 : --currentBlock;
                }
            }
        }
    }

    void blockAction()
    {
       if (blockSelectionList != null && blockSelectionList.Length > 0 && Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(Constants.blockKey))
        {
            GameObject result = blockPlacementList.Find(x => x.transform.position == currentBlockPosition);

            if (result == null)
            {
                GameObject newBlock = GameObject.Instantiate(blockSelectionList[currentBlock], currentBlockPosition, Quaternion.identity);
				blockPlacementList.Add(newBlock);
				this.chatControlInstance.sendBlockInformationToServer(newBlock.GetComponent<MeshRenderer>().material.name, currentBlockPosition);
            }
            else
            {
				this.chatControlInstance.sendBlockInformationToServer(result.GetComponent<MeshRenderer>().material.name, result.transform.position);
                blockPlacementList.Remove(result);
                GameObject.Destroy(result);
            }
        }
    }
}