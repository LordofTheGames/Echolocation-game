Shader "Echolocation/EcholocationProjector"
{
    // Properties of the shader that can be altered in the inspector window
    Properties
    {
        [Header(Textures)]
        _DotTex ("Dot Texture", 2D) = "white" {}                        // Variable for dot image (made in just white/transparent) - receives 2D, white is default
        _GridTex ("Grid Texture", 2D) = "white" {}                      // Variable for grid image
        _AlphaMask ("Spotlight Mask (Soft Circle)", 2D) = "white" {}    // "Spotlight" for lighting up sections of surfaces

        [Header(Settings)]
        [Toggle] _UseMesh ("Use Mesh Grid", Float) = 0                  // Toggle (appears as checkbox - check = 1/true, no check = 0/false) - defaults to dot mode
        // _Color ("Main Colour", Color) = (0,1,1,1)                       // Tint for applying to above image to choose colour (using american spelling :/), takes in RGBA colour - defaults to opaque cyan
        _GridTiling("Mesh Grid Density", Float) = 0.2                   // Controls grid density/size of squares, higher = more denser/smaller squares
        _GlobalVisibility ("Global Visibility", Float) = 1.0            // Starting visibility for each specific instance/pulse
        _Falloff("Depth Projection Limit (Grid only)", Float) = 10.0    // Limits how "far behind" the "window" for the mesh grid visualisation we will look for object surfaces to light up
        
        [Header(Colors)]
        _Color1 ("Color 1", Color) = (1, 0, 0, 1)
        _Color2 ("Color 2", Color) = (0, 1, 0, 1)
        _Color3 ("Color 3", Color) = (0, 0, 1, 1)
    }
    SubShader
    {
        // "Queue"="Transparent" - allows the transparent windows to work properly by rendering them last
        // "RenderType"="Transparent" - tells Unity what kind of object it is, so other effects/objects know how to interact wiht it/don't mess things up
        // "IgnoreProjector"="True" - protection against legacy unity projectors to stop them painting on top of this echolocation projector
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Cull Off // Stop Unity culling dots/visualisation when looking at them from the back

        // Do not save this object's depth to the depth buffer, since it is a window for projection and not an in game physical object
        ZWrite Off

        // Allows transparency to work properly
        // Essentially "mix the colour im painting now (the grid) with the colour that's already there on the screen (surface of an object), based on transparency
        // Final = (New * Alpha) + (Old * (1 - Alpha))
        Blend SrcAlpha OneMinusSrcAlpha

        // Default in shaders - means "Level Of Detail", higher number means more complex
        // Can be used by developer for example to stop rendering if a players computer is struggling
        LOD 100


        // Actual instruction steps - one pass per vertex in quad
        Pass
        {
            CGPROGRAM // Start of "C for Graphics Program"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing    // Tells the compiler to generate a special version of this shader that supports GPU Instancing - so that a window for each of the thousands of rays can by made efficiently in parallel
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"

            struct appdata // Data from 3D model file
            {
                float4 vertex : POSITION;       // Position of vertex, within/realtive to quad space
                float2 uv : TEXCOORD0;          // First UV coordinate
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f // (Vertex to Fragment) Data to be passed from this vertex shader to the pixel/fragment shader, TEXCOORD(0-2) are used as generic empty registers to pass data here
            {
                float4 vertex : SV_POSITION;    // Raw GPU coordinates (clip space: ranges from  -1 to 1), for the gpu to decide which and where pixels should be drawn on the screen
                float4 screenPos : TEXCOORD0;      // Like above vertex, it's the position of pixel on the screen, but will be converted to texture coordinates (0 to 1), to be compatible with the depth texture to find how far past "windows" surfaces of objects are
                float3 worldPos : TEXCOORD1;    // 3D coordinate of quad in the game world
                float2 uv : TEXCOORD2;          // UVs from the mesh grid/dot texture to draw on quad surface
                float randomVal : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D_float _CameraDepthTexture;    // Depth texture for working out how far away things are
            sampler2D _DotTex;
            float4 _DotTex_ST;                      // Automatically set by Unity with dot tiling/offset
            sampler2D _GridTex;
            float4 _GridTex_ST;                     // Automatically set by Unity with grid tiling/offset
            sampler2D _AlphaMask;
            float _GridTiling;
            float4 _Color;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float _UseMesh;
            float _Falloff;
            float _GlobalVisibility;

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED          // Check the machine user is on is capable of GPU instancing
                StructuredBuffer<float4x4> _InstanceMatrices;   // A list containing the position, rotation, and scale of every raycast hit
            #endif

            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED 
                #endif
            }


            // Vertex shader
            v2f vert (appdata v, uint instanceID : SV_InstanceID) // intanceID  is the ID for each instance of the quad mesh/dot - for looking up the correct position in _InstanceMatrices
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); // Ensures that any built-in Unity instancing logic is correctly initialised for this vertex
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    unity_ObjectToWorld = _InstanceMatrices[instanceID]; // Get correct matrix, unity_ObjectToWorld is a built-in variable with lots of pre-made funcitonality
                #endif

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);   // Convert local quad space to world space
                o.worldPos = worldPos.xyz;
                o.vertex = mul(UNITY_MATRIX_VP, worldPos);              // Convert world space to clip space
                o.screenPos = ComputeScreenPos(o.vertex);                  // Calculate screen position for depth lookup

                float4 activeST = lerp(_DotTex_ST, _GridTex_ST, _UseMesh);  // Get tiling/offset depending on mode
                o.uv = v.uv * activeST.xy + activeST.zw;                    // Apply any tiling/offset - we won't use but is good practice

                // Random logic (for picking colour)
                // Get the "Origin" of this specific square instance
                float3 instanceOrigin = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
                // Generate a "Hash" (Random number 0.0 to 1.0) based on that position
                o.randomVal = frac(sin(dot(instanceOrigin, float3(12.9898, 78.233, 45.5432))) * 43758.5453);

                return o;
            }

            // Pixel/fragment shader
            // fixed4 means it returns a colour, SV_TARGET tells Unity to paint this colour on the screen
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;

                if (_UseMesh > 0.5) // Use greater than for safety to avoid errors due to floats, e.g. when it should be 0.0, it's actually 0.000001 and is interpreted as true incorrectly
                {
                    // Step 1: read the depth buffer

                    float2 screenUV = i.screenPos.xy / i.screenPos.w;                                 // Get screen position and divide by w to account for perspective - screenUV is coord (0,0) bttom left to (1,1) top right
                    float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);       // Use that position to look up depthe texture (1 is close, 0 is far - is non-linear)
                    float linearDepth = LinearEyeDepth(rawDepth);                               // Convert from non-linear to "real-world" units, e.g. linearDepth = 10, means 10 metres from camera

                    // Step 2: get word position of surface behind the "window"

                    float3 viewRay = i.worldPos - _WorldSpaceCameraPos;                         // Calculate vector from the camera to the quad's flat surface 
                    float3 viewDir = normalize(viewRay);                                        // Normalise to get just the direction

                    float3 cameraForward = -float3(UNITY_MATRIX_V._m20, UNITY_MATRIX_V._m21, UNITY_MATRIX_V._m22);  // Calculate camera forward vector
                    float rayLength = linearDepth / dot(viewDir, cameraForward);                                    // Adjust depth to account for ray angle to prevent "swimming" on screen edges

                    float3 geometryWorldPos = _WorldSpaceCameraPos + (viewDir * rayLength);     // Calulate position of geometry/object behind this pixel - start from the camera and move along the direction to the quad for the distance to the object behind it

                    // Step 3: calculate normal on surface of the object

                    float3 ddxPos = ddx(geometryWorldPos);                                      // How much does the world position change if I move 1 pixel to the right
                    float3 ddyPos = ddy(geometryWorldPos);                                      // How much does the world position change if I move 1 pixel down
                    float3 pixelNormal = normalize(cross(ddxPos, ddyPos));                      // Cross product of right and down gives the forward vector (the normal)

                    // Step 4: restrict by depth limit (falloff)

                    float dist = distance(geometryWorldPos, i.worldPos);                        // Calculate distacne from the quad/window to the surface behind it
                    float depthAlpha = 1.0 - smoothstep(0.0, _Falloff, dist);                   // Smoothstep(min, max, value) returns value between 0.0 (at/below min) and 1.0 (at/above max), and inverse for a fading effect further from the window
                    if (depthAlpha <= 0.01) discard;                                            // If pixel is invisible stop calculating immediately to save/optimise GPU power

                    // Using Triplanar Mapping for mesh grid

                    float3 weights = abs(pixelNormal);                          // Find which "direction" (front, side, or top) has the most impact
                    weights = pow(weights, 8.0);                                // Male the blending of gridlines "sharp" - higher power = sharper transition
                    weights = weights / (weights.x + weights.y + weights.z);    // Normalise weights so they sum to 1

                    // Project the grid texture from front (xy), top (xz), and side (zy) 
                    // Multiply by _GridTiling to scale the squares up/down
                    float2 uvFront = geometryWorldPos.xy * _GridTiling;
                    float2 uvTop = geometryWorldPos.xz * _GridTiling;
                    float2 uvSide = geometryWorldPos.zy * _GridTiling;

                    // Sample the texture for each
                    fixed4 colFront = tex2D(_GridTex, uvFront);
                    fixed4 colTop = tex2D(_GridTex, uvTop);
                    fixed4 colSide = tex2D(_GridTex, uvSide);

                    // Combine the three samples using the weights  
                    col = colSide * weights.x + colTop * weights.y + colFront * weights.z;

                    // Apply spot mask
                    float spotMask = tex2D(_AlphaMask, i.uv).a;         // Read the soft circle mask texture to trim the square edges of the quad
                    col *= _Color;                                      // Apply colour/tint
                    col.a *= depthAlpha * spotMask * _GlobalVisibility; // Combine all effects - including fading effect
                }
                else
                {
                    // // Using normal Flat Mapping for dots mode
                    // col = tex2D(_DotTex, i.uv);     // Use Quad's standard UV to keep dot perfectly round in the centre
                    // col *= _Color;                  // Apply colour/tint

                    if (i.randomVal < 0.33)
                        col = _Color1;
                    else if (i.randomVal < 0.66)
                        col = _Color2;
                    else
                        col = _Color3;
                        
                    col.a *= _GlobalVisibility;     // Apply fading effect
                }

                return col;
            }
            ENDCG // End of "C for Graphics Program"
        }
    }
}
