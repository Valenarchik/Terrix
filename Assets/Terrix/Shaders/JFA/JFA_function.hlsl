//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

bool IsUVOutOfRange(float2 uv)
{
    return any(uv < 0) || any(uv > 1);
}

void JFA_float(float2 UV, UnityTexture2D Tex, float StepSize, out float4 Out)
{
    float bestDistance = 99999.0;
    float3 bestPos = float3(0, 0, 1);
    
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 UV_off = UV + float2(x, y) * StepSize;

            if (IsUVOutOfRange(UV_off))
            {
                continue;
            }

            
            float3 pos = tex2D(Tex, UV_off).xyz;

            if (pos.z == 1)
            {
                continue;
            }
            
            float dist = distance(pos.xy, UV);
            
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestPos = pos;
            }
        }
    }

    Out = float4(bestPos, 0);
}
#endif //MYHLSLINCLUDE_INCLUDED