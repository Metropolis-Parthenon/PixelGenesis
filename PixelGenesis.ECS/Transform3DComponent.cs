using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis.ECS;

public sealed partial class Transform3DComponent : Component
{
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.CreateFromYawPitchRoll(0,0,0);
    public Vector3 Scale = Vector3.One;

    public Vector3 Forward => -Vector3.Transform(Vector3.UnitZ, Rotation);
    public Vector3 Backward => -Vector3.Transform(Vector3.UnitZ, Rotation);

    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);

    Matrix4x4 _localModelMatrix;
    Matrix4x4 _worldModelMatrix;

    public bool HasLocalChanged { get; private set; }
    public bool HasWorldChanged {  get; private set; }

    public void UpdateModelMatrix()
    {        
        var entity = Entity;

        var newLocalModelMatrix = CreateLocalModelMatrix();

        HasLocalChanged = newLocalModelMatrix != _localModelMatrix;        
        _localModelMatrix = newLocalModelMatrix;
                
        if (entity.Parent is null)
        {
            if(HasLocalChanged)
            {
                HasWorldChanged = true;
                _worldModelMatrix = _localModelMatrix;
            }
            else
            {
                HasWorldChanged = false;
            }
        }
        else
        {
            var parentTransform = entity.Parent.Transform;
            if (HasLocalChanged || parentTransform.HasWorldChanged)
            {
                _worldModelMatrix = parentTransform.GetModelMatrix() * _localModelMatrix;
                HasWorldChanged = true;
            }
            else
            {
                HasWorldChanged = false;                
            }
        }

        for (var i = 0; i < Entity.Children.Length; i++)
        {
            var child = Entity.Children[i];
            child.Transform.UpdateModelMatrix();
        }
    }

    public Matrix4x4 GetModelMatrix()
    {      
        return _worldModelMatrix;
    }

    Matrix4x4 CreateLocalModelMatrix()
    {
        var result = Matrix4x4.Identity;

        result *= Matrix4x4.CreateScale(Scale);
        result *= Matrix4x4.CreateFromQuaternion(Rotation);
        result.Translation = Position;

        return result;
    }
}
