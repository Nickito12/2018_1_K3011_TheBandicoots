using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Sound;
using TGC.Core.Terrain;
using TGC.Core.SceneLoader;
using System.Collections.Generic;
using TGC.Group.Model.Estructuras;
using Microsoft.DirectX.Direct3D;
using System;
using BulletSharp;
using Microsoft.DirectX.DirectInput;
using BulletSharp.Math;
using TGC.Core.Shaders;

namespace TGC.Group.Model.GameObjects.Escenario
{
    public class EscenarioBullet1 : EscenarioBullet
    {
        // El piso del mapa/escenario
        public List<Tuple<TGCBox, RigidBody>> Paredes;
        private RigidBody capsulePJ;
        List<Tuple<TGCBox,RigidBody>> boxes;
        List<PlataformaGiratoriaBullet> giratorias = new List<PlataformaGiratoriaBullet>();

        public override void Init(GameModel _env)
        {
            Env = _env;
            string compilationErrors;
            var d3dDevice = D3DDevice.Instance.Device;
            EfectoRender2D = Microsoft.DirectX.Direct3D.Effect.FromFile(d3dDevice, Env.ShadersDir + "render2D.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            shadowEffect = TgcShaders.loadEffect(Env.ShadersDir + "ShadowMap.fx");
            outrunEffect = TgcShaders.loadEffect(Env.ShadersDir + "OutRun.fx");
            outrunEffect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            outrunEffect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            skeletalShadowEffect = TgcShaders.loadEffect(Env.ShadersDir + "TgcSkeletalShadowMapMeshShader.fx");
            shadowEffect.SetValue("EPSILON", 0.005f);
            g_LightPos = new TGCVector3(0, 1000, 0);
            g_LightDir = new TGCVector3(0, -1, 0);
            g_LightDir.Normalize();
            if (EfectoRender2D == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            EfectoRender2D.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            EfectoRender2D.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);
            try
            {
                texturaVida = TextureLoader.FromFile(d3dDevice, Env.MediaDir + "\\Menu\\heart.png");
            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", Env.MediaDir + "\\Menu\\heart.png"));
            }
            //Crear pisos
            var AlfombraTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "carpet.jpg");
            var PiedraTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "piedra.jpg");
            base.Init(Env);
            Env.Personaje.Mesh.updateBoundingBox();
            var aabb = Env.Personaje.Mesh.BoundingBox;
            capsulePJ = BulletRigidBodyConstructor.CreateCapsule((aabb.PMax.X - aabb.PMin.X)/2, (aabb.PMax.Y-aabb.PMin.Y)/2, new TGCVector3(0,0, 0));
            capsulePJ.CollisionFlags = CollisionFlags.CharacterObject | capsulePJ.CollisionFlags;
            dynamicsWorld.AddRigidBody(capsulePJ);
            boxes = new List<Tuple<TGCBox,RigidBody>>();
            Paredes = new List<Tuple<TGCBox, RigidBody>>();
            var RoomSize = 500f;
            var RoomHeight = 500f;

            //piso
            Paredes.Add(addBox(new TGCVector3(0f, -1f, 0f), new TGCVector3(RoomSize, 1, RoomSize), AlfombraTexture));

            //paredes
            Paredes.Add(addBox(new TGCVector3(0f, -1f, RoomSize / 2), new TGCVector3(RoomSize, RoomHeight, 1), PiedraTexture));
            Paredes.Add(addBox(new TGCVector3(0f, -1f, -RoomSize/2), new TGCVector3(RoomSize, RoomHeight, 1), PiedraTexture));
            Paredes.Add(addBox(new TGCVector3(-RoomSize / 2, -1f, 0f), new TGCVector3(1, RoomHeight, RoomSize), PiedraTexture));
            Paredes.Add(addBox(new TGCVector3(RoomSize / 2, -1f, 0f), new TGCVector3(1, RoomHeight, RoomSize), PiedraTexture));
            Paredes.Add(addBox(new TGCVector3(0f, RoomHeight, RoomSize / 2), new TGCVector3(RoomSize, RoomHeight, 1), PiedraTexture));
            Paredes.Add(addBox(new TGCVector3(0f, RoomHeight, -RoomSize / 2), new TGCVector3(RoomSize, RoomHeight, 1), PiedraTexture));
            Paredes.Add(addBox(new TGCVector3(-RoomSize / 2, RoomHeight, 0f), new TGCVector3(1, RoomHeight, RoomSize), PiedraTexture));
            Paredes.Add(addBox(new TGCVector3(RoomSize / 2, RoomHeight, 0f), new TGCVector3(1, RoomHeight, RoomSize), PiedraTexture));

