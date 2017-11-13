﻿using System.Collections;
using UnityEngine;

public class SoldierCorpse : Corpse
{
    public Transform GunPlaceholder;

    private GameObject equipped;

    public void Equip(GameObject weapon)
    {
        equipped = Instantiate(weapon);
        equipped.transform.parent = GunPlaceholder;
        equipped.transform.localRotation = Quaternion.identity;
        equipped.transform.localPosition = Vector3.zero;
        StartCoroutine(DropItem(Random.Range(0.2f, 0.5f)));
    }

    private IEnumerator DropItem(float delay)
    {
        yield return new WaitForSeconds(delay);
        var parentObject = equipped.transform.parent.GetComponentInParent<Rigidbody>();
        var equippedRigidbody = equipped.GetComponent<Rigidbody>();
        equippedRigidbody.isKinematic = false;
        equipped.GetComponent<Collider>().enabled = true;
        equipped.transform.parent = null;
        var equipWeapon = equipped.GetComponentInChildren<EquipWeapon>();
        if (equipWeapon != null)
        {
            equipWeapon.EnableCollect();
        }
        if (parentObject != null)
        {
            equippedRigidbody.angularVelocity = parentObject.angularVelocity;
            equippedRigidbody.velocity = parentObject.velocity;
        }
    }
}
