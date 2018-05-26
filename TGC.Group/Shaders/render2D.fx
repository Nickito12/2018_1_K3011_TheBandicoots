
// Sharpen

float screen_dx;
float screen_dy;


texture g_RenderTarget;
sampler RenderTarget =
sampler_state
{
    Texture = <g_RenderTarget>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

static const int kernel_r = 3;
static const int kernel_size = 9;
static const float Kernel[kernel_size] =
{
    0, -1,0, 
    -1, 5,-1, 
    0, -1,0
};

void VSCopy(float4 vPos : POSITION, float2 vTex : TEXCOORD0, out float4 oPos : POSITION, out float2 oScreenPos : TEXCOORD0)
{
    oPos = vPos;
    oScreenPos = vTex;
    oPos.w = 1;
}

void Blur(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    Color = 0;
    for (int i = 0; i < kernel_r; ++i)
        for (int j = 0; j < kernel_r; ++j)
            Color += tex2D(RenderTarget, screen_pos + float2((float) (i) / screen_dx, (float) (j) / screen_dy)) * Kernel[i*kernel_r+j];
	Color = clamp(Color, 0, 1);
    Color.a = 1;
}

void PSCopy(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
	Color = tex2D(RenderTarget, screen_pos);
}

technique Sharpen
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 Blur();
    }
}

technique Copy
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSCopy();
    }
}