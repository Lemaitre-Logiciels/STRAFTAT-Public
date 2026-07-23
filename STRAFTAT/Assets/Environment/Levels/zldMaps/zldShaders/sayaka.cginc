#ifndef SAYAKA_ALREADY_HERE
#define SAYAKA_ALREADY_HERE

// さやか [sa.ya.ka] － one header cross-language [Cg, HLSL, GLSL] shader helper library
// https://github.com/counter185/sayaka_shaderlib
// Licensed under CC0 1.0 Universal

#define sayaka_vec2 float2
#define sayaka_vec4 float4
#define sayaka_float float
#define sayaka_int int
#define sayaka_bool int
#define sayaka_sampler2d float4

#ifdef UNITY_UV_STARTS_AT_TOP
#define SAYAKA_IS_IN_UNITY 1
#define sayaka_sampler2d sampler2D
#endif

#ifdef __HLSL_VERSION
#define SAYAKA_IS_IN_HLSL 1
#define sayaka_sampler2d Texture2D
#endif

#define sayaka_mix sayaka_blendcolor

#ifdef SAYAKA_IS_IN_UNITY
sayaka_vec4 sayaka_sampletexture(sayaka_sampler2d samplerState, sayaka_vec2 pos) {
    return tex2D(samplerState, pos);
}
#endif

sayaka_bool sayaka_vec4equals(sayaka_vec4 a, sayaka_vec4 b) {
    return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
}

sayaka_vec2 sayaka_vec2add(sayaka_vec2 a, sayaka_vec2 b) {
    return sayaka_vec2(a.x + b.x, a.y + b.y);
}

sayaka_bool sayaka_pointinbox(sayaka_vec2 posnow, sayaka_vec4 box_xywh) {
    return posnow.x >= box_xywh.x && posnow.x < (box_xywh.x+box_xywh.z) && posnow.y >= box_xywh.y && posnow.y < (box_xywh.y+box_xywh.w);
}

sayaka_int sayaka_indexsplit(sayaka_float pos, sayaka_float segmentSize) {
    return int(floor(pos / segmentSize));
}

sayaka_float sayaka_3ddistance(sayaka_vec4 pos1, sayaka_vec4 pos2) {
    return sqrt(pow(pos2.x - pos1.x, 2) + pow(pos2.y - pos1.y, 2) + pow(pos2.z - pos1.z, 2));
}

sayaka_float sayaka_distancefrompoint(sayaka_vec2 pos, sayaka_vec2 ppoint) {
    return sqrt(pow(ppoint.x - pos.x, 2) + pow(ppoint.y - pos.y, 2));
}

sayaka_vec4 sayaka_blendcolor(sayaka_vec4 color1, sayaka_vec4 color2, sayaka_float percentOfColor2) {
    return percentOfColor2 == 0 ? color1
            : percentOfColor2 * color2.w == 1 ? color2
            : sayaka_vec4(
                color1.x + (color2.x - color1.x) * percentOfColor2 * color2.w,
                color1.y + (color2.y - color1.y) * percentOfColor2 * color2.w,
                color1.z + (color2.z - color1.z) * percentOfColor2 * color2.w,
                color1.w + (color2.w - color1.w) * percentOfColor2 * color2.w
            );
}

sayaka_vec4 sayaka_gradient_2point(sayaka_vec4 color1, sayaka_vec4 color2, sayaka_float from, sayaka_float now, sayaka_float to) {
    return sayaka_blendcolor(color1, color2, 
            now < from ? 0.0
            : now > to ? 1.0
            : (now - from) / (to - from)
        );
}

sayaka_vec4 sayaka_gradient_radial(sayaka_vec4 colorBG, sayaka_vec4 colorGradient, sayaka_vec2 pointNow, sayaka_vec2 center, sayaka_float radius) {
    return sayaka_blendcolor(colorBG, colorGradient, 1.0 - min(max(sayaka_distancefrompoint(pointNow, center) / radius,0),1));
}

sayaka_vec2 sayaka_tileuv(sayaka_vec2 positionNow, sayaka_vec2 tileSize) {
    return sayaka_vec2(
        (positionNow.x - floor(positionNow.x / tileSize.x) * tileSize.x) / tileSize.x,
        (positionNow.y - floor(positionNow.y / tileSize.y) * tileSize.y) / tileSize.y
    );
}

sayaka_vec4 sayaka_rgb2hsl(sayaka_vec4 rgb) {
    sayaka_float cmax = max(max(rgb.x, rgb.y), rgb.z);
    sayaka_float cmin = min(min(rgb.x, rgb.y), rgb.z);
    sayaka_float delta = cmax - cmin;
    sayaka_float h = 0;
    sayaka_float s = 0;
    sayaka_float l = (cmax + cmin) / 2;
    if (delta != 0) {
        s = l > 0.5 ? delta / (2 - cmax - cmin) : delta / (cmax + cmin);
        if (cmax == rgb.x) {
            h = (rgb.y - rgb.z) / delta + (rgb.y < rgb.z ? 6 : 0);
        } else if (cmax == rgb.y) {
            h = (rgb.z - rgb.x) / delta + 2;
        } else {
            h = (rgb.x - rgb.y) / delta + 4;
        }
        h /= 6;
    }
    return sayaka_vec4(h, s, l, rgb.w);
}

sayaka_float sayaka_getcolorlightness(sayaka_vec4 color) {
    sayaka_float cmax = max(max(color.x, color.y), color.z);
    sayaka_float cmin = min(min(color.x, color.y), color.z);
    return (cmax + cmin) / 2;
}

//overlays a color on top of another color, with the top color's alpha as the opacity
sayaka_vec4 sayaka_chain(sayaka_vec4 a, sayaka_vec4 b) {
    sayaka_vec4 ret = sayaka_blendcolor(a, b, 1.0);
    ret.w = max(a.w, b.w);
    return ret;
}

#endif //SAYAKA_ALREADY_HERE