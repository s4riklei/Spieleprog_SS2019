using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer
{
    string name;
    float x;
    float y;
    float z;
    GameObject prefabReference;

    GameObject actualObject;

    public OtherPlayer(string name, float x, float y, float z, GameObject prefabReference)
    {
        this.name = name;
        this.x = x;
        this.y = y;
        this.z = z;
        this.prefabReference = prefabReference;

        this.instantiate();
    }

    public void instantiate()
    {
        this.actualObject = GameObject.Instantiate(prefabReference, new Vector3(x, y, z), Quaternion.identity);
    }

    public void destroy()
    {
        GameObject.Destroy(actualObject);
    }
	
	public void move(Vector3 position)
	{
		actualObject.transform.position = position;
	}
	
	public Vector3 getPosition() {
		return this.actualObject.transform.position;
	}
	
	public string getName() {
		return this.name;
	}

}
