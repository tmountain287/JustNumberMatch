using UnityEngine;

[CreateAssetMenu(fileName = "ProvisioningSettings", menuName = "iOS Build/Provisioning Settings", order = 0)]
public class ProvisioningSettings : ScriptableObject
{
    public string provisioningProfileSpecifier = "";
    public string developmentTeam = "";
    public string codeSignIdentity = "Apple Development";
}
