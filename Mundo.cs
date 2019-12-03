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

        // Vector3 front = new Vector3(0.0f, 0.0f, -1.0f);

        private Vector2 lastPos;

        private bool firstMove = true;
        private static Mundo singletonMundo = null;
        private Cubo cubo;
        private Cubo[,] mapa = new Cubo[50, 50];
        private Camera cam = new Camera(new Vector3(0.0f, 2.5f, 0.0f), 1.0f);

        private double time;

        private bool jumping = false;

        private Cubo selecionado = null;

        private Vector3 castOrigin;
        private Vector3 castDestiny;

        private BBox camBbox = new BBox(-1, -1, -1, 1, 1, 1);

        private static int width;
        private static int height;

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

        private void popularMapa()
        {
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    String rotulo = x + "," + y;
                    Cubo cube = new Cubo(rotulo, null);
                    cube.EscalaXYZ(1, 1, 1);
                    cube.TranslacaoXYZ(x * 2, 0, y * 2);
                    cube.BBox.AtualizarBbox(new Vector3(x * 2, 0, y * 2));
                    //cube.BBox.Atualizar(new Ponto4D(x,0,y));
                    mapa[x, y] = cube;
                }
            }
        }
        private void printarMapa()
        {
            Cubo cube = mapa[0, 0];
            cube.ExibeMatriz();
            Cubo cube1 = mapa[0, 2];
            cube1.ExibeMatriz();
            Cubo cube2 = mapa[1, 2];
            cube2.ExibeMatriz();

        }


        private void desenharMapa()
        {
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    if (mapa[x, y] != null)
                    {
                        Cubo cube = mapa[x, y];

                        //cube.BBox.Desenhar();
                        cube.Desenhar();
                    }

                }
            }
        }
        protected override void OnLoad(EventArgs e)
        {

            cam.Fov = 70f;
            camBbox.Atualizar(new Ponto4D(cam.Position.X, cam.Position.Y, cam.Position.Z));
            CursorVisible = true;
            base.OnLoad(e);
            GL.ClearColor(Color.Gray);
            popularMapa();

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
            for (int i = 0; i < 20; i++)
            {
                cam.Position += cam.Up * 6.0f * (float)time;
                await Task.Delay(1);
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
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button.Equals(MouseButton.Left))
            {
                for (int x = 0; x < mapa.GetLength(0); x++)
                {
                    for (int y = 0; y < mapa.GetLength(1); y++)
                    {
                        if (mapa[x, y] != null)
                        {
                            if (mapa[x, y].ToString().Equals(selecionado.ToString()))
                            {
                                mapa[x, y] = null;
                            }
                        }
                    }
                }
            }
        }

        private void showSelected()
        {
            var mouse = OpenTK.Input.Mouse.GetState();
            castRay(cam.Front.X, cam.Front.Y);
            Console.WriteLine(cam.Front.X + " " + cam.Front.Y);
            float minDistance = float.MaxValue;
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    if (mapa[x, y] != null)
                    {
                        BBox objetoBbox = mapa[x, y].BBox;
                        Vector3 objeto = new Vector3((float)mapa[x, y].Matriz.ObterElemento(12), (float)mapa[x, y].Matriz.ObterElemento(13), (float)mapa[x, y].Matriz.ObterElemento(14));
                        float distance = getDistance(objeto);
                        if (distance <= minDistance)
                        {
                            minDistance = distance;
                            selecionado = mapa[x, y];
                        }

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
            if (keyState.IsKeyDown(Key.W)) //TODO: Tratar colisão
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

            //ProcessInput();

        }


        public void castRay(float x, float y)
        {
            x += width / 2; //ajuste
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

                Console.WriteLine("Caindo");
                cam.Position -= cam.Up * 4.5f * (float)time * 2;
            }

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            desenharMapa();
            if (selecionado != null)
                selecionado.BBox.Desenhar();

            rayCast();
            crosshair.Matriz.AtribuirIdentidade();
            crosshair.EscalaXYZ(0.01, 0.01, 0.01);

            crosshair.TranslacaoXYZ(cam.Front.X * 1.2, cam.Front.Y, cam.Front.Z * 1.2);
            crosshair.TranslacaoXYZ(cam.Position.X, cam.Position.Y, cam.Position.Z);

            crosshair.Desenhar();

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

            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    if (mapa[x, y] != null)
                    {
                        if (isOnTop(mapa[x, y].BBox))
                        {
                            return true;

                        }
                    }

                }
            }
            return false;
        }

    }
}
