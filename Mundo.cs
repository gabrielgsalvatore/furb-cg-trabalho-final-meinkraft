using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Input;
using CG_Biblioteca;
using System.Threading.Tasks;
using System.Threading;

namespace furb_cg_trabalho_final_meinkraft
{
    class Mundo : GameWindow
    {

        private Vector2 lastPos;

        private bool firstMove = true;
        private static Mundo singletonMundo = null;
        private Camera cam = new Camera(new Vector3(0.0f, 15.0f, 0.0f), 1.0f);
        private double time;
        private bool jumping = false;
        private Cubo selecionado = null;
        private Vector3 castOrigin;
        private Vector3 castDestiny;
        private BBox camBbox = new BBox(-1, -1, -1, 1, 1, 1);
        private Dictionary<String, Cubo> map = new Dictionary<String, Cubo>();
        private int mapX = 30; //Ajusta o tamanho do mundo
        private int mapY = 5;
        private int mapZ = 30;
        private static int width;
        private static int height;
        private int currentPlayerHeight;
        private int currentPlayerX;
        private int currentPlayerZ;
        private bool useDisplayList = false;

        public static int Width { get => width; set => width = value; }
        private Cubo crosshairPivot = new Cubo("pivot", null);
        private Cubo crosshair = new Cubo("Aim", null);


        public static int Height { get => height; set => height = value; }
        private Mundo(int width, int height) : base(width, height) { }

        public static Mundo GetInstance(int width, int height)
        {
            if (singletonMundo == null)
            {
                Width = width;
                Height = height;
                singletonMundo = new Mundo(width, height);
            }
            return singletonMundo;
        }

        public void adicionarBloco()
        {
            if (selecionado != null)
            {
                String[] coord = selecionado.ToString().Split(',');
                int x = int.Parse(coord[0]);
                int y = int.Parse(coord[1]) + 1;
                int z = int.Parse(coord[2]);
                String rotulo = x + "," + y + "," + z;
                if (!map.ContainsKey(rotulo))
                {
                    Cubo cube = new Cubo(rotulo, null);
                    cube.EscalaXYZ(1, 1, 1);
                    cube.TranslacaoXYZ(x * 2, y * 2, z * 2);
                    cube.BBox.AtualizarBbox(new Vector3(x * 2, y * 2, z * 2));
                    map.Add(rotulo, cube);
                }

            }
        }

