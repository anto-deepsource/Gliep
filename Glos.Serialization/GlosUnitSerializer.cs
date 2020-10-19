using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
            foreach (var fun in unit.FunctionTable) {
                var funLength = sizeof(GlosUnitFunctionHeader);
                funLength += sizeof(uint) + encoding.GetByteCount(fun.Name);

                foreach (var str in fun.VariableInContext) {
                    funLength += sizeof(uint) + encoding.GetByteCount(str);
                }

                funLength += fun.Code.Length;

                functionSectionLength += funLength;
            }

            var totalLength = headerSize + sectionHeaderSize * 2 + stringSectionLength + functionSectionLength;
            var mem = Marshal.AllocHGlobal(totalLength);
            
            unchecked {
                var header = (GlosUnitHeader*) mem.ToPointer();

                header->Magic = GlosUnitHeader.MagicValue;
                header->FileVersionMinor = 0;
                header->FileVersionMajor = 1;
                header->Reserved = 0;
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
                    functionHeader->CodeLength = (uint) fun.Code.Length;
                    functionHeader->Flags = (uint) (unit.Entry == fid ?  GlosUnitFunctionFlags.Entry : GlosUnitFunctionFlags.Default);
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
    }
}