            // Sobrepisos
            Paredes.Add(addBox(new TGCVector3(0f, 3f, 75f/2f + (200 + 25f / 2f)/2), new TGCVector3(500, 2, 200 + 25f / 2f), AlfombraTexture));
            Paredes.Add(addBox(new TGCVector3(0f, 3f, -(75f/2f) - (200 + 25f / 2f) / 2), new TGCVector3(500, 2, 200 + 25f / 2f), AlfombraTexture));
            Paredes.Add(addBox(new TGCVector3(-250f+75f/2f, 3f, 0), new TGCVector3(75, 2, 75f), AlfombraTexture));

            // Arrastrable/Movible
            addBox(new TGCVector3(-50f, 75f/2f, 0f), new TGCVector3(75, 75, 75), PiedraTexture, 100);

            //Escalones primera pared
            addBox(new TGCVector3(217f, 130f, 100f), new TGCVector3(75, 75, 75), PiedraTexture);
            addBox(new TGCVector3(217f, 210f, 180f), new TGCVector3(75, 75, 75), PiedraTexture);

            // Plataformas Giratorias
            addSpinningPlatform(new TGCVector3(0f, 225f, 0f), new TGCVector3(100, 50, 100), PiedraTexture, 100000000000f);
            addSpinningPlatform(new TGCVector3(0f, 610f, 0f), new TGCVector3(60, 30, 60), PiedraTexture, 100000000000f);
            addSpinningPlatform(new TGCVector3(0f, 660f, 0f), new TGCVector3(50, 25, 50), PiedraTexture, 100000000000f);

            // Escalones segunda pared
            addBox(new TGCVector3(-217f, 210f, 180f), new TGCVector3(75, 75, 75), PiedraTexture);
            addBox(new TGCVector3(-217f, 320f, 80f), new TGCVector3(75, 75, 75), PiedraTexture);
            addBox(new TGCVector3(-217f, 430f, -20f), new TGCVector3(75, 75, 75), PiedraTexture);
            addBox(new TGCVector3(-217f, 540f, -110f), new TGCVector3(75, 75, 75), PiedraTexture);

