﻿using UnityEngine;

namespace Umbrella.Utility
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraSizeAdjuster : MonoBehaviour
    {
        [SerializeField] private Vector2Int _baseAspectRatio = new Vector2Int(16, 9);
        [SerializeField] private float _baseCameraSize = 5;
        [SerializeField, Range(1, 179)] private float _baseCameraFOV = 60;
        [SerializeField] private bool _showBaseAspectArea = false;
        [SerializeField] private Color _baseAspectAreaColor = new Color(0, 1, 0, 0.3f);

        private Camera _camera;

        private float BaseAspect => _baseAspectRatio.x / (float)_baseAspectRatio.y;

        private Material _baseAspectAreaMat;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            AdjustCameraSize();
        }

#if UNITY_EDITOR
        private void Update()
        {
            //AdjustCameraSize();
        }
#endif

        private void AdjustCameraSize()
        {
            //AdjustViewportRect();
            if (_camera.orthographic)
            {
                AdjustOrthographicCameraSize();
            }
            else
            {
                AdjustPerspectiveCameraSize();
            }
        }

        private void AdjustOrthographicCameraSize()
        {
            var baseHorizontalSize = _baseCameraSize * BaseAspect;
            var verticalSize = baseHorizontalSize / _camera.aspect;

            Debug.Log($"Base Aspect: {BaseAspect}, Camera Aspect: {_camera.aspect}");

            if (_camera.aspect < BaseAspect)
            {
                // レターボクシング
                _camera.orthographicSize = verticalSize;
            }
            else
            {
                // ピラーボクシング
                _camera.orthographicSize = _baseCameraSize;
            }
        }


        private void AdjustPerspectiveCameraSize()
        {
            if (_camera.aspect < BaseAspect)
            {
                // letterboxing
                var baseVerticalSize = Mathf.Tan(_baseCameraFOV * 0.5f * Mathf.Deg2Rad);
                var baseHorizontalSize = baseVerticalSize * BaseAspect;
                var verticalSize = baseHorizontalSize / _camera.aspect;
                var verticalFov = Mathf.Atan(verticalSize) * Mathf.Rad2Deg * 2;
                _camera.fieldOfView = verticalFov;
            }
            else
            {
                // pillarboxing
                _camera.fieldOfView = _baseCameraFOV;
            }
        }

        private void OnPostRender()
        {
            if (!_showBaseAspectArea)
            {
                return;
            }

            if (_baseAspectAreaMat == null)
            {
                _baseAspectAreaMat = new Material(Shader.Find("Hidden/Internal-Colored"));
                _baseAspectAreaMat.hideFlags = HideFlags.HideAndDontSave;
            }

            GL.PushMatrix();
            GL.LoadOrtho();

            _baseAspectAreaMat.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.Color(_baseAspectAreaColor);
            var viewport = GetBaseAspectViewportRect();
            GL.Vertex3(viewport.xMin, viewport.yMin, 0);
            GL.Vertex3(viewport.xMax, viewport.yMin, 0);
            GL.Vertex3(viewport.xMax, viewport.yMax, 0);
            GL.Vertex3(viewport.xMin, viewport.yMax, 0);
            GL.End();

            GL.PopMatrix();
        }

        public Rect GetBaseAspectViewportRect()
        {
            if (_camera.aspect < BaseAspect)
            {
                // letterboxing
                var height = Screen.width / BaseAspect;
                var y = (Screen.height - height) * 0.5f;
                return new Rect(0, y / Screen.height, 1, height / Screen.height);
            }
            else
            {
                // pillarboxing
                var width = Screen.height * BaseAspect;
                var x = (Screen.width - width) * 0.5f;
                return new Rect(x / Screen.width, 0, width / Screen.width, 1);
            }
        }
        private void AdjustViewportRect()
        {
            float windowAspect = (float)Screen.width / (float)Screen.height;
            float scaleHeight = windowAspect / BaseAspect;
            float scaleWidth = BaseAspect / windowAspect;

            if (scaleHeight < 1.0f)
            {
                // レターボクシング（上下に黒いバー）
                _camera.rect = new Rect(0, (1.0f - scaleHeight) / 2.0f, 1.0f, scaleHeight);
            }
            else
            {
                // ピラーボクシング（左右に黒いバー）
                _camera.rect = new Rect((1.0f - scaleWidth) / 2.0f, 0, scaleWidth, 1.0f);
            }
        }

    }
}