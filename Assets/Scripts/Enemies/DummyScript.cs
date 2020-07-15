using System.Collections;
using System.Collections.Generic;
using RandomDungeon.Combat;
using UnityEngine;

[RequireComponent(typeof(CombatBody))]
public class DummyScript : MonoBehaviour
{
    private CombatBody body;

    void Start()
    {
        body = GetComponent<CombatBody>();
        body.DeathEvent += Die;
    }

    private void OnDestroy()
    {
        body.DeathEvent -= Die;
    }

    private void Die() {
        Debug.Log("Die");
        Destroy(gameObject);
    }
}
