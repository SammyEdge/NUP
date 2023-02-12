using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeCubes : MonoBehaviour
{
    public GameObject restartButton, explosion;
    private bool collisionSet = false;
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.tag == "Cube" && !collisionSet)    
        {
            for (int i = collision.transform.childCount - 1; i >= 0; --i)
            {
                Transform child = collision.transform.GetChild(i);
                child.gameObject.AddComponent<Rigidbody>();
                child.gameObject.GetComponent<Rigidbody>().AddExplosionForce(70f, Vector3.up, 5f);
                child.SetParent(null);
            }
            restartButton.SetActive(true);
            Camera.main.gameObject.transform.localPosition -= new Vector3(0, 0, 3f);
            Camera.main.gameObject.AddComponent<CameraShake>();


            Vector3 contP = collision.contacts[0].point;
            GameObject explGO = Instantiate(explosion, new Vector3(contP.x, contP.y, contP.z), Quaternion.identity) as GameObject;
            Destroy(explGO, 2.5f);

            if (PlayerPrefs.GetString("music") != "No")
            {
                GetComponent<AudioSource>().Play();
            }

            Destroy(collision.gameObject);
            collisionSet = true;
        }
    }
}
