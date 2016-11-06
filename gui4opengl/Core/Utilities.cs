using System;
using System.Runtime.InteropServices;

using OpenGL;

namespace OpenGL.UI
{
    public static class Utilities
    {
        public static void BufferSubData(uint vboID, BufferTarget target, Vector3[] data, int length)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Gl.BindBuffer(target, vboID);
                Gl.BufferSubData(target, IntPtr.Zero, (IntPtr)(12 * length), handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public static void BufferSubData<T>(uint vboID, BufferTarget target, T[] data, int length)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Gl.BindBuffer(target, vboID);
                Gl.BufferSubData(target, IntPtr.Zero, (IntPtr)(Marshal.SizeOf(data[0]) * length), handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
