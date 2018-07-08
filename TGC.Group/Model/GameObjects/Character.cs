using TGC.Core.SkeletalAnimation;
using TGC.Core.Camara;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Mathematica;
using TGC.Core.Collision;
using TGC.Core.BoundingVolumes;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using BulletSharp;
using TGC.Core.Particle;

namespace TGC.Group.Model.GameObjects
{
    public class Character : GameObject
    {
        // El mesh del personaje
        public TgcSkeletalMesh Mesh;

        float VelocidadY = 0f;
        float Gravedad = -60f;
        float VelocidadTerminal = -50f;
        float DesplazamientoMaximoY = 5f;
        float DesplazamientoMaximoXZ = 5f; // El maximo queda (Max, 0, Max), no el modulo (Para no andar calculando modulos)
        float VelocidadSalto = 90f;
        float VelocidadMovimiento = 40f;
        float ultimoDesplazamientoAdelante = 0f;
        bool CanJump = true;
        bool updateAnimation = true;
        TiposColision TipoColisionActual;
        TiposColision UltimoTipoColision;
        //posicion con respecto a la plataforma
        private TGCVector3 posicionPlataforma;
        private TGCVector3 oldPos;
        TgcMp3Player woah = new TgcMp3Player();
        bool ShowHelp = false;
        public int vidas;
        private TGCVector3 PosBeforeMovingInXZ;  // global 
        bool modoGod = false;
        public bool caida = false;
        public bool yaJugo;
        private string texturePath;
        private string[] textureNames;
        private ParticleEmitter emitter;
        private int particleCount;
        private string textureName;

        public override void Init(GameModel _env)
        {
            Env = _env;

            var SkeletalLoader = new TgcSkeletalLoader();
            Mesh =
                SkeletalLoader.loadMeshAndAnimationsFromFile(
                    // xml del mesh
                    Env.MediaDir + "Piloto\\Pilot-TgcSkeletalMesh.xml",
                    // Carpeta del mesh 
                    Env.MediaDir + "Piloto\\",
                    // Animaciones
                    new[]
                    {
                        Env.MediaDir + "Piloto\\Animations\\Walk-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\CrouchWalk-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\LowKick-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\Jump-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\Run-TgcSkeletalAnim.xml"

                    });

            Mesh.playAnimation("StandBy", true);
            // Eventualmente esto lo vamos a hacer manual
            Mesh.Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
            Mesh.RotateY(FastMath.ToRad(180f));

            //Particulas
            texturePath = Env.MediaDir + "Particulas\\";
            textureNames = new[]
            {
                "pisada.png"
            };
            textureName = textureNames[0];
            particleCount = 40;
            emitter = new ParticleEmitter(texturePath + textureName, particleCount);
            emitter.MaxSizeParticle = 1;
            emitter.Dispersion = 50;
            emitter.CreationFrecuency = 0.1f;
            Reset();
        }

