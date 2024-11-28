//using PixelGenesis._3D.Common.Components;
//using PixelGenesis._3D.Common.Components.Lighting;
//using PixelGenesis.ECS;

//namespace PixelGenesis._3D.Common;

//public static class ComponentInitializer
//{
//    public static void Initialize()
//    {
//        ComponentFactory.AddComponentFactory(typeof(Transform3DComponent), entity => new Transform3DComponent());
//        ComponentFactory.AddComponentFactory(typeof(MeshRendererComponent), entity => new MeshRendererComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(CubeRendererComponent), entity => new CubeRendererComponent(entity.AddComponentIfNotExist<MeshRendererComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(SphereRendererComponent), entity => new SphereRendererComponent(entity.AddComponentIfNotExist<MeshRendererComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(PerspectiveCameraComponent), entity => new PerspectiveCameraComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(DirectionalLightComponent), entity => new DirectionalLightComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(PointLightComponent), entity => new PointLightComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(SpotLightComponent), entity => new SpotLightComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//    }
//}
