
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace hkxPoser
{
    class Renderer2d
    {
        SharpDX.Direct2D1.Factory d2dFactory;
        SharpDX.DirectWrite.Factory dwriteFactory;

        RenderTarget renderTarget;

        SolidColorBrush textBrush;
        SolidColorBrush boneLineBrush;
        SolidColorBrush selectedBoneLineBrush;
        SolidColorBrush xaxisBrush;
        SolidColorBrush yaxisBrush;
        SolidColorBrush zaxisBrush;

        TextFormat textFormat;

        Color textColor = Color.Black;
        Color boneLineColor = new ColorBGRA(100, 100, 230, 255);
        Color selectedBoneLineColor = new ColorBGRA(255, 0, 0, 255);
        Color xaxisColor = new ColorBGRA(255, 0, 0, 255);
        Color yaxisColor = new ColorBGRA(0, 255, 0, 255);
        Color zaxisColor = new ColorBGRA(0, 0, 255, 255);

        hkaSkeleton skeleton;

        public void CreateDeviceIndependentResources(hkaSkeleton skeleton)
        {
            this.skeleton = skeleton;

            d2dFactory = new SharpDX.Direct2D1.Factory();
            dwriteFactory = new SharpDX.DirectWrite.Factory();
        }

        public void CreateDeviceResources(SwapChain swapChain)
        {
            if (renderTarget == null)
            {
                using (Surface sf = swapChain.GetBackBuffer<Surface>(0))
                {
                    renderTarget = new RenderTarget(d2dFactory, sf, new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));
                }

                textBrush = new SolidColorBrush(renderTarget, textColor);
                boneLineBrush = new SolidColorBrush(renderTarget, boneLineColor);
                selectedBoneLineBrush = new SolidColorBrush(renderTarget, selectedBoneLineColor);
                xaxisBrush = new SolidColorBrush(renderTarget, xaxisColor);
                yaxisBrush = new SolidColorBrush(renderTarget, yaxisColor);
                zaxisBrush = new SolidColorBrush(renderTarget, zaxisColor);

                textFormat = new TextFormat(dwriteFactory, "Verdana", FontWeight.Bold, FontStyle.Normal, 14.0f);
            }
        }

        public void DiscardDeviceResources()
        {
            if (renderTarget != null)
            {
                textFormat?.Dispose();

                zaxisBrush?.Dispose();
                yaxisBrush?.Dispose();
                xaxisBrush?.Dispose();
                selectedBoneLineBrush?.Dispose();
                boneLineBrush?.Dispose();
                textBrush?.Dispose();

                renderTarget.Dispose();
                renderTarget = null;
            }
        }

        Matrix world_to_screen;

        public void Update(ref Matrix world_to_screen)
        {
            this.world_to_screen = world_to_screen;
        }

        /// ワールド座標をスクリーン位置へ変換します。
        public Vector3 WorldToScreen(Vector3 v)
        {
            return Vector3.TransformCoordinate(v, world_to_screen);
        }

        public Color ScreenColor { get; set; }

        public void Render(SwapChain swapChain, ref Viewport viewport, hkaBone selected_bone, string anim_filename)
        {
            Size2 size = new Size2(viewport.Width, viewport.Height);

            CreateDeviceResources(swapChain);

            renderTarget.BeginDraw();

            //renderTarget.Clear(ScreenColor);

            DrawCenterAxis();

            DrawBoneTree();
            DrawSelectedBone(selected_bone);

            DrawText(anim_filename, ref size);

            try
            {
                renderTarget.EndDraw();
            }
            catch (SharpDXException ex) when ((uint)ex.HResult == 0x8899000C) // D2DERR_RECREATE_TARGET
            {
                // device has been lost!
                DiscardDeviceResources();
            }
        }

        void DrawText(string anim_filename, ref Size2 size)
        {
            renderTarget.DrawText(string.Format("File: {0}", anim_filename), textFormat, new RectangleF(12, size.Height-12-45-20, size.Width-24, 20), textBrush);
        }

        void DrawLine(Vector3 p0, Vector3 p1, Brush brush)
        {
            renderTarget.DrawLine(new Vector2(p0.X, p0.Y), new Vector2(p1.X, p1.Y), brush);
        }

        void DrawCenterAxis()
        {
            Vector3 center = Vector3.Zero;
            Vector3 xaxis = new Vector3(5, 0, 0);
            Vector3 yaxis = new Vector3(0, 5, 0);
            Vector3 zaxis = new Vector3(0, 0, 5);

            Vector3 scr_center = WorldToScreen(center);
            Vector3 scr_xaxis = WorldToScreen(xaxis);
            Vector3 scr_yaxis = WorldToScreen(yaxis);
            Vector3 scr_zaxis = WorldToScreen(zaxis);

            DrawLine(scr_center, scr_xaxis, xaxisBrush);
            DrawLine(scr_center, scr_yaxis, yaxisBrush);
            DrawLine(scr_center, scr_zaxis, zaxisBrush);
        }

        Vector3 GetBonePositionOnScreen(hkaBone bone)
        {
            Transform t = bone.GetWorldCoordinate();
            return WorldToScreen(t.translation);
        }

        public void DrawBoneTree()
        {
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

                    DrawBoneLine(ref vertices);
                }
            }

            foreach (hkaBone bone in skeleton.bones)
            {
                if (bone.hide)
                    continue;

                Vector3 p0 = GetBonePositionOnScreen(bone);
                p0.Z = 0.0f;

                DrawBoneEllipse(ref p0);
            }

            //DrawBoneAxis(bone);
        }

        /// 選択boneを描画する。
        void DrawSelectedBone(hkaBone selected_bone)
        {
            if (selected_bone == null)
                return;

            Vector3 p1 = GetBonePositionOnScreen(selected_bone);
            p1.Z = 0.0f;

            if (selected_bone.children.Count != 0)
            {
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

                DrawBoneLine(ref vertices, true);
            }

            DrawBoneAxis(selected_bone);

            DrawBoneEllipse(ref p1, true);
        }

        void DrawBoneAxis(hkaBone bone)
        {
            Transform t = bone.GetWorldCoordinate();
            DrawBoneAxis(t);
        }

        SolidColorBrush GetLineBrush(bool selected = false)
        {
            return selected ? selectedBoneLineBrush : boneLineBrush;
        }

        public void DrawBoneLine(ref RawVector2[] vertices, bool selected = false)
        {
            SolidColorBrush lineBrush = GetLineBrush(selected);

            renderTarget.DrawLine(vertices[0], vertices[1], lineBrush);
            renderTarget.DrawLine(vertices[1], vertices[2], lineBrush);
        }

        public void DrawBoneEllipse(ref Vector3 p0, bool selected = false)
        {
            SolidColorBrush lineBrush = GetLineBrush(selected);

            renderTarget.DrawEllipse(new Ellipse(new Vector2(p0.X, p0.Y), 4, 4), lineBrush);
            renderTarget.FillEllipse(new Ellipse(new Vector2(p0.X, p0.Y), 2, 2), lineBrush);
        }

        public void DrawBoneAxis(Transform t)
        {
            Matrix3x3 rotation;
            Matrix3x3.RotationQuaternion(ref t.rotation, out rotation);
            Vector3 xvec = rotation.Row1 * 5.0f;
            Vector3 yvec = rotation.Row2 * 5.0f;
            Vector3 zvec = rotation.Row3 * 5.0f;
            Vector3 location = t.translation;

            Vector3 xaxis = location + xvec;
            Vector3 yaxis = location + yvec;
            Vector3 zaxis = location + zvec;

            Vector3 scr_location = WorldToScreen(location);
            Vector3 scr_xaxis = WorldToScreen(xaxis);
            Vector3 scr_yaxis = WorldToScreen(yaxis);
            Vector3 scr_zaxis = WorldToScreen(zaxis);

            DrawLine(scr_location, scr_xaxis, xaxisBrush);
            DrawLine(scr_location, scr_yaxis, yaxisBrush);
            DrawLine(scr_location, scr_zaxis, zaxisBrush);
        }
    }
}
