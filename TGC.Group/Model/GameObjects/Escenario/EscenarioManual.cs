﻿using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Terrain;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Estructuras;
using System.Collections.Generic;
using TGC.Core.Sound;
using System.Drawing;
using Microsoft.DirectX.DirectInput;
using System;

namespace TGC.Group.Model.GameObjects.Escenario
{
    public abstract class EscenarioManual : Escenario
    {
        protected TgcSkyBox SkyBox;
        protected TgcScene Scene;
        protected TgcSceneLoader Loader;
        public bool ShowGrilla = false;
        protected List<TgcMesh> ListaPozos = new List<TgcMesh>();
        protected List<TgcMesh> ListaPlataformas = new List<TgcMesh>();
        protected List<TgcMesh> ListaPisosResbalosos = new List<TgcMesh>();
        protected List<TgcMesh> ListaMeshesSinColision = new List<TgcMesh>();
        protected List<TgcMesh> MeshConMovimiento = new List<TgcMesh>();
        protected List<CajaEmpujable> ListaCajasEmpujables = new List<CajaEmpujable>();
        protected List<TgcMesh> ListaParedes = new List<TgcMesh>();
        protected const float ROTATION_SPEED = 1f;
        protected List<Plataforma> Plataformas;
        protected List<TgcPlane> ListaPlanos = new List<TgcPlane>();

