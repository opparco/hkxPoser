using SharpDX;

namespace hkxPoser
{
    /// 操作を扱います。
    public interface ICommand
    {
        /// 元に戻す。
        void Undo();

        /// やり直す。
        void Redo();

        /// 実行する。
        bool Execute();
    }

    /// bone属性
    public struct BoneAttr
    {
        /// 回転
        public Quaternion rotation;
        /// 移動
        public Vector3 translation;

        public Quaternion axisRotation;

        public Vector3 axisTranslation;
    }

    /// bone操作
    public class BoneCommand : ICommand
    {
        //操作対象bone
        hkaBone bone = null;
        /// 変更前の属性
        BoneAttr old_attr;
        /// 変更後の属性
        BoneAttr new_attr;

        /// bone操作を生成します。
        public BoneCommand(hkaBone bone)
        {
            this.bone = bone;
            this.old_attr.rotation = bone.patch.rotation;
            this.old_attr.axisRotation = bone.patch.axisRotation;
            this.old_attr.translation = bone.patch.translation;
            this.old_attr.axisTranslation = bone.patch.axisTranslation;
        }

        /// 元に戻す。
        public void Undo()
        {
            bone.patch.rotation = old_attr.rotation;
            bone.patch.axisRotation = this.old_attr.axisRotation;
            bone.patch.translation = old_attr.translation;
            bone.patch.axisTranslation = old_attr.axisTranslation;
        }

        /// やり直す。
        public void Redo()
        {
            bone.patch.rotation = new_attr.rotation;
            bone.patch.axisRotation = new_attr.axisRotation;
            bone.patch.translation = new_attr.translation;
            bone.patch.axisTranslation = new_attr.axisTranslation;
        }

        /// 実行する。
        public bool Execute()
        {
            this.new_attr.rotation = bone.patch.rotation;
            this.new_attr.translation = bone.patch.translation;
            this.new_attr.axisRotation = bone.patch.axisRotation;
            this.new_attr.axisTranslation = bone.patch.axisTranslation;
            bool updated = old_attr.rotation != new_attr.rotation || old_attr.translation != new_attr.translation || old_attr.axisRotation != new_attr.axisRotation || old_attr.axisTranslation != new_attr.axisTranslation;
            return updated;
        }
    }
}
