using UnityEngine;

public class Bootstrapper {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute() {
        Object systems = Object.Instantiate(Resources.Load("Systems"));
        systems.name = "Systems";

        Object.DontDestroyOnLoad(systems);
    } 
}
