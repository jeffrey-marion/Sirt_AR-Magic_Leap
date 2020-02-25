Shader "Unlit/PortalShader"
{
SubShader
	{
		ZWrite off
		ColorMask 0

		Stencil{
		Ref 1 
		Pass replace

		}


		Pass
		{
			
		}
	}
}
