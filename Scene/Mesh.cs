using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Engine.Common;

namespace Engine.Scene
{
    public struct VertexData
    {
        public Vector3 Position;
        public Vector3 Normals;
        public Vector2 UVs;
        public Vector3 Tangents;
        public Vector3 BiTangents;

        public VertexData(Vector3 position, Vector3 normals, Vector2 uvs, Vector3 tangents, Vector3 bitangents)
        {
            Position = position;
            Normals = normals;
            UVs = uvs;
            Tangents = tangents;
            BiTangents = bitangents;
        }
    }

    class Mesh : Entity
    {
        int VAO, VBO, EBO;
        public int triangle_count = 0;

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                UpdateModelMatrix();
            }
        }

        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                UpdateModelMatrix();
            }
        }

        public Vector3 Scale
        {
            get => scale;
            set
            {
                scale = value;
                UpdateModelMatrix();
            }
        }

        public Mesh(VertexData[] vertices, uint[] indices)
        {
            Initialize(vertices, indices);
        }

        public Mesh(string file_path)
        {
            ModelImporter.LoadMesh(file_path, out VertexData[] loaded_vertices, out uint[] loaded_indicies);
            Initialize(loaded_vertices, loaded_indicies);
        }

        private void Initialize(VertexData[] vertices, uint[] indices)
        {
            triangle_count = indices.Length / 3;

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 14 * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 14 * sizeof(float), 5 * sizeof(float));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 11 * sizeof(float));

            UpdateModelMatrix();
        }

        private void UpdateModelMatrix()
        {
            model_matrix = Matrix4.Identity;
            model_matrix *= Matrix4.CreateScale(Scale);
            model_matrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
                            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
                            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));
            model_matrix *= Matrix4.CreateTranslation(Position);
        }

        public override void Render()
        {
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, triangle_count * 3, DrawElementsType.UnsignedInt, 0);
        }
    }
}