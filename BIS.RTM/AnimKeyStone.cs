using BIS.Core.Streams;

namespace BIS.RTM
{
    /// <summary>Specifies anim keystone type id values.</summary>
    public enum AnimKeystoneTypeID
    {
        /// <summary>Specifies the aks step sound value.</summary>
        AKSStepSound,
        /// <summary>Specifies the n anim keystone type id value.</summary>
        NAnimKeystoneTypeID,
        /// <summary>Specifies the aks uninitialized value.</summary>
        AKSUninitialized = -1
    }

    /// <summary>Specifies anim meta data id values.</summary>
    public enum AnimMetaDataID
    {
        /// <summary>Specifies the amd walk cycles value.</summary>
        AMDWalkCycles,
        /// <summary>Specifies the amd anim length value.</summary>
        AMDAnimLength,
        /// <summary>Specifies the n anim meta data id value.</summary>
        NAnimMetaDataID,
        /// <summary>Specifies the amd uninitialized value.</summary>
        AMDUninitialized = -1
    }

    /// <summary>Represents anim key stone.</summary>
    public class AnimKeyStone
    {
        /// <summary>Gets the id.</summary>
        public AnimKeystoneTypeID ID { get; private set; }
        /// <summary>Gets the string id.</summary>
        public string StringID { get; private set; }
        /// <summary>Gets the time.</summary>
        public float Time { get; private set; }
        /// <summary>Gets the value.</summary>
        public string Value { get; private set; }

        /// <summary>Initializes a new AnimKeyStone instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public AnimKeyStone(BinaryReaderEx input)
        {
            ID = (AnimKeystoneTypeID)input.ReadInt32();
            StringID = input.ReadAsciiz();
            Time = input.ReadSingle();
            Value = input.ReadAsciiz();
        }
    }
}
