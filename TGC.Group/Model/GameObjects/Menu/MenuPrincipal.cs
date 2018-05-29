using System;
using System.Collections.Generic;

using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;

using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Textures;
using TGC.Core;

using TGC.Core.Geometry;
using TGC.Core.Mathematica;


namespace TGC.Group.Model.GameObjects.Menu
{
    public delegate void EnterPressed();

    enum MainOptions
    {
        NewGame,
        LoadGame,
        SaveGame,
        Options,
        ReadThis,
        QuitGame
    }
   
    public class MenuItem
    {
        //Comando a ejecutar
        //Clase para ejecutar un comando
        readonly float SPRITE_WIDTH = 350;
        readonly float SPRITE_HEIGHT = 45;
       
        Sprite sprite;
        bool enable;
        private EnterPressed enterPressed;
        public EnterPressed EnterPressed
        {
            set
            {
                enterPressed = value;
            }
        }



        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        public Sprite Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

        public MenuItem(string filePath, Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            
            
            //TgcTexture textura;
            sprite = new Sprite(d3dDevice);
            Bitmap bitmap = new Bitmap(filePath); //imagen de fondooo
            Texture textura;
            try
            {
                //ImageInformation image = new ImageInformation();
                /* textura = TextureLoader.FromFile(d3dDevice, filePath, 0, 0, 0, Usage.RenderTarget, Format.A8R8G8B8,
                     Pool.Default, Filter.Linear, Filter.Linear, Color.Magenta.ToArgb(),ref ImageInformation); */
                 textura = TextureLoader.FromFile(d3dDevice, filePath);
            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", filePath));
            }
            // Texture textura = new Texture(D3DDevice.Instance.Device, bitmap, Usage.None, Pool.Default);
            //Texture textura = TextureLoader.FromFile(D3DDevice.Instance.Device, filePath);
            //Texture textura =  Texture.FromBitmap(D3DDevice.Instance.Device, bitmap, Usage.RenderTarget, Pool.Default);
          
           
            //sprite.Draw(textura, Rectangle.Empty, TGCVector3.Empty, TGCVector3.Empty, Color.White);
        }

        public void Ejecutar()
        {
            if (enterPressed != null) enterPressed();
        }



    }

    public class GameMenu
    {
      
        readonly float POS_X0 = 360;
        readonly float POS_Y0 = 260;
        readonly float SEP_ITEMS = 60;
        readonly float POS_Y0_TITLE = 20;
        readonly float POS_X0_TITLE = 300;

        List<MenuItem> menuItems = new List<MenuItem>();
        //Drawer drawer = new Drawer();
        Sprite title;
        Sprite selector;
        bool enable = true;
        int selected;
        bool primerEsc;
        private float factor;
        Microsoft.DirectX.Direct3D.Device d3dDevice;
        GameModel Model;

        public bool Enable
        {
            get { return enable; }
            set
            {
                enable = value;
                if (enable)
                {
                    primerEsc = true;
                    Cursor.Show();
                }
                else
                    Cursor.Hide();
            }
        }

        public GameMenu(string filePath,Microsoft.DirectX.Direct3D.Device d3dDevice, GameModel Env)
        {
            this.Model = Env;
            this.d3dDevice = d3dDevice;
            title = new Sprite(d3dDevice);
           
            Vector3 posicion = new Vector3(POS_X0_TITLE * factor, POS_Y0_TITLE * factor,0);

            selector = new Sprite(d3dDevice);
            Bitmap bitmap = new Bitmap(filePath);

           
            MenuItem newGame = new MenuItem(Env.MediaDir + "\\Menu\\BotonIniciarPartida.jpg", d3dDevice); //este no se de que es

           

            AddItem(newGame); //imagen de cada boton en particular
           /* AddItem(new MenuItem());
            AddItem(new MenuItem());
            AddItem(new MenuItem());
            AddItem(new MenuItem());
            AddItem(new MenuItem()); */
        }

        public void AddItem(MenuItem item)
        {
             
            menuItems.Add(item);
        }

        public void Render()
        {
            if (!enable)
                return;
            ProcessInput();
            Bitmap bitmap = new Bitmap(Model.MediaDir + "\\Menu\\FondoMenu.png"); //imagen del boton falta poner la ruta 
                                                                                 // Texture textura = new Texture(d3dDevice, bitmap, Usage.None, Pool.Default);
            Texture textura = TextureLoader.FromFile(D3DDevice.Instance.Device, Model.MediaDir + "\\Menu\\FondoMenu.png");
            Vector3 centro = new Vector3(0, 0, 0);
            Vector3 posicion = new Vector3(POS_X0 * factor, (POS_Y0 + menuItems.Count * SEP_ITEMS) * factor, 0);
           
            foreach (MenuItem unItem in menuItems)
            {
                unItem.Sprite.Draw(textura, centro, posicion, 0);
            }
            
            

        }

        protected void ProcessInput()
        {
            var Input = Model.Input; 

            if (Input.keyPressed(Key.Escape))
            {
                if (!primerEsc)
                    Enable = false;
                else
                    primerEsc = false;
            }

            if (Input.keyPressed(Key.W))
            {
                if (selected > 0)
                    selected--;
            }

            if (Input.keyPressed(Key.S))
            {
                if (selected < menuItems.Count - 1)
                    selected++;
            }

            if (Input.keyPressed(Key.Space))
            {
              
                menuItems[selected].Ejecutar();
            }
        }


        private ImageInformation imageInformation;

        /// <summary>
        ///     Returns the image information of the bitmap.
        /// </summary>
        public ImageInformation ImageInformation
        {
            get { return imageInformation; }
            set { imageInformation = value; }
        }
    }
   
}
