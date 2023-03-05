﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using System.Windows.Forms;

namespace hkxPoser
{
    class Viewer : IDisposable
    {
        Control control;
        int startFrame;
        SharpDX.Direct3D11.Device device;
        SharpDX.DXGI.Factory1 factory1;
        SwapChain swapChain;

        Renderer3d renderer3d;
        Renderer2d renderer2d;

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
                    camera.Move(dx * 0.01f, -dy * 0.01f);
                    break;
                case MouseButtons.Middle:
                    camera.MoveView(-dx * 0.3125f, dy * 0.3125f, 0.0f);
                    break;
                case MouseButtons.Right:
                    camera.MoveView(0.0f, 0.0f, -dy * 0.3125f);
                    break;
            }

            lastScreenPoint.X = e.X;
            lastScreenPoint.Y = e.Y;
        }

        public void AttachMouseEventHandler(Control control)
        {
            control.MouseDown += new MouseEventHandler(form_OnMouseDown);
            control.MouseMove += new MouseEventHandler(form_OnMouseMove);
            control.KeyDown += new KeyEventHandler(Control_KeyDown);

        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
           // ((Form2)control).ReceiveKeyDown(sender, e);
            Console.WriteLine("keydown");
        }

        public void DetachMouseEventHandler(Control control)
        {
            control.MouseMove -= new MouseEventHandler(form_OnMouseMove);
            control.MouseDown -= new MouseEventHandler(form_OnMouseDown);
            control.KeyDown -= new KeyEventHandler(Control_KeyDown);
        }

        Viewport viewport;

        Matrix view;
        Matrix proj;
        Matrix wvp;
        Matrix world_to_screen;

        hkaSkeleton skeleton;
        hkaAnimation anim;

        public event EventHandler LoadAnimationEvent;

        void CreateViewportAndProjection(ref System.Drawing.Size clientSize)
        {
            viewport = new Viewport(0, 0, clientSize.Width, clientSize.Height, 0.0f, 1.0f);

            Matrix.PerspectiveFovRH(
                    (float)(Math.PI / 6.0),
                    (float)viewport.Width / (float)viewport.Height,
                    1.0f,
                    500.0f,
                    out proj);
        }

        protected void form_Resize(object sender, EventArgs e)
        {
            System.Console.WriteLine("Viewer.form_Resize");

            renderer2d.DiscardDeviceResources();
            renderer3d.DiscardDeviceResources();

            System.Drawing.Size clientSize = control.ClientSize;
            CreateViewportAndProjection(ref clientSize);

            SwapChainDescription desc = swapChain.Description;
            swapChain.ResizeBuffers(desc.BufferCount, viewport.Width, viewport.Height, desc.ModeDescription.Format, desc.Flags);

            renderer3d.CreateDeviceResources(swapChain, ref viewport);
        }

        static SampleDescription DetectSampleDescription(SharpDX.Direct3D11.Device device, Format format)
        {
            var desc = new SampleDescription();
            for (int sampleCount = SharpDX.Direct3D11.Device.MultisampleCountMaximum; sampleCount > 0; --sampleCount)
            {
                int qualiryLevels = device.CheckMultisampleQualityLevels(format, sampleCount);
                if (qualiryLevels > 0)
                {
                    desc.Count = sampleCount;
                    desc.Quality = qualiryLevels - 1;
                    break;
                }
            }
            Console.WriteLine("sample count {0} quality {1}", desc.Count, desc.Quality);
            return desc;
        }

        public bool InitializeGraphics(Control control)
        {
            startFrame = 0;
            this.control = control;

            AttachMouseEventHandler(control);
            control.Resize += new System.EventHandler(form_Resize);

            System.Drawing.Size clientSize = control.ClientSize;

            // Create Device and SwapChain
            device = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 });

            factory1 = new SharpDX.DXGI.Factory1();
            swapChain = new SwapChain(factory1, device, new SwapChainDescription()
            {
                ModeDescription = new ModeDescription(clientSize.Width, clientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = DetectSampleDescription(device, Format.D32_Float_S8X24_UInt),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = control.Handle,
                IsWindowed = true,
                SwapEffect = SwapEffect.Discard
            });
            // Ignore all windows events
            factory1.MakeWindowAssociation(control.Handle, WindowAssociationFlags.IgnoreAll);

            CreateViewportAndProjection(ref clientSize);

            string skel_file = Path.Combine(Application.StartupPath, @"resources\skeleton.bin");
            skeleton = new hkaSkeleton();
            skeleton.Load(skel_file);

            string anim_file = Path.Combine(Application.StartupPath, @"resources\idle.bin");
            anim = new hkaAnimation();
            if (anim.Load(anim_file))
            {
                string source_file = Path.ChangeExtension(anim_file, ".hkx");
                LoadAnimationSuccessful(source_file);
            }

            renderer3d.InitializeGraphics(device, skeleton);
            renderer3d.CreateDeviceResources(swapChain, ref viewport);

            renderer2d.CreateDeviceIndependentResources(skeleton);

            return true;
        }

        public void AssignAnimationPose(int idx)
        {
            if (anim.numOriginalFrames > 1)
                // loop animation. not pose.
                idx %= anim.numOriginalFrames - 1;
            hkaPose pose = anim.pose[idx];
            int nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
            for (int i = 0; i < nbones; i++)
            {
                skeleton.bones[i].local = pose.transforms[i];
            }
        }


        public void ApplyPatchToAnimation()
        {
            foreach (hkaPose pose in anim.pose)
            {
                int nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
                for (int i = 0; i < nbones; i++)
                {
                    pose.transforms[i] *= skeleton.bones[i].patch;
                }
            }
            ClearPatch();
            command_man.ClearCommands();
        }

        public void makeToAnimation(int toFrame)
        {

            if (startFrame >= toFrame)
            {
                string caption = "Error Detected in Input";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("Failed to make animation", caption, buttons);
                return;
            }

            hkaPose pose = anim.pose[startFrame];
            hkaPose toPose = anim.pose[toFrame];
            int nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
            for (int i = 0; i < nbones; i++)
            {
                for (int frame = startFrame + 1; frame < toFrame; ++frame)
                {
                    if (skeleton.bones[i].patch.rotation != Quaternion.Identity || skeleton.bones[i].patch.translation != Vector3.Zero)
                    {
                        Transform toTransfrom = toPose.transforms[i] * skeleton.bones[i].patch;
                        Transform fromTransfrom = pose.transforms[i];
                        float ratio = (float)(frame - startFrame) / (float)(toFrame - startFrame);
                        Quaternion rotation = Quaternion.Slerp(fromTransfrom.rotation, toTransfrom.rotation, ratio);
                        Vector3 trans = fromTransfrom.translation + (toTransfrom.translation - fromTransfrom.translation) * ratio;
                        anim.pose[frame].transforms[i] = new Transform(trans, rotation, 1.0f);
                    }


                }
            }



            for (int frame = toFrame; frame < anim.pose.Length; ++frame)
            {
                pose = anim.pose[frame];
                nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
                for (int i = 0; i < nbones; i++)
                {
                    pose.transforms[i] *= skeleton.bones[i].patch;
                }
            }

            ClearPatch();
            command_man.ClearCommands();
        }



        public void makeToAnimationBezier(int toFrame)
        {

            if (startFrame >= toFrame)
            {
                string caption = "Error Detected in Input";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("Failed to make animation", caption, buttons);
                return;
            }

            hkaPose pose = anim.pose[startFrame];
            hkaPose toPose = anim.pose[toFrame];
            float curveStrength = 0.1f;
            int nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
            for (int i = 0; i < nbones; i++)
            {
                for (int frame = startFrame + 1; frame < toFrame; ++frame)
                {
                    if(skeleton.bones[i].patch.rotation != Quaternion.Identity || skeleton.bones[i].patch.translation != Vector3.Zero)
                    {
                        Transform toTransfrom = toPose.transforms[i] * skeleton.bones[i].patch;
                        Transform fromTransfrom = pose.transforms[i];
                        float ratio = (float)(frame - startFrame) / (float)(toFrame - startFrame);
                        ratio = ratio * ratio * (3f - 2f * ratio); // Bezier 0.33,0, 0.66,1
                        Quaternion rotation = Quaternion.Slerp(fromTransfrom.rotation, toTransfrom.rotation, ratio);
                        Vector3 trans = fromTransfrom.translation + (toTransfrom.translation - fromTransfrom.translation) * ratio;
                        anim.pose[frame].transforms[i] = new Transform(trans, rotation, 1.0f);
                    }


                }
            }

            for (int frame = toFrame; frame < anim.pose.Length; ++frame)
            {
                pose = anim.pose[frame];
                nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
                for (int i = 0; i < nbones; i++)
                {
                    pose.transforms[i] *= skeleton.bones[i].patch;
                }
            }


            ClearPatch();
            command_man.ClearCommands();
        }

            public void deleteAnimationFrame(int toFrame)
        {
            hkaPose[] pose1;
            int length = anim.pose.Length - (toFrame - startFrame + 1);
            if (length < 1 || startFrame >= toFrame)
                return;
            pose1 = new hkaPose[length];
            for (int i = 0; i < startFrame; i++)
            {
                pose1[i] = anim.pose[i];
            }

            for (int i = toFrame + 1; i < anim.pose.Length; i++)
            {
                pose1[startFrame + i - toFrame - 1] = anim.pose[i];
            }
            anim.numOriginalFrames = pose1.Length;
            anim.duration = anim.duration * ((float)(anim.pose.Length - length) / (float)(anim.pose.Length));
            anim.pose = pose1;
            if (LoadAnimationEvent != null)
                LoadAnimationEvent(this, EventArgs.Empty);

            AssignAnimationPose(0);
        }


        public void ClearPatch()
        {
            foreach (hkaBone bone in skeleton.bones)
            {
                bone.patch = new Transform();
            }
        }

        public static string EscapeFileName(string file)
        {
            return '"' + file + '"';
        }

        public static string CreateRawFileName(string file)
        {
            string basename = Path.GetFileNameWithoutExtension(file);
            return Path.Combine(Application.StartupPath, @"tmp\" + basename + ".bin");
        }

        public void LoadAnimation_1(string source_file, string hctout_file)
        {
            string raw_file = CreateRawFileName(source_file);
            {
                ProcessStartInfo info = new ProcessStartInfo(
                        Path.Combine(Application.StartupPath, @"bin\hkdump-bin.exe"),
                        EscapeFileName(hctout_file) + " -o " + EscapeFileName(raw_file));
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;

                Process process = Process.Start(info);
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
                if (process.ExitCode == 0) // successful
                {
                    if (anim.Load(raw_file))
                    {
                        LoadAnimationSuccessful(source_file);
                    }
                }
            }
        }

        public static string CreateHctOutputFileName(string file)
        {
            string basename = Path.GetFileNameWithoutExtension(file);
            return Path.Combine(Application.StartupPath, @"win32\" + basename + ".hkx");
        }

        public void LoadAnimation(string source_file)
        {
            string hctout_file = CreateHctOutputFileName(source_file);
            {
                ProcessStartInfo info = new ProcessStartInfo(
                        Path.Combine(Application.StartupPath, @"bin\hct.exe"),
                        EscapeFileName(source_file) + " -o " + EscapeFileName(hctout_file));
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;

                Process process = Process.Start(info);
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
                if (process.ExitCode == 0) // successful
                {
                    LoadAnimation_1(source_file, hctout_file);
                }
            }
        }

        string anim_filename = "idle.hkx";

        void LoadAnimationSuccessful(string source_file)
        {
            this.anim_filename = Path.GetFileName(source_file);

            if (LoadAnimationEvent != null)
                LoadAnimationEvent(this, EventArgs.Empty);

            AssignAnimationPose(0);
        }

        public void SaveAnimation(string dest_file)
        {
            string raw_file = CreateRawFileName(dest_file);

            ApplyPatchToAnimation();

            //anim.numOriginalFrames = 1;
            //anim.duration = 1.0f/30.0f;
            anim.Save(raw_file);

            ProcessStartInfo info = new ProcessStartInfo(
                    Path.Combine(Application.StartupPath, @"bin\hkconv.exe"),
                    EscapeFileName(raw_file) + " -o " + EscapeFileName(dest_file));
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            Process process = Process.Start(info);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
        }

        SimpleCamera camera = new SimpleCamera();

        public int GetNumFrames()
        {
            return anim.numOriginalFrames;
        }

        public void SetCurrentPose(int pose_i)
        {
            AssignAnimationPose(pose_i);
        }

        public void Update()
        {
            camera.Update();
            view = camera.ViewMatrix;
            wvp = view * proj;
            world_to_screen = view * proj * CreateViewportMatrix(viewport);
            renderer3d.Update(ref wvp);
            renderer2d.Update(ref world_to_screen);
        }

        public void Render()
        {
            //TODO: create resources if discarded
            renderer3d.Render();
            renderer2d.Render(swapChain, ref viewport, selected_bone, anim_filename);

            swapChain.Present(0, PresentFlags.None);
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
        public Vector3 WorldToScreen(Vector3 v)
        {
            return Vector3.TransformCoordinate(v, world_to_screen);
        }

        Vector3 GetBonePositionOnScreen(hkaBone bone)
        {
            Transform t = bone.GetWorldCoordinate();
            return WorldToScreen(t.translation);
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

        public bool SelectBone(String name)
        {
            bool found = false;
            hkaBone found_bone = null;
            foreach (hkaBone bone in skeleton.bones)
            {
                if (bone.hide)
                    continue;
                if(name.Equals(bone.name))
                {
                    found_bone = bone;
                    found = true;
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

        public CommandManager command_man { get; }
        BoneCommand bone_command = null;

        public Viewer(Settings settings)
        {
            renderer3d = new Renderer3d();
            renderer3d.ScreenColor = settings.ScreenColor;

            renderer2d = new Renderer2d();
            renderer2d.ScreenColor = settings.ScreenColor;

            command_man = new CommandManager();
        }

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
                command_man.Execute(bone_command);

            bone_command = null;
        }

        /// 選択boneを指定軸方向に移動します。
        public void TranslateAxis(int dx, int dy, Vector3 axis)
        {
            hkaBone bone = selected_bone;

            if (bone == null)
                return;

            axis = Vector3.Transform(axis, bone.local.rotation * bone.patch.rotation);

            float len = dx * 0.0125f;
            bone.patch.translation += axis * len;
        }

        public void TranslateRootAxis(int dx, int dy, Vector3 axis)
        {
            hkaBone bone = selected_bone;  

            if (bone == null)
                return;
            float len = dx * 0.025f;
            bone.patch.axisTranslation += axis * len;
        }

        /// 選択boneを指定軸中心に回転します。
        public void RotateAxis(int dx, int dy, Vector3 axis)
        {
            hkaBone bone = selected_bone;

            if (bone == null)
                return;

            float angle = dx * 0.005f;
            bone.patch.rotation *= Quaternion.RotationAxis(axis, angle);
        }


        public void RotateCenterAxis(int dx, int dy, Vector3 axis)
        {
            hkaBone bone = selected_bone;

            if (bone == null)
                return;

            float angle = dx * 0.01f;
            bone.patch.axisRotation *= Quaternion.RotationAxis(axis, angle);
        }

        public void exportPose(int idx)
        {
            hkaPose pose = anim.pose[idx];
            BinaryWriter bw = new BinaryWriter(new FileStream("pose_temp.dat", FileMode.Create));
            pose.WriteExport(bw);
            bw.Close();
            Console.WriteLine("pose.transforms.Length {0}, floats.Length: {1}", pose.transforms.Length, pose.floats.Length);
            int nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
            for (int i = 0; i < nbones; i++)
            {
                skeleton.bones[i].local = pose.transforms[i];
                Console.WriteLine(skeleton.bones[i].name);
                skeleton.bones[i].local.Dump();
            }
            bw.Close();
        }

        public void exportAnimation(int idx)
        {
         
            BinaryWriter bw = new BinaryWriter(new FileStream("anime_temp.dat", FileMode.Create));
            anim.Write(bw,startFrame, idx);
            //pose.WriteExport(bw);
            bw.Close();
            Console.WriteLine("pose.transforms.Length {0}, floats.Length: {1}", anim.numTransforms, anim.numFloats);
        }

        public void importAnimation(int idx)
        {
            hkaAnimation hkaTemp = new hkaAnimation();


            BinaryReader br = new BinaryReader(new FileStream("anime_temp.dat", FileMode.Open));
            hkaTemp.Read(br);
            Console.WriteLine("anime.transforms.Length {0}, floats.Length: {1}", hkaTemp.numTransforms, anim.numFloats);

            int length = hkaTemp.pose.Length;
            hkaPose[] newPoses = new hkaPose[length + anim.pose.Length];

            Array.Copy(anim.pose, newPoses, idx + 1);
            Array.Copy(anim.pose, idx + 1, newPoses, idx + length + 1, newPoses.Length - idx - length - 1);
            Array.Copy(hkaTemp.pose, 0, newPoses, idx + 1, length);
            int orgLength = anim.pose.Length;
            anim.pose = newPoses;
            anim.numOriginalFrames = newPoses.Length;
            anim.duration = anim.duration * (float)newPoses.Length / (float)orgLength;

            if (LoadAnimationEvent != null)
                LoadAnimationEvent(this, EventArgs.Empty);

            AssignAnimationPose(idx + length);

        }

        public void importPose()
        {
            hkaPose pose = new hkaPose();

            BinaryReader br = new BinaryReader(new FileStream("pose_temp.dat", FileMode.Open));

            pose.readimport(br);

            int nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);

            for (int i = 0; i < nbones; i++)
            {
                if (!"NPC Root [Root]".Equals(skeleton.bones[i].name) && !"NPC COM [COM ]".Equals(skeleton.bones[i].name))
                   {
                    Quaternion invertPatch = skeleton.bones[i].local.rotation;
                    invertPatch.Invert();
                    skeleton.bones[i].patch.rotation = invertPatch * pose.transforms[i].rotation;
                    skeleton.bones[i].patch.translation = pose.transforms[i].translation - skeleton.bones[i].local.translation;
                }
            }
            br.Close();

        }

        public void processKey(Keys key)
        {
            switch (key)
            {
                case Keys.Space:
                    SelectBone("NPC COM [COM ]");
                    break;
                case Keys.D1:
                    SelectBone("NPC Spine [Spn0]");
                    break;
                case Keys.D2:
                    SelectBone("NPC Spine1 [Spn1]");
                    break;
                case Keys.D3:
                    SelectBone("NPC Spine2 [Spn2]");
                    break;
                case Keys.D4:
                    SelectBone("NPC Neck [Neck]");
                    break;
                case Keys.E:
                    SelectBone("NPC R UpperArm [RUar]");
                    break;
                case Keys.R:
                    SelectBone("NPC L UpperArm [LUar]");
                    break;
                case Keys.D:
                    SelectBone("NPC R Forearm [RLar]");
                    break;
                case Keys.F:
                    SelectBone("NPC L Forearm [LLar]");
                    break;
                case Keys.C:
                    SelectBone("NPC R Hand [RHnd]");
                    break;
                case Keys.V:
                    SelectBone("NPC L Hand [LHnd]");
                    break;
                case Keys.T:
                    SelectBone("NPC R Thigh [RThg]");
                    break;
                case Keys.Y:
                    SelectBone("NPC L Thigh [LThg]");
                    break;
                case Keys.G:
                    SelectBone("NPC R Calf [RClf]");
                    break;
                case Keys.H:
                    SelectBone("NPC L Calf [LClf]");
                    break;
                case Keys.B:
                    SelectBone("NPC R Foot [Rft ]");
                    break;
                case Keys.N:
                    SelectBone("NPC L Foot [Lft ]");
                    break;
                case Keys.Q:
                    RotateAxis(-10, 0, new SharpDX.Vector3(1, 0, 0));
                    break;
                case Keys.W:
                    RotateAxis(10, 0, new SharpDX.Vector3(1, 0, 0));
                    break;
                case Keys.A:
                    RotateAxis(-10, 0, new SharpDX.Vector3(0, 1, 0));
                    break;
                case Keys.S:
                    RotateAxis(10, 0, new SharpDX.Vector3(0, 1, 0));
                    break;
                case Keys.Z:
                    RotateAxis(-10, 0, new SharpDX.Vector3(0, 0, 1));
                    break;
                case Keys.X:
                    RotateAxis(10, 0, new SharpDX.Vector3(0, 0, 1));
                    break;

            }
        }


        public void importPoseFrame(int frame)
        {
            hkaPose pose = new hkaPose();

            BinaryReader br = new BinaryReader(new FileStream("pose_temp.dat", FileMode.Open));

            pose.readimport(br);

            int nbones = System.Math.Min(skeleton.bones.Length, pose.transforms.Length);
            nbones = System.Math.Min(anim.pose[frame].transforms.Length, nbones);
            
            for (int i = 0; i < nbones; i++)
            {
                anim.pose[frame].transforms[i] = pose.transforms[i];
                skeleton.bones[i].local = pose.transforms[i];
            }
        }

        public void Dispose()
        {
            System.Console.WriteLine("Viewer.Dispose");

            renderer2d.DiscardDeviceResources();
            renderer3d.Dispose();

            Utilities.Dispose(ref swapChain);
            Utilities.Dispose(ref factory1);
            Utilities.Dispose(ref device);
        }

        public hkaBone getSelectedBone()
        {
            return selected_bone;
        }

        public void SetStartFrame(int idx)
        {
            this.startFrame = idx;
        }
    }
}
