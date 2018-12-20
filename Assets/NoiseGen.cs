using UnityEngine;
using System.Collections;

public class NoiseGen
{
	private int octaves;
	private  float lacunarity;
	private float gain;
	private float perlinScale;

	public NoiseGen()
    {
    }

	public NoiseGen(int octaves, float lacunarity, float gain, float perlinScale)
	{
		this.octaves = octaves;
		this.lacunarity = lacunarity;
		this.gain = gain;
		this.perlinScale = perlinScale;
	}

	public float GetPerlinNoise(float x, float z)
	{
		return (2 * Mathf.PerlinNoise (x, z) - 1);
	}

	public float GetFractalNoise(float x, float z)
	{
		float fractalNoise = 0;

		float frequency = 1;
		float amplitude = 1;

		for (int i=0; i < octaves; i++)
		{
			float xVal = x * frequency * perlinScale;
			float zVal = z * frequency * perlinScale;

			fractalNoise += amplitude * GetPerlinNoise (xVal, zVal);

			frequency *= lacunarity;
			amplitude *= gain;
		}

		return fractalNoise;
	}
}
