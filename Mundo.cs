using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Input;
using CG_Biblioteca;

namespace furb_cg_trabalho_final_meinkraft
{
    class Mundo : GameWindow
    {

        // Vector3 front = new Vector3(0.0f, 0.0f, -1.0f);

        private Vector2 lastPos;
        private bool firstMove;
        private bool colfront = false;
        private bool colback = false;
        private bool colleft = false;
        private bool colright = false;

        private static Mundo singletonMundo = null;
        private Cubo cubo;
        private Cubo[,] mapa = new Cubo[50, 50];
        private Camera cam = new Camera(new Vector3(0.0f, 0.0f, 0.0f), 1.0f);
        private double time;


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
                    cube.TranslacaoXYZ(x*2, 0, y*2);
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
                    cube.Desenhar();
                }
            }
        }
        protected override void OnLoad(EventArgs e)
        {
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
                Console.WriteLine(cam.Front);
            }

            base.OnMouseMove(e);
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = cam.GetProjectionMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        private bool checkCollisionZ() //TODO: Tratar colisão
        {
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    if (cam.Position.Z == mapa[x, y].Matriz.ObterElemento(14))
                    {
                        return true;
                    }
                }
            }
            return false;
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
            if (keyState.IsKeyDown(Key.Space))
            {
                cam.Position += cam.Up * 2.0f * (float)time;
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

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            desenharMapa();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            this.SwapBuffers();
        }





    }
}
