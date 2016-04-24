using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeoGui.Core;
using OpenTK.Graphics.OpenGL;
using Color = Microsoft.Xna.Framework.Color;

namespace NeoGui
{
    public class Game : Microsoft.Xna.Framework.Game, INeoGuiDelegate
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly RasterizerState rasterizerState = new RasterizerState {
            ScissorTestEnable = true,
            CullMode = CullMode.None
        };
        private readonly DepthStencilState depthStencilState = new DepthStencilState {
            DepthBufferEnable = false,
            DepthBufferWriteEnable = false
        };
        private RenderTarget2D renderTarget;
        private SpriteBatch spriteBatch;
        private BasicEffect basicEffect;
        private SpriteFont font;
        private Texture2D pixel;
        
        private bool inputInited;
        private readonly InputState input = new InputState();
        private NeoGuiContext ui;

        private int frameRate = 0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        public Vec2 TextSize(string text, int fontId = 0)
        {
            var size = font.MeasureString(text);
            return new Vec2(size.X, size.Y);
        }

        private struct Dot
        {
            public Vec3 Pos;
            public NeoGui.Core.Color Color;
        }
        private readonly List<Dot> debugDots = new List<Dot>();
        public void DrawDot(Vec3 p, NeoGui.Core.Color? c = null)
        {
            debugDots.Add(new Dot {Pos = p, Color = c ?? NeoGui.Core.Color.Red});
        }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";

            var trans = new Transform();
            trans.MakeIdentity();
            //trans.Translation.X += 10;
            trans.Scale = Vec3.ScaleIdentity * 0.5f;
            trans.Rotation = Quat.FromAxisAngle(1, 0, 0, (float)Math.PI);
            trans.Rotation = Quat.FromEulerAngles(0, (float)Math.PI, 0);

            var v1 = new Vec3(1, 0, 0);
            var v2 = trans.ApplyForward(v1);

            var m = new Mat4();
            trans.ToMatrix(out m);
            var v3 = m.Mul(v1);

            Debug.WriteLine("v2: " + v2);
            Debug.WriteLine("v3: " + v3);
            
            Debug.WriteLine("sizeof(Transform): " + Marshal.SizeOf(typeof(Transform)));
            Debug.WriteLine("sizeof(Mat4): " + Marshal.SizeOf(typeof(Mat4)));

            trans.MakeIdentity();
            trans.Translation = new Vec3(1, 1, 0);
            trans.ToMatrix(out m);

            Debug.WriteLine("Mat0: " + Matrix.CreateTranslation(1, 1, 0));
            Debug.WriteLine("Mat1: " + m);


            trans.MakeIdentity();
            trans.Rotation = Quat.FromAxisAngle(new Vec3(1, 2, 3).Normalized, 1f);
            Vec3 ax, ay, az;
            trans.GetAxes(out ax, out ay, out az);
            Debug.WriteLine("ax: " + ax);
            Debug.WriteLine("ay: " + ay);
            Debug.WriteLine("az: " + az);
            Debug.WriteLine("ax x ay: " + ax.Cross(ay));