        public override void Update()
        {
            Mesh.BoundingBox = Mesh.Animations["Walk"].BoundingBox;
            var ElapsedTime = Env.ElapsedTime;
            var Input = Env.Input;
            float VelocidadAdelante = 0f;
            var Diff = Env.NuevaCamara.LookAt - Env.NuevaCamara.Position;
            Diff.Y = 0;
            TipoColisionActual = TiposColision.SinColision;

            if (CanJump && Input.keyPressed(Key.Space))
            {
                VelocidadY = VelocidadSalto;
                CanJump = false;
            }

            D3DDevice.Instance.Device.RenderState.FillMode = Input.keyDown(Key.F4) ? FillMode.WireFrame : FillMode.Solid;
            if ((Input.keyDown(Key.W) || Input.keyDown(Key.UpArrow)) && Input.keyDown(Key.LeftShift))
                VelocidadAdelante += VelocidadMovimiento * 2;
            else if(Input.keyDown(Key.W) || Input.keyDown(Key.UpArrow))
                VelocidadAdelante += VelocidadMovimiento;
            if (Input.keyDown(Key.S) || Input.keyDown(Key.DownArrow))
                VelocidadAdelante -= VelocidadMovimiento;
            if (Input.keyDown(Key.F8))
                VelocidadMovimiento += 10 * ElapsedTime;
            if (Input.keyDown(Key.F9))
                VelocidadMovimiento -= 10 * ElapsedTime;
            if (Input.keyDown(Key.M))
            {               
                Env.CambiarEscenario("Menu");
            }
            if (Input.keyPressed(Key.G))
            {
                if (!modoGod)
                {
                    VelocidadMovimiento = 250;
                    VelocidadSalto = 250;
                    VelocidadTerminal = -150;
                    modoGod = true;
                    
                }
                else
                {
                    restaurarVelocidades();
                }
            }
            if (Input.keyPressed(Key.H))
                ShowHelp = !ShowHelp;
            if (Input.keyDown(Key.F10))
            {
                VelocidadSalto += 10 * ElapsedTime;
                VelocidadTerminal -= 2 * ElapsedTime;
            }
            if (Input.keyDown(Key.F11))
            {
                VelocidadSalto -= 10 * ElapsedTime;
                VelocidadTerminal += 2 * ElapsedTime;
            }
            Env.NuevaCamara.keyboardMovement = 0;
            if (Input.keyDown(Key.D) || Input.keyDown(Key.RightArrow))
                Env.NuevaCamara.keyboardMovement += 1;
            if (Input.keyDown(Key.A) || Input.keyDown(Key.LeftArrow))
                Env.NuevaCamara.keyboardMovement -= 1;
            if (Input.keyDown(Key.R))
                Mesh.Position = new TGCVector3(0, 1, 0);

            var versorAdelante = TGCVector3.Normalize(Diff);
            VelocidadY = FastMath.Clamp(VelocidadY + Gravedad * ElapsedTime, VelocidadTerminal, -VelocidadTerminal);
            var LastPos = Mesh.Position;
            // Colision en Y
            oldPos = Mesh.Position;
            Mesh.Position += new TGCVector3(0, FastMath.Clamp(VelocidadY * ElapsedTime, -DesplazamientoMaximoY, DesplazamientoMaximoY), 0);
            CheckColisionY(ElapsedTime);
            if (UltimoTipoColision == TiposColision.PisoResbaloso)
                VelocidadAdelante += ultimoDesplazamientoAdelante;
            else
                ultimoDesplazamientoAdelante = 0;

            MoveXZ(versorAdelante * FastMath.Clamp(VelocidadAdelante * ElapsedTime, -DesplazamientoMaximoXZ, DesplazamientoMaximoXZ));
            UltimoTipoColision = TipoColisionActual;
            ultimoDesplazamientoAdelante += VelocidadAdelante * ElapsedTime;

            updateAnimation = true;
            
           if (Mesh.Position.Y != LastPos.Y)
           {
                SetAnimation("Jump", false);
           }
           else if (Input.keyDown(Key.LeftShift)|| Input.keyDown(Key.RightShift)){
                SetAnimation("Run");
                updateAnimation = VelocidadAdelante != 0;
                emitter.Position = Mesh.Position;
            }
            else if (Input.keyDown(Key.C)) {
                SetAnimation("CrouchWalk");
                updateAnimation = VelocidadAdelante != 0;
            }
            else 
                if (VelocidadAdelante != 0)
                    SetAnimation("Walk");
                else
                    SetAnimation("StandBy");
            

            Env.NuevaCamara.Target = Mesh.Position;
            Mesh.Rotation = new TGCVector3(0, Env.NuevaCamara.rotY + FastMath.PI, 0);
            if (updateAnimation)
                Mesh.updateAnimation(ElapsedTime);

            emitter.Position = Mesh.Position;
        }

        //setPosicion con respecto a la plataforma
        internal void setposition(TGCVector3 tGCVector3)
        {
            posicionPlataforma = tGCVector3;
        }

