using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPostProcessing : MonoBehaviour
{
   public Material PostProcessingMaterial;

	public bool usePostProcessing = false;


private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
	if (usePostProcessing && PostProcessingMaterial != null)
		{
			Graphics.Blit(source, destination, PostProcessingMaterial);
		}
	}
}
