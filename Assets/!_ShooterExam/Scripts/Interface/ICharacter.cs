using UnityEngine;

public interface ICharacter
{
    public void Heal();
    public void Damage(float damage);
    public void RpcDeath();
}