        private void popularMapa()
        {
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    for (int z = 0; z < mapZ; z++)
                    {
                        String rotulo = x + "," + y + "," + z;
                        Cubo cube = new Cubo(rotulo, null);
                        cube.EscalaXYZ(1, 1, 1);
                        cube.TranslacaoXYZ(x * 2, y * 2, z * 2);
                        cube.BBox.AtualizarBbox(new Vector3(x * 2, y * 2, z * 2));
                        map.Add(rotulo, cube);
                    }
                }
            }
        }


        private void desenharMapa()
        {
            foreach (var c in map)
            {
                c.Value.Desenhar();
            }

        }
        protected override void OnLoad(EventArgs e)
        {

            cam.Fov = 70f;
            camBbox.Atualizar(new Ponto4D(cam.Position.X, cam.Position.Y, cam.Position.Z));
            CursorVisible = false;
            base.OnLoad(e);
            GL.ClearColor(Color.Gray);
            popularMapa();
            if (useDisplayList)
            {
                var index = GL.GenLists(1);
                if (index != 0)
                {
                    GL.NewList(index, ListMode.Compile);
                    desenharMapa();
                    GL.EndList();
                }
            }
            else
            {
                desenharMapa();
            }
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused) // check to see if the window is focused  
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
                showSelected();
            }

            base.OnMouseMove(e);
        }

        private async void Jump()
        {
            jumping = true;
            for (int i = 0; i < 35; i++)
            {
                cam.Position = Vector3.Add(cam.Position, new Vector3(0, 10 * (float)time, 0));
                await Task.Delay(01);
            }
            await Task.Delay(20);
            jumping = false;
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = cam.GetProjectionMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }
        protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (!jumping && isGrounded())
                {
                    Jump();
                }

            }
            if (e.Key == Key.Plus)
            {
                useDisplayList = !useDisplayList;
                Console.WriteLine("Usando DisplayList: "+useDisplayList);
                if (useDisplayList)
                {
                    var index = GL.GenLists(1);
                    if (index != 0)
                    {
                        GL.NewList(index, ListMode.Compile);
                        desenharMapa();
                        GL.EndList();
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button.Equals(MouseButton.Left))
            {
                map.Remove(selecionado.ToString());
            }
            if (e.Button.Equals(MouseButton.Right))
            {
                adicionarBloco();
            }

            if (selecionado != null && useDisplayList)
            {
                GL.DeleteLists(1, 1);
                var index = GL.GenLists(1);
                if (index != 0)
                {
                    GL.NewList(index, ListMode.Compile);
                    desenharMapa();
                    GL.EndList();
                }

            }


        }

        private void showSelected()
        {
            var mouse = OpenTK.Input.Mouse.GetState();
            castRay(cam.Front.X, cam.Front.Y);
            float minDistance = float.MaxValue;
            foreach (var item in map)
            {
                String[] coord = item.Key.Split(',');
                if (int.Parse(coord[1]) >= currentPlayerHeight - 1 && int.Parse(coord[0]) <= currentPlayerX * 5 && int.Parse(coord[2]) <= currentPlayerZ * 5)
                {
                    Cubo objeto = item.Value;
                    Vector3 distObjeto = new Vector3((float)objeto.Matriz.ObterElemento(12), (float)objeto.Matriz.ObterElemento(13), (float)objeto.Matriz.ObterElemento(14));
                    float distance = getDistance(distObjeto);
                    if (distance <= minDistance)
                    {
                        minDistance = distance;
                        selecionado = objeto;
                    }
                }
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.Escape))
            {
                this.Close();
            }
            if (keyState.IsKeyDown(Key.W))
            {
                cam.Position += cam.ViewFront * 4.0f * (float)time;

            }
            if (keyState.IsKeyDown(Key.S))
            {
                cam.Position -= cam.ViewFront * 4.0f * (float)time;

            }
            if (keyState.IsKeyDown(Key.A))
            {
                cam.Position -= cam.Right * 4.0f * (float)time;

            }
            if (keyState.IsKeyDown(Key.D))
            {
                cam.Position += cam.Right * 4.0f * (float)time;

            }

            var mouse = OpenTK.Input.Mouse.GetState();
            time = e.Time;
            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                float deltaX = mouse.X - lastPos.X;
                float deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                cam.Yaw += deltaX;
                cam.Pitch -= deltaY;

            }

            base.OnUpdateFrame(e);


        }


        public void castRay(float x, float y)
        {
            x += width / 2; //ajuste para manter relativamente no centro da tela
            y += height / 2;
            float mouseX = (2f * x) / (Mundo.Width) - 1;
            float mouseY = (2f * y) / Mundo.Height - 1;

            Vector4 startRay = new Vector4(mouseX, mouseY, -1, 1);
            Vector4 endRay = new Vector4(mouseX, mouseY, 1, 1);


            Matrix4 trans = cam.GetViewMatrix() * cam.GetProjectionMatrix();
            trans.Invert();
            startRay = Vector4.Transform(startRay, trans);
            endRay = Vector4.Transform(endRay, trans);

            castOrigin = startRay.Xyz / startRay.W;
            castDestiny = endRay.Xyz / endRay.W;
        }

        public float getDistance(Vector3 alvo)
        {
            Vector3 x = castOrigin;
            Vector3 y = castDestiny;

            return Vector3.Cross(Vector3.Subtract(alvo, x), Vector3.Subtract(alvo, y)).Length / Vector3.Subtract(y, x).Length;
        }

        public void rayCast()
        {
            Vector3 origin = cam.Position;

            Vector3 destiny = cam.Front;
            origin.Y = -(float)0.1;

            GL.PointSize(10);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(origin);
            GL.Vertex3(origin + destiny * 100);
            GL.End();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = cam.GetViewMatrix();
            camBbox.AtualizarBbox(cam.Position);
            camBbox.ProcessarCentro();
            camBbox.Desenhar();


            if (!isGrounded() && !jumping)
            {
                cam.Position = Vector3.Subtract(cam.Position, new Vector3(0, 6 * (float)time, 0));
            }

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            if (selecionado != null)
                selecionado.BBox.Desenhar();

            if (useDisplayList)
                GL.CallList(1);
            else
                desenharMapa();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            this.SwapBuffers();
        }



        private bool isOnTop(BBox bbox)
        {

            if (camBbox.obterCentro.X >= bbox.obterMenorX && camBbox.obterCentro.X <= bbox.obterMaiorX && camBbox.obterCentro.Z >= bbox.obterMenorZ && camBbox.obterCentro.Z <= bbox.obterMaiorZ)
            {
                double delta = camBbox.obterMenorY - bbox.obterMaiorY;
                if (delta >= -0.01 && delta <= 0.5)
                {

                    return true;
                }
            }
            return false;
        }

        private bool isGrounded()
        {
            foreach (var item in map)
            {
                if (isOnTop(item.Value.BBox))
                {
                    String[] coord = item.Key.Split(',');
                    currentPlayerHeight = int.Parse(coord[1]);
                    currentPlayerX = int.Parse(coord[0]);
                    currentPlayerZ = int.Parse(coord[2]);
                    return true;
                }
            }
            return false;
        }
    }

}
