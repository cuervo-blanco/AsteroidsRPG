/*******************************************************************************
Minimal Linux platform support for the Unity–Wwise integration.
*******************************************************************************
*/

#if (UNITY_STANDALONE_LINUX && !UNITY_EDITOR) || UNITY_EDITOR_LINUX
// ─────────────────────────────────────────────────────────────────────────────
// 1)  USER-LEVEL SETTINGS (sample-rate, plugin path, etc.). You can expand
//     these later; for now we only expose sample-rate to match the Mac stub.
// ─────────────────────────────────────────────────────────────────────────────
public partial class AkCommonUserSettings
{
    partial void SetSampleRate(AkPlatformInitSettings settings)
    {
        settings.uSampleRate = m_SampleRate;       // use inspector-chosen rate
    }

    protected partial string GetPluginPath()
    {
#if UNITY_EDITOR_LINUX
        // Editor: point to Integration package’s DSP libs so play-mode works
        return System.IO.Path.GetFullPath(
            AkUtilities.GetPathInPackage("Runtime/Plugins/Linux/x86_64/")
        );
#else
        // Player build: Plugins/<arch>/ .so files sit next to the executable
        return System.IO.Path.Combine(UnityEngine.Application.dataPath,
            "Plugins" + System.IO.Path.DirectorySeparatorChar);
#endif
    }
}
#endif  // UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX


// ─────────────────────────────────────────────────────────────────────────────
// 2)  FULL PLATFORM-SETTINGS SCRIPTABLEOBJECT
//     (mirrors AkMacSettings / AkWindowsSettings structure)
// ─────────────────────────────────────────────────────────────────────────────
public class AkLinuxSettings : AkWwiseInitializationSettings.PlatformSettings
{
#if UNITY_EDITOR
    // Register once so UpdatePlatforms() knows about “Linux”
    [UnityEditor.InitializeOnLoadMethod]
    private static void AutomaticPlatformRegistration()
    {
        if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
            return;

        RegisterPlatformSettingsClass<AkLinuxSettings>("Linux");
    }
#endif // UNITY_EDITOR


    // ─── ADVANCED SETTINGS CLASS ────────────────────────────────────────────
    [System.Serializable]
    public class PlatformAdvancedSettings : AkCommonAdvancedSettings
    {
        // Add Linux-specific advanced knobs here if you need them later

        public override void CopyTo(AkPlatformInitSettings settings)
        {
            // Nothing extra for now.
        }
    }


    protected override AkCommonUserSettings GetUserSettings()     => UserSettings;
    protected override AkCommonAdvancedSettings GetAdvancedSettings() => AdvancedSettings;
    protected override AkCommonCommSettings GetCommsSettings()    => CommsSettings;

    [UnityEngine.HideInInspector] public AkCommonUserSettings  UserSettings;
    [UnityEngine.HideInInspector] public PlatformAdvancedSettings AdvancedSettings;
    [UnityEngine.HideInInspector] public AkCommonCommSettings  CommsSettings;
}

