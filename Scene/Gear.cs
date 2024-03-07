using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Scene
{
    public struct PosNorm
    {
        public Vector3 Position;
        public Vector3 Normals;

        public PosNorm(Vector3 position, Vector3 normals)
        {
            Position = position;
            Normals = normals;
        }
    }

    class Gear : Entity
    {
        int VAO, VBO, EBO;
        private List<PosNorm> vertex_data;
        private List<uint> indices;

        public int triangle_count = 0;
        public int vertice_count = 0;
        public uint teeth_count = 12;
        public float inner_radius = 1.0f;
        public float outer_radius = 1.5f;
        private float height = 0.2f;

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

        public Gear(uint teeth, float innerRadius, float outerRadius, float height)
        {
            teeth_count = teeth;
            inner_radius = innerRadius;
            outer_radius = outerRadius;
            this.height = height;

            Initialize();
        }

        private void Initialize()
        {
            GenerateData();

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertex_data.Count * 6 * sizeof(float), vertex_data.ToArray(), BufferUsageHint.DynamicDraw);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            UpdateModelMatrix();
        }

        private void GenerateData()
        {
            vertex_data = new List<PosNorm>();

            // Top Face Loop
            for (int i = 0; i < teeth_count * 2; i++)
            {
                float step = MathHelper.DegreesToRadians(MathHelper.MapRange(i, 0.0f, teeth_count * 2, 0.0f, 360.0f));
                float x = (float)MathHelper.Cos(step);
                float y = (float)MathHelper.Sin(step);

                // inner loop
                vertex_data.Add(new PosNorm(new Vector3(x * inner_radius, y * inner_radius, height), Vector3.UnitY));
                // outer loop
                vertex_data.Add(new PosNorm(new Vector3(x * outer_radius, y * outer_radius, height), Vector3.UnitY));
            }

            // Bottom Face Loop
            for (int i = 0; i < teeth_count * 2; i++)
            {
                float step = MathHelper.DegreesToRadians(MathHelper.MapRange(i, 0.0f, teeth_count * 2, 0.0f, 360.0f));
                float x = (float)MathHelper.Cos(step);
                float y = (float)MathHelper.Sin(step);

                // inner loop
                vertex_data.Add(new PosNorm(new Vector3(x * inner_radius, y * inner_radius, -height), -Vector3.UnitY));
                // outer loop
                vertex_data.Add(new PosNorm(new Vector3(x * outer_radius, y * outer_radius, -height), -Vector3.UnitY));
            }

            indices = new();
            if (inner_radius < outer_radius)
            {
                for (uint i = 0; i < teeth_count * 4 - 2; i += 2)
                {
                    indices.Add(i);
                    indices.Add(i + 1);
                    indices.Add(i + 2);
                    indices.Add(i + 3);
                    indices.Add(i + 2);
                    indices.Add(i + 1);
                }

                indices.Add(teeth_count * 4 - 2);
                indices.Add(teeth_count * 4 - 1);
                indices.Add(0);
                indices.Add(0);
                indices.Add(teeth_count * 4 - 1);
                indices.Add(1);

                for (uint i = teeth_count * 4; i < teeth_count * 8 - 2; i += 2)
                {
                    indices.Add(i + 2);
                    indices.Add(i + 1);
                    indices.Add(i);
                    indices.Add(i + 1);
                    indices.Add(i + 2);
                    indices.Add(i + 3);
                }

                indices.Add(teeth_count * 4);
                indices.Add(teeth_count * 8 - 1);
                indices.Add(teeth_count * 8 - 2);
                indices.Add(teeth_count * 4 + 1);
                indices.Add(teeth_count * 8 - 1);
                indices.Add(teeth_count * 4);
            }

            else
            {
                // reverse indice direction 3 by 3
                // 1, 2, 3
                // =>
                // 3, 2, 1
            }

            vertice_count = vertex_data.Count;
            triangle_count = indices.Count;

            for (int v = 0; v < vertex_data.Count - 3; v++)
            {
               vertex_data[v] = new PosNorm(
                    vertex_data[v].Position,
                    new Vector3(Vector3.Cross(
                        vertex_data[v].Position - vertex_data[v + 1].Position,
                        vertex_data[v + 2].Position - vertex_data[v].Position))
                ); 
            }
        }

        public void UpdateGearData()
        {
            GenerateData();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertex_data.Count * 6 * sizeof(float), vertex_data.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.DynamicDraw);
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
            GL.DrawElements(PrimitiveType.Triangles, triangle_count, DrawElementsType.UnsignedInt, 0);
        }
    }
}