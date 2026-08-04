using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using BIS.Core.Streams;

namespace BIS.Core.Config
{
    #region Enums
    /// <summary>Specifies entry type values.</summary>
    public enum EntryType : byte
    {
        /// <summary>Represents class.</summary>
        Class,
        /// <summary>Specifies the value value.</summary>
        Value,
        /// <summary>Specifies the array value.</summary>
        Array,
        /// <summary>Specifies the class decl value.</summary>
        ClassDecl,
        /// <summary>Specifies the class delete value.</summary>
        ClassDelete,
        /// <summary>Specifies the array spec value.</summary>
        ArraySpec
    }

    /// <summary>Specifies value type values.</summary>
    public enum ValueType : byte
    {
        /// <summary>Specifies the generic value.</summary>
        Generic, // generic = string
        /// <summary>Specifies the float value.</summary>
        Float,
        /// <summary>Specifies the int value.</summary>
        Int,
        /// <summary>Specifies the array value.</summary>
        Array, //not used?
        /// <summary>Specifies the expression value.</summary>
        Expression,
        /// <summary>Specifies the n spec value type value.</summary>
        NSpecValueType,
        /// <summary>Specifies the int64 value.</summary>
        Int64,
    }
    #endregion

    #region ParamEntries
    /// <summary>Represents param entry.</summary>
    public abstract class ParamEntry
    {
        /// <summary>Gets the name.</summary>
        public string Name { get; protected set; }

        /// <summary>Reads param entry from the underlying data.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <returns>The resulting value.</returns>
        public static ParamEntry ReadParamEntry(BinaryReaderEx input)
        {
            var entryType = (EntryType)input.ReadByte();

            switch(entryType)
            {
                case EntryType.Class:
                    return new ParamClass(input);

                case EntryType.Array:
                    return new ParamArray(input);

                case EntryType.Value:
                    return new ParamValue(input);

                case EntryType.ClassDecl:
                    return new ParamExternClass(input);

                case EntryType.ClassDelete:
                    return new ParamDeleteClass(input);

                case EntryType.ArraySpec:
                    return new ParamArraySpec(input);

                default: throw new ArgumentException("Unknown ParamEntry Type", nameof(entryType));
            }
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <returns>The resulting value.</returns>
        public virtual string ToString(int indentionLevel = 0) => base.ToString();
        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString() => ToString(0);
    }

    /// <summary>Represents param class.</summary>
    public class ParamClass : ParamEntry
    {
        /// <summary>Gets the base class name.</summary>
        public string BaseClassName { get; private set; }
        /// <summary>Gets the entries.</summary>
        public List<ParamEntry> Entries { get; private set; }

        /// <summary>Initializes a new ParamClass instance.</summary>
        public ParamClass()
        {
            BaseClassName = "";
            Name = "";
            Entries = new List<ParamEntry>(20);
        }

        /// <summary>Initializes a new ParamClass instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="baseclass">The baseclass value.</param>
        /// <param name="entries">The entries value.</param>
        public ParamClass(string name, string baseclass, IEnumerable<ParamEntry> entries)
        {
            BaseClassName = baseclass;
            Name = name;
            Entries = entries.ToList();
        }

        /// <summary>Initializes a new ParamClass instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="entries">The entries value.</param>
        public ParamClass(string name, IEnumerable<ParamEntry> entries): this(name, "", entries) { }

        /// <summary>Initializes a new ParamClass instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="entries">The entries value.</param>
        public ParamClass(string name, params ParamEntry[] entries) : this(name, (IEnumerable<ParamEntry>)entries) { }

        /// <summary>Initializes a new ParamClass instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ParamClass(BinaryReaderEx input)
        {
            Name = input.ReadAsciiz();
            var offset = input.ReadUInt32();
            var oldPos = input.Position;
            input.Position = offset;
            ReadCore(input);
            input.Position = oldPos;
        }

        /// <summary>Initializes a new ParamClass instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="fileName">The file path.</param>
        public ParamClass(BinaryReaderEx input, string fileName)
        {
            Name = fileName;
            ReadCore(input);
        }

        /// <summary>Gets class.</summary>
        /// <param name="name">The name value.</param>
        /// <returns>The resulting value.</returns>
        public ParamClass GetClass(string name)
        {
            return Entries.OfType<ParamClass>().FirstOrDefault(c => c.Name == name);
        }
        /// <summary>Gets a named parameter array converted to the requested element type.</summary>
        /// <typeparam name="T">The t type.</typeparam>
        /// <param name="name">The name value.</param>
        /// <returns>The resulting values.</returns>
        public T[] GetArray<T>(string name)
        {
            return Entries.OfType<ParamArray>().FirstOrDefault(c => c.Name == name)?.ToArray<T>();
        }

