Shader "Custom/VertexColour" {

	SubShader
	{
		//start 
		CGPROGRAM

		#pragma surface surf Lambert 

		struct Input
		{
			//input variable for the vertex colour - rgba so float4
			float4 vertColour : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			//set the mesh's albedo to be the vertex colour.
			o.Albedo = IN.vertColour; //.rgb;
		}
		//end
		ENDCG
	}

	FallBack "Diffuse"
}