            var bx = trans.Rotation * Vec3.UnitX;
            var by = trans.Rotation * Vec3.UnitY;
            var bz = trans.Rotation * Vec3.UnitZ;
            Debug.WriteLine("bx: " + bx);
            Debug.WriteLine("by: " + by);
            Debug.WriteLine("bz: " + bz);
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24,
                1,
                RenderTargetUsage.PreserveContents);
            IsMouseVisible = true;
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");
            basicEffect = new BasicEffect(GraphicsDevice) {
                TextureEnabled = true,
                VertexColorEnabled = true
            };
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new [] { Color.White });

            ui = new NeoGuiContext(this);
        }
        
        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            ++frameCounter;
            debugDots.Clear();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(1)) {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            if (!inputInited) {
                UpdateInput(gameTime);
                inputInited = true;
            }

            UpdateInput(gameTime);
            var bounds = GraphicsDevice.Viewport.Bounds;
            TestUi.DoUi(ui, bounds.Width, bounds.Height);
            ui.RunUpdateTraversals();

            base.Update(gameTime);
        }

        private void UpdateInput(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            input.Time = gameTime.TotalGameTime.TotalSeconds;
            input.MousePos = new Vec2(mouseState.X, mouseState.Y);
            input.MouseButtonDown[(int)MouseButton.Left] = mouseState.LeftButton == ButtonState.Pressed;
            input.MouseButtonDown[(int)MouseButton.Right] = mouseState.RightButton == ButtonState.Pressed;
            input.MouseButtonDown[(int)MouseButton.Middle] = mouseState.MiddleButton == ButtonState.Pressed;
            ui.Input.SetNewState(input);
        }

        protected override bool BeginDraw()
        {
            return ui.DirtyDrawCommandBuffers.Count > 0;
        }

        private void DrawUi()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            
            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, depthStencilState, rasterizerState, basicEffect);
            foreach (var buffer in ui.DirtyDrawCommandBuffers) {
                for (var i = 0; i < buffer.Count; ++i) {
                    var command = buffer[i];
                    switch (command.Type) {
                    case DrawCommandType.SetClipRect:
                        GraphicsDevice.ScissorRectangle = command.SetClipRect.ClipRect.ToMonoGameRectangle();
                        break;
                    case DrawCommandType.SetTransform:
                        spriteBatch.End();
                        Mat4 mat4;
                        command.SetTransform.Transform.ToMatrix(out mat4);
                        Matrix matrix;
                        FromNeoGuiToMonoGameMatrix(out matrix, ref mat4);
                        basicEffect.World = matrix * Matrix.CreateScale(1, 1, 0);
                        spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, depthStencilState, rasterizerState, basicEffect);
                        break;
                    case DrawCommandType.SolidRect:
                        spriteBatch.Draw(pixel, command.SolidRect.Rect.ToMonoGameRectangle(), command.SolidRect.Color.ToMonoGameColor());
                        break;
                    case DrawCommandType.TexturedRect:
                        break;
                    case DrawCommandType.Text:
                        spriteBatch.DrawString(font, ui.GetInternedString(command.Text.StringId), command.Text.Vec2.ToMonoGameVector2(), command.Text.Color.ToMonoGameColor());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
            spriteBatch.End();
            GraphicsDevice.ScissorRectangle = GraphicsDevice.Viewport.Bounds;
 
            GraphicsDevice.SetRenderTarget(null);
        }

        protected override void Draw(GameTime gameTime)
        {
            var viewport = GraphicsDevice.Viewport;
            basicEffect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * 
                             Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);

            DrawUi();
            
            spriteBatch.Begin();
            spriteBatch.Draw(renderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
            /*spriteBatch.End();

            foreach (var dot in debugDots) {
                var trans = new Transform();
                trans.MakeIdentity();
                trans.Translation = dot.Pos;
                Mat4 mat4;
                trans.ToMatrix(out mat4);
                Matrix matrix;
                FromNeoGuiToMonoGameMatrix(out matrix, ref mat4);
                basicEffect.World = matrix;
                spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, depthStencilState, rasterizerState, basicEffect);
                spriteBatch.Draw(pixel, new Rectangle(-2, -2, 4, 4), dot.Color.ToMonoGameColor());
                spriteBatch.End();
            }*/
            
            var memMegaBytes = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            string fps = $"fps: {frameRate}  mem: {memMegaBytes:F1} MB";
           // spriteBatch.Begin();
            spriteBatch.DrawString(font, fps, new Vector2(5, 1), Color.White);
            spriteBatch.DrawString(font, fps, new Vector2(4, 0), Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
        

        public static void FromNeoGuiToMonoGameMatrix(out Matrix b, ref Mat4 a)
        {
            b.M11 = a.M11;
            b.M12 = a.M21;
            b.M13 = a.M31;
            b.M14 = a.M41;
            
            b.M21 = a.M12;
            b.M22 = a.M22;
            b.M23 = a.M32;
            b.M24 = a.M42;
            
            b.M31 = a.M13;
            b.M32 = a.M23;
            b.M33 = a.M33;
            b.M34 = a.M43;
            
            b.M41 = a.M14;
            b.M42 = a.M24;
            b.M43 = a.M34;
            b.M44 = a.M44;
        }
    }
}
