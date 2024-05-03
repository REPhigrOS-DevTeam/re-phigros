namespace MainCore.Common
{
    public class EmptySingleton : MonoSingleton<EmptySingleton>
    {
        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}