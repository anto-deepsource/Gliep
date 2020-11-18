using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GeminiLab.Glos.CodeGenerator;

namespace GeminiLab.Glos.Serialization {
    public static class GlosUnitSerializer {
        public static unsafe byte[] Serialize(IGlosUnit unit) {
            var encoding = new UTF8Encoding(false);

            var headerSize = sizeof(GlosUnitHeader);
            var sectionHeaderSize = sizeof(GlosUnitSectionHeader);

            var stringSectionLength = 0;
            foreach (var str in unit.StringTable) {
                stringSectionLength += sizeof(uint) + encoding.GetByteCount(str);
            }

            var functionSectionLength = 0;
            var funLengths = new List<int>();
            foreach (var fun in unit.FunctionTable) {
                var funLength = sizeof(GlosUnitFunctionHeader);
                funLength += sizeof(uint) + encoding.GetByteCount(fun.Name);

                foreach (var str in fun.VariableInContext) {
                    funLength += sizeof(uint) + encoding.GetByteCount(str);
                }

                funLength += fun.Code.Length;

                funLengths.Add(funLength);
                functionSectionLength += funLength;
            }

            var totalLength = headerSize + sectionHeaderSize * 2 + stringSectionLength + functionSectionLength;
            var mem = Marshal.AllocHGlobal(totalLength);

            unchecked {
                var header = (GlosUnitHeader*) mem.ToPointer();

                header->Magic = GlosUnitHeader.MagicValue;
                header->FileVersionMinor = 0;
                header->FileVersionMajor = 1;
                header->FileLength = (uint) totalLength;
                header->FirstSectionOffset = (uint) headerSize;

                var stringSectionHeader = (GlosUnitSectionHeader*) (mem + headerSize).ToPointer();

                stringSectionHeader->SectionHeaderLength = (uint) sectionHeaderSize;
                stringSectionHeader->SectionLength = (uint) (sectionHeaderSize + stringSectionLength);
                stringSectionHeader->NextOffset = (uint) (headerSize + sectionHeaderSize + stringSectionLength);
                stringSectionHeader->SectionType = (ushort) GlosUnitSectionType.StringSection;
                stringSectionHeader->Reserved = 0;

                var ptr = (byte*) (mem + headerSize + sectionHeaderSize).ToPointer();
                foreach (var str in unit.StringTable) {
                    fixed (char* chars = str) {
                        var byteCnt = encoding.GetByteCount(str);
                        *(uint*) ptr = (uint) byteCnt;
                        encoding.GetBytes(chars, str.Length, ptr + sizeof(uint), 0x7fffffff);

                        ptr += sizeof(uint) + byteCnt;
                    }
                }

                var functionSectionHeader = (GlosUnitSectionHeader*) (mem + headerSize + sectionHeaderSize + stringSectionLength);

                functionSectionHeader->SectionHeaderLength = (uint) sectionHeaderSize;
                functionSectionHeader->SectionLength = (uint) (sectionHeaderSize + functionSectionLength);
                functionSectionHeader->NextOffset = 0;
                functionSectionHeader->SectionType = (ushort) GlosUnitSectionType.FunctionSection;
                functionSectionHeader->Reserved = 0;

                ptr = (byte*) functionSectionHeader + sectionHeaderSize;
                int fid = 0;
                foreach (var fun in unit.FunctionTable) {
                    var functionHeader = (GlosUnitFunctionHeader*) ptr;

                    functionHeader->FunctionHeaderSize = (uint) sizeof(GlosUnitFunctionHeader);
                    functionHeader->FunctionSize = (uint) funLengths[fid];
                    functionHeader->CodeLength = (uint) fun.Code.Length;
                    functionHeader->Flags = (uint) (unit.Entry == fid ? GlosUnitFunctionFlags.Entry : GlosUnitFunctionFlags.Default);
                    functionHeader->VariableInContextCount = (uint) fun.VariableInContext.Count;
                    functionHeader->LocalVariableCount = (uint) fun.LocalVariableSize;

                    ptr += sizeof(GlosUnitFunctionHeader);

                    var byteCnt = encoding.GetByteCount(fun.Name);
                    *(uint*) ptr = (uint) byteCnt;
                    fixed (char* chars = fun.Name) {
                        encoding.GetBytes(chars, fun.Name.Length, ptr + sizeof(uint), 0x7fffffff);
                        ptr += sizeof(uint) + byteCnt;
                    }

                    foreach (var vic in fun.VariableInContext) {
                        byteCnt = encoding.GetByteCount(vic);
                        *(uint*) ptr = (uint) byteCnt;
                        fixed (char* chars = vic) {
                            encoding.GetBytes(chars, vic.Length, ptr + sizeof(uint), 0x7fffffff);
                            ptr += sizeof(uint) + byteCnt;
                        }
                    }

                    fixed (byte* code = fun.Code) {
                        Buffer.MemoryCopy(code, ptr, Int64.MaxValue, fun.Code.Length);
                    }

                    ptr += fun.Code.Length;

                    ++fid;
                }
            }

            var result = new byte[totalLength];

            Marshal.Copy(mem, result, 0, totalLength);
            Marshal.FreeHGlobal(mem);

            return result;
        }

