using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Camara;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Mathematica;
using TGC.Group.Model.GameObjects;

namespace TGC.Group.Model.GameObjects
{
    public class Character : GameObject
    {
        // La referencia al GameModel del juego
        GameModel env;
        // El mesh del personaje
        private TgcSkeletalMesh mesh;
        //Referencia a la camara para facil acceso
        TgcCamera Camara;

        // Un salto de verdad seria con una aceleracion y gravedad
        // Pero esto sirve por ahora (?)
        float jumpTime = 1; // Tiempo en segundos de salto
        float currentJumpTime = 0; // Tiempo transcurrido del salto actual
        float jumpHeight = 20; // Altura maxima del salto

        public override void Init(GameModel _env)
        {
            env = _env;
            Camara = env.Camara;

            var skeletalLoader = new TgcSkeletalLoader();
            mesh =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    // xml del mesh
                    env.MediaDir + "Robot\\Robot-TgcSkeletalMesh.xml",
                    // Carpeta del mesh 
                    env.MediaDir + "Robot\\",
                    // Animaciones
                    new[]
                    {
                        env.MediaDir + "Robot\\Caminando-TgcSkeletalAnim.xml",
                        env.MediaDir + "Robot\\Parado-TgcSkeletalAnim.xml"
                    });
        }
        public override void Update()
        {
            var ElapsedTime = env.ElapsedTime;
            var Input = env.Input;
            if (Input.keyPressed(Key.Space))
            {
                if (currentJumpTime <= 0)
                    currentJumpTime = jumpTime;
            }
            var lookAt = Camara.LookAt;
            var camara = Camara.Position;
            if (currentJumpTime > 0)
            {
                TGCVector3 deltaHeight;
                var thisFramesJumpTime = FastMath.Min(ElapsedTime, currentJumpTime);
                if (currentJumpTime > jumpTime / 2)
                {
                    deltaHeight = new TGCVector3(0, jumpHeight / jumpTime * thisFramesJumpTime, 0);
                }
                else
                {
                    deltaHeight = new TGCVector3(0, -jumpHeight / jumpTime * thisFramesJumpTime, 0);
                }
                lookAt += deltaHeight;
                camara += deltaHeight;
                currentJumpTime -= thisFramesJumpTime;
            }
            var velocidadAdelante = 0f;
            var velocidadLado = 0f;
            if (Input.keyDown(Key.UpArrow))
                velocidadAdelante += 25f;
            if (Input.keyDown(Key.DownArrow))
                velocidadAdelante -= 25f;
            if (Input.keyDown(Key.RightArrow))
                velocidadLado -= 35f;
            if (Input.keyDown(Key.LeftArrow))
                velocidadLado += 35f;
            var versorAdelante = TGCVector3.Normalize(Camara.LookAt - Camara.Position);
            var versorCostado = TGCVector3.Normalize(TGCVector3.Cross(versorAdelante, new TGCVector3(0, 1, 0)));
            camara += versorAdelante * velocidadAdelante * ElapsedTime;
            lookAt += versorAdelante * velocidadAdelante * ElapsedTime + versorCostado * velocidadLado * env.ElapsedTime;
            Camara.SetCamera(camara, lookAt);
        }
        public override void Render()
        {
            mesh.Render();
        }
        public override void Dispose()
        {
            mesh.Dispose();
        }

        internal void Move(TGCVector3 posPj, TGCVector3 posCamara)
        { 
            mesh.Position = posPj;
            Camara.SetCamera(posCamara, mesh.Position); 
        }
    }
}
