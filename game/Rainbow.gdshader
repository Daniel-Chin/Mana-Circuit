shader_type canvas_item;

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, .6666666666, .3333333333, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
	vec2 pos = UV - 0.5;
	float theta = atan(pos.y / pos.x);
	if (pos.x < 0.0) {
		theta = theta + 3.141591653238;
	}
	float c = sin(TIME * 2.0);
	theta += TIME * 0.2 + c * 0.1;
	float v;
	if (abs(pos.x) > abs(pos.y)) {
		v = abs(pos.x);
	} else {
		v = abs(pos.y);
	}
	v += c * .05;
	v = pow(v, 1.7);
	vec3 rgb = hsv2rgb(vec3(theta * 0.15915494309189535, 1.0, v));
	COLOR = vec4(rgb.xyz, 1.0);
}
