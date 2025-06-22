#if UNITY_EDITOR || UNITY_STANDALONE_LINUX
using System;
using UnityEngine;

/// <summary>
/// Minimal Linux platform support for the Unity-Wwise integration you’re using.
/// Mirrors the pattern in AkCommonPlatformSettings:
///     • stores a concrete Advanced-settings object
///     • returns it via GetAdvancedSettings()
/// </summary>
[Serializable]
public class AkLinuxPlatformInitializationSettings : AkCommonPlatformSettings
{
    // Concrete advanced-settings object for Linux
    public AkLinuxAdvancedSettings advancedSettings = new AkLinuxAdvancedSettings();
    public AkCommonUserSettings userSettings = new AkCommonUserSettings();
    public AkCommonCommSettings commsSettings = new AkCommonCommSettings();

    // Mandatory override — must return AkCommonAdvancedSettings
    protected override AkCommonUserSettings GetUserSettings()
    {
        return userSettings;
    }

	protected override AkCommonCommSettings GetCommsSettings()
    {
        return commsSettings;
    }
    protected override AkCommonAdvancedSettings GetAdvancedSettings()
    {
        return advancedSettings;
    }
}

/// <summary>
/// Concrete advanced-settings class for Linux.
/// It only needs to inherit from AkCommonAdvancedSettings; we can add
/// Linux-specific fields here later if desired.
/// </summary>
[Serializable]
public class AkLinuxAdvancedSettings : AkCommonAdvancedSettings
{
    // leave empty unless you need Linux-specific tweaks
}
#endif

