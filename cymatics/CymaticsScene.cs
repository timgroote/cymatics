using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using cymatics.Controls;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using WpfGlslControl;
using WpfOpenGlControl;


namespace cymatics
{
    public class CymaticsScene
    {
        //  Constants that specify the attribute indexes.
        const uint AttributeIndexPosition = 0;

        //Texture Names Array
        private uint[] _glTextureArray = new uint[2] {0, 0};

        public string CompilationFailureText = "";

        float resolutionX;
        float resolutionY;

        float time;

        //  The vertex buffer array which contains the vertex and texture coords buffers.
        VertexBufferArray vertexBufferArray;
        VertexBufferArray texCoordsBufferArray;

        protected virtual void OnCompilationEvent(EventArgs e)
        {
            EventHandler handler = Compilation;
            handler?.Invoke(this, new EventArgs());
        }

        protected Boolean needsRefresh;
        public event EventHandler Compilation;

        private string fragmentShaderSource;
        public string FragmentShaderSource {
            get { return fragmentShaderSource; }
            set { fragmentShaderSource = value;
                needsRefresh = true;
            }
        }
        
        //  The shader program for our vertex and fragment shader.
        private ShaderProgram shaderProgram;

        public float resolutionMultiplier = 1;

        public CymaticsScene()
        {
            this.FragmentShaderSource = "";
        }

        public CymaticsScene(string fragShaderSource)
        {
            this.FragmentShaderSource = fragShaderSource;
        }

        /// <summary>
        /// Initialises the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public void Initialise(GLSLControl gl)
        {
            time = DateTime.Now.Millisecond / 1000;
            gl.ShaderSourceCode = this.FragmentShaderSource;

            gl.OnBeforeRender = delegate (WpfOpenGLControl control)
            {
                if (needsRefresh)
                {
                    Initialise(gl);
                    if(isValid)
                        needsRefresh = false;
                }

                time += 0.1f;
                gl.SetUniform("iGlobalTime", time);
                resolutionX = (float)control.ActualWidth;
                resolutionY = (float)control.ActualHeight;
                gl.SetUniform("iResolution", resolutionX, resolutionY);

                return control;
            };

            //Generate Textures
            //gl.GenTextures(2, _glTextureArray);
            //shaderProgram.BindAttributeLocation(gl, glTextureArray[0], "iChannel0");
            //shaderProgram.BindAttributeLocation(gl, glTextureArray[1], "iChannel1");
            //var ch0Loc = shaderProgram.GetUniformLocation(gl, "iChannel0");
            //gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            //gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[0]);
            //gl.Uniform1(ch0Loc, 0);

            //var ch1Loc = shaderProgram.GetUniformLocation(gl, "iChannel1");
            //gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            //gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[1]);
            //gl.Uniform1(ch1Loc, 1);

            isValid = gl.ShaderLog.Length == 0;
            CompilationFailureText = gl.ShaderLog;
            OnCompilationEvent(new EventArgs());
        }

        public bool isValid { get; set; }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public void Draw(WpfShaderControl gl, float inputWidth, float inputHeight)
        {
            if (!isValid)
                return;

            //todo : multiscaling

            resolutionX = inputWidth;
            resolutionY = inputHeight;
            
            
            //gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            //  Bind the shader
            //shaderProgram.Bind(gl);
            
            //pass uniforms
            gl.SetUniform("iResolution", resolutionX / resolutionMultiplier, resolutionY / resolutionMultiplier, 0.0f);
            gl.SetUniform("iGlobalTime", time);
//            shaderProgram.SetUniform3(gl, "iResolution", resolutionX / resolutionMultiplier, resolutionY / resolutionMultiplier, 0.0f);
//            shaderProgram.SetUniform1(gl, "iGlobalTime", time);

            time += 0.1f;

            //  Bind the out vertex array.
//            vertexBufferArray.Bind(gl);
//            texCoordsBufferArray.Bind(gl);

            //Bind Textures
            //var channel0Location = shaderProgram.GetUniformLocation(gl, "iChannel0");
            //gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            //gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[0]);
            //gl.Uniform1(channel0Location, 0);

            //var channel1Location = shaderProgram.GetUniformLocation(gl, "iChannel1");
            //gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            //gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[1]);
            //gl.Uniform1(channel1Location, 1);

            //  Draw the square.
//            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 6);

            //  Unbind our vertex array and shader.
//            vertexBufferArray.Unbind(gl);
//            texCoordsBufferArray.Unbind(gl);
//
//
//            shaderProgram.Unbind(gl);
        }

        /// <summary>
        /// The width of the texture images.
        /// </summary>
        private int[] _width = new int[2] {0, 0};

        /// <summary>
        /// The height of the texture images.
        /// </summary>
        private int[] _height = new int[2] {0, 0};

        /// <summary>
        /// updates pixel data of the desired texture.
        /// </summary>
        public void UpdateTextureBitmap(OpenGL gl, int texIndex, Bitmap image)
        {
            int[] textureMaxSize = {0};
            gl.GetInteger(OpenGL.GL_MAX_TEXTURE_SIZE, textureMaxSize);

            if (image == null) return;

            //	Find the target width and height sizes, which is just the highest
            //	posible power of two that'll fit into the image.
            int targetWidth = textureMaxSize[0];
            int targetHeight = textureMaxSize[0];

            //Console.WriteLine("Updating Tex " + texIndex + "Tex Max Size : " + targetWidth + "x" + targetHeight);

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Width < size)
                {
                    targetWidth = size / 2;
                    break;
                }
                if (image.Width == size)
                    targetWidth = size;

            }

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Height < size)
                {
                    targetHeight = size / 2;
                    break;
                }
                if (image.Height == size)
                    targetHeight = size;
            }

            //  If need to scale, do so now.
            if (image.Width != targetWidth || image.Height != targetHeight)
            {
                //  Resize the image.
                Image newImage = image.GetThumbnailImage(targetWidth, targetHeight, null, IntPtr.Zero);

                //  Destory the old image, and reset.
                image.Dispose();
                image = (Bitmap) newImage;
            }

            //  Lock the image bits (so that we can pass them to OGL).
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //	Set the width and height.
            _width[texIndex] = image.Width;
            _height[texIndex] = image.Height;

            gl.ActiveTexture(texIndex == 0 ? OpenGL.GL_TEXTURE0 : OpenGL.GL_TEXTURE1);

            //	Bind our texture object (make it the current texture).
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[texIndex]);

            //  Set the image data.
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int) OpenGL.GL_RGBA,
                _width[texIndex], _height[texIndex], 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                bitmapData.Scan0);

            //  Unlock the image.
            image.UnlockBits(bitmapData);

            //  Dispose of the image file.
            image.Dispose();

            //  Set linear filtering mode.
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
        }
    }
}
