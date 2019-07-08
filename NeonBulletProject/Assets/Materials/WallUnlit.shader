Shader "Unlit/WallUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Size ("Size", float) = 1
		_Resolution ("Resolution", Vector) = (1280.0, 720.0, 0.0, 0.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Size;
			float4 _Resolution;
			static const float PI = 3.14159265f;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			float random(float2 st) {
				return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
			}

			float noise(float2 p) {
				float2 ip = floor(p);
				float2 u = frac(p);
				u = u * u * (3.0 - 2.0 * u);

				float res = lerp(
					lerp(random(ip), random(ip + float2(1.0, 0.0)), u.x),
					lerp(random(ip + float2(0.0, 1.0)), random(ip + float2(1.0, 1.0)), u.x), u.y);
				return res * res;
			}

			float randomRange(in float2 seed, in float min, in float max) {
				return min + random(seed) * (max - min);
			}

			float2 rotate2D(float2 position, float theta)
			{
				float2x2 m = float2x2(cos(theta), -sin(theta), sin(theta), cos(theta));
				return mul(position, m);
			}
			
			float3 hsv2rgb(float3 c) {
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

			//returns 1 for inside circ, 0 for outside
			float circle(in float2 _st, in float2 pos, in float _radius) {

				float circEdge = 20.0 / 1200.0; // todo iResolution.x

				float2 dist = _st - pos;
				return 1. - smoothstep(_radius - (_radius * circEdge),
					_radius + (_radius * circEdge),
					dot(dist, dist) * 4.0);
			}

			float3 gilmoreCol(float x) {
				//offset hue to put red in middle
				float hue = frac((1.0 - x) - 0.45);
				//saturation is higher for warmer colors
				float sat = 0.3 + sin(x * PI) * 0.5;
				//brightness higher in middle
				float bri = (smoothstep(0., 0.6, x) - smoothstep(0.6, 1.0, x)) * .6 + 0.3;
				return float3(hue, sat, bri);
			}

			fixed4 frag(v2f i) : SV_Target
			{

				float3 pos = mul(unity_ObjectToWorld, i.vertex);
				// sample the texture
				float4 col = 0;
				float2 uv = i.uv * 4.2; //_Size;
				
				col.r = uv.y / 5.0;
				col.g = uv.y/1.5;

				float stripeHeight = 0.125;

				/*
				if (i.uv.y < stripeHeight) {
					col.rgb = float3(0.568, 0.094, 0.439);
				} else if (i.uv.y < stripeHeight * 2.0) {
					col.rgb = float3(0.7568, 0.239, 0.6392);
				} else if (i.uv.y < stripeHeight * 3.0) {
					col.rgb = float3(0.49, 0.47, 0.62);
				} else if (i.uv.y < stripeHeight * 4.0) {
					col.rgb = float3(0.28, 0.64, 0.59);
				} else if (i.uv.y < stripeHeight * 5.0) {
					col.rgb = float3(0.10, 0.76, 0.52);
				} else if (i.uv.y < stripeHeight * 6.0) {
					col.rgb = float3(1.0, 0.0, 0.0);
				} else if (i.uv.y < stripeHeight * 6.0) {
					col.rgb = float3(0.12, 0.47, 0.37);
				} else if (i.uv.y < stripeHeight * 6.0) {
					col.rgb = float3(0.10, 0.23, 0.22);
				}
				*/

				if (i.uv.y < stripeHeight) {
					col.rgb *= 0.6;
				} else if (i.uv.y < (stripeHeight * 2.0) + noise(uv + float2(_Time.x + 0.1, _Time.y)) / 10.0) {
					col.rgb *= 0.6;
				} else if (i.uv.y < stripeHeight * 3.0 - noise(uv+float2(_Time.x, _Time.y)) / 10.0) { // - noise(uv) / 20.0
					col.rgb *= 0.7;
				} else if (i.uv.y < stripeHeight * 4.0 - noise(uv + float2(_Time.x, _Time.y)) / 10.0) {
					col.rgb *= 0.7;
				} else if (i.uv.y < (sin(10.0*_Time.x)/10.0)*(stripeHeight * 5.0) + noise(uv + float2(_Time.x+0.1, _Time.y)) / 10.0) {
					col.rgb *= 0.8;
				} else if (i.uv.y < stripeHeight * 6.0 ) {
					col.rgb *= 0.8;
				} else if (i.uv.y < stripeHeight * 6.0 - noise(uv + float2(_Time.x, _Time.y)) / 10.0) {
					col.rgb *= 0.8;
				} else if (i.uv.y < stripeHeight * 6.0 + noise(uv + float2(_Time.x, _Time.y)) / 10.0) {
					col.rgb *= 0.8;
				}

				// col.b = (1.0 - uv.y*20.0);
				

				float2 uvs = float2(i.vertex.xy - 0.5 * _Resolution.xy) / min(_Resolution.x, _Resolution.y);
				float2 tileDims = float2(0.5, 0.5);

				uvs = rotate2D(uvs, cos(_Time.y / 30.));

				float colId = floor(uvs.x * tileDims.x);

				float rndColumn = random(float2(colId, 687.890));
				uvs.y += _Time.y * (rndColumn) / 40.;
				
				float rnd = random(floor(uvs.xy * tileDims) + floor(_Time.y / 20.0));

				//mostly green w/ some reds
				float3 tileHSV;
				if (rnd < 0.9) {
					tileHSV = gilmoreCol(rnd / 2.6);
				}
				else {
					tileHSV = gilmoreCol(rnd - 0.4);
				}
				
				//get random int 0 - 3 per tile
				float tileRnd = random(floor(uvs.xy * tileDims) * 88.89);
				tileRnd = floor(tileRnd * 4.);

				//st is 0-1 coords within tile 
				float2 st = frac(uvs * tileDims);

				//flip tiles
				if (tileRnd == 1.) {
					st.y = 1.0 - st.y;
				}
				else if (tileRnd == 2.) {
					st.x = 1.0 - st.x;
				}
				else if (tileRnd == 3.) {
					st.x = 1.0 - st.x;
					st.y = 1.0 - st.y;
				}

				//draw circles
				float circ = circle(st, float2(0,0), 4.);
				tileHSV.z *= circ;

				//column shadows
				float hShadow = smoothstep(0.4, 0., frac(-uvs.x * 1200.0)) * 0.12;
				tileHSV.z -= hShadow;

				//slight vertical hue shift
				float vShift = smoothstep(0.9, 0., st.y) * 0.03;
				tileHSV.x -= vShift;

				//screen vertical brightness gradient
				tileHSV.z -= frac(1.0 - uv.y) * 0.3;


				
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
				float3 ccc = lerp(hsv2rgb(tileHSV), col, 0.98);
				//return float4(hsv2rgb(col), 1.0);
				return float4(ccc, 1.0);
            }
            ENDCG
        }
    }
}
