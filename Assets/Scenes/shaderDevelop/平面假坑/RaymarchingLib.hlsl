#ifndef RAY_MARCHING_LIB_INCLUDED
#define RAY_MARCHING_LIB_INCLUDED

#endif


// Helful Resources
// ----------------
// Ray Marching Blog Post by Michael Walczyk
// https://michaelwalczyk.com/blog-ray-marching.html
// Inigo Quilez SDF Functions
// https://iquilezles.org/articles/distfunctions/


static const int NUM_OF_STEPS = 16;
static const float MIN_DIST_TO_SDF = 0.01;
static const float MAX_DIST_TO_TRAVEL = 1000.0;

float opUnion( float d1, float d2 ) { return min(d1,d2); }

float opSubtraction( float d1, float d2 ) { return max(-d1,d2); }

float opIntersection( float d1, float d2 ) { return max(d1,d2); }

float opSmoothUnion(float d1, float d2, float k) {
  float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
  return lerp(d2, d1, h) - k * h * (1.0 - h);
}

float opSmoothSubtraction( float d1, float d2, float k ) {
    float h = clamp( 0.5 - 0.5*(d2+d1)/k, 0.0, 1.0 );
    return lerp( d2, -d1, h ) + k*h*(1.0-h); }


float opSmoothIntersection( float d1, float d2, float k ) {
    float h = clamp( 0.5 - 0.5*(d2-d1)/k, 0.0, 1.0 );
    return lerp( d2, d1, h ) + k*h*(1.0-h); }

float sdfPlane(float3 p, float3 n, float h) {
  // n must be normalized
  return dot(p, n) + h;
}

float sdfSphere(float3 p, float3 c, float r) {
  return length(p - c) - r;
}

float sdBox( float3 p, float3 b )
{
  float3 q = abs(p) - b;
  return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}

float map(float3 p) {
  float m = 0;
  float radius = 2.0;
  float3 center = float3(0.0, 0.0, 0.0);
  float sphere = sdfSphere(p, center, radius);
  //
  // // part 1.2 - display plane
  float h = 0.0;
  float3 normal = float3(0.0, 1.0, 0.0);
  float plane = sdfPlane(p, normal, h);

  float box = sdBox(p, float3(2, 99999, 2));
  // m = min(sphere, plane);
  //
  // // part 4 - add smooth blending
  // m = opSmoothUnion(box, plane, 0.5);
  //
  // m = opSmoothSubtraction(box, plane, 0.5);
  // m = opSmoothIntersection(sdfPlane(p, -normal, -2.0), m, 0.5);
   m = opSubtraction(box, plane);
   m = opIntersection(sdfPlane(p, -normal, -2.0), m);
  
  
  // m = max(-sphere, plane);
  // return plane;
  // return sphere;
  
  return m;
}

float rayMarch(float3 ro, float3 rd, float depth) {
  float dist = depth;
  
  for (int i = 0; i <= NUM_OF_STEPS; i++) {
    float3 currentPos = ro + rd * dist;
    float distToSdf = map(currentPos);
  
    if (distToSdf < MIN_DIST_TO_SDF) {
      break;
    }
  
    dist = dist + distToSdf;
  
    if (dist > MAX_DIST_TO_TRAVEL) {
      break;
    }
  }

  return dist;
}

// float3 getNormal(float3 p) {
//   vec2 d = vec2(0.01, 0.0);
//   float gx = map(p + d.xyy) - map(p - d.xyy);
//   float gy = map(p + d.yxy) - map(p - d.yxy);
//   float gz = map(p + d.yyx) - map(p - d.yyx);
//   float3 normal = float3(gx, gy, gz);
//   return normalize(normal);
// }

// float3 render(vec2 uv) {
//   float3 color = float3(0.0, 0.0, 0.0);
//
//   // note: ro -> ray origin, rd -> ray direction
//   float3 ro = float3(0.0, -0.7, -3);
//   float3 rd = float3(uv, 1.0);
//   rd = normalize(float3(uv, 1.0));
//
//   float dist = rayMarch(ro, rd, MAX_DIST_TO_TRAVEL);
//
//   if (dist < MAX_DIST_TO_TRAVEL) {
//     // part 1 - display ray marching result
//     color = float3(1.0);
//
//     // part 2.1 - calculate normals
//     // calculate normals at the exact point where we hit SDF
//     float3 p = ro + rd * dist;
//     float3 normal = getNormal(p);
//     color = normal;
//
//     // part 2.2 - add lighting
//
//     // part 2.2.1 - calculate diffuse lighting
//     float3 lightColor = float3(1.0);
//     float lightRotaSpeed = 3.0;
//     float3 lightSource = float3(2.5+cos(u_time*lightRotaSpeed), 2.5, -2.5+sin(u_time*lightRotaSpeed));
//     float diffuseStrength = max(0.0, dot(normalize(lightSource), normal));
//     float3 diffuse = lightColor * diffuseStrength;
//
//     // part 2.2.2 - calculate specular lighting
//     float3 viewSource = normalize(ro);
//     float3 reflectSource = normalize(reflect(-lightSource, normal));
//     float specularStrength = max(0.0, dot(viewSource, reflectSource));
//     specularStrength = pow(specularStrength, 64.0);
//     float3 specular = specularStrength * lightColor;
//
//     // part 2.2.3 - calculate lighting
//     float3 lighting = diffuse * 0.75 + specular * 0.25;
//     color = lighting;
//
//     // part 3 - add shadows
//
//     // part 3.1 - update the ray origin and ray direction
//     float3 lightDirection = normalize(lightSource);
//     float distToLightSource = length(lightSource - p);
//     ro = p + normal * 0.1;
//     rd = lightDirection;
//
//     // part 3.2 - ray march based on new ro + rd
//     float dist = rayMarch(ro, rd, distToLightSource);
//     if (dist < distToLightSource) {
//       color = color * float3(0.25);
//     }
//
//     // note: add gamma correction
//     color = pow(color, float3(1.0 / 2.2));
//   }
//
//   return color;
// }

// void main() {
//   vec2 uv = 2.0 * gl_FragCoord.xy / u_resolution - 1.0;
// //   note: properly center the shader in full screen mode
// //   uv = (2.0 * gl_FragCoord.xy - u_resolution) / u_resolution.y;
//   float3 color = float3(0.0);
//   uv.x *= u_resolution.x / u_resolution.y;
//   color = render(uv);
// //   color = float3(uv, 0.0);
//   gl_FragColor = vec4(color, 1.0);
// }