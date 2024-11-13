using PixelGenesis._3D.Common.Components;
using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common;

public static class ComponentInitializer
{
    public static void Initialize()
    {
        ComponentFactory.AddComponentFactory(typeof(Transform3DComponent), entity => new Transform3DComponent());
        ComponentFactory.AddComponentFactory(typeof(MeshRendererComponent), entity => new MeshRendererComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
        ComponentFactory.AddComponentFactory(typeof(PerspectiveCameraComponent), entity => new PerspectiveCameraComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
    }
}
