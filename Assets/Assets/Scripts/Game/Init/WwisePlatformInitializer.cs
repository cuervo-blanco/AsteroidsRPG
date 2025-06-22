using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class WwisePlatformInitializer : MonoBehaviour {
    void Awake() {
#if UNITY_STANDALONE_LINUX
        AkBasePathGetter.GetCustomPlatformName = (ref string platformName) => {
            platformName = "StandaloneLinux64";
        };
#endif
    }
}
