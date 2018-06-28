#version 330 core

uniform sampler2D iChannel0;
uniform vec3 iResolution;
uniform float iGlobalTime;
void main( void ) {
    float dist= 2.;
	vec2 p = gl_FragCoord.xy / iResolution.xy * 2. - 1.;
	p.x *= iResolution.x/iResolution.y;
	p*=2.;
	p=abs(p);
	float a = atan(p.y,p.x);
	float l = log(length(p))*1.;
        float c = sin((l+cos(a*1.-sin(a*3.+iGlobalTime)-iGlobalTime*2.2)+a*2.-iGlobalTime*1.));
	
	 c*=log(abs(l*dist));
	gl_FragColor = vec4( c,c*c,-c,1.0 );
}
