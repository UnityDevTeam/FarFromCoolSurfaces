using UnityEngine;

//[ExecuteInEditMode]
public class MolScript : MonoBehaviour
{
	public int molCount = 100000;
	public Shader shader;
	
	private Material mat;
	
	private RenderTexture colorTexture;
	private RenderTexture colorTexture2;	
	
	private ComputeBuffer cbDrawArgs;
	private ComputeBuffer cbPoints;
	private ComputeBuffer cbMols;

	private RenderTexture[] mrtTex;
	private RenderBuffer[] mrtRB;
	
	private void CreateResources ()
	{
		if (cbDrawArgs == null)
		{
			cbDrawArgs = new ComputeBuffer (1, 16, ComputeBufferType.DrawIndirect);
			var args = new int[4];
			args[0] = 0;
			args[1] = 1;
			args[2] = 0;
			args[3] = 0;
			cbDrawArgs.SetData (args);
		}
		
		if (cbMols == null)
		{
			Vector4[] molPositions = new Vector4[molCount];
			
			for (var i=0; i < molCount; i++)
			{
				molPositions[i].Set((UnityEngine.Random.value - 0.5f) * 10.0f, 
				                    (UnityEngine.Random.value - 0.5f) * 10.0f,
				                    (UnityEngine.Random.value - 0.5f) * 10.0f,
				                    1);
			}
			
			cbMols = new ComputeBuffer (molPositions.Length, 16); 
			cbMols.SetData(molPositions);
		}
		
		if (cbPoints == null)
		{
			cbPoints = new ComputeBuffer (Screen.width * Screen.height, 16, ComputeBufferType.Append);
		}
		
		if (colorTexture == null)
		{
			colorTexture = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
			colorTexture.Create();
		}
		
		if (colorTexture2 == null)
		{
			colorTexture2 = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
			colorTexture2.Create();
		}

		if (this.mrtTex == null)
		{
			this.mrtTex  =   new RenderTexture[4];
			this.mrtRB    =   new RenderBuffer[4];

			this.mrtTex[0] = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
			this.mrtTex[1] = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
			this.mrtTex[2] = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
			this.mrtTex[3] = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);

			for( int i = 0; i < this.mrtTex.Length; i++ )
				this.mrtRB[i] = this.mrtTex[i].colorBuffer;

		}
		
		if (mat == null)
		{
			mat = new Material(shader);
			mat.hideFlags = HideFlags.HideAndDontSave;
		}
	}
	
	private void ReleaseResources ()
	{
		if (cbDrawArgs != null) cbDrawArgs.Release (); cbDrawArgs = null;
		if (cbPoints != null) cbPoints.Release(); cbPoints = null;
		if (cbMols != null) cbMols.Release(); cbMols = null;
		
		if (colorTexture != null) colorTexture.Release(); colorTexture = null;
		if (colorTexture2 != null) colorTexture2.Release(); colorTexture2 = null;	
		for( int i = 0; i < this.mrtTex.Length; i++ ) {
			if (mrtTex[i] != null) {mrtTex[i].Release(); mrtTex[i]=null;}
		}
		Object.DestroyImmediate (mat);
	}
	
	void OnDisable ()
	{
		ReleaseResources ();
	}
	
	void OnPostRender()
	{
		CreateResources ();
		
		Graphics.SetRenderTarget (colorTexture);
		GL.Clear (true, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));		
		mat.SetBuffer ("molPositions", cbMols);
		mat.SetPass(1);
		Graphics.DrawProcedural(MeshTopology.Points, molCount);

		Graphics.SetRandomWriteTarget (1, cbPoints);
		Graphics.Blit (colorTexture, colorTexture2, mat, 0);
		Graphics.ClearRandomWriteTargets ();		
		ComputeBuffer.CopyCount (cbPoints, cbDrawArgs, 0);
		/*
		RenderTexture.active = null;
		GL.Clear (true, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		mat.SetBuffer ("atomPositions", cbPoints);
		mat.SetPass(2);
		Graphics.DrawProceduralIndirect(MeshTopology.Points, cbDrawArgs);
		*/


		Graphics.SetRenderTarget (this.mrtTex[0]);
		GL.Clear (true, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		Graphics.SetRenderTarget (this.mrtTex[1]);
		GL.Clear (true, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		Graphics.SetRenderTarget (this.mrtTex[2]);
		GL.Clear (true, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		Graphics.SetRenderTarget (this.mrtTex[3]);
		GL.Clear (true, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		Graphics.SetRenderTarget (this.mrtRB,this.mrtTex[0].depthBuffer);
		mat.SetBuffer ("atomPositions", cbPoints);
		mat.SetPass(3);
		Graphics.DrawProceduralIndirect(MeshTopology.Points, cbDrawArgs);

	}

	void OnRenderImage (RenderTexture source, RenderTexture destination){
		//! iso-surface creation
		Graphics.Blit (this.mrtTex[0], destination);
		/*
		RenderTexture.active = null;
		mat.SetTexture("slab0", this.mrtTex[0]);
		mat.SetTexture("slab1", this.mrtTex[1]);
		mat.SetTexture("slab2", this.mrtTex[2]);
		mat.SetTexture("slab3", this.mrtTex[3]);
		Graphics.Blit (source, destination, mat, 4);
		*/

	}
}