using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundscapeControl : MonoBehaviour 
{	
	[SerializeField]
	public AudioGroup[] audioGroups;
	
	public float fadeTime = 1f;
	float masterVol = 1f;
	
	Dictionary<AudioSource, float> curFading;
	
	static SoundscapeControl instance;
	
	void Awake()
	{
		if ( instance != null )
			Destroy ( this );
		else
		{
			instance = this;
	
			curFading = new Dictionary<AudioSource, float>();
		
			// setup audiogroups
			foreach( AudioGroup audioGroup in audioGroups )
			{
				audioGroup.targVols = new float[audioGroup.sources.Length];
				audioGroup.curVolumes = new float[audioGroup.sources.Length];
				for( int i = 0; i < audioGroup.targVols.Length; i++ )
				{
					audioGroup.targVols[i] = audioGroup.sources[i].volume;
					audioGroup.sources[i].volume = 0;
					audioGroup.curVolumes[i] = 0;
				}
			}
		}
	}
	
	void Start()
	{		
		// by default, fades in the first group at launch
		if ( audioGroups.Length > 0 )
			FadeInGroup( audioGroups[0] );
	}
	
	public static void SetMasterVolume( float newVol )
	{
		instance.masterVol = Mathf.Clamp01 ( newVol );
		
		foreach( AudioGroup group in instance.audioGroups )
		{
			for( int i = 0; i < group.sources.Length; i++ )
				group.sources[i].volume = group.curVolumes[i] * instance.masterVol;
		}
	}
	
	public static void FadeInGroup( string groupName )
	{
		foreach( AudioGroup audioGroup in instance.audioGroups )
		{
			if ( audioGroup.groupName == groupName )
			{
				instance.FadeInGroup( audioGroup );
				return;
			}
		}
		
		Debug.LogWarning ( "No audiogroup by name " + groupName );
	}
	
	public static void FadeOutGroup( string groupName )
	{
		foreach( AudioGroup audioGroup in instance.audioGroups )
		{
			if ( audioGroup.groupName == groupName )
			{
				instance.FadeOutGroup( audioGroup );
				return;
			}
		}
		
		Debug.LogWarning ( "No audiogroup by name " + groupName );
	}
	
	public static void FadeSourceTo( AudioSource source, float toVolume )
	{
		instance.FadeSource ( source, toVolume );
	}
	
	void FadeInGroup( AudioGroup audioGroup )
	{
		for ( int i = 0; i < audioGroup.sources.Length; i++ )
		{
			AudioSource source = audioGroup.sources[i];
			if ( source && i < audioGroup.targVols.Length )
				FadeSource ( source, audioGroup.targVols[i], audioGroup, i );
			else
				Debug.LogWarning ( "Audiogroup " + audioGroup.groupName + " isn't set up properly" );
		}
	}
	
	void FadeOutGroup( AudioGroup audioGroup )
	{
		for ( int i = 0; i < audioGroup.sources.Length; i++ )
		{
			AudioSource source = audioGroup.sources[i];
			if ( source )
				FadeSource ( source, 0, audioGroup, i );
		}
	}	
	
	void FadeSource( AudioSource toFade, float toVolume, AudioGroup group = null, int groupIndex = -1 )
	{
		if ( curFading.ContainsKey ( toFade ) )
			curFading[toFade] = toVolume;
		else
		{
			curFading.Add ( toFade, toVolume );
			StartCoroutine ( Fade( toFade, group, groupIndex ) );
		}
	}
	
	IEnumerator Fade( AudioSource toFade, AudioGroup group = null, int groupIndex = -1 )
	{
		if ( !curFading.ContainsKey( toFade ) )
			yield break;
		
		if ( !toFade.isPlaying && curFading[toFade] > 0 )
			toFade.Play ();
		
		float startVolume = group != null ? group.curVolumes[groupIndex] : toFade.volume;
		float targVolume = curFading[toFade];
		float fadeAmount = 0f;
		
		while ( fadeAmount < 1f )
		{
			if ( !curFading.ContainsKey( toFade ) )
				yield break;
			
			// if audio is refaded, restart the fade from current volume
			if ( targVolume != curFading[toFade] )
			{
				StartCoroutine ( Fade ( toFade ) );
				yield break;
			}
			
			float newVol = Mathf.Lerp ( startVolume, targVolume, fadeAmount );
			if ( group != null )
			{
				group.curVolumes[groupIndex] = newVol;
				newVol *= masterVol;
			}
		
			toFade.volume = newVol;		
			fadeAmount += Time.deltaTime / fadeTime;

			yield return 0;
		}
		
		if ( group != null )
			group.curVolumes[groupIndex] = curFading[toFade];
		
		toFade.volume = curFading[toFade] * ( group != null ? masterVol : 1f );
		
		if ( curFading[toFade] <= 0 )
			toFade.Pause ();
		
		curFading.Remove ( toFade );
	}
}

[System.Serializable]
public class AudioGroup
{
	
	public string groupName;
	public AudioSource[] sources;
	
	[HideInInspector]
	public float[] targVols;	// will be allocated at launch from current volume of audiosources
	
	[HideInInspector]
	public float[] curVolumes;
}