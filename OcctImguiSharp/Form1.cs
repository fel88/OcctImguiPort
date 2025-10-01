using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Wgl;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            glcontrol = new GLControl(new GLControlSettings()
            {
                NumberOfSamples = 4,
                StencilBits = 8,

            });
            glcontrol.MouseMove += Panel1_MouseMove;
            glcontrol.MouseUp += Panel1_MouseUp;
            glcontrol.MouseDown += Panel1_MouseDown;

            glcontrol.MouseWheel += Panel1_MouseWheel;
            Shown += Form1_Shown;

            panel1.Controls.Add(glcontrol);
            glcontrol.Dock = DockStyle.Fill;
            glcontrol.Paint += Glcontrol_Paint;
            glcontrol.Load += Glcontrol_Load;
            glcontrol.Resize += Panel1_Resize;
            FormClosing += Form1_FormClosing;

        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {            
            d.cleanup();
        }
        private int CompileProgram(string vertexShader, string fragmentShader)
        {
            int program = GL.CreateProgram();

            int vert = CompileShader(ShaderType.VertexShader, vertexShader);
            int frag = CompileShader(ShaderType.FragmentShader, fragmentShader);

            GL.AttachShader(program, vert);
            GL.AttachShader(program, frag);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetProgramInfoLog(program);
                throw new Exception($"Could not link program: {log}");
            }

            GL.DetachShader(program, vert);
            GL.DetachShader(program, frag);

            GL.DeleteShader(vert);
            GL.DeleteShader(frag);

            return program;

            static int CompileShader(ShaderType type, string source)
            {
                int shader = GL.CreateShader(type);

                GL.ShaderSource(shader, source);
                GL.CompileShader(shader);

                GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
                if (status == 0)
                {
                    string log = GL.GetShaderInfoLog(shader);
                    throw new Exception($"Failed to compile {type}: {log}");
                }

                return shader;
            }
        }


        private const string vertexShaderStr = @"  // Vertex Shader
    #version 330 core
layout (location = 0) in vec3 aPos;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

    //#layout (location = 0) in vec3 aPos;
    void main()
    {
  gl_Position = projection * view * model * vec4(aPos, 1.0);
      //gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
    }

";

        private const string fragmentShaderStr = @"  // Fragment  Shader
    #version 330 core
    out vec4 FragColor;
    void main()
    {
        FragColor = vec4(1.0f, 0.5f, 0.5f, 1.0f); // Orange color
    }
";
        private const string VertexShaderSource = @"#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec4 aColor;

out vec4 fColor;

uniform mat4 MVP;

void main()
{
    gl_Position = vec4(aPos, 1) * MVP;
    fColor = aColor;
}
";
        float[] vertices = {
        -0.5f, -0.5f, 0.0f, // Bottom-left vertex
         0.5f, -0.5f, 0.0f, // Bottom-right vertex
         0.0f,  0.5f, 0.0f  // Top vertex
    };
        private const string FragmentShaderSource = @"#version 330 core

in vec4 fColor;

out vec4 oColor;

