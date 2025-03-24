namespace MyGame.Unity
{
    using Arch.Core;
    using MyGame.Model;
    using UnityEngine;

    public class RunGame : MonoBehaviour
    {
        [SerializeField]
        private Shader _shader; // Reference the shader so it is included in build
        private GameModule _gameModule;
        private UnityModule _unityModule;

        private void Start()
        {
            Application.targetFrameRate = 60;
            var world = World.Create();
            _gameModule = new GameModule(world);
            _unityModule = new UnityModule(world);
            var quitQuery = new QueryDescription().WithAll<QuitEvent>();
        }

        private void Update()
        {
            _unityModule.Update();
            _gameModule.Update(Time.deltaTime);
        }
    }
}