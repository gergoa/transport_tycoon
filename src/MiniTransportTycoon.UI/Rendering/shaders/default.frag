#version 460 core

in vec3 vs_out_col;
in vec2 vs_out_texCoord;
in vec3 vs_out_normal;
in vec3 vs_out_wp;

out vec4 fs_out_col;

uniform sampler2D u_texture;

uniform vec3 u_lightDir = vec3(-0.5, -1.0, -0.3);
uniform vec3 u_lightColor = vec3(1.0, 0.95, 0.9); 
uniform float u_ambient = 0.5;

void main() {
    vec3 actualCol = vs_out_col;
    bool hasStop = false;

    if (abs(vs_out_col.r - 0.99) < 0.01 && abs(vs_out_col.g - 0.01) < 0.01 && abs(vs_out_col.b - 0.02) < 0.01) {
        hasStop = true;
        actualCol = vec3(0.5, 0.5, 0.5); 
    }

    vec4 texColor = texture(u_texture, vs_out_texCoord);
   
    vec3 baseColor = length(texColor.rgb) > 0.0 ? actualCol * texColor.rgb : actualCol; 
    
    // ambient
    vec3 ambient = u_ambient * u_lightColor;
    
    // basic diffuse
    vec3 norm = normalize(vs_out_normal);
    vec3 lightDir = normalize(-u_lightDir);
    
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * u_lightColor;
    
    // ambient + diffuse
    vec3 finalColor = (ambient + diffuse) * baseColor;
    
    // stop sign cheating
    if (hasStop) {
        vec2 localGrid = fract(vs_out_wp.xz);
        vec2 p = localGrid - vec2(0.5);
        
        float d = max(abs(p.x), abs(p.y));
        d = max(d, (abs(p.x) + abs(p.y)) * 0.707106); 
        
        if (d < 0.25) {
            finalColor = mix(finalColor, vec3(0.9, 0.1, 0.1), 0.75); 
        } 
        else if (d < 0.28) {

            finalColor = mix(finalColor, vec3(1.0, 1.0, 1.0), 0.8);
        }
    }
    
    fs_out_col = vec4(finalColor, 1.0);
}