using OpenTK.Mathematics;

namespace Engine.Scene
{
    class Entity
    {
        public Matrix4 model_matrix = Matrix4.Identity;
        public Vector3 position = Vector3.Zero;
        public Vector3 scale = Vector3.One;
        public Vector3 rotation = Vector3.Zero;

        public virtual void Render()
        {

        }
    }
}