        protected void AddMesh(string carpeta, string nombre, TGCVector3 pos, int rotation = 0, TGCVector3? scale = null)
        {
            scale = scale ?? new TGCVector3(1, 1, 1);
            var Mesh = Loader.loadSceneFromFile(Env.MediaDir + 
                "Meshes\\" + carpeta + "\\" + nombre + "\\" + 
                nombre + "-TgcScene.xml").Meshes[0];
            Mesh.AutoTransform = true;
            Mesh.Position = pos;
            Mesh.Scale = scale.Value;
            Mesh.RotateY(FastMath.ToRad(rotation));
            Scene.Meshes.Add(Mesh);

        }
        protected void CreateSkyBox(TGCVector3 center, TGCVector3 size, string name)
        {
            SkyBox = new TgcSkyBox();
            SkyBox.Center = center;
            SkyBox.Size = size;
            var TexturesPath = Env.MediaDir + name + "\\";
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, TexturesPath + "Up.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, TexturesPath + "Down.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, TexturesPath + "Left.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, TexturesPath + "Right.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, TexturesPath + "Back.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, TexturesPath + "Front.jpg");
            SkyBox.Init();
        }
        public override void Render() { baseRender(); }
        public void baseRender()
        {
            SkyBox.Render();
            //Dibujar bounding boxes de los mesh (Debugging)
            if (Env.Input.keyDown(Key.LeftControl) || Env.Input.keyDown(Key.RightControl))
            {
                foreach (TgcMesh mesh in Scene.Meshes)
                    mesh.BoundingBox.Render();
                foreach (TgcMesh mesh in ListaPozos)
                    mesh.BoundingBox.Render();
            }
            foreach (var plataforma in Plataformas)
            {
                plataforma.Mesh.Render();
            }
            foreach (var pozo in ListaPozos)
            {
                pozo.Render();
            }
            foreach (var caja in ListaCajasEmpujables)
            {
                caja.Mesh.Render();
            }
            Grilla.render(Env.Frustum, ShowGrilla);
        }
        public override void Dispose()
        {
            cancionPcpal.closeFile();
            SkyBox.Dispose();
            Scene.DisposeAll();
        }
        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
            // null => no hay colision
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var Mesh in Scene.Meshes.FindAll(m => m.Enabled))
            {
                if (!ListaMeshesSinColision.Contains(Mesh) && EscenarioManual.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    if (ListaPozos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                        break;
                    }
                    else if (ListaPlataformas.Contains(Mesh))
                    {
                        if (Mesh.BoundingBox.PMax.Y > Env.Personaje.Mesh.Position.Y || Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.Position.Y)
                        {
                            Colisionador = Mesh.BoundingBox;
                            break;
                        }

                    }
                    else if (ListaPisosResbalosos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.PisoResbaloso);
                        break;
                    }
                    else
                    {
                        Colisionador = Mesh.BoundingBox;
                    }
                    break;
                }
            }
            foreach (var caja in ListaCajasEmpujables)
            {
                var aabb = caja.Mesh.BoundingBox;
                if (!EscenarioManual.testAABBAABB(aabb, boundingBox))
                    continue;
                var oldCajaPos = caja.Mesh.Position;
                caja.ColisionXZ(Env.Personaje);
                bool colisionDeCaja = false;
                foreach (var Mesh in Scene.Meshes.FindAll(m => m.Enabled))
                {
                    if (EscenarioManual.testAABBAABB(aabb, Mesh.BoundingBox))
                    {
                        colisionDeCaja = true;
                        continue;
                    }
                }
                if (colisionDeCaja)
                {
                    Colisionador = aabb;
                    caja.Mesh.Position = oldCajaPos;
                    break;
                }
                foreach (var pozo in ListaPozos)
                {
                    if (EscenarioManual.testAABBAABBXZIn(aabb, pozo.BoundingBox))
                    {
                        caja.caer();
                        continue;
                    }
                }
            }
            foreach (var Mesh in MeshConMovimiento)
            {
                if (!ListaMeshesSinColision.Contains(Mesh) && EscenarioManual.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    if (ListaPozos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                    }
                    else if (ListaPlataformas.Contains(Mesh))
                    {
                        if (Mesh.BoundingBox.PMax.Y > Env.Personaje.Mesh.Position.Y || (Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.BoundingBox.PMax.Y && Mesh.BoundingBox.PMax.Y > Env.Personaje.Mesh.BoundingBox.PMax.Y))
                            Colisionador = Mesh.BoundingBox;
                    }
                    else if (ListaPisosResbalosos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.PisoResbaloso);
                    }
                    else
                    {
                        Colisionador = Mesh.BoundingBox;
                    }
                    break;
                }
            }
            return Colisionador;
        }

        public override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox)
        {
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var plataforma in Plataformas)
            {
                if (EscenarioManual.testAABBAABB(plataforma.Mesh.BoundingBox, boundingBox))
                {
                    if (plataforma.Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.BoundingBox.PMax.Y && plataforma.Mesh.BoundingBox.PMin.Y + (plataforma.Mesh.BoundingBox.PMax.Y - plataforma.Mesh.BoundingBox.PMin.Y) * 0.5f > Env.Personaje.Mesh.BoundingBox.PMax.Y)
                    {
                        Env.Personaje.setposition(plataforma.Mesh.BoundingBox.PMin - (Env.Personaje.Mesh.BoundingBox.PMax - Env.Personaje.Mesh.BoundingBox.PMin));
                        Env.Personaje.SetTipoColisionActual(TiposColision.Techo);
                    }
                    else
                    {
                        if (plataforma.Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.Position.Y)
                            Colisionador = plataforma.Mesh.BoundingBox;
                        Env.Personaje.setposition(plataforma.deltaPosicion());
                        Env.Personaje.SetTipoColisionActual(TiposColision.Caja);
                    }
                }
            }
            foreach (var caja in ListaCajasEmpujables)
            {
                var colision = caja.ColisionY(Env.Personaje, Env.ElapsedTime);
                if (colision != null)
                    Colisionador = colision;
            }
            if (Colisionador != null)
                return Colisionador;
            var piso = ColisionConPiso(boundingBox);
            if (piso != null)
            {
                bool agujero = false;
                foreach (var pozo in ListaPozos)
                {
                    if (EscenarioManual.testAABBAABBXZIn(boundingBox, pozo.BoundingBox))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                        agujero = true;
                        break;
                    }
                }
                if (!agujero)
                    Colisionador = piso;
            }
            return Colisionador;
        }
        public override void Update()
        {
            if (Env.ElapsedTime > 10000)
                return;
            if (Env.Personaje.Position().Y <= -100)
            {
                Env.Personaje.vidas--;
                if (Env.Personaje.vidas <= 0)
                    Env.CambiarEscenario("Menu");
                else
                    Reset();
            }
            ShowGrilla = Env.Input.keyDown(Key.F3);
            foreach (var plataforma in Plataformas)
            {
                plataforma.Update(Env.ElapsedTime);
            }
            Env.NuevaCamara.UpdateCamera(this);
            if (cancionPcpal.getStatus() != TgcMp3Player.States.Playing)
            {
                cancionPcpal.closeFile();
                cancionPcpal.play(true);
            }
            Env.Personaje.Update();
        }
        public override List<TgcBoundingAxisAlignBox> listaColisionesConCamara()
        {
            return new List<TgcBoundingAxisAlignBox>();
        }
        public override void Reset()
        {
            Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
        }
    }
}
