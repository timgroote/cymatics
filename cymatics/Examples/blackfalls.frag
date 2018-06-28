#version 330 core
#ifdef GL_ES
precision lowp float;
#endif

uniform float iGlobalTime;
uniform vec3 iResolution;

const float count = 7.0;

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float Hash( vec2 p, in float s)
{
    vec3 p2 = vec3(p.xy,27.0 * abs(sin(s)));
    return fract(sin(dot(p2,vec3(27.1,61.7, 12.4)))*273758.5453123);
}

mat2 m =mat2(0.8,0.6, -0.6, 0.8);

float rand(vec2 n) { 
	return fract(sin(dot(n, vec2(12.9898, 4.1414))) * 43758.5453);
}

float noise(vec2 n) {
	const vec2 d = vec2(0.0, 1.0);
  	vec2 b = floor(n), f = smoothstep(vec2(0.0), vec2(1.0), fract(n));
	return mix(mix(rand(b), rand(b + d.yx), f.x), mix(rand(b + d.xy), rand(b + d.yy), f.x), f.y);
}

float fbm(vec2 p){
	float f=.0;
	f-= .500*noise(p); p*= m*2.02;
	f+= .150*noise(p); p*= m*2.03;
	f+= .650*noise(p); p*= m*2.01;
	f+= .65*noise(p); p*= m*2.04;
	
	f/= 0.9375;
	
	return f;
}

void main( void ) 
{
	vec2 uv = ( gl_FragCoord.xy / iResolution.xy ) * 2.0 - 1.0;
	uv.x *= iResolution.x/iResolution.y;
	
	vec3 finalColor = vec3( 0.0 );
	for( float i=1.; i < count; ++i )
	{
		float t = abs(1./((uv.x + smoothstep(0.2,1.,fbm( uv + iGlobalTime/i))) * (i*5.0)));
		finalColor +=  smoothstep(0.2,.0,t * vec3( i * 0.65 , 10, 2.0 ));
	}
	
	finalColor=hsv2rgb(finalColor);
	
	gl_FragColor = vec4( finalColor, 1.0 );
}