#ifndef ECHO_PROTOCOL_INTRUDER_DREAD_SURFACE_INCLUDED
#define ECHO_PROTOCOL_INTRUDER_DREAD_SURFACE_INCLUDED

// Authoring in Nathan's original scan space (TEXCOORD2, metres), not world
// position or animated/deformed position. Surface regions follow both the
// restored R4 head and the asymmetric R5 sculpt without swimming over skin.
float DreadRegion(float3 p, float3 centre, float3 radius)
{
    float3 d = (p - centre) / radius;
    return exp(-2.0 * dot(d, d));
}

void ApplyIntruderDreadSurface(float3 p, float2 uv, float3 scan, float3 tint,
    float apron, float strength, inout float3 albedo, inout float3 normalTS,
    inout float smoothness)
{
    float luminance = dot(scan, float3(.2126, .7152, .0722));
    float front = smoothstep(.035, .085, p.z);
    float face = smoothstep(1.545, 1.605, p.y) * (1.0 - smoothstep(.12, .16, abs(p.x)));
    float hands = smoothstep(.746, .781, abs(p.x));
    float exposed = saturate(max(face, hands));

    // Dry, depleted skin still carries the scan's pores, stubble and variation.
    // Pressure blanching is broad across jaw/cheeks, not painted black sockets.
    float cheeks = saturate(DreadRegion(p, float3(-.050, 1.681, .104), float3(.033, .033, .031))
        + DreadRegion(p, float3(.047, 1.678, .107), float3(.029, .035, .030)));
    float jaw = DreadRegion(p, float3(.024, 1.636, .071), float3(.062, .040, .053));
    float forehead = DreadRegion(p, float3(0, 1.754, .100), float3(.054, .021, .029));
    float nose = DreadRegion(p, float3(0, 1.699, .133), float3(.016, .026, .028));
    float lips = DreadRegion(p, float3(0, 1.651, .115), float3(.031, .008, .018)) * front;
    float lipCorner = DreadRegion(p, float3(-.026, 1.651, .112), float3(.007, .013, .014));
    float lowerLids = saturate(
        DreadRegion(p, float3(-.030, 1.710, .103), float3(.024, .0035, .017))
        + DreadRegion(p, float3(.029, 1.710, .103), float3(.023, .0035, .017))) * front;
    float pigment = smoothstep(.025, .15, luminance);
    float3 flesh = lerp(scan, luminance * float3(1.18, 1.12, .98), .55 * pigment);
    // Residual warmth belongs to lips/ears; the cheeks and pressure on the jaw
    // are comparatively pale. No blood decal, arbitrary sore or scalp paint.
    flesh *= float3(.96, 1.015, 1.035);
    flesh = lerp(flesh, luminance * float3(1.24, 1.20, 1.07),
        saturate(cheeks * .40 + jaw * .25 + hands * .18) * pigment);
    flesh = lerp(flesh, scan * float3(.68, .63, .65), saturate(lips * .70));
    // The small damp mouth corner follows the compression; it is not a cut.
    flesh *= 1.0 - .17 * lipCorner;
    float hair = smoothstep(1.768, 1.797, p.y);
    flesh = lerp(flesh, scan * .83, hair);
    float skinGloss = .18 + .16 * forehead + .34 * nose + .55 * lips + .46 * lowerLids;
    skinGloss = lerp(skinGloss, .15, hair);

    // Heavy graphite work cloth: the licensed normal and luminance keep the
    // folds/seams. Saturation is tied to contact regions, not a noise wash.
    float sleeveEnd = DreadRegion(float3(abs(p.x), p.y, p.z),
        float3(.705, 1.525, -.016), float3(.080, .070, .075));
    float collar = DreadRegion(p, float3(0, 1.528, .034), float3(.123, .060, .125));
    float underarms = DreadRegion(float3(abs(p.x), p.y, p.z),
        float3(.222, 1.420, .005), float3(.090, .100, .120));
    float knees = DreadRegion(float3(abs(p.x), p.y, p.z),
        float3(.126, .485, .085), float3(.105, .130, .090));
    float hem = 1.0 - smoothstep(.10, .24, p.y);
    float wetCloth = saturate(collar * .85 + underarms * .70 + sleeveEnd * .85 + hem * .60);
    float3 cloth = lerp(float3(.055, .060, .055), float3(.074, .099, .108),
        smoothstep(.90, 1.10, p.y)) * (.48 + 1.15 * luminance);
    cloth *= 1.0 - .58 * wetCloth;
    cloth = lerp(cloth, float3(.102, .096, .077) * (.45 + luminance), knees * .28);
    float clothGloss = .13 + .29 * wetCloth;
    float3 result = lerp(cloth, flesh, exposed);
    float resultGloss = lerp(clothGloss, skinGloss, exposed);
    float3 resultNormal = normalTS;
    resultNormal.xy *= lerp(1.0, .55, saturate(nose + lips + lowerLids));

    if (apron > 0)
    {
        // Dark rubber replaces the old alternating ochre stripes. Edge wear is
        // attached to the actual bib's UV boundary; specular follows real folds.
        // Fine scuff variation is derivative filtered so it cannot sparkle.
        float wraps = smoothstep(.63, .66, abs(p.x));
        float straps = smoothstep(1.431, 1.450, p.y) * (1.0 - wraps);
        float edge = 1.0 - smoothstep(.012, .045, min(uv.x, 1.0 - uv.x));
        edge = max(edge, smoothstep(.91, .985, uv.y));
        float rub = DreadRegion(p, float3(.055, 1.10, .18), float3(.13, .26, .07));
        float waterLine = .81 + .045 * sin(p.x * 19.0 + .7);
        float soaked = 1.0 - smoothstep(waterLine - .055, waterLine + .065, p.y);
        float phase = p.y * 480.0 + p.x * 93.0;
        float scuff = sin(phase) * saturate(1.0 - fwidth(phase) * .5);
        float3 rubber = lerp(float3(.086, .079, .051), float3(.030, .035, .029), soaked * .82);
        rubber = lerp(rubber, float3(.155, .124, .065), saturate(edge * .64 + rub * .22));
        rubber *= 1.0 + scuff * .025;
        rubber = lerp(rubber, float3(.063, .059, .043), straps * .6);
        float rubberGloss = lerp(.34, .64, soaked) - edge * .18 - rub * .09;
        // Wrist bindings are worn canvas, never luminous white watch bands.
        float bindingEdge = smoothstep(.733, .752, abs(p.x));
        float3 binding = lerp(float3(.185, .172, .125), float3(.070, .073, .055), bindingEdge);
        result = lerp(rubber, binding, wraps);
        resultGloss = lerp(rubberGloss, .17, wraps);
        resultNormal = float3(0, 0, 1);
    }

    albedo = lerp(albedo, result * tint, saturate(strength));
    smoothness = lerp(smoothness, saturate(resultGloss), saturate(strength));
    normalTS = normalize(lerp(normalTS, resultNormal, saturate(strength)));
}
#endif
