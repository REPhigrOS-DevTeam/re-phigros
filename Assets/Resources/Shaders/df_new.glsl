#version 100

#ifdef GL_FRAGMENT_PRECISION_HIGH
precision highp float;
#else
precision mediump float;
#endif
precision highp int;

uniform sampler2D u_image;
varying lowp vec2 v_texCoord;
const vec2 center = vec2(0.5, 0.5);
const vec3 luminosityFactor = vec3(0.2125, 0.7154, 0.0721);
const vec3 averageColor = vec3(0.5, 0.5, 0.5);

// RadialBlur
uniform float radialBlurRadius;
uniform float radialBlurDeform;
#define _DownSampleTex u_image
vec4 radialBlur() {
  lowp vec4 color = texture2D(u_image, v_texCoord);
  vec2 dist = v_texCoord - center;
  float r2 = dot(dist, dist);
  vec2 dist1 = exp2(log2(sqrt(r2) * radialBlurDeform) * radialBlurRadius) * inversesqrt(r2) * dist;
  vec4 blur = color;
  for(int i = 1; i < 10; i++) blur += texture2D(_DownSampleTex, v_texCoord - 0.1 * float(i) * dist1);
  vec2 d2 = vec2(max(radialBlurRadius * 0.15, 0.18), sqrt(r2)) - 0.18;
  float dp = clamp(d2.y / d2.x, 0.0, 1.0);
  return color + (dp * dp * (3.0 - 2.0 * dp)) * (blur * 0.1 - color);
}

// ESCControl
uniform float exposure;
uniform float brightness;
// #define exposure brightness+1.0
uniform float saturation;
uniform float contrast;
vec3 escControl(vec3 color) {
  vec3 grayScale = vec3(dot(color, luminosityFactor));
  color = mix(grayScale, color, saturation);
  color = mix(averageColor, color, contrast);
  color = color * exposure;
  return color;
}

// Vignette
uniform float smoothness;
uniform float vignetteRadius;
uniform float darkness;
uniform float lineOpacity;

vec3 vignette(vec3 color) {
  vec2 coord = v_texCoord - center;
  float dist = sqrt(dot(coord, coord));
  float uc = clamp((dist - vignetteRadius) / smoothness, 0.0, 1.0);
  float ud = max((sin(v_texCoord.y * 850.0) * 0.5 - 0.25) * 4.0, 0.0);
  float p0 = uc * uc * (3.0 - uc * 2.0) * darkness;
  float p1 = ud * ud * (3.0 - ud * 2.0) * lineOpacity;
  return abs(1.0 - p0) * (color + p1);
}

// Main
void main() {
  vec4 color = radialBlur();
  color.rgb = escControl(color.rgb);
  color.rgb = vignette(color.rgb);
  gl_FragColor = color;
}