void main()
{
    oColor = fColor;
}
"; int vao;
        bool inited = false;
        int shaderProgram;
        private void Glcontrol_Load(object? sender, EventArgs e)
        {
            hglrc = wglGetCurrentContext();

            initWithOpentk(hglrc.Value);
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            shaderProgram = CompileProgram(vertexShaderStr, fragmentShaderStr);

            // Error checking for linking
            CubeShader = CompileProgram(VertexShaderSource, FragmentShaderSource);
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, IndexData.Length * sizeof(int), IndexData, BufferUsageHint.StaticDraw);

            PositionBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexData.Length * sizeof(float) * 3, VertexData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            ColorBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, ColorData.Length * sizeof(float) * 4, ColorData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            inited = true;
        }
        private static readonly Color4[] ColorData = new Color4[]
        {
            Color4.Silver, Color4.Silver, Color4.Silver, Color4.Silver,
            Color4.Honeydew, Color4.Honeydew, Color4.Honeydew, Color4.Honeydew,
            Color4.Moccasin, Color4.Moccasin, Color4.Moccasin, Color4.Moccasin,
            Color4.IndianRed, Color4.IndianRed, Color4.IndianRed, Color4.IndianRed,
            Color4.PaleVioletRed, Color4.PaleVioletRed, Color4.PaleVioletRed, Color4.PaleVioletRed,
            Color4.ForestGreen, Color4.ForestGreen, Color4.ForestGreen, Color4.ForestGreen,
        };
        private static readonly Vector3[] VertexData = new Vector3[]
       {
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),

            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),

            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),

            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),

            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),

            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
       };
        nint? hglrc;

        private void Glcontrol_Paint(object? sender, PaintEventArgs e)
        {
            //glcontrol.MakeCurrent();
            // hglrc = wglGetCurrentContext();
            //  init();
            /*
            GL.ClearColor(Color4.MidnightBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

          
            glcontrol.SwapBuffers();*/
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            glcontrol.Focus();

            //init();
        }

        private void Panel1_Resize(object? sender, EventArgs e)
        {
            d?.Resize(panel1.Width, panel1.Height);
        }

        private void Panel1_MouseWheel(object? sender, MouseEventArgs e)
        {
            Point cursorPosition = System.Windows.Forms.Cursor.Position;

            var pos = panel1.PointToClient(cursorPosition);

            d?.MouseScroll(pos.X, pos.Y, e.Delta);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //d?.Key(btn);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Panel1_MouseDown(object? sender, MouseEventArgs e)
        {
            Point cursorPosition = System.Windows.Forms.Cursor.Position;

            var pos = glcontrol.PointToClient(cursorPosition);
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

            var pos = glcontrol.PointToClient(cursorPosition);
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

            var pos = glcontrol.PointToClient(cursorPosition);

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

        // Example P/Invoke signature for wglChoosePixelFormatARB
        [DllImport("opengl32.dll", EntryPoint = "wglChoosePixelFormatARB")]
        public static extern bool wglChoosePixelFormatARB(IntPtr hdc, int[] piAttribIList, float[] pfAttribFList, uint nMaxFormats, [Out] int[] piFormats, out uint nNumFormats);

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

        void drawAxes()
        {
            GL.Begin(PrimitiveType.Lines);

            // X-axis (Red)
            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f);

            // Y-axis (Green)
            GL.Color3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);

            // Z-axis (Blue)
            GL.Color3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);

            GL.End();
        }

        private int CubeShader;


        private int VAO;
        private int EBO;
        private int PositionBuffer;
        private int ColorBuffer;
        Matrix4 projection;
        private static readonly int[] IndexData = new int[]
     {
             0,  1,  2,  2,  3,  0,
             4,  5,  6,  6,  7,  4,
             8,  9, 10, 10, 11,  8,
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
            20, 21, 22, 22, 23, 20,
     };

        private float _angle = 0.0f;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (d != null && inited)
            {
                d.iterate();

                if (d.showTrinagle)
                {
                    glcontrol.MakeCurrent();
                    var r = GL.GetBoolean(GetPName.DepthTest);
                    GL.Disable(EnableCap.DepthTest);

                    Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
                    Vector3 cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
                    Vector3 cameraUpVector = new Vector3(0.0f, 1.0f, 0.0f);

                    Matrix4 view = Matrix4.LookAt(cameraPosition, cameraTarget, cameraUpVector);
                    //Matrix4 model = Matrix4.Identity;
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), (float)Width / Height, 0.1f, 100f);
                    projection = Matrix4.CreateOrthographic(Width, Height, -100.1f, 100f);
                    Matrix4 model = Matrix4.Identity;
                    model = Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(_angle));
                    model *= Matrix4.CreateScale(100, 100, 100);
                    model *= Matrix4.CreateTranslation(-Width / 2 + 50, 0, 0);
                    _angle += 2.51f;

                    GL.UseProgram(shaderProgram);

                    int modelLoc = GL.GetUniformLocation(shaderProgram, "model");
                    int viewLoc = GL.GetUniformLocation(shaderProgram, "view");
                    int projLoc = GL.GetUniformLocation(shaderProgram, "projection");

                    GL.UniformMatrix4(modelLoc, false, ref model); // Model matrix (identity for a static object)
                    GL.UniformMatrix4(viewLoc, false, ref view);
                    GL.UniformMatrix4(projLoc, false, ref projection);
                    GL.BindVertexArray(vao);
                    //GL.Color3(Color.Green);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // Draw 3 vertices a

                }
                glcontrol.SwapBuffers();
                //SwapBuffers(GetDC(panel1.Handle));

            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
        }
        GLControl glcontrol;

        void initWithOpentk(nint hglrc)
        {


            d = new GlfwOcctViewManaged();
            //d.runWnt(panel1.Handle, hglrc);

            d.runOpenTk(glcontrol.Context.WindowPtr, hglrc);

        }
        void init()
        {

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
            // ...
            wglMakeCurrent(dc, hglrc);
            var pixelformatdescriptor = new PIXELFORMATDESCRIPTOR();
            pixelformatdescriptor.Init();


            var WGL_NUMBER_PIXEL_FORMATS_ARB = 0x2000;
            var WGL_DRAW_TO_WINDOW_ARB = 0x2001;
            var WGL_DRAW_TO_BITMAP_ARB = 0x2002;
            var WGL_ACCELERATION_ARB = 0x2003;
            var WGL_NEED_PALETTE_ARB = 0x2004;
            var WGL_NEED_SYSTEM_PALETTE_ARB = 0x2005;
            var WGL_SWAP_LAYER_BUFFERS_ARB = 0x2006;
            var WGL_SWAP_METHOD_ARB = 0x2007;
            var WGL_NUMBER_OVERLAYS_ARB = 0x2008;
            var WGL_NUMBER_UNDERLAYS_ARB = 0x2009;
            var WGL_TRANSPARENT_ARB = 0x200A;
            var WGL_TRANSPARENT_RED_VALUE_ARB = 0x2037;
            var WGL_TRANSPARENT_GREEN_VALUE_ARB = 0x2038;
            var WGL_TRANSPARENT_BLUE_VALUE_ARB = 0x2039;
            var WGL_TRANSPARENT_ALPHA_VALUE_ARB = 0x203A;
            var WGL_TRANSPARENT_INDEX_VALUE_ARB = 0x203B;
            var WGL_SHARE_DEPTH_ARB = 0x200C;
            var WGL_SHARE_STENCIL_ARB = 0x200D;
            var WGL_SHARE_ACCUM_ARB = 0x200E;
            var WGL_SUPPORT_GDI_ARB = 0x200F;
            var WGL_SUPPORT_OPENGL_ARB = 0x2010;
            var WGL_DOUBLE_BUFFER_ARB = 0x2011;
            var WGL_STEREO_ARB = 0x2012;
            var WGL_PIXEL_TYPE_ARB = 0x2013;
            var WGL_COLOR_BITS_ARB = 0x2014;
            var WGL_RED_BITS_ARB = 0x2015;
            var WGL_RED_SHIFT_ARB = 0x2016;
            var WGL_GREEN_BITS_ARB = 0x2017;
            var WGL_GREEN_SHIFT_ARB = 0x2018;
            var WGL_BLUE_BITS_ARB = 0x2019;
            var WGL_BLUE_SHIFT_ARB = 0x201A;
            var WGL_ALPHA_BITS_ARB = 0x201B;
            var WGL_ALPHA_SHIFT_ARB = 0x201C;
            var WGL_ACCUM_BITS_ARB = 0x201D;
            var WGL_ACCUM_RED_BITS_ARB = 0x201E;
            var WGL_ACCUM_GREEN_BITS_ARB = 0x201F;
            var WGL_ACCUM_BLUE_BITS_ARB = 0x2020;
            var WGL_ACCUM_ALPHA_BITS_ARB = 0x2021;
            var WGL_DEPTH_BITS_ARB = 0x2022;
            var WGL_STENCIL_BITS_ARB = 0x2023;
            var WGL_AUX_BUFFERS_ARB = 0x2024;
            var WGL_NO_ACCELERATION_ARB = 0x2025;
            var WGL_GENERIC_ACCELERATION_ARB = 0x2026;
            var WGL_FULL_ACCELERATION_ARB = 0x2027;
            var WGL_SWAP_EXCHANGE_ARB = 0x2028;
            var WGL_SWAP_COPY_ARB = 0x2029;
            var WGL_SWAP_UNDEFINED_ARB = 0x202A;
            var WGL_TYPE_RGBA_ARB = 0x202B;
            var WGL_TYPE_COLORINDEX_ARB = 0x202C;

            var WGL_SAMPLE_BUFFERS_ARB = 0x2041;

            var WGL_SAMPLES_ARB = 0x2042;

            int[] iPixelFormatAttribList =
            [
                WGL_SAMPLE_BUFFERS_ARB,
                    1,
                    WGL_SAMPLES_ARB,
                    8,
                ];

            /*iPixelFormatAttribList[0] = WGL_DRAW_TO_WINDOW_ARB;
            iPixelFormatAttribList[1] = GL_TRUE;
            iPixelFormatAttribList[2] = WGL_SUPPORT_OPENGL_ARB;
            iPixelFormatAttribList[3] = GL_TRUE;
            iPixelFormatAttribList[4] = WGL_DOUBLE_BUFFER_ARB;
            iPixelFormatAttribList[5] = GL_TRUE;
            iPixelFormatAttribList[6] = WGL_PIXEL_TYPE_ARB;
            iPixelFormatAttribList[7] = WGL_TYPE_RGBA_ARB;
            iPixelFormatAttribList[8] = WGL_COLOR_BITS_ARB;
            iPixelFormatAttribList[9] = DesiredColorBits;
            iPixelFormatAttribList[10] = WGL_DEPTH_BITS_ARB;
            iPixelFormatAttribList[11] = DesiredDepthBits;
            iPixelFormatAttribList[12] = WGL_STENCIL_BITS_ARB;
            iPixelFormatAttribList[13] = 0;
            iPixelFormatAttribList[14] = WGL_SAMPLE_BUFFERS_ARB;
            iPixelFormatAttribList[15] = GL_TRUE;
            iPixelFormatAttribList[16] = WGL_SAMPLES_ARB;
            iPixelFormatAttribList[17] = NumAASamples;
            iPixelFormatAttribList[18] = 0;*/

            int iPixelFormat, iNumFormats;
            // Example attributes for 4x multisampling
            int[] attribList = new int[]
            {
        WGL_DRAW_TO_WINDOW_ARB, 1,
        WGL_SUPPORT_OPENGL_ARB, 1,
        WGL_DOUBLE_BUFFER_ARB, 1,
        WGL_PIXEL_TYPE_ARB, WGL_TYPE_RGBA_ARB,
        WGL_COLOR_BITS_ARB, 32,
        WGL_DEPTH_BITS_ARB, 24,
        WGL_STENCIL_BITS_ARB, 8,
        WGL_SAMPLE_BUFFERS_ARB, 1, // Enable sample buffers
        WGL_SAMPLES_ARB, 4,       // Request 4 samples (4x MSAA)
        0 // Terminator
            };


            // glcontrol.Context.MakeCurrent();
            // dc = GetDC(glcontrol.Context.WindowPtr);

            // ... inside a method where an OpenTK GraphicsContext is current
            hglrc = wglGetCurrentContext();

            int[] pixelFormats = new int[1]; // We only need one best match


            // Get the number of available formats
            //            if (Wgl.Arb.ChoosePixelFormat(dc, attribList, null, 0, null,out int num_formats))
            //            {
            //          //  }
            ////                if (OpenTK.Graphics.Wgl.Wgl.Arb.ChoosePixelFormat(dc, attribList, new float[0], 1, pixelFormats, out numFormats) && numFormats > 0){

            //            //}
            //            //if (wglChoosePixelFormatARB(dc, attribList, null, 1, pixelFormats, out numFormats) && numFormats > 0)
            //            //{
            //                int pixelFormatIndex = pixelFormats[0];
            //                // Now use this pixelFormatIndex with SetPixelFormat
            //                // ...

            //                //var pixelFormat = ChoosePixelFormat(dc, ref pixelformatdescriptor);
            //                if (!SetPixelFormat(dc, pixelFormatIndex, ref pixelformatdescriptor))
            //                    throw new Win32Exception(Marshal.GetLastWin32Error());


            //                if ((hglrc = wglCreateContext(dc)) == IntPtr.Zero)
            //                    throw new Win32Exception(Marshal.GetLastWin32Error());


            //            }

            d = new GlfwOcctViewManaged();
            //d.runWnt(panel1.Handle, hglrc);

            d.runOpenTk(glcontrol.Context.WindowPtr, hglrc);
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
        [DllImport("opengl32.dll")]
        public static extern IntPtr wglGetCurrentContext();

    }
}