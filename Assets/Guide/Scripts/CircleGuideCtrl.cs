/****************************************************************************
 * Copyright (c) 2017 liuzhenhua@putao.com
 ****************************************************************************/
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.Guide
{
	/// <summary>
	/// 圆形遮罩引导
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class CircleGuideCtrl : MonoBehaviour, ICanvasRaycastFilter
	{
		[Header("高亮目标")] public Image Target;
		[Header("是否播放动画")]public bool ShowAnim;
		[Header("收缩时间")] public float ShrinkTime = 0.5f;

		private Canvas _canvas;
		private Material _material;
		/// <summary>
		/// 区域范围缓存
		/// </summary>
		private Vector3[] _corners = new Vector3[4];
		/// <summary>
		/// 镂空区域半径
		/// </summary>
		private float _radius;
		/// <summary>
		/// 当前高亮区域的半径
		/// </summary>
		private float _currentRadius = 0f;


		void Awake()
		{
			//获取画布
			_canvas = GameObject.FindObjectOfType<Canvas>();
			if (_canvas == null)
			{
				Debug.LogError("There is not a Canvas!");
			}
			//材质初始化
			_material = new Material(Shader.Find("UI/Guide/CircleGuide"));
			GetComponent<Image>().material = _material;
			InitData();
		}

		void Update()
		{
			PlayShrinkAnim();
		}
		
		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			if (Target == null)
				return true;

			return !RectTransformUtility.RectangleContainsScreenPoint(Target.rectTransform, sp, eventCamera);
		}

		private void InitData()
		{
			Target.rectTransform.GetWorldCorners(_corners);
			//获取最终高亮区域半径
			_radius = Vector2.Distance(World2CanvasPos(_canvas, _corners[0]), World2CanvasPos(_canvas, _corners[3])) / 2f;
			//计算圆心
			float x = _corners[0].x + (_corners[3].x - _corners[0].x) / 2f;
			float y = _corners[0].y + (_corners[1].y - _corners[0].y) / 2f;
			Vector3 centerWorld = new Vector3(x, y, 0);
			Vector2 center = World2CanvasPos(_canvas, centerWorld);
			//Apply 设置数据到shader中
			Vector4 centerMat = new Vector4(center.x, center.y, 0, 0);
			_material.SetVector("_Center", centerMat);
			//计算当前高亮显示区域半径
			RectTransform canvasRectTransform = _canvas.transform as RectTransform;
			canvasRectTransform.GetWorldCorners(_corners);
			foreach (Vector3 corner in _corners)
			{
				_currentRadius = Mathf.Max(Vector3.Distance(World2CanvasPos(_canvas, corner), corner), _currentRadius);
			}
			float initRadius = ShowAnim ? _currentRadius : _radius;
			_material.SetFloat("_Slider", initRadius);
		}

		private float _shrinkVelocity = 0f;
		private void PlayShrinkAnim()
		{
			if (!ShowAnim)
				return;
			float value = Mathf.SmoothDamp(_currentRadius, _radius, ref _shrinkVelocity, ShrinkTime);
			if (!Mathf.Approximately(value, _currentRadius))
			{
				_currentRadius = value;
				_material.SetFloat("_Slider", _currentRadius);
			}
		}

		/// <summary>
		/// 世界坐标向画布坐标转换
		/// </summary>
		/// <param name="canvas">画布</param>
		/// <param name="world">世界坐标</param>
		/// <returns>返回画布上的二维坐标</returns>
		private Vector2 World2CanvasPos(Canvas canvas, Vector3 worldPos)
		{
			Vector2 position;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
				worldPos, canvas.GetComponent<Camera>(), out position);
			return position;
		}
	}
}
