#version 330 core
out vec3 color;

in vec2 UVs;
in vec3 normals;
in vec3 fragPos;

uniform vec3 viewPos;

const float constant = 1;
const float linear = 0.09;
const float quadratic = 0.032;
const float PI = 3.14159265359;

const float sun_strength = 3.0;
const vec3 sun_direction = vec3(1.0, 1.0, 1.0);

float Fresnel(vec3 N, vec3 V, float Power);

vec3 CalcDirectionalLight(vec3 direction, vec3 V, vec3 N, vec3 F0, vec3 alb, float rough, float metal);
float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);
vec3 ACESFilm(vec3 x);

const vec3 albedo = vec3(1);
float roughness = 0.25;
float metallic = 0.0;

void main()
{ 
    vec3 V = normalize(viewPos - fragPos);
    vec3 L = normalize(sun_direction);
    vec3 H = normalize(V + L);
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    vec3 calc = CalcDirectionalLight(sun_direction, V, normals, F0, albedo, roughness, metallic);
    // vec3 calc = vec3(Fresnel(normals, V, 3));

    color = calc;
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0 / 2.2));
}

float Fresnel(vec3 N, vec3 V, float Power)
{
    return pow(1.0 - dot(N, V), Power);
}

vec3 CalcDirectionalLight(vec3 direction, vec3 V, vec3 N, vec3 F0, vec3 alb, float rough, float metal)
{
    // Calc per light radiance
    vec3 L = normalize(direction);
    vec3 H = normalize(V + L);
    vec3 radiance = vec3(1) * sun_strength;

    // Cook-Torrance BRDF
    float NDF = DistributionGGX(N, H, rough);   
    float G   = GeometrySmith(N, V, L, rough);
    vec3 F    = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);

    vec3 numerator    = NDF * G * F; 
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular = numerator / denominator;

    vec3 kS = F;
    vec3 kD = vec3(1) - kS;
    kD *= 1 - metal;

    float NDotL = max(dot(N, L), 0.0);

    return (kD * alb / PI + specular) * radiance * NDotL;
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

vec3 ACESFilm(vec3 x)
{
    float a = 2.51;
    float b = 0.03;
    float c = 2.43;
    float d = 0.59;
    float e = 0.14;
    return (x * (a * x + b)) / (x * (c * x + d) + e);
}