namespace Remedy.Damagables
{
    /// <summary>
    /// The type of death, recieved from a Damage Instigator to the Damagable, and used to control death behaviour.
    /// </summary>
    public enum DeathType
    {
        OffMap,
        Shot,
        BluntForce,
        Ragdoll
    }
}