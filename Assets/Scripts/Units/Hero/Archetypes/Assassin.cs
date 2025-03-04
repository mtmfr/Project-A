using System.Collections;
using UnityEngine;

public class Assassin : Hero
{
    [Header("Effect")]
    [SerializeField] private GameObject blood;
    [SerializeField] GameObject bloodSpawn;



    protected override IEnumerator AttackCR(int damage)
    {
        rb.linearVelocity = Vector2.zero;
        anim.Play("Attack");
        var copy = Instantiate(blood, new Vector2(bloodSpawn.transform.position.x, bloodSpawn.transform.position.y), Quaternion.identity);
        CharacterEvent.AttackHit(damage, opponent.GetInstanceID());
        yield return new WaitForSeconds(1 / attSpeed);
        Destroy(copy);
        OnAttack(damage);
    }
}