        private void ReadCore(BinaryReaderEx input)
        {
            BaseClassName = input.ReadAsciiz();

            var nEntries = input.ReadCompactInteger();
            Entries = Enumerable.Range(0, nEntries).Select(_ => ReadParamEntry(input)).ToList();
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <param name="onlyClassBody">The only class body value.</param>
        /// <returns>The resulting value.</returns>
        public string ToString(int indentionLevel, bool onlyClassBody)
        {
            var ind = new string(' ', indentionLevel * 4);
            var classBody = new StringBuilder();

            var indLvl = (onlyClassBody) ? indentionLevel : indentionLevel + 1;
            foreach (var entry in Entries)
                classBody.AppendLine(entry.ToString(indLvl));

            var classHead = (string.IsNullOrEmpty(BaseClassName)) ?
                $"{ind}class {Name}" :
                $"{ind}class {Name} : {BaseClassName}";

            if (onlyClassBody)
                return classBody.ToString();

            return
$@"{classHead}
{ind}{{
{classBody.ToString()}{ind}}};";
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <returns>The resulting value.</returns>
        public override string ToString(int indentionLevel = 0) => ToString(indentionLevel, false);
    }

    /// <summary>Represents param array.</summary>
    public class ParamArray : ParamEntry
    {
        /// <summary>Gets the array.</summary>
        public RawArray Array { get; private set; }

        /// <summary>Initializes a new ParamArray instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ParamArray(BinaryReaderEx input)
        {
            Name = input.ReadAsciiz();
            Array = new RawArray(input);
        }

        /// <summary>Initializes a new ParamArray instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="values">The values value.</param>
        public ParamArray(string name, IEnumerable<RawValue> values)
        {
            Name = name;
            Array = new RawArray(values);
        }

        /// <summary>Initializes a new ParamArray instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="values">The values value.</param>
        public ParamArray(string name, params RawValue[] values): this(name, (IEnumerable < RawValue >)values) { }

        /// <summary>Converts the raw array entries to the requested element type.</summary>
        /// <typeparam name="T">The t type.</typeparam>
        /// <returns>The resulting values.</returns>
        public T[] ToArray<T>()
        {
            return Array.Entries.Select(e => e.Get<T>()).ToArray();
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <returns>The resulting value.</returns>
        public override string ToString(int indentionLevel = 0)
        {
            return $"{new string(' ', indentionLevel * 4)}{Name}[]={Array.ToString()};";
        }
    }

    /// <summary>Represents param array spec.</summary>
    public class ParamArraySpec : ParamEntry
    {
        /// <summary>Gets or sets the flag.</summary>
        public int Flag { get; }

        /// <summary>Gets the array.</summary>
        public RawArray Array { get; private set; }

        /// <summary>Initializes a new ParamArraySpec instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ParamArraySpec(BinaryReaderEx input)
        {
            Flag = input.ReadInt32();
            Name = input.ReadAsciiz();
            Array = new RawArray(input);
        }

        /// <summary>Initializes a new ParamArraySpec instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="flag">The flag value.</param>
        /// <param name="values">The values value.</param>
        public ParamArraySpec(string name, int flag, IEnumerable<RawValue> values)
        {
            Name = name;
            Flag = flag;
            Array = new RawArray(values);
        }

        /// <summary>Initializes a new ParamArraySpec instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="flag">The flag value.</param>
        /// <param name="values">The values value.</param>
        public ParamArraySpec(string name, int flag, params RawValue[] values) : this(name, flag, (IEnumerable<RawValue>)values) { }

        /// <summary>Converts the raw array entries to the requested element type.</summary>
        /// <typeparam name="T">The t type.</typeparam>
        /// <returns>The resulting values.</returns>
        public T[] ToArray<T>()
        {
            return Array.Entries.Select(e => e.Get<T>()).ToArray();
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <returns>The resulting value.</returns>
        public override string ToString(int indentionLevel = 0)
        {
            if (Flag == 1)
            {
                return $"{new string(' ', indentionLevel * 4)}{Name}[]+={Array.ToString()};";
            }
            return $"{new string(' ', indentionLevel * 4)}{Name}[]={Array.ToString()}; // Unknown flag {Flag}";
        }
    }

    /// <summary>Represents param value.</summary>
    public class ParamValue : ParamEntry
    {
        /// <summary>Gets the value.</summary>
        public RawValue Value { get; private set; }

        /// <summary>Initializes a new ParamValue instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="value">The value to process.</param>
        public ParamValue(string name, bool value)
        {
            Name = name;
            Value = new RawValue(value ? 1 : 0);
        }
        /// <summary>Initializes a new ParamValue instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="value">The value to process.</param>
        public ParamValue(string name, int value)
        {
            Name = name;
            Value = new RawValue(value);
        }
        /// <summary>Initializes a new ParamValue instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="value">The value to process.</param>
        public ParamValue(string name, float value)
        {
            Name = name;
            Value = new RawValue(value);
        }
        /// <summary>Initializes a new ParamValue instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="value">The value to process.</param>
        public ParamValue(string name, string value)
        {
            Name = name;
            Value = new RawValue(value);
        }

        /// <summary>Initializes a new ParamValue instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ParamValue(BinaryReaderEx input)
        {
            var subtype = (ValueType)input.ReadByte();
            Name = input.ReadAsciiz();
            Value = new RawValue(input, subtype);
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <returns>The resulting value.</returns>
        public override string ToString(int indentionLevel=0)
        {
            return $"{new string(' ', indentionLevel * 4)}{Name}={Value.ToString()};";
        }
    }

    /// <summary>Represents param extern class.</summary>
    public class ParamExternClass : ParamEntry
    {
        /// <summary>Initializes a new ParamExternClass instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ParamExternClass(BinaryReaderEx input) : this(input.ReadAsciiz()) { }

        /// <summary>Initializes a new ParamExternClass instance.</summary>
        /// <param name="name">The name value.</param>
        public ParamExternClass(string name)
        {
            Name = name;
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <returns>The resulting value.</returns>
        public override string ToString(int indentionLevel = 0)
        {
            return $"{new string(' ', indentionLevel * 4)}class {Name};";
        }
    }
    /// <summary>Represents param delete class.</summary>
    public class ParamDeleteClass : ParamEntry
    {
        /// <summary>Initializes a new ParamDeleteClass instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ParamDeleteClass(BinaryReaderEx input) : this(input.ReadAsciiz()) { }

        /// <summary>Initializes a new ParamDeleteClass instance.</summary>
        /// <param name="name">The name value.</param>
        public ParamDeleteClass(string name)
        {
            Name = name;
        }

        /// <summary>Converts this value to string.</summary>
        /// <param name="indentionLevel">The indention level value.</param>
        /// <returns>The resulting value.</returns>
        public override string ToString(int indentionLevel = 0)
        {
            return $"{new string(' ', indentionLevel * 4)}delete {Name};";
        }
    }
    #endregion

    #region ParamValues
    /// <summary>Represents raw array.</summary>
    public class RawArray
    {
        /// <summary>Gets the entries.</summary>
        public List<RawValue> Entries { get; private set; }

        /// <summary>Initializes a new RawArray instance.</summary>
        /// <param name="values">The values value.</param>
        public RawArray(IEnumerable<RawValue> values)
        {
            Entries = values.ToList();
        }

        /// <summary>Initializes a new RawArray instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public RawArray(BinaryReaderEx input)
        {
            var nEntries = input.ReadCompactInteger();
            Entries = Enumerable.Range(0, nEntries).Select(_ => new RawValue(input)).ToList();
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            var valStr = string.Join(", ", Entries.Select(x => x.ToString()));
            return $"{{{valStr}}}";
        }
    }

    /// <summary>Represents raw value.</summary>
    public class RawValue
    {
        /// <summary>Gets the type.</summary>
        public ValueType Type { get; protected set; }
        /// <summary>Gets the value.</summary>
        public object Value { get; private set; }

        /// <summary>Initializes a new RawValue instance.</summary>
        /// <param name="v">The v value.</param>
        public RawValue(string v)
        {
            Type = ValueType.Generic;
            Value = v;
        }

        /// <summary>Initializes a new RawValue instance.</summary>
        /// <param name="v">The v value.</param>
        public RawValue(int v)
        {
            Type = ValueType.Int;
            Value = v;
        }

        /// <summary>Initializes a new RawValue instance.</summary>
        /// <param name="v">The v value.</param>
        public RawValue(long v)
        {
            Type = ValueType.Int64;
            Value = v;
        }

        /// <summary>Initializes a new RawValue instance.</summary>
        /// <param name="v">The v value.</param>
        public RawValue(float v)
        {
            Type = ValueType.Float;
            Value = v;
        }

        /// <summary>Initializes a new RawValue instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public RawValue(BinaryReaderEx input) : this(input, (ValueType)input.ReadByte()) { }

        /// <summary>Initializes a new RawValue instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="type">The type value.</param>
        public RawValue(BinaryReaderEx input, ValueType type)
        {
            Type = type;
            switch (Type)
            {
                case ValueType.Expression: goto case ValueType.Generic;
                case ValueType.Generic:
                    Value = input.ReadAsciiz();
                    break;
                case ValueType.Float:
                    Value = input.ReadSingle();
                    break;
                case ValueType.Int:
                    Value = input.ReadInt32();
                    break;
                case ValueType.Int64:
                    Value = input.ReadInt64();
                    break;
                case ValueType.Array:
                    Value = new RawArray(input);
                    break;

                default: throw new ArgumentException();
            }
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            if (Type == ValueType.Expression || Type == ValueType.Generic)
                return $"\"{Value}\"";

            if (Type == ValueType.Float)
                return ((float)Value).ToString(CultureInfo.InvariantCulture);

            return Value.ToString();
        }

        internal T Get<T>()
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
    }
    #endregion
}
