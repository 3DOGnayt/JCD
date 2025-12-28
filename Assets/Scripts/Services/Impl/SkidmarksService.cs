using System.Collections.Generic;
using Configs.Impl;
using Data;
using Services;
using UnityEngine;
using UnityEngine.Rendering;

public class SkidmarksService : ISkidmarksService
{
    private readonly SkidmarksParameters _parameters;
    private readonly Mesh _mesh;
    private readonly MeshFilter _meshFilter;
    private readonly MeshRenderer _meshRenderer;

    private class WheelState
    {
        public int LastIndex = -1;
        public float Alpha;
        public bool WasSkidding;

        public Vector3 LastGroundPos;
        public bool HasLastGroundPos;
    }

    private readonly Dictionary<WheelCollider, WheelState> _wheelStates = new();

    private class MarkSection
    {
        public Vector3 Pos;
        public Vector3 Normal;
        public Vector4 Tangent;
        public Vector3 PosL;
        public Vector3 PosR;
        public Color32 Colour;
        public int LastIndex;
    }

    private readonly int _maxMarks;
    private readonly float _markWidth;
    private readonly float _groundOffset;
    private readonly float _minSqrDistance;
    private readonly float _maxOpacity;

    private readonly MarkSection[] _marks;
    private int _markIndex;

    private readonly Vector3[] _vertices;
    private readonly Vector3[] _normals;
    private readonly Vector4[] _tangents;
    private readonly Color32[] _colors;
    private readonly Vector2[] _uvs;
    private readonly int[] _triangles;

    private bool _meshDirty;
    private bool _boundsSet;

    private readonly Color32 _baseColor = Color.black;

    public SkidmarksService(SkidmarksParameters parameters)
    {
        _parameters = parameters;

        _maxMarks = Mathf.Max(16, _parameters.MaxMarks);
        _markWidth = _parameters.MarkWidth;
        _groundOffset = _parameters.GroundOffset;
        _minSqrDistance = _parameters.MinDistance * _parameters.MinDistance;
        _maxOpacity = _parameters.MaxOpacity;

        var go = new GameObject("Skidmarks");
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        _meshFilter = go.AddComponent<MeshFilter>();
        _meshRenderer = go.AddComponent<MeshRenderer>();

        _mesh = new Mesh();
        _mesh.MarkDynamic();

        _meshFilter.sharedMesh = _mesh;

        _meshRenderer.material = _parameters.Material;
        _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;
        _meshRenderer.lightProbeUsage = LightProbeUsage.Off;

        _marks = new MarkSection[_maxMarks];
        for (var i = 0; i < _maxMarks; i++)
            _marks[i] = new MarkSection();

        _vertices = new Vector3[_maxMarks * 4];
        _normals = new Vector3[_maxMarks * 4];
        _tangents = new Vector4[_maxMarks * 4];
        _colors = new Color32[_maxMarks * 4];
        _uvs = new Vector2[_maxMarks * 4];
        _triangles = new int[_maxMarks * 6];

        _markIndex = 0;
    }

    public void UpdateWheel(Rigidbody rb, WheelCollider wheel, bool skidNow)
    {
        if (rb == null || wheel == null)
            return;

        var state = GetWheelState(wheel);

        var grounded = wheel.GetGroundHit(out var hit);
        if (!grounded)
        {
            skidNow = false;
        }

        var dt = Time.fixedDeltaTime;
        var target = skidNow ? 1f : 0f;
        var speed = skidNow ? _parameters.FadeInSpeed : _parameters.FadeOutSpeed;

        state.Alpha = Mathf.MoveTowards(state.Alpha, target, speed * dt);

        if (grounded)
        {
            var offsetPos = GetSkidmarkPosition(hit.point, rb);
            state.LastGroundPos = offsetPos;
            state.HasLastGroundPos = true;
        }

        if (!skidNow && state.Alpha <= 0.001f)
        {
            state.Alpha = 0f;
            state.LastIndex = -1;
            state.WasSkidding = false;
            return;
        }

        if (state.Alpha <= 0.001f || !grounded)
        {
            state.WasSkidding = skidNow;
            return;
        }

        var opacity = Mathf.Clamp01(Mathf.Max(state.Alpha, _parameters.MinVisibleAlpha));

        if (skidNow && !state.WasSkidding && state.HasLastGroundPos)
            state.LastIndex = AddSkidMarkInternal(state.LastGroundPos, hit.normal, opacity, -1);

        var skidPoint = GetSkidmarkPosition(hit.point, rb);

        var newIndex = AddSkidMarkInternal(skidPoint, hit.normal, opacity, state.LastIndex);
        state.LastIndex = newIndex;
        state.WasSkidding = skidNow;

        if (_meshDirty)
            ApplyMesh();
    }

    private WheelState GetWheelState(WheelCollider wheel)
    {
        if (!_wheelStates.TryGetValue(wheel, out var state))
        {
            state = new WheelState();
            _wheelStates.Add(wheel, state);
        }

        return state;
    }

