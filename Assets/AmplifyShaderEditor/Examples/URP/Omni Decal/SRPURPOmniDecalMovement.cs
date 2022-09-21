using UnityEngine;

[ExecuteInEditMode]
public class SRPURPOmniDecalMovement : MonoBehaviour
{
	private Transform m_transform;
	private Vector3 m_speed = new Vector3( 1 , 0 , 0 );
	private void Awake()
	{
		m_transform = transform;
	}

	void Update()
	{
		m_transform.position += Time.deltaTime * m_speed;
		if( Mathf.Abs( m_transform.position.x ) > 2 )
			m_speed = -m_speed;
	}
}
