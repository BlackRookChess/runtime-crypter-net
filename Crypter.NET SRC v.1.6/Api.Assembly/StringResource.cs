using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Api.Assembly
{
    /// <summary>
    /// This structure depicts the organization of data in a file-version resource. It contains a string 
    /// that describes a specific aspect of a file, such as a file's version, its copyright notices, 
    /// or its trademarks.
    /// http://msdn.microsoft.com/en-us/library/aa909025.aspx
    /// </summary>
    public class StringResource
    {
        Kernel32.RESOURCE_HEADER _header;
        string _key;
        string _value;

        public Kernel32.RESOURCE_HEADER Header
        {
            get
            {
                return _header;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }
        }

        public string StringValue
        {
            get
            {
                if (null == _value)
                    return null;

                return _value.Trim("\0".ToCharArray());
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }       
        }

        public StringResource(string key)
        {
            _key = key;
            _header.wType = 1;
            _header.wLength = 0;
            _header.wValueLength = 0;
        }

        public StringResource(IntPtr lpRes)
        {
            Read(lpRes);
        }

        public void Read(IntPtr lpRes)
        {
            _header = (Kernel32.RESOURCE_HEADER)Marshal.PtrToStructure(
                lpRes, typeof(Kernel32.RESOURCE_HEADER));

            IntPtr pKey = new IntPtr(lpRes.ToInt32() + Marshal.SizeOf(_header));
            _key = Marshal.PtrToStringUni(pKey);

            IntPtr pValue = ResourceUtil.Align(pKey.ToInt32() + (_key.Length + 1) * 2);
            _value = _header.wValueLength > 0 ? Marshal.PtrToStringUni(pValue, _header.wValueLength) : null;
        }

        public void Write(BinaryWriter w)
        {
            // write the block info
            long headerPos = w.BaseStream.Position;
            // wLength
            w.Write((UInt16) _header.wLength);
            // wValueLength
            w.Write((UInt16) _header.wValueLength);
            // wType
            w.Write((UInt16) _header.wType);
            // szKey
            w.Write(Encoding.Unicode.GetBytes(_key));
            // null terminator
            w.Write((UInt16) 0);
            // pad fixed info
            ResourceUtil.PadToDWORD(w);
            long valuePos = w.BaseStream.Position;
            if (_value != null)
            {
                // Value
                w.Write(Encoding.Unicode.GetBytes(_value));
            }
            ResourceUtil.WriteAt(w, (w.BaseStream.Position - valuePos) / 2, headerPos + 2);
            ResourceUtil.PadToDWORD(w);
            ResourceUtil.WriteAt(w, w.BaseStream.Position - headerPos, headerPos);
        }
    }
}