    private Vector3 GetSkidmarkPosition(Vector3 hitPoint, Rigidbody rb)
    {
        var maxOffset = _parameters.ForwardOffsetMax;
        if (maxOffset <= 0f || rb == null)
            return hitPoint;

        var velocity = rb.velocity;
        velocity.y = 0f;

        var speed = velocity.magnitude;
        if (speed < 0.01f)
            return hitPoint;

        var time = Mathf.Clamp01(speed / _parameters.ReferenceSpeedMps);
        var offsetDistance = Mathf.Lerp(0f, maxOffset, time);

        var dir = velocity / speed;

        return hitPoint + dir * offsetDistance;
    }

    private int AddSkidMarkInternal(Vector3 pos, Vector3 normal, float opacity, int lastIndex)
    {
        if (opacity <= 0f)
            return -1;

        if (opacity > 1f)
            opacity = 1f;

        var col = _baseColor;
        col.a = (byte)(opacity * 255 * _maxOpacity);

        return AddSkidMarkInternal(pos, normal, col, lastIndex);
    }

    private int AddSkidMarkInternal(Vector3 pos, Vector3 normal, Color32 colour, int lastIndex)
    {
        if (colour.a == 0)
            return -1;

        MarkSection lastSection = null;
        var distAndDirection = Vector3.zero;

        var newPos = pos + normal * _groundOffset;

        if (lastIndex != -1)
        {
            lastSection = _marks[lastIndex];
            distAndDirection = newPos - lastSection.Pos;

            if (distAndDirection.sqrMagnitude < _minSqrDistance)
                return lastIndex;

            if (distAndDirection.sqrMagnitude > _minSqrDistance * 10f)
            {
                lastIndex = -1;
                lastSection = null;
            }
        }

        var curSection = _marks[_markIndex];

        curSection.Pos = newPos;
        curSection.Normal = normal;
        curSection.Colour = colour;
        curSection.LastIndex = lastIndex;

        if (lastSection != null)
        {
            var xDirection = Vector3.Cross(distAndDirection, normal).normalized;

            curSection.PosL = curSection.Pos + xDirection * _markWidth * 0.5f;
            curSection.PosR = curSection.Pos - xDirection * _markWidth * 0.5f;
            curSection.Tangent = new Vector4(xDirection.x, xDirection.y, xDirection.z, 1f);

            if (lastSection.LastIndex == -1)
            {
                lastSection.Tangent = curSection.Tangent;
                lastSection.PosL = curSection.PosL;
                lastSection.PosR = curSection.PosR;
            }
        }

        UpdateSkidmarksMeshForIndex(_markIndex);

        var curIndex = _markIndex;
        _markIndex = (_markIndex + 1) % _maxMarks;

        _meshDirty = true;

        return curIndex;
    }

    private void UpdateSkidmarksMeshForIndex(int index)
    {
        var curr = _marks[index];
        if (curr.LastIndex == -1)
            return;

        var last = _marks[curr.LastIndex];

        var vi = index * 4;
        var ti = index * 6;

        _vertices[vi + 0] = last.PosL;
        _vertices[vi + 1] = last.PosR;
        _vertices[vi + 2] = curr.PosL;
        _vertices[vi + 3] = curr.PosR;

        _normals[vi + 0] = last.Normal;
        _normals[vi + 1] = last.Normal;
        _normals[vi + 2] = curr.Normal;
        _normals[vi + 3] = curr.Normal;

        _tangents[vi + 0] = last.Tangent;
        _tangents[vi + 1] = last.Tangent;
        _tangents[vi + 2] = curr.Tangent;
        _tangents[vi + 3] = curr.Tangent;

        _colors[vi + 0] = last.Colour;
        _colors[vi + 1] = last.Colour;
        _colors[vi + 2] = curr.Colour;
        _colors[vi + 3] = curr.Colour;

        _uvs[vi + 0] = new Vector2(0f, 0f);
        _uvs[vi + 1] = new Vector2(1f, 0f);
        _uvs[vi + 2] = new Vector2(0f, 1f);
        _uvs[vi + 3] = new Vector2(1f, 1f);

        _triangles[ti + 0] = vi + 0;
        _triangles[ti + 1] = vi + 2;
        _triangles[ti + 2] = vi + 1;

        _triangles[ti + 3] = vi + 2;
        _triangles[ti + 4] = vi + 3;
        _triangles[ti + 5] = vi + 1;
    }

    private void ApplyMesh()
    {
        _meshDirty = false;

        _mesh.vertices = _vertices;
        _mesh.normals = _normals;
        _mesh.tangents = _tangents;
        _mesh.colors32 = _colors;
        _mesh.uv = _uvs;
        _mesh.triangles = _triangles;

        if (!_boundsSet)
        {
            _mesh.bounds = new Bounds(Vector3.zero, new Vector3(10000f, 10000f, 10000f));
            _boundsSet = true;
        }

        _meshFilter.sharedMesh = _mesh;
    }
}