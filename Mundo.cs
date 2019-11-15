using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Input;



namespace furb_cg_trabalho_final_meinkraft
{
    class Mundo : GameWindow
    {
        private static Mundo singletonMundo = null;

        private Mundo(int width, int height) : base(width, height) { }

        public static Mundo GetInstance(int width, int height){
            if(singletonMundo == null)
                singletonMundo = new Mundo(width, height);
            return singletonMundo;
        }


        protected override void OnLoad(EventArgs e)
        {
        base.OnLoad(e);
        GL.ClearColor(Color.Gray);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
        base.OnUpdateFrame(e);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(-300, 300, -300, 300, -10, 10);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();   


        this.SwapBuffers();
        }

      }
}
