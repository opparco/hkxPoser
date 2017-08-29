using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct3D9;
using System.Windows.Forms;

using ObjectRef = System.Int32;
using StringRef = System.Int32;

namespace hkxPoser
{
    class Viewer : IDisposable
    {
        Device device;

        /// <summary>
        /// マウスポイントしているスクリーン座標
        /// </summary>
        protected Point lastScreenPoint = Point.Zero;

        /// マウスボタンを押したときに実行するハンドラ
        protected void form_OnMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    SelectBone();
                    break;
            }

            lastScreenPoint.X = e.X;
            lastScreenPoint.Y = e.Y;
        }

        /// マウスを移動したときに実行するハンドラ
        protected void form_OnMouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - lastScreenPoint.X;
            int dy = e.Y - lastScreenPoint.Y;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    camera.Move(dx, -dy, 0.0f);
                    break;
                case MouseButtons.Middle:
                    camera.MoveView(-dx * 0.3125f, dy * 0.3125f);
                    break;
                case MouseButtons.Right:
                    camera.Move(0.0f, 0.0f, -dy * 0.3125f);
                    break;
            }

            lastScreenPoint.X = e.X;
            lastScreenPoint.Y = e.Y;
        }

        public void AttachMouseEventHandler(Control control)
        {
            control.MouseDown += new MouseEventHandler(form_OnMouseDown);
            control.MouseMove += new MouseEventHandler(form_OnMouseMove);
        }

        public void DetachMouseEventHandler(Control control)
        {
            control.MouseMove -= new MouseEventHandler(form_OnMouseMove);
            control.MouseDown -= new MouseEventHandler(form_OnMouseDown);
        }

        Matrix world;
        Matrix view;
        Matrix proj;
        Matrix wvp;

        hkaSkeleton skeleton;
        hkaAnimation anim;

        internal Sprite sprite = null;
        internal Texture dot_texture = null;

        public bool InitializeGraphics(Control control)
        {
            AttachMouseEventHandler(control);

            PresentParameters pp = new PresentParameters();
            pp.Windowed = true;
            pp.SwapEffect = SwapEffect.Discard;

            device = new Device(new Direct3D(), 0, DeviceType.Hardware, control.Handle, CreateFlags.HardwareVertexProcessing, pp);

            world = Matrix.Identity;

            Matrix.PerspectiveFovRH(
                    (float)(Math.PI / 6.0),
                    (float)device.Viewport.Width / (float)device.Viewport.Height,
                    1.0f,
                    500.0f,
                    out proj);

            skeleton = new hkaSkeleton();
            skeleton.Load(Path.Combine(Application.StartupPath, @"resources\skeleton.bin"));

            anim = new hkaAnimation();
            anim.Load(Path.Combine(Application.StartupPath, @"resources\idle.bin"));

            AssignAnimationPose();

            sprite = new Sprite(device);
            dot_texture = Texture.FromFile(device, Path.Combine(Application.StartupPath, @"resources\dot.png"));

            return true;
        }

        public void AssignAnimationPose()
        {
            int len = System.Math.Min(skeleton.bones.Length, anim.transforms.Length);
            for (int i = 0; i < len; i++)
            {
                skeleton.bones[i].local = anim.transforms[i];
            }
        }

        public static string EscapeFileName(string file)
        {
            return '"' + file + '"';
        }

        public static string CreateTempFileName(string file)
        {
            string basename = Path.GetFileNameWithoutExtension(file);
            return Path.Combine(Application.StartupPath, @"tmp\" + basename + ".bin");
        }

        public void LoadAnimation(string source_file)
        {
            string file = CreateTempFileName(source_file);

            ProcessStartInfo info = new ProcessStartInfo(
                    Path.Combine(Application.StartupPath, @"bin\hkdump-bin.exe"),
                    EscapeFileName(source_file) + " -o " + EscapeFileName(file));
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            Process process = Process.Start(info);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
            if (process.ExitCode == 0) // successful
            {
                if (anim.Load(file))
                {
                    AssignAnimationPose();
                }
            }
        }

        public void SaveAnimation(string dest_file)
        {
            string file = CreateTempFileName(dest_file);

            anim.numOriginalFrames = 1;
            anim.duration = 1.0f/30.0f;
            anim.Save(file);

            ProcessStartInfo info = new ProcessStartInfo(
                    Path.Combine(Application.StartupPath, @"bin\hkconv.exe"),
                    EscapeFileName(file) + " -o " + EscapeFileName(dest_file));
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            Process process = Process.Start(info);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
        }

        SimpleCamera camera = new SimpleCamera();

        public void Update()
        {
            camera.Update();
            view = camera.ViewMatrix;
            wvp = world * view * proj;
        }

        public void Render()
        {
            device.Clear(ClearFlags.Target, new ColorBGRA(192, 192, 192, 255), 1.0f, 0);

            device.BeginScene();

            DrawCenterAxis();

            DrawBoneTree();
            DrawSelectedBone();

            device.EndScene();

            device.Present();
        }

        private void DrawCenterAxis()
        {
            Line line = new Line(device);
            line.Width = 2;

            // draw axis
            line.DrawTransform(new Vector3[2] { Vector3.Zero, new Vector3(5, 0, 0) }, wvp, new ColorBGRA(255, 0, 0, 255));
            line.DrawTransform(new Vector3[2] { Vector3.Zero, new Vector3(0, 5, 0) }, wvp, new ColorBGRA(0, 255, 0, 255));
            line.DrawTransform(new Vector3[2] { Vector3.Zero, new Vector3(0, 0, 5) }, wvp, new ColorBGRA(0, 0, 255, 255));

            line.Dispose();
        }

        /// <summary>
        /// viewport行列を作成します。
        /// </summary>
        /// <param name="viewport">viewport</param>
        /// <returns>viewport行列</returns>
        public static Matrix CreateViewportMatrix(Viewport viewport)
        {
            Matrix m = Matrix.Identity;
            m.M11 = (float)viewport.Width / 2;
            m.M22 = -1.0f * (float)viewport.Height / 2;
            m.M33 = (float)viewport.MaxDepth - (float)viewport.MinDepth;
            m.M41 = (float)(viewport.X + viewport.Width / 2);
            m.M42 = (float)(viewport.Y + viewport.Height / 2);
            m.M43 = viewport.MinDepth;
            return m;
        }

        /// ワールド座標をスクリーン位置へ変換します。
        public static Vector3 WorldToScreen(Vector3 v, Viewport viewport, Matrix view, Matrix proj)
        {
            return Vector3.TransformCoordinate(v, view * proj * CreateViewportMatrix(viewport));
        }

        /// ワールド座標をスクリーン位置へ変換します。
        public Vector3 WorldToScreen(Vector3 v)
        {
            return WorldToScreen(v, device.Viewport, view, proj);
        }

        Vector3 GetBonePositionOnScreen(hkaBone bone)
        {
            Transform t = bone.GetWorldCoordinate();
            return WorldToScreen(t.translation);
        }

        Color SelectedBoneLineColor = new ColorBGRA(100, 100, 230, 255);

        public void DrawBoneTree()
        {
            Line line = new Line(device);
            foreach (hkaBone bone in skeleton.bones)
            {
                if (bone.hide)
                    continue;

                Vector3 p0 = GetBonePositionOnScreen(bone);
                p0.Z = 0.0f;

                if (bone.parent != null && !bone.parent.hide)
                {
                    Vector3 p1 = GetBonePositionOnScreen(bone.parent);
                    p1.Z = 0.0f;

                    Vector3 pd = p0 - p1;
                    float len = pd.Length();
                    float scale = 4.0f / len;
                    Vector2 p3 = new Vector2(p1.X + pd.Y * scale, p1.Y - pd.X * scale);
                    Vector2 p4 = new Vector2(p1.X - pd.Y * scale, p1.Y + pd.X * scale);

                    RawVector2[] vertices = new RawVector2[3];
                    vertices[0] = new RawVector2(p3.X, p3.Y);
                    vertices[1] = new RawVector2(p0.X, p0.Y);
                    vertices[2] = new RawVector2(p4.X, p4.Y);
                    line.Draw(vertices, SelectedBoneLineColor);
                }
            }
            line.Dispose();
            line = null;

            Rectangle rect = new Rectangle(0, 16, 15, 15); //bone circle
            Vector3 rect_center = new Vector3(7, 7, 0);
            sprite.Begin(SpriteFlags.AlphaBlend);
            foreach (hkaBone bone in skeleton.bones)
            {
                if (bone.hide)
                    continue;

                Vector3 p0 = GetBonePositionOnScreen(bone);
                p0.Z = 0.0f;

                sprite.Draw(dot_texture, Color.White, rect, rect_center, p0);
            }
            sprite.End();

            //DrawBoneAxis(bone);
        }

        Color BoneLineColor = new ColorBGRA(255, 0, 0, 255);

        /// 選択boneを描画する。
        void DrawSelectedBone()
        {
            if (selected_bone == null)
                return;

            Vector3 p1 = GetBonePositionOnScreen(selected_bone);
            p1.Z = 0.0f;

            if (selected_bone.children.Count != 0)
            {
                Line line = new Line(device);

                hkaBone bone = selected_bone.children[0];
                Vector3 p0 = GetBonePositionOnScreen(bone);
                p0.Z = 0.0f;

                Vector3 pd = p0 - p1;
                float len = pd.Length();
                float scale = 4.0f / len;
                Vector2 p3 = new Vector2(p1.X + pd.Y * scale, p1.Y - pd.X * scale);
                Vector2 p4 = new Vector2(p1.X - pd.Y * scale, p1.Y + pd.X * scale);

                RawVector2[] vertices = new RawVector2[3];
                vertices[0] = new RawVector2(p3.X, p3.Y);
                vertices[1] = new RawVector2(p0.X, p0.Y);
                vertices[2] = new RawVector2(p4.X, p4.Y);
                line.Draw(vertices, BoneLineColor);

                line.Dispose();
                line = null;
            }

            DrawBoneAxis(selected_bone);

            Rectangle rect = new Rectangle(16, 16, 15, 15); //bone circle
            Vector3 rect_center = new Vector3(7, 7, 0);
            sprite.Begin(SpriteFlags.AlphaBlend);
            sprite.Draw(dot_texture, Color.White, rect, rect_center, p1);
            sprite.End();
        }

        void DrawBoneAxis(hkaBone bone)
        {
            Transform t = bone.GetWorldCoordinate();

            Matrix3x3 rotation;
            Matrix3x3.RotationQuaternion(ref t.rotation, out rotation);
            Vector3 xvec = rotation.Row1 * 5.0f;
            Vector3 yvec = rotation.Row2 * 5.0f;
            Vector3 zvec = rotation.Row3 * 5.0f;
            Vector3 location = t.translation;

            Line line = new Line(device);
            line.Width = 2;

            // draw axis
            line.DrawTransform(new Vector3[2] { location, location + xvec }, wvp, new ColorBGRA(255, 0, 0, 255));
            line.DrawTransform(new Vector3[2] { location, location + yvec }, wvp, new ColorBGRA(0, 255, 0, 255));
            line.DrawTransform(new Vector3[2] { location, location + zvec }, wvp, new ColorBGRA(0, 0, 255, 255));

            line.Dispose();
        }

        hkaBone selected_bone = null;

        /// boneを選択します。
        /// returns: boneを見つけたかどうか
        public bool SelectBone()
        {
            bool found = false;

            //スクリーン座標からboneを見つけます。
            //衝突する頂点の中で最も近い位置にあるboneを返します。

            float x = lastScreenPoint.X;
            float y = lastScreenPoint.Y;

            int width = 5;//頂点ハンドルの幅
            float min_z = 1e12f;

            hkaBone found_bone = null;

            foreach (hkaBone bone in skeleton.bones)
            {
                if (bone.hide)
                    continue;

                Vector3 p2 = GetBonePositionOnScreen(bone);
                if (p2.X - width <= x && x <= p2.X + width && p2.Y - width <= y && y <= p2.Y + width)
                {
                    if (p2.Z < min_z)
                    {
                        min_z = p2.Z;
                        found = true;
                        found_bone = bone;
                    }
                }
            }

            if (found)
            {
                selected_bone = found_bone;
                Console.WriteLine("Select Bone: {0}", selected_bone.name);

                camera.Center = selected_bone.GetWorldCoordinate().translation;
                camera.UpdateTranslation();
            }
            return found;
        }

        /// 操作リスト
        public List<ICommand> commands = new List<ICommand>();
        int command_id = 0;

        /// 指定操作を実行します。
        public void Execute(ICommand command)
        {
            if (command.Execute())
            {
                if (command_id == commands.Count)
                    commands.Add(command);
                else
                    commands[command_id] = command;
                command_id++;
            }
        }

        /// 操作を消去します。
        public void ClearCommands()
        {
            commands.Clear();
            command_id = 0;
        }

        /// ひとつ前の操作による変更を元に戻せるか。
        public bool CanUndo()
        {
            return (command_id > 0);
        }

        /// ひとつ前の操作による変更を元に戻します。
        public void Undo()
        {
            if (!CanUndo())
                return;

            command_id--;
            Undo(commands[command_id]);
        }

        /// 指定操作による変更を元に戻します。
        public void Undo(ICommand command)
        {
            command.Undo();
        }

        /// ひとつ前の操作による変更をやり直せるか。
        public bool CanRedo()
        {
            return (command_id < commands.Count);
        }

        /// ひとつ前の操作による変更をやり直します。
        public void Redo()
        {
            if (!CanRedo())
                return;

            Redo(commands[command_id]);
            command_id++;
        }

        /// 指定操作による変更をやり直します。
        public void Redo(ICommand command)
        {
            command.Redo();
        }

        BoneCommand bone_command = null;

        /// bone操作を開始します。
        public void BeginBoneCommand()
        {
            if (selected_bone == null)
                return;

            bone_command = new BoneCommand(selected_bone);
        }

        /// boneを操作中であるか。
        public bool HasBoneCommand()
        {
            return bone_command != null;
        }

        /// bone操作を終了します。
        public void EndBoneCommand()
        {
            if (bone_command != null)
                Execute(bone_command);

            bone_command = null;
        }

        /// 選択boneを指定軸方向に移動します。
        public void TranslateAxis(int dx, int dy, Vector3 axis)
        {
            hkaBone bone = selected_bone;

            if (bone == null)
                return;

            axis = Vector3.Transform(axis, bone.local.rotation);

            float len = dx * 0.0125f;
            bone.local.translation += axis * len;
        }

        /// 選択boneを指定軸中心に回転します。
        public void RotateAxis(int dx, int dy, Vector3 axis)
        {
            hkaBone bone = selected_bone;

            if (bone == null)
                return;

            float angle = dx * 0.005f;
            bone.local.rotation *= Quaternion.RotationAxis(axis, angle);
        }

        public void Dispose()
        {
            dot_texture?.Dispose();
            sprite?.Dispose();
            device?.Dispose();
        }
    }
}
