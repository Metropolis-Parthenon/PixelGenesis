namespace PixelGenesis.ECS.Systems;

public interface ISystem
{
    public bool IsEnabled { get; }
    public void Update();
    public void FixedUpdate();
}
