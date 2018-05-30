using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Group.Model.GameObjects;
using TGC.Core.Collision;
using TGC.Group.Model.GameObjects.Escenario;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        // El personaje principal
        public Character Personaje = new Character();
        public Escenario Escenario;
        // Para precargar todos los escenarios
        public List<Escenario> Escenarios = new List<Escenario>();
        public TgcThirdPersonCamera NuevaCamara;
        public float CameraOffsetHeight = 20;
        public float CameraOffsetForward = -75;


        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = "Grupo The Bandicoots";
            Description = "Crash Bandicoot - Plataformas - 2018";
        }
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            NuevaCamara = new TgcThirdPersonCamera(new TGCVector3(0,0,0), 20, -75, Input);
            Camara = NuevaCamara;
            Personaje.Init(this);
            Escenario = new Escenario1();
            Escenario.Init(this);
            Escenarios.Add(Escenario);
            Escenario = new EscenarioMenu();
            Escenarios.Add(Escenario);
            Personaje.Init(this);
            Escenario.Init(this);
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();
            if(Escenario!=null)
                Escenario.Update();
            PostUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            //PreRender();
            
            Escenario.Render();
            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            //PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            Personaje.Dispose();
            foreach(GameObject Esc in Escenarios)
                Esc.Dispose();
        }
        public void limpiarTexturas() { ClearTextures(); }
        public void RenderizaAxis() { RenderAxis(); }
        public void RenderizaFPS() { RenderFPS(); }
        public void CambiarEscenario(int num)
        {
            Escenario = Escenarios[num];
            Escenario.Reset();
        }
    }
}