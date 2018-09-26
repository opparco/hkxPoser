using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SharpDX;
using SharpDX.Direct3D;

/// <summary>
/// カメラ
/// </summary>
public class SimpleCamera
{
    //角度
    Vector3 angle;

    //回転中心
    Vector3 center;

    //位置変位
    Vector3 translation;

    //カメラ移動方向ベクトル
    Vector3 dirD;

    //更新する必要があるか
    bool needUpdate;

    //view行列
    Matrix view;

    /// <summary>
    /// 角度
    /// </summary>
    public Vector3 Angle { get { return angle; } set { angle = value; } }

    /// <summary>
    /// 回転中心
    /// </summary>
    public Vector3 Center { get { return center; } set { center = value; } }

    /// <summary>
    /// 位置変位
    /// </summary>
    public Vector3 Translation { get { return translation; } set { translation = value; } }

    /// <summary>
    /// 更新する必要があるか
    /// </summary>
    public bool NeedUpdate { get { return needUpdate; } }

    /// <summary>
    /// view行列
    /// </summary>
    public Matrix ViewMatrix { get { return view; } }

    /// <summary>
    /// カメラを生成します。
    /// </summary>
    public SimpleCamera()
    {
        Reset();
        dirD = Vector3.Zero;
        view = Matrix.Identity;
    }

    /// <summary>
    /// カメラの位置と姿勢をリセットします。
    /// </summary>
    public void Reset()
    {
        angle = new Vector3(MathUtil.PiOverTwo, 0.0f, MathUtil.Pi);
        center = Vector3.Zero;
        ResetTranslation();
    }

    /// <summary>
    /// view座標上のカメラの位置をリセットします。
    /// </summary>
    public void ResetTranslation()
    {
        // screen center is "NPC COM [COM ]"
        translation = new Vector3(0.0f, 68.9113f, 280.0f);
        needUpdate = true;
    }

    /// <summary>
    /// カメラの位置を更新します。
    /// </summary>
    /// <param name="dirX">移動方向（経度）</param>
    /// <param name="dirY">移動方向（緯度）</param>
    public void Move(float dirX, float dirY)
    {
        if (dirX == 0.0f && dirY == 0.0f)
            return;

        dirD.X += dirX;
        dirD.Y += dirY;
        needUpdate = true;
    }

    /// <summary>
    /// カメラの位置と姿勢を更新します。
    /// </summary>
    public void Update()
    {
        if (!needUpdate)
            return;

        angle.X += +dirD.Y;
        angle.Z += -dirD.X;

        Matrix m;
        GetRotation(out m);
        m.M41 = center.X;
        m.M42 = center.Y;
        m.M43 = center.Z;
        m.M44 = 1;

        view = Matrix.Invert(m) * Matrix.Translation(-translation);

        //差分をリセット
        ResetDefValue();
        needUpdate = false;
    }

    /// <summary>
    /// view行列を取得します。
    /// </summary>
    public Matrix GetViewMatrix()
    {
        return view;
    }

    /// <summary>
    /// 回転中心を設定します。
    /// </summary>
    /// <param name="center">回転中心</param>
    public void SetCenter(Vector3 center)
    {
        this.center = center;
        needUpdate = true;
    }
    /// <summary>
    /// 回転中心を設定します。
    /// </summary>
    /// <param name="x">回転中心x座標</param>
    /// <param name="y">回転中心y座標</param>
    /// <param name="z">回転中心z座標</param>
    public void SetCenter(float x, float y, float z)
    {
        SetCenter(new Vector3(x, y, z));
    }

    /// <summary>
    /// view座標上の位置を設定します。
    /// </summary>
    /// <param name="translation">view座標上の位置</param>
    public void SetTranslation(Vector3 translation)
    {
        this.translation = translation;
        needUpdate = true;
    }
    /// <summary>
    /// 位置変位を設定します。
    /// </summary>
    /// <param name="x">X変位</param>
    /// <param name="y">Y変位</param>
    /// <param name="z">Z変位</param>
    public void SetTranslation(float x, float y, float z)
    {
        SetTranslation(new Vector3(x, y, z));
    }

    /// centerを変更してもviewを維持できるようにtranslationを更新する。
    public void UpdateTranslation()
    {
        Matrix m;
        GetRotation(out m);
        m.M41 = center.X;
        m.M42 = center.Y;
        m.M43 = center.Z;
        m.M44 = 1;

        m *= view;

        Vector3 t = new Vector3(m.M41, m.M42, m.M43);
        this.translation = -t;
        needUpdate = true;
    }

    public void GetRotation(out Matrix m)
    {
        m = Matrix.RotationY(angle.Y) * Matrix.RotationX(angle.X) * Matrix.RotationZ(angle.Z);
    }

    /// <summary>
    /// 角度を設定します。
    /// </summary>
    /// <param name="angle">角度</param>
    public void SetAngle(Vector3 angle)
    {
        this.angle = angle;
        needUpdate = true;
    }
    /// <summary>
    /// 角度を設定します。
    /// </summary>
    /// <param name="x">X軸回転角</param>
    /// <param name="y">Y軸回転角</param>
    /// <param name="z">Z軸回転角</param>
    public void SetAngle(float x, float y, float z)
    {
        SetAngle(new Vector3(x, y, z));
    }

    /// <summary>
    /// view座標上で移動します。
    /// </summary>
    /// <param name="dx">X軸移動距離</param>
    /// <param name="dy">Y軸移動距離</param>
    /// <param name="dz">Z軸移動距離</param>
    public void MoveView(float dx, float dy, float dz)
    {
        this.translation.X += dx;
        this.translation.Y += dy;
        this.translation.Z += dz;
        needUpdate = true;
    }

    /// <summary>
    /// 差分をリセットします。
    /// </summary>
    protected void ResetDefValue()
    {
        dirD = Vector3.Zero;
    }
}
