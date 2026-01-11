using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MornLib
{
    internal sealed class MornNovelLifetimeScope : LifetimeScope
    {
        [SerializeField] private MornNovelControllerMono _novelController;
        [SerializeField] private MornNovelSettings _novelSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_novelController);
            builder.RegisterInstance(_novelSettings);
            builder.RegisterBuildCallback(
                resolver =>
                {
                    foreach (var rootGameObject in gameObject.scene.GetRootGameObjects())
                    {
                        resolver.InjectGameObject(rootGameObject);
                    }
                });
        }
    }
}