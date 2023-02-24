/**
 * skeleton.bin anim.bin format
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using SharpDX;

public class Transform
{
    public Vector3 translation;
    public Quaternion rotation;
    public float scale;

    public Quaternion axisRotation;
    public Vector3 axisTranslation;

    public Transform()
    {
        this.translation = Vector3.Zero;
        this.axisTranslation = Vector3.Zero;
        this.rotation = Quaternion.Identity;
        this.scale = 1.0f;
        this.axisRotation = Quaternion.Identity;
    }

    public Transform(Vector3 translation, Quaternion rotation, float scale)
    {
        this.translation = translation;
        this.rotation = rotation;
        this.scale = scale;
        this.axisRotation = Quaternion.Identity;
        this.axisTranslation = Vector3.Zero;
    }


    public Transform(Transform t)
    {
        this.translation = t.translation;
        this.rotation = t.rotation;
        this.scale = t.scale;
        this.axisRotation = t.axisRotation;
        this.axisTranslation = t.axisTranslation;
    }

    public static Transform operator *(Transform t1, Transform t2)
    {

        if(t1.axisTranslation != Vector3.Zero && t1.axisTranslation != Vector3.Zero)
        {
            Transform result = new Transform(t1);
            result.axisTranslation += t2.axisTranslation;
            return result;
        }
        else
        {
            if (t2.axisTranslation != Vector3.Zero)
            {
             return  new Transform(
             t1.translation + t2.axisTranslation,
             t1.rotation * t2.rotation,
             t1.scale * t2.scale);
            }
        }

        if(t1.axisRotation != Quaternion.Identity && t2.axisRotation == Quaternion.Identity)
        {
            Vector3 result = new Vector3();
            Vector3.Transform(ref t2.translation, ref t1.axisRotation, out result);
            return new Transform(result, t1.axisRotation * t2.rotation , t1.scale * t2.scale);

        }

        if (t2.axisRotation != Quaternion.Identity && t1.axisRotation == Quaternion.Identity)
        {
            Vector3 result = new Vector3();
            Vector3.Transform(ref t1.translation, ref t2.axisRotation, out result);
            return new Transform(result, t2.axisRotation * t1.rotation, t1.scale * t2.scale);

        }

        return new Transform(
            t1.translation + Vector3.Transform(t2.translation, t1.rotation) * t1.scale,
            t1.rotation * t2.rotation,
            t1.scale * t2.scale);
    }

    public void Dump()
    {
        Console.WriteLine("Translation: {0:F6} {1:F6} {2:F6}", translation.X, translation.Y, translation.Z);
        Console.WriteLine("Rotation: {0:F6} {1:F6} {2:F6} {3:F6}", rotation.W, rotation.X, rotation.Y, rotation.Z);
        Console.WriteLine("Scale: {0:F6}", scale);
    }

    public void Read(BinaryReader reader)
    {
        Vector4 t;
        Vector4 scale;

        reader.ReadVector4(out t);
        reader.ReadQuaternion(out this.rotation);
        reader.ReadVector4(out scale);

        this.translation = new Vector3(t.X, t.Y, t.Z);
        this.scale = scale.Z;
    }

    public void Write(BinaryWriter writer)
    {
        Vector4 t = new Vector4(this.translation, 0);
        Vector4 scale = new Vector4(this.scale, this.scale, this.scale, 0);

        writer.Write(ref t);
        writer.Write(ref rotation);
        writer.Write(ref scale);
    }
}

public class hkaBone
{
    public string name;
    public short idx;
    public bool hide = false;
    public hkaBone parent = null;
    public List<hkaBone> children = new List<hkaBone>();
    public Transform local;
    public Transform patch;

    public void Read(BinaryReader reader)
    {
        this.name = reader.ReadCString();
    }

    public void Write(BinaryWriter writer)
    {
        writer.WriteCString(this.name);
    }

    public Transform GetWorldCoordinate()
    {
        Transform t = new Transform();
        hkaBone bone = this;
        while (bone != null)
        {
            t = bone.local * bone.patch * t;
            bone = bone.parent;
        }
        return t;
    }
}

public class hkaSkeleton
{
    public string name;
    public short[] parentIndices;
    public hkaBone[] bones;
    public Transform[] referencePose;
    public float[] referenceFloats;
    public string[] floatSlots;

    /// load skeleton.bin
    public void Load(string filename)
    {
        using (Stream stream = File.OpenRead(filename))
            Load(stream);
    }

    public void Load(Stream stream)
    {
        using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default))
        {
            string head = reader.ReadHeaderString();
            uint version = reader.ReadUInt32();
            // should be 0x01000200
            int nskeletons = reader.ReadInt32();
            // should be 1 or 2
            Read(reader);

            int nanimations = reader.ReadInt32();
            // should be 0
        }
    }

    public void Read(BinaryReader reader)
    {
        /// A user name to aid in identifying the skeleton
        this.name = reader.ReadCString();

        /// Parent relationship
        int nparentIndices = reader.ReadInt32();
        this.parentIndices = new short[nparentIndices];
        for (int i=0; i<nparentIndices; i++)
        {
            this.parentIndices[i] = reader.ReadInt16();
        }

        /// Bones for this skeleton
        int nbones = reader.ReadInt32();
        this.bones = new hkaBone[nbones];
        for (int i=0; i<nbones; i++)
        {
            hkaBone bone = new hkaBone();
            bone.Read(reader);
            this.bones[i] = bone;
        }

        for (short i=0; i<nbones; i++)
        {
            hkaBone bone = this.bones[i];
            bone.idx = i;
            short parent_idx = this.parentIndices[i];
            if (parent_idx != -1)
            {
                hkaBone parent = this.bones[parent_idx];
                bone.parent = parent;
                parent.children.Add(bone);
            }
        }

        // hide NPC Root
        this.bones[0].hide = true;

#if false
        // hide since Camera3rd
        if (nbones != 1)
        {
            bool hide = false;
            for (int i=1; i<nbones; i++)
            {
                hkaBone bone = this.bones[i];
                if (bone.parent == null)
                    hide = true;
                bone.hide = hide;
            }
        }
#endif

        /// The reference pose for the bones of this skeleton. This pose is stored in local space.
        int nreferencePose = reader.ReadInt32();
        this.referencePose = new Transform[nreferencePose];
        for (int i=0; i<nreferencePose; i++)
        {
            Transform t = new Transform();
            t.Read(reader);
            this.referencePose[i] = t;
        }

        for (int i=0; i<nbones; i++)
        {
            hkaBone bone = this.bones[i];
            bone.local = this.referencePose[i];
            bone.patch = new Transform();
        }

        /// The reference values for the float slots of this skeleton. This pose is stored in local space.
        int nreferenceFloats = reader.ReadInt32();
        this.referenceFloats = new float[nreferenceFloats];
        for (int i=0; i<nreferenceFloats; i++)
        {
            this.referenceFloats[i] = reader.ReadSingle();
        }

        /// Floating point track slots. Often used for auxiliary float data or morph target parameters etc.
        /// This defines the target when binding animations to a particular rig.
        int nfloatSlots = reader.ReadInt32();
        this.floatSlots = new string[nfloatSlots];
        for (int i=0; i<nfloatSlots; i++)
        {
            this.floatSlots[i] = reader.ReadCString();
        }
    }

    /// save skeleton.bin
    public void Save(string filename)
    {
        using (Stream stream = File.Create(filename))
            Save(stream);
    }

    public void Save(Stream stream)
    {
        using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Default))
        {
            string head = "hkdump File Format, Version 1.0.2.0";
            uint version = 0x01000200;
            int nskeletons = 1;
            int nanimations = 0;

            writer.WriteHeaderString(head);
            writer.Write(version);
            writer.Write(nskeletons);

            Write(writer);

            writer.Write(nanimations);
        }
    }

    public void Write(BinaryWriter writer)
    {
        /// A user name to aid in identifying the skeleton
        writer.WriteCString(this.name);

        /// Parent relationship
        {
            int nparentIndices = this.parentIndices.Length;
            writer.Write((int)nparentIndices);
            for (int i = 0; i < nparentIndices; i++)
            {
                writer.Write(this.parentIndices[i]);
            }
        }
        /// Bones for this skeleton
        {
            int nbones = this.bones.Length;
            writer.Write((int)nbones);
            for (int i = 0; i < nbones; i++)
            {
                this.bones[i].Write(writer);
            }
        }
        /// The reference pose for the bones of this skeleton. This pose is stored in local space.
        {
            int nreferencePose = this.referencePose.Length;
            writer.Write((int)nreferencePose);
            for (int i = 0; i < nreferencePose; i++)
            {
                this.referencePose[i].Write(writer);
            }
        }
        /// The reference values for the float slots of this skeleton. This pose is stored in local space.
        writer.Write((int)0);
        /// Floating point track slots. Often used for auxiliary float data or morph target parameters etc.
        /// This defines the target when binding animations to a particular rig.
        writer.Write((int)0);
    }
}

public class hkaPose
{
    public float time;
    public Transform[] transforms;
    public float[] floats;

    public void Read(BinaryReader reader, int numTransforms, int numFloats)
    {
        this.time = reader.ReadSingle();

        this.transforms = new Transform[numTransforms];
        for (int i = 0; i < numTransforms; i++)
        {
            Transform t = new Transform();
            t.Read(reader);
            this.transforms[i] = t;
        }

        this.floats = new float[numFloats];
        for (int i = 0; i < numFloats; i++)
        {
            this.floats[i] = reader.ReadSingle();
        }
    }

    public void WriteExport(BinaryWriter writer)
    {

        writer.Write(transforms.Length);
        writer.Write(floats.Length);
        Write(writer);
    }


    public void readimport(BinaryReader reader)
    {
        int numTransforms;
        int numFloats;
        numTransforms = reader.ReadInt32();
        numFloats = reader.ReadInt32();
        Read(reader, numTransforms, numFloats);

    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(this.time);

        for (int i = 0, len = this.transforms.Length; i < len; i++)
        {
            this.transforms[i].Write(writer);
        }

        for (int i = 0, len = this.floats.Length; i < len; i++)
        {
            writer.Write(this.floats[i]);
        }
    }
}

public class Annotation
{
    public float time;
    public string text;

    public void Read(BinaryReader reader)
    {
        this.time = reader.ReadSingle();
        this.text = reader.ReadCString();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(this.time);
        writer.WriteCString(this.text);
    }
}

public class hkaDefaultMotion
{
    public Vector4 up;
    public Vector4 forward;
    public float duration;
    public Vector4[] referenceFrameSamples;

    public void Read(BinaryReader reader)
    {
        int frameType = reader.ReadInt32();
        //Console.WriteLine("frameType: {0} ", frameType);
        switch (frameType)
        {
        case 1:
            {
                reader.ReadVector4(out this.up);
                reader.ReadVector4(out this.forward);

                this.duration = reader.ReadSingle();

                int numReferenceFrameSamples = reader.ReadInt32();
                //Console.WriteLine("numReferenceFrameSamples: {0} ", numReferenceFrameSamples);
                this.referenceFrameSamples = new Vector4[numReferenceFrameSamples];
                for (int i = 0; i < numReferenceFrameSamples; i++)
                {
                    reader.ReadVector4(out this.referenceFrameSamples[i]);
                }
            }
            break;
        default:
            break;
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)1); // frameType

        writer.Write(ref this.up);
        writer.Write(ref this.forward);

        writer.Write(this.duration);

        int numReferenceFrameSamples = this.referenceFrameSamples.Length;
        writer.Write(numReferenceFrameSamples);
        for (int i = 0; i < numReferenceFrameSamples; i++)
        {
            writer.Write(ref this.referenceFrameSamples[i]);
        }
    }
}

public class hkaAnimation
{
    public int numOriginalFrames;
    public float duration;

    public hkaPose[] pose;
    public Annotation[] annotations;
    public hkaDefaultMotion defaultMotion;

    public int numTransforms { get { return pose[0].transforms.Length; } }
    public int numFloats { get { return pose[0].floats.Length; } }
    public bool hasExtractedMotion { get { return defaultMotion != null; } }

    /// load anim.bin
    public bool Load(string filename)
    {
        using (Stream stream = File.OpenRead(filename))
            return Load(stream);
    }

    public bool Load(Stream stream)
    {
        using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default))
        {
            string head = reader.ReadHeaderString();
            //TODO: throw exception
            uint version = reader.ReadUInt32();
            if (version != 0x01000200)
            {
		Console.WriteLine("Error: version mismatch! Abort.");
                return false;
            }
            int nskeletons = reader.ReadInt32();
            if (nskeletons != 0)
            {
		Console.WriteLine("Error: #skeletons should be 0 but {0}! Abort.", nskeletons);
                return false;
            }
            int nanimations = reader.ReadInt32();
            if (nanimations != 1)
            {
		Console.WriteLine("Error: #animations should be 1 but {0}! Abort.", nanimations);
                return false;
            }
            Read(reader);
        }
        return true;
    }

    public void Read(BinaryReader reader)
    {
        /// Returns the number of original samples / frames of animation.
        this.numOriginalFrames = reader.ReadInt32();
        /// The length of the animation cycle in seconds
    	this.duration = reader.ReadSingle();
        /// The number of bone tracks to be animated.
        int numTransforms = reader.ReadInt32();
        /// The number of float tracks to be animated
        int numFloats = reader.ReadInt32();

        /// Get a subset of the first 'maxNumTracks' transform tracks (all tracks from 0 to maxNumTracks-1 inclusive), and the first 'maxNumFloatTracks' float tracks of a pose at a given time.

        this.pose = new hkaPose[numOriginalFrames];
        for (int i = 0; i < numOriginalFrames; i++)
        {
            this.pose[i] = new hkaPose();
            this.pose[i].Read(reader, numTransforms, numFloats);
        }

        /// The annotation tracks associated with this skeletal animation.

        int numAnnotationTracks = reader.ReadInt32();
        //Console.WriteLine("numAnnotationTracks: {0} ", numAnnotationTracks);
        {
            int numAnnotations = reader.ReadInt32();
            //Console.WriteLine("numAnnotations: {0} ", numAnnotations);

            this.annotations = new Annotation[numAnnotations];
            for (int i = 0; i < numAnnotations; i++)
            {
                this.annotations[i] = new Annotation();
                this.annotations[i].Read(reader);
            }
        }
        for (int x = 1; x < numAnnotationTracks; x++)
        {
            int numAnnotations = reader.ReadInt32();
            //Console.WriteLine("numAnnotations: {0} ", numAnnotations);
        }
#if false
        byte hasExtractedMotion = reader.ReadByte();
        //Console.WriteLine("hasExtractedMotion: {0} ", hasExtractedMotion);
        if (hasExtractedMotion != (byte)0)
        {
            this.defaultMotion = new hkaDefaultMotion();
            this.defaultMotion.Read(reader);
        }
#endif
    }

    /// save anim.bin
    public void Save(string filename)
    {
        using (Stream stream = File.Create(filename))
            Save(stream);
    }

    public void Save(Stream stream)
    {
        using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Default))
        {
            string head = "hkdump File Format, Version 1.0.2.0";
            uint version = 0x01000200;
            int nskeletons = 0;
            int nanimations = 1;

            writer.WriteHeaderString(head);
            writer.Write(version);
            writer.Write(nskeletons);
            writer.Write(nanimations);

            Write(writer);
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(this.numOriginalFrames);
        writer.Write(this.duration);

        writer.Write(this.numTransforms);
        writer.Write(this.numFloats);

        for (int i = 0; i < numOriginalFrames; i++)
        {
            this.pose[i].Write(writer);
        }

        int numAnnotationTracks = this.numTransforms; // why
        writer.Write(numAnnotationTracks);
        {
            int numAnnotations = this.annotations.Length;
            writer.Write(numAnnotations);

            for (int i = 0; i < numAnnotations; i++)
            {
                this.annotations[i].Write(writer);
            }
        }
        for (int i = 1; i < numAnnotationTracks; i++)
        {
            writer.Write((int)0); // numAnnotations
        }
        if (this.hasExtractedMotion)
        {
            writer.Write((byte)1); // hasExtractedMotion
            this.defaultMotion.Write(writer);
        }
        else
        {
            writer.Write((byte)0); // hasExtractedMotion
        }
    }
}
