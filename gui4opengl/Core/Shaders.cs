using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace OpenGL.UI
{
    internal static class Shaders
    {
        public static ShaderProgram SolidUIShader;
        public static ShaderProgram TexturedUIShader;
        public static ShaderProgram FontShader;
        public static ShaderProgram GradientShader;
        public static ShaderProgram HueShader;

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
                GradientShader = InitShader(gradientVertexShaderSource, gradientFragmentShaderSource);
                HueShader = InitShader(hueVertexShaderSource, hueFragmentShaderSource);

                initialized = true;
            }
            catch (Exception)
            {
            }
            
            return initialized;
        }

        public static void Dispose()
        {
            if (!initialized) return;

            if (SolidUIShader != null) SolidUIShader.Dispose();
            if (TexturedUIShader != null) TexturedUIShader.Dispose();
            if (FontShader != null) FontShader.Dispose();
            if (GradientShader != null) GradientShader.Dispose();
            if (HueShader != null) HueShader.Dispose();
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
                var shaderParam = program["ui_projection_matrix"];
                if (shaderParam != null)
                {
                    program.Use();
                    program["ui_projection_matrix"].SetValue(projectionMatrix);
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

void main(void)
{
  uv = in_uv;
  gl_Position = ui_projection_matrix * vec4(in_position.x + position.x, in_position.y + position.y, 0, 1);
}";

        private static string FontFragmentSource = @"
#version 140

uniform sampler2D active_texture;
uniform vec3 color;

in vec2 uv;

void main(void)
{
  vec4 t = texture2D(active_texture, uv);
  gl_FragColor = vec4(t.rgb * color, t.a);
}";

        public static string gradientVertexShaderSource = @"
uniform mat4 ui_projection_matrix;
uniform mat4 model_matrix;

attribute vec3 in_position;
attribute vec3 in_uv;

varying vec3 uv;
varying vec3 position;

void main(void)
{
  position = in_position;
  uv = in_uv;
  gl_Position = ui_projection_matrix * model_matrix * vec4(in_position, 1);
}";

        private static string gradientFragmentShaderSource = @"
uniform vec3 hue;
uniform vec2 sel;
varying vec3 uv;
varying vec3 position;

void main(void)
{
  int posx = int(position.x);
  int posy = int(position.y);

  if (posx == 0 || posx == 149 || posy == 0 || posy == 149) gl_FragColor = vec4(0, 0, 0, 1);
  else
  {
    vec3 gradient = mix(vec3(1, 1, 1), hue, uv.x);
    float distance = (uv.x - sel.x) * (uv.x - sel.x) + (uv.y - sel.y) * (uv.y - sel.y);
    bool ring3 = (distance >= 0.0005 && distance < 0.001);
    bool ring2 = (distance >= 0.00025 && distance < 0.0005);
    bool ring1 = (distance >= 0.0001 && distance < 0.00025);
    gl_FragColor = (ring3 || ring1 ? vec4(0, 0, 0, 1) : (ring2 ? vec4(1, 1, 1, 1) : vec4(mix(vec3(0, 0, 0), gradient, uv.y), 1)));
  }
}";

        public static string hueVertexShaderSource = @"
uniform mat4 ui_projection_matrix;
uniform mat4 model_matrix;

attribute vec3 in_position;
attribute vec3 in_uv;

varying vec3 uv;
varying vec3 position;

void main(void)
{
  position = in_position;
  uv = in_uv;
  gl_Position = ui_projection_matrix * model_matrix * vec4(in_position, 1);
}";

        private static string hueFragmentShaderSource = @"
varying vec3 uv;
varying vec3 position;

uniform float hue;

float HUE2RGB(float p, float q, float t)
{
  if (t < 0.0) t += 1.0;
  if (t > 1.0) t -= 1.0;
  if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
  if (t < 1.0 / 2.0) return q;
  if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
  return p;
}

vec3 HSL2RGB(float h, float s, float l)
{
  float r, g, b;

  if (s == 0.0) r = g = b = l;
  else
  {
    float q = (l < 0.5 ? l * (1.0 + s) : l + s - l * s);
    float p = 2.0 * l - q;
    r = HUE2RGB(p, q, h + 1.0 / 3.0);
    g = HUE2RGB(p, q, h);
    b = HUE2RGB(p, q, h - 1.0 / 3.0);
  }

  return vec3(r, g, b);
}

void main(void)
{
  int posx = int(position.x);
  int posy = int(position.y);

  if (posx == 10 || posx == 25) gl_FragColor = vec4(0, 0, 0, 1);
  else if (position.x >= 10.0 && position.x <= 25.0) gl_FragColor = (posy == 0 || posy == 149 ? vec4(0, 0, 0, 1) : vec4(HSL2RGB(uv.y, 1.0, 0.5), 1));
  else if (position.x < 8.0)
  {
    float distance = abs(position.y - hue);
    if (int(6.0 - position.x) == int(distance)) gl_FragColor = vec4(0, 0, 0, 1);
    else if (6.0 - position.x > distance) gl_FragColor = (int(position.x) == 0 ? vec4(0, 0, 0, 1) : vec4(HSL2RGB(hue / 150.0, 1.0, 0.5), 1));
    else gl_FragColor = vec4(0, 0, 0, 0);
  }
  else gl_FragColor = vec4(0, 0, 0, 0);
}";
        #endregion
    }
}
