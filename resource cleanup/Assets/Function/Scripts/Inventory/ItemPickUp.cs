using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private bool Reach;

    public GameObject itemPickUp;
    private GameObject flash;

    public AudioSource pickUpSound;

    void Start()
    {
        Reach = false;
        flash = GameObject.Find("Flash");        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Reach")
        {
            Reach = true;
            pickUpText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Reach")
        {
            Reach = false;
            pickUpText.SetActive(false);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && Reach)
        {
            flash.GetComponent<FlashLight>().batteries += 1;
            pickUpSound.Play();
            Reach = false;
            itemPickUp.SetActive(false);
            Destroy(gameObject);
        }
    }
}

