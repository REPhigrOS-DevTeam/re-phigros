using System.Runtime.InteropServices;

namespace Unimage
{
    public static unsafe class Interop
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_ANDROID
        const string LIBRARY_NAME = "libUnimage";
#elif UNITY_IOS || UNITY_WEBGL
		const string LIBRARY_NAME = "__Internal";
#endif

        [DllImport(LIBRARY_NAME)]
        public static extern void* unimage_create();

        [DllImport(LIBRARY_NAME)]
        public static extern void unimage_free(void* handle);

        [DllImport(LIBRARY_NAME)]
        public static extern void unimage_load_raw(void* handle, void* data, int width, int height, byte format);

        [DllImport(LIBRARY_NAME)]
        public static extern byte unimage_load(void* handle, void* data, uint length);

        [DllImport(LIBRARY_NAME)]
        public static extern int unimage_get_width(void* handle);

        [DllImport(LIBRARY_NAME)]
        public static extern int unimage_get_height(void* handle);

        [DllImport(LIBRARY_NAME)]
        public static extern byte unimage_get_format(void* handle);

        [DllImport(LIBRARY_NAME)]
        public static extern byte unimage_copy_to_memory(void* handle, void* buffer);

        [DllImport(LIBRARY_NAME)]
        public static extern byte unimage_resize(void* handle, int width, int height);

        [DllImport(LIBRARY_NAME)]
        public static extern byte* unimage_get_error_message(void* handle);

        [DllImport(LIBRARY_NAME)]
        public static extern void* unimage_get_buffer(void* handle);
        
        [DllImport(LIBRARY_NAME)]
        public static extern byte unimage_copy_from(void* handle, void* unimage);
        
        [DllImport(LIBRARY_NAME)]
        public static extern byte unimage_clip(void* handle, int x, int y, int width, int height);
    }
}

