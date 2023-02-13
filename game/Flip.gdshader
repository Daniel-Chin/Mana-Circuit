shader_type canvas_item;

void vertex() {
    float t = VERTEX.x;
    VERTEX.x = VERTEX.y;
    VERTEX.y = t;
}
