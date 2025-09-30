using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            panel1.MouseMove += Panel1_MouseMove;
            panel1.MouseUp += Panel1_MouseUp;
            panel1.MouseDown += Panel1_MouseDown;

            panel1.MouseWheel += Panel1_MouseWheel;
            panel1.Resize += Panel1_Resize;
            Shown += Form1_Shown;
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            init();
        }

        private void Panel1_Resize(object? sender, EventArgs e)
        {
            d.Resize(panel1.Width, panel1.Height);
        }

        private void Panel1_MouseWheel(object? sender, MouseEventArgs e)
        {
            Point cursorPosition = System.Windows.Forms.Cursor.Position;

            var pos = panel1.PointToClient(cursorPosition);

            d?.MouseScroll(pos.X, pos.Y, e.Delta);
        }

        private void Panel1_MouseDown(object? sender, MouseEventArgs e)
        {
            Point cursorPosition = System.Windows.Forms.Cursor.Position;

            var pos = panel1.PointToClient(cursorPosition);
            int btn = 1;
            if (e.Button == MouseButtons.Right)
                btn = 3;
            if (e.Button == MouseButtons.Middle)
                btn = 2;

            d?.MouseDown(btn, pos.X, pos.Y);
        }

        private void Panel1_MouseUp(object? sender, MouseEventArgs e)
        {
            Point cursorPosition = System.Windows.Forms.Cursor.Position;

            var pos = panel1.PointToClient(cursorPosition);
            int btn = 1;
            if (e.Button == MouseButtons.Right)
                btn = 3;
            if (e.Button == MouseButtons.Middle)
                btn = 2;
            d?.MouseUp(btn, pos.X, pos.Y);
        }

        private void Panel1_MouseMove(object? sender, MouseEventArgs e)
        {

            Point cursorPosition = System.Windows.Forms.Cursor.Position;

            var pos = panel1.PointToClient(cursorPosition);

            d?.MouseMove(pos.X, pos.Y);
        }

        private const string TITLE = "Simple Window";
        private const int WIDTH = 1024;
        private const int HEIGHT = 800;

        private const int GL_COLOR_BUFFER_BIT = 0x00004000;

        private delegate void glClearColorHandler(float r, float g, float b, float a);
        private delegate void glClearHandler(int mask);

        private static glClearColorHandler glClearColor;
        private static glClearHandler glClear;
        private static void ChangeRandomColor()
        {
            var r = (float)rand.NextDouble();
            var g = (float)rand.NextDouble();
            var b = (float)rand.NextDouble();
            glClearColor(r, g, b, 1.0f);
        }
        
        [DllImport("opengl32.dll", SetLastError = true)]
        public static extern int wglMakeCurrent(IntPtr hdc, IntPtr hglrc);
        [DllImport("opengl32.dll", SetLastError = true)]
        public static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        private static Random rand;
        GlfwOcctViewManaged d;


        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            public void Init()
            {
                nSize = (ushort)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR));
                nVersion = 1;
                dwFlags = PFD_FLAGS.PFD_DRAW_TO_WINDOW | PFD_FLAGS.PFD_SUPPORT_OPENGL | PFD_FLAGS.PFD_DOUBLEBUFFER | PFD_FLAGS.PFD_SUPPORT_COMPOSITION;
                iPixelType = PFD_PIXEL_TYPE.PFD_TYPE_RGBA;
                cColorBits = 24;
                cRedBits = cRedShift = cGreenBits = cGreenShift = cBlueBits = cBlueShift = 0;
                cAlphaBits = cAlphaShift = 0;
                cAccumBits = cAccumRedBits = cAccumGreenBits = cAccumBlueBits = cAccumAlphaBits = 0;
                cDepthBits = 32;
                cStencilBits = cAuxBuffers = 0;
                iLayerType = PFD_LAYER_TYPES.PFD_MAIN_PLANE;
                bReserved = 0;
                dwLayerMask = dwVisibleMask = dwDamageMask = 0;
            }
            ushort nSize;
            ushort nVersion;
            PFD_FLAGS dwFlags;
            PFD_PIXEL_TYPE iPixelType;
            byte cColorBits;
            byte cRedBits;
            byte cRedShift;
            byte cGreenBits;
            byte cGreenShift;
            byte cBlueBits;
            byte cBlueShift;
            byte cAlphaBits;
            byte cAlphaShift;
            byte cAccumBits;
            byte cAccumRedBits;
            byte cAccumGreenBits;
            byte cAccumBlueBits;
            byte cAccumAlphaBits;
            byte cDepthBits;
            byte cStencilBits;
            byte cAuxBuffers;
            PFD_LAYER_TYPES iLayerType;
            byte bReserved;
            uint dwLayerMask;
            uint dwVisibleMask;
            uint dwDamageMask;
        }

        [Flags]
        public enum PFD_FLAGS : uint
        {
            PFD_DOUBLEBUFFER = 0x00000001,
            PFD_STEREO = 0x00000002,
            PFD_DRAW_TO_WINDOW = 0x00000004,
            PFD_DRAW_TO_BITMAP = 0x00000008,
            PFD_SUPPORT_GDI = 0x00000010,
            PFD_SUPPORT_OPENGL = 0x00000020,
            PFD_GENERIC_FORMAT = 0x00000040,
            PFD_NEED_PALETTE = 0x00000080,
            PFD_NEED_SYSTEM_PALETTE = 0x00000100,
            PFD_SWAP_EXCHANGE = 0x00000200,
            PFD_SWAP_COPY = 0x00000400,
            PFD_SWAP_LAYER_BUFFERS = 0x00000800,
            PFD_GENERIC_ACCELERATED = 0x00001000,
            PFD_SUPPORT_DIRECTDRAW = 0x00002000,
            PFD_DIRECT3D_ACCELERATED = 0x00004000,
            PFD_SUPPORT_COMPOSITION = 0x00008000,
            PFD_DEPTH_DONTCARE = 0x20000000,
            PFD_DOUBLEBUFFER_DONTCARE = 0x40000000,
            PFD_STEREO_DONTCARE = 0x80000000
        }

        public enum PFD_LAYER_TYPES : byte
        {
            PFD_MAIN_PLANE = 0,
            PFD_OVERLAY_PLANE = 1,
            PFD_UNDERLAY_PLANE = 255
        }

        public enum PFD_PIXEL_TYPE : byte
        {
            PFD_TYPE_RGBA = 0,
            PFD_TYPE_COLORINDEX = 1
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int ChoosePixelFormat(IntPtr hdc, [In] ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);

        private void button1_Click(object sender, EventArgs e)
        {

        }
        [DllImport("Gdi32.dll", SetLastError = true)]
        public static extern bool SwapBuffers(IntPtr hdc);
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (d != null)
            {
                d.iterate();
                SwapBuffers(GetDC(panel1.Handle));

            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
        }

        void init() { 
            // Set some common hints for the OpenGL profile creation
            
            // Create window
            //var window = Glfw.CreateWindow(WIDTH, HEIGHT, TITLE, GLFW.Monitor.None, GLFW.Window.None);
            //Glfw.MakeContextCurrent(window);
            IntPtr dc = GetDC(panel1.Handle); // myControl is your Form or control
                                              // ... set pixel format ...
            IntPtr hglrc = wglCreateContext(dc);
            hglrc = wglCreateContext(dc);
            if (hglrc == IntPtr.Zero)
            {
                // Handle error, e.g., throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var pixelformatdescriptor = new PIXELFORMATDESCRIPTOR();
            pixelformatdescriptor.Init();

            var pixelFormat = ChoosePixelFormat(dc, ref pixelformatdescriptor);
            if (!SetPixelFormat(dc, pixelFormat, ref pixelformatdescriptor))
                throw new Win32Exception(Marshal.GetLastWin32Error());


            if ((hglrc = wglCreateContext(dc)) == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // ...
            wglMakeCurrent(dc, hglrc);


            d = new GlfwOcctViewManaged();
            d.runWnt(panel1.Handle, hglrc);
            rand = new Random();

            /*
                         // The object oriented approach
                         using (var window = new NativeWindow(WIDTH, HEIGHT, "hh"))
                         {
                             window.CenterOnScreen();
                             //window.KeyPress += WindowOnKeyPress;
                             while (!window.IsClosing)
                             {
                                 Glfw.PollEvents();
                                 window.SwapBuffers();
                             }
                         }
            */



            return;
            //// Effectively enables VSYNC by setting to 1.
            //Glfw.SwapInterval(1);

            //// Find center position based on window and monitor sizes
            //var screenSize = Glfw.PrimaryMonitor.WorkArea;
            //var x = (screenSize.Width - WIDTH) / 2;
            //var y = (screenSize.Height - HEIGHT) / 2;
            //Glfw.SetWindowPosition(window, x, y);

            //// Set a key callback
            //Glfw.SetKeyCallback(window, KeyCallback);


            //glClearColor = Marshal.GetDelegateForFunctionPointer<glClearColorHandler>(Glfw.GetProcAddress("glClearColor"));
            //glClear = Marshal.GetDelegateForFunctionPointer<glClearHandler>(Glfw.GetProcAddress("glClear"));


            //var tick = 0L;
            //ChangeRandomColor();

            //while (!Glfw.WindowShouldClose(window))
            //{
            //    // Poll for OS events and swap front/back buffers
            //    Glfw.PollEvents();
            //    Glfw.SwapBuffers(window);

            //    // Change background color to something random every 60 draws
            //    if (tick++ % 60 == 0)
            //        ChangeRandomColor();

            //    // Clear the buffer to the set color
            //    glClear(GL_COLOR_BUFFER_BIT);
            //}
        }
    }
}