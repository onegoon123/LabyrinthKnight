using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    [Header("업그레이드 패널 참조")]
    public UpgradePanel upgradePanel;

    public void UpgradeHealth() => TryUpgrade(UpgradeType.Health);
    public void UpgradeAttack() => TryUpgrade(UpgradeType.Attack);
    public void UpgradeDefense() => TryUpgrade(UpgradeType.Defense);
    public void UpgradeAttackSpeed() => TryUpgrade(UpgradeType.AttackSpeed);
    public void UpgradeCritChance() => TryUpgrade(UpgradeType.CritChance);
    public void UpgradeCritDamage() => TryUpgrade(UpgradeType.CritDamage);

    private void TryUpgrade(UpgradeType type)
    {
        if (upgradePanel == null)
        {
            Debug.LogWarning("ShopSystem: UpgradePanel이 설정되어 있지 않습니다.");
            return;
        }

        upgradePanel.TryUpgrade(type);
    }
}
