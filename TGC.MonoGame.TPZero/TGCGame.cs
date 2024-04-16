using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Clase principal del juego.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }
        private CityScene City { get; set; }
        private Model CarModel { get; set; }
        private Matrix CarWorld { get; set; }
        private FollowCamera FollowCamera { get; set; }
        //vectores físicos
        private Vector3 Position { get; set; }
        private Vector3 Velocity { get; set; }
        private Vector3 Acceleration { get; set; } = Vector3.Zero;
        //vectores físicos
        private Quaternion Rotation { get; set; } = Quaternion.Identity;
        private Vector3 RotationAxis {get; set; } = Vector3.UnitY;
        private float RotationAngle = 0f;


        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Se encarga de la configuracion y administracion del Graphics Device.
            Graphics = new GraphicsDeviceManager(this);

            // Carpeta donde estan los recursos que vamos a usar.
            Content.RootDirectory = "Content";

            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        /// <summary>
        ///     Llamada una vez en la inicializacion de la aplicacion.
        ///     Escribir aca todo el codigo de inicializacion: Todo lo que debe estar precalculado para la aplicacion.
        /// </summary>
        protected override void Initialize()
        {
            // Enciendo Back-Face culling.
            // Configuro Blend State a Opaco.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            // Creo una camaar para seguir a nuestro auto.
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            // Configuro la matriz de mundo del auto.
            CarWorld = Matrix.Identity;

            base.Initialize();
        }

        /// <summary>
        ///     Llamada una sola vez durante la inicializacion de la aplicacion, luego de Initialize, y una vez que fue configurado GraphicsDevice.
        ///     Debe ser usada para cargar los recursos y otros elementos del contenido.
        /// </summary>
        protected override void LoadContent()
        {
            // Creo la escena de la ciudad.
            City = new CityScene(Content);

            // La carga de contenido debe ser realizada aca.
            CarModel = Content.Load<Model>(ContentFolder3D + "scene/car");

            base.LoadContent();
        }

        /// <summary>
        ///     Es llamada N veces por segundo. Generalmente 60 veces pero puede ser configurado.
        ///     La logica general debe ser escrita aca, junto al procesamiento de mouse/teclas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Caputo el estado del teclado.
            var keyboardState = Keyboard.GetState();
            var ElapsedGameTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Salgo del juego.
                Exit();
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                Acceleration = Vector3.Transform(Vector3.UnitZ * 100f, Rotation) * (- 1);
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                Acceleration = Vector3.Transform(Vector3.UnitZ * 100f, Rotation);
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                RotationAngle += 0.01f;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                RotationAngle -= 0.01f;
            }   

            // La logica debe ir aca.
            Rotation = Quaternion.CreateFromAxisAngle(RotationAxis, RotationAngle);

            var direction = Vector3.Transform(Vector3.UnitZ, Rotation);

            Velocity += Acceleration * ElapsedGameTime;
            Position += direction * Velocity * ElapsedGameTime * 0.5f;
            CarWorld = Matrix.CreateScale(1.0f) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
            
            //Vuelvo a setear la aceleración en 0 para que no acelere siempre
            Acceleration = Vector3.Zero;

            // Actualizo la camara, enviandole la matriz de mundo del auto.
            FollowCamera.Update(gameTime, CarWorld);

            base.Update(gameTime);
        }


        /// <summary>
        ///     Llamada para cada frame.
        ///     La logica de dibujo debe ir aca.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Limpio la pantalla.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Dibujo la ciudad.
            City.Draw(gameTime, FollowCamera.View, FollowCamera.Projection);

            // El dibujo del auto debe ir aca.
            CarModel.Draw(CarWorld, FollowCamera.View, FollowCamera.Projection);

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos cargados.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos cargados dessde Content Manager.
            Content.Unload();

            base.UnloadContent();
        }
    }
}