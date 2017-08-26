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

    public Transform()
    {
        this.translation = Vector3.Zero;
        this.rotation = Quaternion.Identity;
        this.scale = 1.0f;
    }

    public Transform(Vector3 translation, Quaternion rotation, float scale)
    {
        this.translation = translation;
        this.rotation = rotation;
        this.scale = scale;
    }

    public static Transform operator *(Transform t1, Transform t2)
    {
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
    internal short idx;
    internal bool hide = false;
    internal hkaBone parent = null;
    internal List<hkaBone> children = new List<hkaBone>();
    internal Transform local;

    public void Read(BinaryReader reader)
    {
        this.name = reader.ReadCString();
    }

    internal Transform GetWorldCoordinate()
    {
        Transform t = new Transform();
        hkaBone bone = this;
        //int i = 0;
        while (bone != null)
        {
            //Console.WriteLine(" local loop idx {0} Ref {1}", i, node.self_ref);
            t = bone.local * t;
            bone = bone.parent;
            //i++;
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
            // should be 0x01000000
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
}

public class hkaAnimation
{
    public int numOriginalFrames;
    public float duration;
    //public int numTransforms;
    //public int numFloats;
    public float time;
    public Transform[] transforms;
    public float[] floats;

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
            if (version != 0x01000000)
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

        this.time = reader.ReadSingle();

        this.transforms = new Transform[numTransforms];
        for (int i=0; i<numTransforms; i++)
        {
            Transform t = new Transform();
            t.Read(reader);
            this.transforms[i] = t;
        }

        this.floats = new float[numFloats];
        for (int i=0; i<numFloats; i++)
        {
            this.floats[i] = reader.ReadSingle();
        }
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
            string head = "hkdump File Format, Version 1.0.0.0";
            uint version = 0x01000000;
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
        writer.Write(this.transforms.Length);
        writer.Write(this.floats.Length);
        writer.Write(this.time);
        for (int i=0, len=this.transforms.Length; i<len; i++)
        {
            this.transforms[i].Write(writer);
        }
        for (int i=0, len=this.floats.Length; i<len; i++)
        {
            writer.Write(this.floats[i]);
        }
    }
}
