using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;


namespace cymatics
{
    public class CymaticsScene
    {
        //  Constants that specify the attribute indexes.
        const uint AttributeIndexPosition = 0;

        //Texture Names Array
        private uint[] _glTextureArray = new uint[2] {0, 0};

        float resolutionX;
        float resolutionY;

        float time;

        //  The vertex buffer array which contains the vertex and texture coords buffers.
        VertexBufferArray vertexBufferArray;
        VertexBufferArray texCoordsBufferArray;

        public string FragmentShaderSource { get; set; }

        private bool _needsRefresh = true;
        
        //  The shader program for our vertex and fragment shader.
        private ShaderProgram shaderProgram;


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
        public void Initialise(OpenGL gl)
        {
            time = DateTime.Now.Millisecond / 1000;
            //  Set a blue clear colour.
            //gl.ClearColor(0.4f, 0.6f, 0.9f, 0.0f);

            //  Create the shader program.
            var vertexShaderSource = ResourceHelper.LoadTextFromRecource("cymatics.Shaders.Main.vert");

            try
            {
                shaderProgram = new ShaderProgram();
                shaderProgram.Create(gl, vertexShaderSource, FragmentShaderSource, null);
                shaderProgram.BindAttributeLocation(gl, AttributeIndexPosition, "position");
                shaderProgram.AssertValid(gl);
            }
            catch (ShaderCompilationException se)
            {
                Debug.WriteLine(" shader compilation failure");
                Debug.WriteLine(" -------------- ");
                Debug.WriteLine(se.Message);
                Debug.WriteLine(se.CompilerOutput);
                Debug.WriteLine(se.HelpLink);
                Debug.WriteLine(" -------------- ");
                isValid = false;
                _needsRefresh = false;
                return;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                isValid = false;
                _needsRefresh = true;
                return;
            }

            //Generate Textures
            gl.GenTextures(2, _glTextureArray);
            //shaderProgram.BindAttributeLocation(gl, glTextureArray[0], "iChannel0");
            //shaderProgram.BindAttributeLocation(gl, glTextureArray[1], "iChannel1");
            var ch0Loc = shaderProgram.GetUniformLocation(gl, "iChannel0");
            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[0]);
            gl.Uniform1(ch0Loc, 0);

            var ch1Loc = shaderProgram.GetUniformLocation(gl, "iChannel1");
            gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[1]);
            gl.Uniform1(ch1Loc, 1);
            
            //  Now create the geometry for the square.
            CreateVerticesForSquare(gl);

            _needsRefresh = false;
            isValid = true;
        }

        public bool isValid { get; set; }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public void Draw(OpenGL gl, float inputWidth, float inputHeight)
        {
            if (_needsRefresh)
                Initialise(gl);

            if (!isValid)
                return;

            //todo : multiscaling

            resolutionX = inputWidth;
            resolutionY = inputHeight;
            
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            //  Bind the shader
            shaderProgram.Bind(gl);
            
            //pass uniforms
            shaderProgram.SetUniform3(gl, "iResolution", resolutionX, resolutionY, 0.0f);
            shaderProgram.SetUniform1(gl, "iGlobalTime", time);

            time += 0.1f;

            //  Bind the out vertex array.
            vertexBufferArray.Bind(gl);
            texCoordsBufferArray.Bind(gl);

            //Bind Textures
            var channel0Location = shaderProgram.GetUniformLocation(gl, "iChannel0");
            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[0]);
            gl.Uniform1(channel0Location, 0);

            var channel1Location = shaderProgram.GetUniformLocation(gl, "iChannel1");
            gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[1]);
            gl.Uniform1(channel1Location, 1);

            //  Draw the square.
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 6);

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(gl);
            texCoordsBufferArray.Unbind(gl);


            shaderProgram.Unbind(gl);
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


        /// <summary>
        /// Creates the geometry for the square, also creating the vertex buffer array.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        private void CreateVerticesForSquare(OpenGL gl)
        {
            var vertices = new float[18];
            vertices[0] = -0.5f;
            vertices[1] = -0.5f;
            vertices[2] = 0.0f; // Bottom left corner  

            vertices[3] = -0.5f;
            vertices[4] = .5f;
            vertices[5] = 0.0f; // Top left corner  

            vertices[6] = .5f;
            vertices[7] = .5f;
            vertices[8] = 0.0f; // Top Right corner  

            vertices[9] = .5f;
            vertices[10] = -.5f;
            vertices[11] = 0.0f; // Bottom right corner  

            vertices[12] = -.5f;
            vertices[13] = -.5f;
            vertices[14] = 0.0f; // Bottom left corner  

            vertices[15] = .5f;
            vertices[16] = .5f;
            vertices[17] = 0.0f; // Top Right corner   


            var texcoords = new float[12];
            texcoords[0] = -1.0f;
            texcoords[1] = -1.0f;
            texcoords[2] = -1.0f;
            texcoords[3] = 1.0f;
            texcoords[4] = 1.0f;
            texcoords[5] = 1.0f;
            texcoords[6] = 1.0f;
            texcoords[7] = -1.0f;
            texcoords[8] = -1.0f;
            texcoords[9] = -1.0f;
            texcoords[10] = 1.0f;
            texcoords[11] = 1.0f;

            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            texCoordsBufferArray = new VertexBufferArray();
            texCoordsBufferArray.Create(gl);
            texCoordsBufferArray.Bind(gl);


            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);

            var texCoordsBuffer = new VertexBuffer();
            texCoordsBuffer.Create(gl);
            texCoordsBuffer.Bind(gl);
            texCoordsBuffer.SetData(gl, 1, texcoords, false, 2);
            
            //  Unbind the vertex array
            vertexBufferArray.Unbind(gl);
            texCoordsBufferArray.Unbind(gl);
        }

    }
}
