using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.CodeGenerator {
    internal static class ListExtensions {
        internal static void AddOp(this IList<byte> list, GlosOp op) => list.Add((byte) op);

        internal static void AddInteger32(this IList<byte> list, int item) {
            int i = item;
            unsafe {
                byte* b = (byte*) &i;
                list.Add(b[0]);
                list.Add(b[1]);
                list.Add(b[2]);
                list.Add(b[3]);
            }
        }

        internal static void AddInteger64(this IList<byte> list, long item) {
            long l = item;
            unsafe {
                byte* b = (byte*) &l;
                list.Add(b[0]);
                list.Add(b[1]);
                list.Add(b[2]);
                list.Add(b[3]);
                list.Add(b[4]);
                list.Add(b[5]);
                list.Add(b[6]);
                list.Add(b[7]);
            }
        }

        internal static void SetInteger32(this IList<byte> list, int offset, int item) {
            if (offset + 4 > list.Count) throw new ArgumentOutOfRangeException(nameof(offset));

            int i = item;
            unsafe {
                byte* b = (byte*) &i;
                list[offset] = b[0];
                list[offset + 1] = b[1];
                list[offset + 2] = b[2];
                list[offset + 3] = b[3];
            }
        }

        /*
        internal static void SetInteger64(this IList<byte> list, int offset, long item) {
            if (offset + 8 >= list.Count) throw new ArgumentOutOfRangeException(nameof(offset));

            long l = item;
            unsafe {
                byte* b = (byte*)&l;
                list[offset] = b[0];
                list[offset + 1] = b[1];
                list[offset + 2] = b[2];
                list[offset + 3] = b[3];
                list[offset + 4] = b[4];
                list[offset + 5] = b[5];
                list[offset + 6] = b[6];
                list[offset + 7] = b[7];
            }
        }
        */
    }
}
