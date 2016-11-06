using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace OpenGL.UI
{
    public static class Shaders
    {
        public static ShaderProgram SolidUIShader;
        public static ShaderProgram TexturedUIShader;
        public static ShaderProgram FontShader;

        private static List<ShaderProgram> LoadedPrograms = new List<ShaderProgram>();

        public enum ShaderVersion
        {
            GLSL120,
            GLSL140
        }

        public static ShaderVersion Version = ShaderVersion.GLSL140;
        private static bool initialized = false;

        public static bool Init(ShaderVersion shaderVersion = ShaderVersion.GLSL140)
        {
            if (initialized) return true;

            Version = shaderVersion;

            try
            {
                SolidUIShader = InitShader(UISolidVertexSource, UISolidFragmentSource);
                TexturedUIShader = InitShader(UITexturedVertexSource, UITexturedFragmentSource);
                FontShader = InitShader(FontVertexSource, FontFragmentSource);

                initialized = true;
            }
            catch (Exception)
            {
            }
            
            return initialized;
        }

        private static char[] newlineChar = new char[] { '\n' };
        private static char[] unixNewlineChar = new char[] { '\r' };

        public static string ConvertShader(string shader, bool vertexShader)
        {
            // there are a few rules to convert a shader from 140 to 120
            // the first is to remove the keywords 'in' and 'out' and replace with 'attribute'
            // the next is to remove camera uniform blocks
            StringBuilder sb = new StringBuilder();

            string[] lines = shader.Split(newlineChar);

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim(unixNewlineChar);
                if (lines[i].StartsWith("uniform Camera"))
                {
                    i += 3;

                    sb.AppendLine("uniform mat4 projection_matrix;");
                    sb.AppendLine("uniform mat4 view_matrix;");
                }
                else if (lines[i].StartsWith("#version 140")) sb.AppendLine("#version 130");
                else if (lines[i].StartsWith("in ")) sb.AppendLine((vertexShader ? "attribute " : "varying ") + lines[i].Substring(3));
                else if (lines[i].StartsWith("out ") && vertexShader) sb.AppendLine("varying " + lines[i].Substring(4));
                else sb.AppendLine(lines[i]);
            }

            return sb.ToString();
        }

        public static ShaderProgram InitShader(string vertexSource, string fragmentSource)
        {
            if (Version == ShaderVersion.GLSL120)
            {
                vertexSource = ConvertShader(vertexSource, true);
                fragmentSource = ConvertShader(fragmentSource, false);
            }

            ShaderProgram program = new ShaderProgram(vertexSource, fragmentSource);
            LoadedPrograms.Add(program);

            return program;
        }

        public static void UpdateUIProjectionMatrix(Matrix4 projectionMatrix)
        {
            foreach (var program in LoadedPrograms)
            {
                for (int i = 0; i < program.VertexShader.ShaderParams.Length; i++)
                {
                    if (program.VertexShader.ShaderParams[i].Name == "ui_projection_matrix")
                    {
                        program.Use();
                        program["ui_projection_matrix"].SetValue(projectionMatrix);
                    }
                }
            }
        }

        #region UI Shader Source
        private static string UITexturedVertexSource = @"
#version 140

uniform vec3 position;
uniform mat4 ui_projection_matrix;

in vec3 in_position;
in vec2 in_uv;

out vec2 uv;

void main(void)
{
  uv = in_uv;
  
  gl_Position = ui_projection_matrix * vec4(position + in_position, 1);
}";

        private static string UITexturedFragmentSource = @"
uniform sampler2D active_texture;

in vec2 uv;

void main(void)
{
  gl_FragColor = texture2D(active_texture, uv);
}";

        private static string UISolidVertexSource = @"
#version 140

uniform vec3 position;
uniform mat4 ui_projection_matrix;

in vec3 in_position;

void main(void)
{
  gl_Position = ui_projection_matrix * vec4(position + in_position, 1);
}";

        private static string UISolidFragmentSource = @"
#version 140

uniform vec4 color;

void main(void)
{
  gl_FragColor = color;
}";

        private static string FontVertexSource = @"
#version 140

uniform vec2 position;
uniform mat4 ui_projection_matrix;

in vec3 in_position;
in vec2 in_uv;

out vec2 uv;
out vec3 color;

vec3 unpackColor(float f) {
    float rb = floor(f / 100);
    float rg = floor(f - rb * 100);
    float rr = f - rb * 100 - rg;

    return vec3(rr, rg / 100.0, rb / 100.0);
}

void main(void)
{
  uv = in_uv;
  color = unpackColor(in_position.z);
  gl_Position = ui_projection_matrix * vec4(in_position.x + position.x, in_position.y + position.y, 0, 1);
}";

        private static string FontFragmentSource = @"
#version 140

uniform sampler2D active_texture;

in vec2 uv;
in vec3 color;

void main(void)
{
  vec4 t = texture2D(active_texture, uv);
  gl_FragColor = vec4(t.rgb * color, t.a);
}";
        #endregion
    }
}
