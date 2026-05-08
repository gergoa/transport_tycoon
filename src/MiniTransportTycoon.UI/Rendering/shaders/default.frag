#version 460 core

in vec3 vs_out_col;
in vec2 vs_out_texCoord;
in vec3 vs_out_normal;

out vec4 fs_out_col;

uniform sampler2D u_texture;

uniform vec3 u_lightDir = vec3(-0.5, -1.0, -0.3);
uniform vec3 u_lightColor = vec3(1.0, 0.95, 0.9); 
uniform float u_ambient = 0.5;

void main() {
    vec4 texColor = texture(u_texture, vs_out_texCoord);
    vec3 baseColor = length(texColor.rgb) > 0.0 ? vs_out_col * texColor.rgb : vs_out_col; 
    
    // ambient
    vec3 ambient = u_ambient * u_lightColor;
    
    // basic diffuse
    vec3 norm = normalize(vs_out_normal);
    vec3 lightDir = normalize(-u_lightDir);
    
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * u_lightColor;
    
    // ambient + diffuse
    vec3 finalColor = (ambient + diffuse) * baseColor;
    
    fs_out_col = vec4(finalColor, 1.0);
}