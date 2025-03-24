namespace MyGame.Unity
{
    using Arch.Core;
    using MyGame.Model;
    using UnityEngine;

    public class UnityModule
    {
        private readonly QueryDescription _inputsQuery;
        private readonly Material _material;
        private readonly Mesh _mesh;
        private readonly QueryDescription _rectangleQuery;
        private readonly QueryDescription _windowQuery;
        private readonly World _world;

        public UnityModule(World world)
        {
            _world = world;
            _rectangleQuery = new QueryDescription().WithAll<Rectangle, Position>();
            _windowQuery = new QueryDescription().WithAll<Window>();
            _inputsQuery = new QueryDescription().WithAll<PlayerInput>();
            _mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, 0.5f, 0),
                    new Vector3(0.5f, 0.5f, 0)
                },
                triangles = new[]
                {
                    0,
                    2,
                    1,
                    1,
                    2,
                    3
                }
            };

            _mesh.RecalculateNormals();

            _material = new Material(Shader.Find("Unlit/Color"));
        }

        public void Update()
        {
            var cam = Camera.main;
            _world.Query(in _rectangleQuery,
                         (ref Rectangle r, ref Position pos) =>
                         {
                             var matrix = Matrix4x4.TRS(cam.ScreenToWorldPoint(new Vector3(pos.X, pos.Y, 1)),
                                                        Quaternion.identity,
                                                        new Vector3(r.Width, r.Height, 1));

                             Graphics.DrawMesh(_mesh, matrix, _material, 0);
                         });

            _world.Query(in _windowQuery,
                         (ref Window window) =>
                         {
                             window.Width = Screen.width;
                             window.Height = Screen.height;
                         });

            _world.Query(in _inputsQuery,
                         (ref PlayerInput input) =>
                         {
                             input.Horizontal = Input.GetAxis("Horizontal");
                             input.Vertical = Input.GetAxis("Vertical");
                         });
        }
    }
}