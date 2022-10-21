#ifndef TOON_FIRE_INCLUDED
#define TOON_FIRE_INCLUDED

float2 hash(float2 p){
    p = float2( dot(p,float2(137.1,373.7)), dot(p,float2(269.5,183.7)) ); 
    return frac(sin(p)*43758.37);
}

float worley(float2 p, float in_timeInSeconds){
    float2 n = floor(p);
    float2 f = frac(p);
    float r = 1.;
    for(int i=-2;i<=2;i++){
        for(int j=-2;j<=2;j++){
            float2 o = hash(n+float2(i,j));
            o = sin(in_timeInSeconds/2. + hash(n+float2(i,j))*6.28)*0.5+0.5;//animate
            o += float2(i,j);
            float D1 = distance(o,f);//Euclidean
            r = min(r,D1);
        }
    }
    return r;
}

#endif