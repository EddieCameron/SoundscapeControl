using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundscapeControl : MonoBehaviour 
{	
	[SerializeField]
	public AudioGroup[] audioGroups;
	
	string currentGroup;
	
	public float fadeTime = 1f;
	
	Dictionary<AudioSource, float> curFading;
	
	static SoundscapeControl instance;
	
	void Awake()
	{
		if ( instance != null )
			Destroy ( this );
		else
			instance = this;
	}
	
	void Start()
	{
		// setup audiogroups
		foreach( AudioGroup audioGroup in audioGroups )
		{
			audioGroup.targVols = new float[audioGroup.sources.Length];
			for( int i = 0; i < audioGroup.targVols.Length; i++ )
			{
				audioGroup.targVols[i] = audioGroup.sources[i].volume;
				audioGroup.sources[i].volume = 0;
			}
		}
		
		curFading = new Dictionary<AudioSource, float>();
		
		// by default, fades in the first group at launch
		if ( audioGroups.Length > 0 )
			FadeInGroup( audioGroups[0] );
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
	
	void FadeInGroup( AudioGroup audioGroup )
	{
		for ( int i = 0; i < audioGroup.sources.Length; i++ )
		{
			AudioSource source = audioGroup.sources[i];
			if ( source )
			{
				if ( curFading.ContainsKey ( source ) )
					curFading[source] = audioGroup.targVols[i];
				else
				{
					curFading.Add ( source, audioGroup.targVols[i] );
					StartCoroutine ( Fade( source ) );
				}
			}
		}
	}
	
	void FadeOutGroup( AudioGroup audioGroup )
	{
		for ( int i = 0; i < audioGroup.sources.Length; i++ )
		{
			AudioSource source = audioGroup.sources[i];
			if ( source )
			{
				if ( curFading.ContainsKey ( source ) )
					curFading[source] = 0f;
				else
				{
					curFading.Add ( source, 0 );
					StartCoroutine ( Fade( source ) );
				}
			}
		}
	}	
	
	IEnumerator Fade( AudioSource toFade )
	{
		if ( !curFading.ContainsKey( toFade ) )
			yield break;
		
		if ( !toFade.isPlaying && curFading[toFade] > 0 )
			toFade.Play ();
		
		float startVolume = toFade.volume;
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
			
			toFade.volume = Mathf.Lerp ( startVolume, targVolume, fadeAmount );
			fadeAmount += Time.deltaTime / fadeTime;
			Debug.Log ( toFade.name + " " + fadeAmount + " " + toFade.volume );
			yield return 0;
		}
		
		toFade.volume = curFading[toFade];
		
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
}