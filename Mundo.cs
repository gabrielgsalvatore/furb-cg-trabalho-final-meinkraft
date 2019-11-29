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
        private bool freecam = false;
        private bool jumping = false;
        private bool onAir = false;

        private BBox camBbox = new BBox(-1, -1, -1, 1, 1, 1);


        private Mundo(int width, int height) : base(width, height) { }

        public static Mundo GetInstance(int width, int height)
        {
            if (singletonMundo == null)
                singletonMundo = new Mundo(width, height);
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
                    Cubo cube = mapa[x, y];

                    cube.BBox.Desenhar();
                    cube.Desenhar();

                }
            }
        }
        protected override void OnLoad(EventArgs e)
        {

            camBbox.Atualizar(new Ponto4D(cam.Position.X, cam.Position.Y, cam.Position.Z));
            CursorVisible = false;
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
            }

            base.OnMouseMove(e);
        }

        private async void Jump()
        {
            jumping = true;
            for(int i = 0; i < 20; i++){
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

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
            {
                this.Close();
            }
            if (keyState.IsKeyDown(Key.W)) //TODO: Tratar colisão
            {
                cam.Position += cam.ViewFront * 2.0f * (float)time;
            }
            if (keyState.IsKeyDown(Key.S))
            {
                cam.Position -= cam.ViewFront * 2.0f * (float)time;
            }
            if (keyState.IsKeyDown(Key.A))
            {
                cam.Position -= cam.Right * 2.0f * (float)time;
            }
            if (keyState.IsKeyDown(Key.D))
            {
                cam.Position += cam.Right * 2.0f * (float)time;

            }

            if (keyState.IsKeyDown(Key.L))
            {

                if (!freecam)
                {
                    freecam = true;
                    Console.WriteLine("freecam: " + freecam);
                }
                freecam = false;
                Console.WriteLine("freecam: " + freecam);



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


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = cam.GetViewMatrix();
            camBbox.AtualizarBbox(cam.Position);
            camBbox.ProcessarCentro();
            camBbox.Desenhar();
            onAir = false;
            if (!isGrounded() && !jumping)
            {
                onAir = true;
                Console.WriteLine("Caindo");
                cam.Position -= cam.Up * 4.5f * (float)time * 2;
            }




            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
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

            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    if (isOnTop(mapa[x, y].BBox))
                    {
                        return true;

                    }
                }
            }
            return false;
        }

    }
}
