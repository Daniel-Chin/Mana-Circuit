shader_type canvas_item;

uniform float aspect_ratio = .833333;
// uniform vec2 offset_r = vec2(-3.0, 4.0);
uniform vec2 offset_g = vec2(-0.5, -.2);
// uniform vec2 offset_b = vec2(40.0, 99.0);

float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

// float f_r(vec2 screen_pos) {
// 	vec2 pos = offset_r + screen_pos;
// 	float dist = length(pos);
// 	float phase = 1.0 - mod(dist, .3) * 3.3333;
// 	return phase * .1;
// }

vec2 f_g(vec2 screen_pos) {
	vec2 pos = round((offset_g + screen_pos) * 64.0) / 64.0;
	float dist = length(pos);
	float str = pow((sin(TIME) + 1.0) * .5, 3);
	float accent = pow(rand(pos), 12);
	pos.x += round(TIME * 2.0);
	if (accent > .6 * str && rand(pos) < .1) {
		accent *= (2.0 + str * 2.0);
	}
	float phase = 1.0 - pow(mod(dist, .3) * 3.3333, 2);
	return vec2(phase * .1, accent * .1);
}

// float f_b(vec2 screen_pos) {
// 	vec2 pos = offset_b + screen_pos;
// 	float dist = length(pos);
// 	float phase = 1.0 - mod(dist, .3) * 3.3333;
// 	return phase * .1;
// }

void fragment() {
	vec2 screen_pos = vec2(UV.x, UV.y * aspect_ratio);
	// float r = f_r(screen_pos);
	vec2 g = f_g(screen_pos);
	// float b = f_b(screen_pos);
	COLOR = vec4(g.y, g.x + g.y, 0.0, 1.0);
}
