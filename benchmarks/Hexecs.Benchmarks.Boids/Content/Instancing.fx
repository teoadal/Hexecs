// Параметры от MonoGame
matrix Projection;

// Данные геометрии (один квадрат)
struct VertexInput {
    float4 Position : POSITION0;
};

// Данные инстанса (из твоего InstanceData)
struct InstanceInput {
    float4 PositionScale : POSITION1; // X, Y, Scale, Rotation
    float4 Color : COLOR0;
};

struct PixelInput {
    float4 Position : SV_Position;
    float4 Color : COLOR0;
};

PixelInput VertexShaderFunction(VertexInput v, InstanceInput i) {
    PixelInput output;
    
    // Масштабируем и позиционируем
    float2 worldPos = (v.Position.xy * i.PositionScale.z) + i.PositionScale.xy;
    
    // Умножаем на проекцию
    output.Position = mul(float4(worldPos, 0, 1), Projection);
    output.Color = i.Color;
    
    return output;
}

float4 PixelShaderFunction(PixelInput input) : COLOR0 {
    return input.Color;
}

technique Instancing {
    pass P0 {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}