        public void restaurarVelocidades()
        {
            VelocidadMovimiento = 40;
            VelocidadSalto = 90;
            VelocidadTerminal = -50;
            modoGod = false;
        }
        public void RenderHUD()
        {

            int textY = 20;
            var c = Color.BlanchedAlmond;
            Env.DrawText.drawText("H: Mostrar Ayuda", 0, textY, c); textY += 20;
            if (ShowHelp)
            {
                Env.DrawText.drawText("[Pos pj]: " + TGCVector3.PrintVector3(Mesh.Position), 0, textY, c); textY += 20;
                Env.DrawText.drawText("Velocidad Y: " + VelocidadY.ToString(), 0, textY, c); textY += 20;
                Env.DrawText.drawText("Ctrl: Render BB", 0, textY, c); textY += 20;
                Env.DrawText.drawText("Shift: Crouch", 0, textY, c); textY += 20;
                Env.DrawText.drawText("R: Reiniciar Posición", 0, textY, c); textY += 20;
                Env.DrawText.drawText("F8/F9: +/- velocidad (" + VelocidadMovimiento + ")", 0, textY, c); textY += 20;
                Env.DrawText.drawText("F10/F11: +/- salto (" + VelocidadSalto + ")", 0, textY, c); textY += 20;
                Env.DrawText.drawText("Mesh renderizados: " + Env.Escenario.Grilla.DrawCount + "/" + Env.Escenario.Grilla.modelos.Count, 0, textY, c); textY += 20;
                Env.DrawText.drawText("F3: Mostrar KdTree", 0, textY, c); textY += 20;
                Env.DrawText.drawText("F4: WireFrame", 0, textY, c); textY += 20;
                Env.DrawText.drawText("F5: Activar/Desactivar colisiones de camara", 0, textY, c); textY += 20;
                Env.DrawText.drawText("G: Modo god", 0, textY, c); textY += 20;
                Env.DrawText.drawText("F6: Desactivar sharpen", 0, textY, c); textY += 20;
                Env.DrawText.drawText("M: Menu Principal", 0, textY, c); textY += 20;
            }
            if (modoGod)
            {

                Env.DrawText.drawText("God activado", D3DDevice.Instance.Width / 2, 20, Color.Chocolate);
            }
            if (Env.Input.keyDown(Key.LeftControl) || Env.Input.keyDown(Key.RightControl))
                Mesh.BoundingBox.Render();
        }
        public override void Render(Escenario.Escenario esc)
        {
            var ElapsedTime = Env.ElapsedTime;
            Mesh.Transform = TGCMatrix.Scaling(Mesh.Scale)
                            * TGCMatrix.RotationYawPitchRoll(Mesh.Rotation.Y, Mesh.Rotation.X, Mesh.Rotation.Z)
                            * TGCMatrix.Translation(Mesh.Position);
            esc.RenderObject(Mesh);
            //IMPORTANTE PARA PERMITIR ESTE EFECTO.
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();
            if (Mesh.CurrentAnimation.Name == "Run")
            {
                emitter.render(ElapsedTime);
            }
        }

        public override void Dispose()
        {
            Mesh.Dispose();
            //Liberar recursos
            emitter.dispose();
        }

        public bool CheckColision(out TgcBoundingAxisAlignBox Collider)
        {
            Collider = Env.Escenario.ColisionXZ(Mesh.BoundingBox);
            var Colision = Collider == null;
            return Colision;
        }

        internal void Move(TGCVector3 posPj, TGCVector3 posCamara)
        { 
            Mesh.Position = posPj;
            Env.NuevaCamara.SetCamera(posCamara, Mesh.Position); 
        }

        public bool SetAnimation(string animationName, bool loop=true)
        {
            var AlreadySet = Mesh.CurrentAnimation.Name == animationName;
            if (!AlreadySet)
                Mesh.playAnimation(animationName, loop);
            return !AlreadySet;
        }
        public void SetTipoColisionActual(TiposColision resultadoColision)
        {
            TipoColisionActual = resultadoColision;
        }
        public void MoveXZ(TGCVector3 movimiento, TgcBoundingAxisAlignBox lastCollider = null)
        {
            PosBeforeMovingInXZ = Mesh.Position;
            Mesh.Position += movimiento;
            var Collider = Env.Escenario.ColisionXZ(Mesh.BoundingBox);
            if (movimiento == TGCVector3.Empty) {
                return;
            }
            if (Collider == null && TipoColisionActual == TiposColision.Pozo)
            {
                Pozo();
            }
            //El personaje se movera con la plataforma
            else if (Collider == null && TipoColisionActual == TiposColision.Caja)
            {

                Mesh.Move(posicionPlataforma);
            } 
            else if (Collider != null)
            {
                Collider = Collider.clone();
                Collider.scaleTranslate(TGCVector3.Empty, new TGCVector3(1.05f, 1.05f, 1.05f));
                var movementRay = PosBeforeMovingInXZ - Mesh.Position;
                var rs = TGCVector3.Empty;

                if (((Mesh.BoundingBox.PMax.X > Collider.PMax.X && movementRay.X > 0) ||
                    (Mesh.BoundingBox.PMin.X < Collider.PMin.X && movementRay.X < 0)) &&
                    ((Mesh.BoundingBox.PMax.Z > Collider.PMax.Z && movementRay.Z > 0) ||
                    (Mesh.BoundingBox.PMin.Z < Collider.PMin.Z && movementRay.Z < 0)))
                {
                    //Este primero es un caso particularse dan las dos condiciones simultaneamente entonces para saber de que lado moverse hay que hacer algunos calculos mas.
                    //por el momento solo se esta verificando que la posicion actual este dentro de un bounding para moverlo en ese plano.
                    if (Mesh.Position.X > Collider.PMin.X && Mesh.Position.X < Collider.PMax.X)
                    {
                        //El personaje esta contenido en el bounding X
                        //Sliding Z Dentro de X
                        rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                    }
                    if (Mesh.Position.Z > Collider.PMin.Z && Mesh.Position.Z < Collider.PMax.Z)
                    {
                        //El personaje esta contenido en el bounding Z
                        //Sliding X Dentro de Z
                        rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                    }
                    //Seria ideal sacar el punto mas proximo al bounding que colisiona y chequear con eso, en ves que con la posicion.
                }
                else
                {
                    if ((Mesh.BoundingBox.PMax.X > Collider.PMax.X && movementRay.X > 0) ||
                        (Mesh.BoundingBox.PMin.X < Collider.PMin.X && movementRay.X < 0))
                    {
                        //Sliding X
                        rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                    }
                    if ((Mesh.BoundingBox.PMax.Z > Collider.PMax.Z && movementRay.Z > 0) ||
                        (Mesh.BoundingBox.PMin.Z < Collider.PMin.Z && movementRay.Z < 0))
                    {
                        //Sliding Z
                        rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                    }
                }
                Mesh.Position = PosBeforeMovingInXZ;
                MoveXZ(rs * -0.9f, Collider);
            }
        }
        private void CheckColisionY(float ElapsedTime)
        {
            TgcBoundingAxisAlignBox Collider = Env.Escenario.ColisionY(Mesh.BoundingBox);
            if (Collider != null)
            {
                Mesh.Position = new TGCVector3(Mesh.Position.X, FastMath.Clamp(Mesh.Position.Y, Collider.PMax.Y, Collider.PMax.Y + 2), Mesh.Position.Z);
                CanJump = VelocidadY < 0;
            }
            if (TipoColisionActual == TiposColision.Pozo)
            {
                Pozo();
            }
            else if (TipoColisionActual == TiposColision.Caja)
            {
                
                Mesh.Move(posicionPlataforma);

            }
            
            else if (TipoColisionActual == TiposColision.Techo)
            {
                Mesh.Position = new TGCVector3(oldPos.X, posicionPlataforma.Y, oldPos.Z);
                VelocidadY = 0;
            }
            
        }
        public TGCVector3 Position() { return Mesh.Position; }
        public void Position(TGCVector3 pos) { Mesh.Position = pos; }
        public void Vidas(int cantVidas) { vidas = cantVidas; }
        public void Pozo()
        {
            CanJump = false;          
            SetAnimation("Jump", false);
            woah.closeFile();
            woah.FileName = Env.MediaDir + "\\Sound\\woah.mp3";
            woah.play(false);
        }