            Reset();
            dynamicsWorld.SetInternalTickCallback((DynamicsWorld world, float timeStep) =>
            {
                for (int i = 0; i < world.Dispatcher.NumManifolds; i++)
                {
                    var manifold = world.Dispatcher.GetManifoldByIndexInternal(i);
                    PlataformaGiratoriaBullet giratoria = null;
                    if (manifold.Body0 == capsulePJ)
                        giratoria = giratorias.Find(g => g.Body == manifold.Body1);
                    else if (manifold.Body1 == capsulePJ)
                        giratoria = giratorias.Find(g => g.Body == manifold.Body0);
                    if (giratoria != null)
                    {
                        Vector3 pmax, pmin, aux;
                        giratoria.Body.CollisionShape.GetAabb(giratoria.Body.WorldTransform, out aux, out pmax);
                        capsulePJ.CollisionShape.GetAabb(capsulePJ.WorldTransform, out pmin, out aux);
                        if (pmin.Y >= pmax.Y - 1)
                        {
                            var t = capsulePJ.CenterOfMassTransform;
                            t.Origin += giratoria.Delta;
                            capsulePJ.CenterOfMassTransform = t;
                        }
                    }
                }
            }
            );
        }
        public void addSpinningPlatform(TGCVector3 pos, TGCVector3 size, TgcTexture text, float mass = 1f, float friction=0f, float inertia=0f)
        {
            var b = addBox(pos, size, text, mass, friction, inertia);
            this.giratorias.Add(new PlataformaGiratoriaBullet(50f, b.Item1, b.Item2, new TGCVector3(b.Item2.CenterOfMassPosition), 3f));
        }
        public Tuple<TGCBox, RigidBody> addBox(TGCVector3 pos, TGCVector3 size, TgcTexture text, float mass = 0f, float friction = 0f, float? inertia = null)
        {
            var box = TGCBox.fromSize(size, text);
            var uv = FastMath.Max(size.Y, FastMath.Max(size.X, size.Z));
            box.UVTiling = new TGCVector2(FastMath.Ceiling(uv / 80), FastMath.Ceiling(uv/80));
            box.updateValues();
            var boxBody = BulletRigidBodyConstructor.CreateBox(box.Size * 0.5f, mass, pos, 0, 0, 0, friction, inertia);
            dynamicsWorld.AddRigidBody(boxBody);
            var pair = new Tuple<TGCBox, RigidBody>(box, boxBody);
            boxes.Add(pair);
            box.AutoTransform = false;
            boxBody.Restitution = 0f;
            return pair;
        }
        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
            return null;
        }
        public override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox)
        {
            return null;
        }
        public override void Update()
        {
            if (Env.ElapsedTime > 10000)
                return;
            Env.Personaje.UpdateBullet(this);
            if (Env.Personaje.Position().Y <= -500)
            {
                Env.Personaje.vidas--;
                if (Env.Personaje.vidas <= 0)
                    Env.CambiarEscenario("Menu");
                else
                    Reset();
            }
            Env.NuevaCamara.UpdateCamera(this);
            if (cancionPcpal.getStatus() != TgcMp3Player.States.Playing)
            {
                cancionPcpal.closeFile();
                cancionPcpal.play(true);
            }
            dynamicsWorld.StepSimulation(1 / 60f);
            Env.Personaje.Mesh.Position = new TGCVector3(capsulePJ.CenterOfMassPosition);
            Env.NuevaCamara.Target = Env.Personaje.Mesh.Position;
            var Mesh = Env.Personaje.Mesh;
            Env.Personaje.Mesh.Transform = TGCMatrix.Scaling(Mesh.Scale) *
                TGCMatrix.RotationYawPitchRoll(Mesh.Rotation.Y, Mesh.Rotation.X, Mesh.Rotation.Z) *
                TGCMatrix.Translation(new TGCVector3(0, (Mesh.BoundingBox.PMin.Y - Mesh.BoundingBox.PMax.Y)/2, 0)) *
                new TGCMatrix(capsulePJ.InterpolationWorldTransform);
            foreach (var pair in boxes)
            {
                var box = pair.Item1;
                box.Transform = new TGCMatrix(pair.Item2.CenterOfMassTransform);
                box.AutoTransform = false;
            }
            foreach(var giratoria in giratorias)
                giratoria.Update(Env.ElapsedTime);
        }

        public override RigidBody cuerpoPJ()
        {
            return capsulePJ;
        }

        public override void Reset()
        {
            Env.NuevaCamara = new TgcThirdPersonCamera(new TGCVector3(0, 0, 0), 20, -75, Env.Input);
            Env.Camara = Env.NuevaCamara;
            Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
            var t = capsulePJ.CenterOfMassTransform;
            t.Origin = new Vector3(0, 1, 0);
            capsulePJ.CenterOfMassTransform = t;
        }

        public override List<TgcBoundingAxisAlignBox> listaColisionesConCamara()
        {
            return Paredes.ConvertAll((Tuple<TGCBox, RigidBody> x) => {
                x.Item1.Position = new TGCVector3(x.Item2.CenterOfMassPosition);
                var aabb = x.Item1.BoundingBox;
                var delta = aabb.PMax - aabb.PMin;
                return new TgcBoundingAxisAlignBox(aabb.PMin - delta*0.1f - TGCVector3.One*2.5f, aabb.PMax + delta * 0.1f + TGCVector3.One * 2.5f);
            });
        }

        public override void RenderRealScene()
        {
            RenderHUD();
            RenderScene();
            if (Env.Input.keyDown(Key.LeftControl) || Env.Input.keyDown(Key.LeftControl))
                foreach (var x in dynamicsWorld.CollisionObjectArray)
                {
                    Vector3 min, max;
                    x.CollisionShape.GetAabb(x.WorldTransform, out min, out max);
                    new TgcBoundingAxisAlignBox(new TGCVector3(min), new TGCVector3(max)).Render();
                }
        }
        public override void RenderScene()
        {
            foreach (var pair in boxes)
            {
                var box = pair.Item1;
                RenderObject(box);
            }
            RenderObject(Env.Personaje.Mesh);
        }
        public override void Dispose()
        {
            foreach (var pair in boxes)
            {
                var box = pair.Item1;
                var body = pair.Item2;
                body.Dispose();
                box.Dispose();
            }
            base.Dispose();
        }

        public override TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox)
        {
            return null;
        }
        public override int getElements()
        {
            return 0;
        }
    }
}
