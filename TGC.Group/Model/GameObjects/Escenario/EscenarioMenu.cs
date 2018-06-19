using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Sound;

delegate void ClickDelegate();

namespace TGC.Group.Model.GameObjects.Escenario
{
    interface Button
    {
        void Render(GameModel Env);
        TgcBoundingAxisAlignBox AABB();
        void Click();
    }
    class QuadButton : Button
    {
        string text;
        TGCQuad quad;
        TgcBoundingAxisAlignBox aabb;
        int x, y;
        ClickDelegate onClick;
        public System.Drawing.Font font = new System.Drawing.Font("Arial", 24, FontStyle.Bold);
        
        public QuadButton(TGCVector3 pos, float X, float Y, string text, ClickDelegate onClick, Color color, float sizeX = 50, float sizeY = 4)
        {
            quad = new TGC.Core.Geometry.TGCQuad();
            quad.Center = new TGCVector3(pos.X + X, pos.Y - 50, pos.Z + Y);
            quad.Size = new TGCVector2(sizeX, sizeY);
            quad.Normal = new TGCVector3(0, 1, 0);
            quad.Color = color;
            quad.updateValues();
            var s = quad.Size * 0.5f;
            aabb = new TgcBoundingAxisAlignBox(quad.Center - new TGCVector3(s.X, 0, s.Y), quad.Center + new TGCVector3(s.X, 0, s.Y), quad.Center, new TGCVector3(1, 1, 1));
            x = (int)(X * 650 / 50 + 525);
            //x = (int)(X* 650 / 20 + 650 - font.Size/5*text.Length);
            y= (int)(Y * 350 / 20 + 350);
            this.onClick = onClick;
            this.text = text;
        }
        public void Render(GameModel Env)
        {
            Env.DrawText.changeFont(font);
            Env.DrawText.drawText(text, x, y, Color.White);
            Env.DrawText.changeFont(TGC.Core.Text.TgcText2D.VERDANA_10);
            quad.Render();
        }
        public void Click()
        {
            onClick();
        }
        public TgcBoundingAxisAlignBox AABB()
        { return aabb; }
    }
    class SpriteButton : Button
    {
        TgcBoundingAxisAlignBox aabb;
        int x, y;
        ClickDelegate onClick;
        Sprite sprite;
        Texture textura;
        TGCVector3 Center;
        TGCVector2 Size;

        public SpriteButton(TGCVector3 pos, float X, float Y, string filePath, ClickDelegate onClick, float sizeX = 50, float sizeY = 4)
        {
            var d3dDevice = D3DDevice.Instance.Device;
            //TgcTexture textura;
            sprite = new Sprite(d3dDevice);
            
            try
            {
                 textura = TextureLoader.FromFile(d3dDevice, filePath);
            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", filePath));
            }
            Center = new TGCVector3(pos.X + X, pos.Y - 50, pos.Z + Y);
            Size = new TGCVector2(sizeX, sizeY);
            var s = Size * 0.5f;
            aabb = new TgcBoundingAxisAlignBox(Center - new TGCVector3(s.X, 0, s.Y), Center + new TGCVector3(s.X, 0, s.Y), Center, new TGCVector3(1, 1, 1)); ;
            x = (int)(X * 650 / 40 + 680);
            y = (int)(Y * 350 / 20 + 365);
            Size = Size * 17;
            x -= (int)(Size.X * 0.5);
            y -= (int)(Size.Y * 0.5);
            this.onClick = onClick;
        }
        public void Render(GameModel Env)
        {
            sprite.Begin(SpriteFlags.None);
            sprite.Draw2D(textura, Rectangle.Empty, new SizeF(Size.X, Size.Y), new PointF(x,y),  Color.White);
            sprite.End();
        }
        public void Click()
        {
            onClick();
        }
        public TgcBoundingAxisAlignBox AABB()
        { return aabb; }
    }
    class EscenarioMenu : Escenario1
    {
        public TGCVector3 pos;
        public List<Button> buttons = new List<Button>();
        private TgcPickingRay pickingRay;
        
        public override void Init(GameModel _env)
        {
           
            base.Init(_env);
            Reset();
            buttons.Add(new QuadButton(pos, 0, -10, "Nueva Partida",() => {
                Env.Personaje.Reset();
                Env.CambiarEscenario("Escenario1");
                // Env.CambiarEscenario(0);
                }, Color.DarkCyan));
            /*buttons.Add(new SpriteButton(pos, 0, -15, Env.MediaDir + "\\Menu\\BotonIniciarPartida.jpg", () => {
                Env.Personaje.Reset();
                Env.CambiarEscenario(0);
            })); */
            buttons.Add(new QuadButton(pos, 0, -5, "Continuar", () => {
                Env.Personaje.yaJugo = true;
                Env.CambiarEscenario("Escenario1");
            }, Color.DarkCyan));
            /*buttons.Add(new SpriteButton(pos, 0, -15, Env.MediaDir + "\\Menu\\Continuar.jpg", () => {              
                Env.CambiarEscenario(0);
            }));*/
            buttons.Add(new QuadButton(pos, 0, 0, "Cargar Partida", () => {
                Env.LoadPartida();
                Env.CambiarEscenario("Escenario1");
            }, Color.DarkCyan));

            buttons.Add(new QuadButton(pos, 0, 5, "Guardar Partida", () => {
                Env.guardarPartida();

            }, Color.DarkCyan));
            buttons.Add(new QuadButton(pos, 0, 10, "Modo Bullet", () => {
                Env.Personaje.Reset();
                Env.CambiarEscenario("Bullet1");
            }, Color.DarkCyan));
            buttons.Add(new QuadButton(pos, 0, 15, "Exit", () => { Environment.Exit(0); }, Color.DarkCyan));
            pickingRay = new TgcPickingRay(Env.Input);
            useShadows = false;
        }
        public override void Render()
        {
            if (Env.Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colision en base a posicion del mouse
                pickingRay.updateRay();

                foreach (var button in buttons)
                {
                    var aabb = button.AABB();
                    TGCVector3 collisionPoint;
                    var selected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out collisionPoint);
                    if (selected)
                    {
                        button.Click(); 
                        break;
                    }
                }
            }
            preRender3D();
            baseRender();
            foreach (var plano in ListaPlanos)
            {
                plano.Render();
            }
            
            foreach (var button in buttons)
            {
                button.Render(Env);
            }
            postRender3D();
            render2D();
        }
        public override void Update()
        {
            if (Env.ElapsedTime > 10000)
                return;
            if (Env.Personaje.Position().Y <= -100)
                Env.Personaje.Position(new TGCVector3(0, 1, 0));
            ShowGrilla = Env.Input.keyDown(Key.F3);
            foreach (var plataforma in Plataformas)
            {
                plataforma.Update(Env.ElapsedTime);
            }
            if (cancionPcpal.getStatus() != TgcMp3Player.States.Playing)
            {
                cancionPcpal.closeFile();
                cancionPcpal.play(true);
            }
        }
        public override void Reset()
        {
            Env.Camara = new Core.Camara.TgcCamera();
            pos = new TGCVector3(1200, 800, -300);
            Env.Camara.SetCamera(pos, new TGCVector3(pos.X, 0, pos.Z - 1));
        }
    }
}
