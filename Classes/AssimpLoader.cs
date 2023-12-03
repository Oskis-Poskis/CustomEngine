using Assimp;
using OpenTK.Mathematics;

using Engine.Scene; 

namespace Engine.Common
{
    public static class ModelImporter
    {
        public static void LoadMesh(string path, out VertexData[] loaded_vertices, out uint[] loaded_indicies)
        {
            var importer = new AssimpContext();
            var scene = importer.ImportFile(path,
                PostProcessPreset.TargetRealTimeMaximumQuality |
                PostProcessSteps.GenerateSmoothNormals |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.Triangulate);

            Assimp.Mesh mesh = scene.Meshes[0];
            int vertex_count = mesh.VertexCount;
            int index_count = mesh.FaceCount * 3;

            loaded_vertices = new VertexData[vertex_count];

            for (int i = 0; i < vertex_count; i++)
            {
                loaded_vertices[i].Position = FromVector(mesh.Vertices[i]);
                loaded_vertices[i].Normals = FromVector(mesh.Normals[i]);
                loaded_vertices[i].UVs = FromVector2D(new Vector2D(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
                loaded_vertices[i].Tangents = FromVector(mesh.Tangents[i]);
                loaded_vertices[i].BiTangents = FromVector(mesh.BiTangents[i]);
            }

            loaded_indicies = new uint[index_count];
            for (int i = 0, j = 0; i < mesh.FaceCount; i++)
            {
                var face = mesh.Faces[i];
                for (int k = 0; k < 3; k++)
                {
                    loaded_indicies[j++] = (uint)face.Indices[k];
                }
            }
        }

        private static Vector3 FromVector(Vector3D vec)
        {
            Vector3 v;
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            return v;
        }

        private static Vector2 FromVector2D(Vector2D vec)
        {
            Vector2 v;
            v.X = vec.X;
            v.Y = vec.Y;
            return v;
        }
    }
}