using UnityEngine;

public class LineRendererSinWave : MonoBehaviour
{
	[SerializeField]
	private	float			start = 0;			// ���� ���� x ��ġ
	[SerializeField]
	private	float			end = 5;			// ���� �� x ��ġ
	[SerializeField][Range(5, 50)]
	private	int				points = 5;			// ���� ���� (�������� �ε巯�� � ǥ��)
	[SerializeField][Min(0.1f)]
	private	float			amplitude = 1;		// ���� (Sin �׷����� y�� ����)
	[SerializeField][Min(0.5f)]
	private	float			frequency = 1;		// ������

	private	LineRenderer	lineRenderer;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		Play();
	}

	private void Play()
	{
		lineRenderer.positionCount = points;

		for ( int i = 0; i < points; ++ i )
		{
			// i�� 0.0-1.0 ������ ������ ����ȭ
			float t = (float)i / (points - 1);
			// start���� end ��ġ���� points ������ ���� �����ϰ� ��ġ
			float x = Mathf.Lerp(start, end, t);
			// 2*Mathf.PI = 360�̰�, t�� 0.0~1.0 ������ ���̱� ������ �� ���� ���ϸ� 1 ������ ���� �׷����� �ϼ��ǰ�,
			// frequency�� ���ϱ� ������ frequency ���� ���� �������� �����ȴ�.
			float y = amplitude * Mathf.Sin(2 * Mathf.PI * t * frequency);
			
			lineRenderer.SetPosition(i, new Vector3(x, y, 0));
		}
	}
}

