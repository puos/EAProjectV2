using Debug = EAFrameWork.Debug;

public class EA_GameEvents : EA_FrameWorkEvents
{
    public delegate void OnAttackMsg(EA_CCharBPlayer AttackActor, EA_CCharBPlayer AttackedActor, EA_ItemAttackWeaponInfo AttackedWeaponInfo, uint projectileId);
    public static OnAttackMsg onAttackMsg = delegate (EA_CCharBPlayer AttackActor, EA_CCharBPlayer AttackedActor, EA_ItemAttackWeaponInfo AttackedWeaponInfo, uint projectileId)
    {
        uint attacker_id = (AttackActor != null) ? AttackActor.GetObjID() : CObjGlobal.InvalidObjID;
        uint attacked_id = (AttackedActor != null) ? AttackedActor.GetObjID() : CObjGlobal.InvalidObjID;

        if (showLog) Debug.Log("LogicEvent - onAttackMsg" + " attacker :" + attacker_id + " attacked :" + attacked_id +
                               " attacked weaponType :" + AttackedWeaponInfo.weaponType + " projectile id :" + projectileId);
    };
}