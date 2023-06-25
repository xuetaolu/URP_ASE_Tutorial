using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[ExecuteAlways]
public class TimelineConfig : MonoBehaviour
{
    [Range(0f, 10f)]
    public float m_speed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        PlayableDirector playableDirector = GetComponent<PlayableDirector>();
        if (playableDirector != null)
        {
            var playableGraph = playableDirector.playableGraph;
            if (!playableGraph.IsValid())
            {
                playableDirector.RebuildGraph();
            }

            if (playableGraph.IsValid())
            {
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(m_speed);
            }
        }

         
    }
}
