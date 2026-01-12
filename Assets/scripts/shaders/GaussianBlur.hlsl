void BlurUV_float(
    float2 UV,
    float Spread,      // max offset in UV units
    float GridSize,    // number of samples
    out float2 OutUV
)
{
    float2 offset = float2(0.0, 0.0);

    int radius = int(GridSize);

    for (int i = -radius; i <= radius; i++)
    {
        float weight = exp(-(i*i) / (2.0 * Spread * Spread));
        // add random-ish offset in UV space
        offset += float2(i, i) * weight / GridSize; 
    }

    OutUV = UV + offset * Spread;
}
