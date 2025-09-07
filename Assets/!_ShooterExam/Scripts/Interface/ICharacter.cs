using UnityEngine;

public interface ICharacter
{
    public int Hp { get; set; }
    public int Power { get; set; }
    public void Heal();
    public void Damage();
    public void Death();
}