        public TGCVector3 posBeforeMovingInXZ()
        {
            return PosBeforeMovingInXZ;
        }
        public void Reset()
        {
            yaJugo = false;
            vidas = 3;
        }
        TGCVector3 prevPos;
        public void UpdateBullet(EscenarioBullet ph)
        {
            var Input = Env.Input;
            Env.NuevaCamara.keyboardMovement = 0;
            if (Input.keyDown(Key.D) || Input.keyDown(Key.RightArrow))
                Env.NuevaCamara.keyboardMovement += 1;
            if (Input.keyDown(Key.A) || Input.keyDown(Key.LeftArrow))
                Env.NuevaCamara.keyboardMovement -= 1;

            var Forward = Env.NuevaCamara.LookAt - Env.NuevaCamara.Position;
            Forward.Y = 0;
            Forward.Normalize();
            float strength = 125f;
            if (Input.keyDown(Key.W) || Input.keyDown(Key.UpArrow))
            {
                var cuerpo = ph.cuerpoPJ();
                cuerpo.ActivationState = ActivationState.ActiveTag;
                cuerpo.AngularVelocity = TGCVector3.Empty.ToBsVector;
                cuerpo.ApplyImpulse(strength * Forward.ToBsVector*Env.ElapsedTime, cuerpo.CenterOfMassPosition);
            }
            if (Input.keyDown(Key.S) || Input.keyDown(Key.DownArrow))
            {
                var cuerpo = ph.cuerpoPJ();
                cuerpo.ActivationState = ActivationState.ActiveTag;
                cuerpo.AngularVelocity = TGCVector3.Empty.ToBsVector;
                cuerpo.ApplyImpulse(-strength * Forward.ToBsVector * Env.ElapsedTime, cuerpo.CenterOfMassPosition);
            }
            var delta = prevPos.Y - Mesh.Position.Y;
            prevPos = Mesh.Position;
            if (Input.keyPressed(Key.Space) && delta == 0f)
            {
                var cuerpo = ph.cuerpoPJ();
                cuerpo.ActivationState = ActivationState.ActiveTag;
                cuerpo.AngularVelocity = TGCVector3.Empty.ToBsVector;
                cuerpo.ApplyCentralForce(60*strength * TGCVector3.Up.ToBsVector);
            }
            Mesh.Rotation = new TGCVector3(0, Env.NuevaCamara.rotY + FastMath.PI, 0);
        }
    }
}