        public static void SerializeToStream(IGlosUnit unit, Stream stream) {
            stream.Write(Serialize(unit).AsSpan());
        }

        public unsafe static IGlosUnit? Deserialize(byte[] source) {
            var encoding = new UTF8Encoding(false);
            var builder = new UnitBuilder();
            IntPtr mem = IntPtr.Zero;

            try {
                var inputSize = source.Length;

                mem = Marshal.AllocHGlobal(inputSize);
                Marshal.Copy(source, 0, mem, inputSize);

                var basePtr = (byte*) mem.ToPointer();

                var header = (GlosUnitHeader*) basePtr;
                if (header->Magic != GlosUnitHeader.MagicValue
                 || header->FileVersion > 0x10000
                 || header->FileLength > inputSize) {
                    return null;
                }

                var nextSectionOffset = header->FirstSectionOffset;
                GlosUnitSectionHeader* sectionHeader;
                while (nextSectionOffset > 0 && nextSectionOffset < header->FileLength) {
                    sectionHeader = (GlosUnitSectionHeader*) (basePtr + nextSectionOffset);
                    nextSectionOffset = sectionHeader->NextOffset;

                    var sectionBodyPtr = ((byte*) sectionHeader) + sectionHeader->SectionHeaderLength;
                    var sectionBodySize = sectionHeader->SectionLength - sectionHeader->SectionHeaderLength;
                    var sectionBodyEnd = sectionBodyPtr + sectionBodySize;

                    var ptr = sectionBodyPtr;

                    switch ((GlosUnitSectionType) sectionHeader->SectionType) {
                    case GlosUnitSectionType.FunctionSection:
                        while (ptr < sectionBodyEnd) {
                            var funHeader = (GlosUnitFunctionHeader*) ptr;
                            ptr += sizeof(GlosUnitFunctionHeader);

                            var name = ReadString(ref ptr, null);
                            var vic = new List<string>();
                            for (int i = 0; i < funHeader->VariableInContextCount; ++i) {
                                vic.Add(ReadString(ref ptr, null));
                            }
                            
                            var fid = builder.AddFunctionRaw(new ReadOnlySpan<byte>(ptr, (int) funHeader->CodeLength), (int) funHeader->LocalVariableCount, vic, name);

                            if (((GlosUnitFunctionFlags)funHeader->Flags & GlosUnitFunctionFlags.Entry) == GlosUnitFunctionFlags.Entry) {
                                builder.Entry = fid;
                            }

                            ptr = (byte*) funHeader + funHeader->FunctionSize;
                        }
                        break;
                    case GlosUnitSectionType.StringSection:
                        while (ptr < sectionBodyEnd) {
                            builder.AddNewString(ReadString(ref ptr, sectionBodyEnd));
                        }
                        break;
                    default:
                        return null;
                    }
                }

                return builder.GetResult();
            } catch (Exception e) {
                return null;
            } finally {
                Marshal.FreeHGlobal(mem);
            }

            string ReadString(ref byte* ptr, byte* limit) {
                if (limit != null && ptr + sizeof(uint) > limit) {
                    throw new ArgumentOutOfRangeException(nameof(ptr));
                }

                var len = *(uint*) ptr;
                ptr += sizeof(uint);

                if (limit != null && ptr + len > limit) {
                    throw new ArgumentOutOfRangeException(nameof(ptr));
                }

                var rv = encoding.GetString(ptr, (int)len);
                ptr += len;
                return rv;
            }
        }
    }
}
