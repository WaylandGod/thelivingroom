﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public PlayerSettings settings;
    public GameObject playerAttackPrefab;

    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D _boxCollider2D;
    private Vector2 _lastMovement = Vector2.down;
    private Vector2 _lastDirection = Vector2.down;
    private float _lastFire = 0;

    [HideInInspector]
    public bool movementSettingsFoldout = true;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        var movement = new Vector2(Input.GetAxis("Horizontal") * settings.speed, Input.GetAxis("Vertical") * settings.speed);
        var direction = GetDirection(movement);

        //_animator.SetFloat("Horizontal", movement.x);
        //_animator.SetFloat("Vertical", movement.y);
        //_animator.SetFloat("Magnitude", movement.magnitude);

        _rigidbody2D.velocity = movement;

        if (Input.GetAxis("Jump") > 0 && CanFire())
        {
            _animator.SetTrigger("Attack");
            _lastFire = Time.timeSinceLevelLoad;

            var b = _boxCollider2D.bounds;
            var bottomLeft = _boxCollider2D.bounds.min;
            var topRight = _boxCollider2D.bounds.max;
            var bottomRight = new Vector3(topRight.x, bottomLeft.y, transform.position.z);
            var topLeft = new Vector3(bottomLeft.x, topRight.y, transform.position.z);

            GameObject meshObject = new GameObject("mesh");
            meshObject.transform.parent = transform;

           var whooshAttack = Instantiate(playerAttackPrefab, meshObject.transform);
            whooshAttack.GetComponent<ParticleSystem>().Play();

            var mesh = new Mesh();
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
            meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            meshObject.AddComponent<PlayerAttackBehaviour>();
            
            mesh.Clear();

            if (_lastDirection == Vector2.up)
            {
                var topTopLeft = new Vector3(topLeft.x, topLeft.y + settings.range, topLeft.z);
                var topTopRight = new Vector3(topRight.x, topRight.y + settings.range, topRight.z);

                mesh.vertices = new Vector3[] {
                    topLeft,
                    topTopLeft,
                    topTopRight,
                    topRight
                };
            }
            else if (_lastDirection == Vector2.left)
            {
                var bottomLeftLeft = new Vector3(bottomLeft.x - settings.range, bottomLeft.y, bottomLeft.z);
                var topLeftLeft = new Vector3(topLeft.x - settings.range, topLeft.y, topLeft.z);

                mesh.vertices = new Vector3[]
                {
                    bottomLeft,
                    bottomLeftLeft,
                    topLeftLeft,
                    topLeft
                };
            }
            else if (_lastDirection == Vector2.right)
            {
                var topRightRight = new Vector3(topRight.x + settings.range, topRight.y, topRight.z);
                var bottomRightRight = new Vector3(bottomRight.x + settings.range, bottomRight.y, bottomRight.z);

                mesh.vertices = new Vector3[]
                {
                    topRight,
                    topRightRight,
                    bottomRightRight,
                    bottomRight
                };
            }
            else if (_lastDirection == Vector2.down)
            {
                var bottomBottomRight = new Vector3(bottomRight.x, bottomRight.y - settings.range, bottomRight.z);
                var bottomBottomLeft = new Vector3(bottomLeft.x, bottomLeft.y - settings.range, bottomLeft.z);

                mesh.vertices = new Vector3[]
                {
                    bottomRight,
                    bottomBottomRight,
                    bottomBottomLeft,
                    bottomLeft
                };
            }

            mesh.triangles = new int[] { 0, 1, 3, 3, 1, 2 };
            mesh.RecalculateNormals();

            var boxCollider = meshObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;

            Destroy(meshObject, .3f);
        }

        if (movement != Vector2.zero)
        {
            _lastDirection = direction;
            _lastMovement = movement;
        }
    }

    private bool CanFire()
    {
        return Time.timeSinceLevelLoad - _lastFire > settings.cooldown;
    }

    private void OnGUI()
    {
        GUI.TextArea(new Rect(0, 0, 200, 40), "Hoz: " + System.Math.Round(Input.GetAxis("Horizontal"), 3).ToString("#.###") + " ||| Vert: " + System.Math.Round(Input.GetAxis("Vertical"), 3).ToString("#.###") + " ||| Cooldown: " + CanFire());
    }

    private Vector2 GetDirection(Vector2 v)
    {
        var compass = new Vector2[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        var maxDot = -Mathf.Infinity;
        var ret = Vector3.zero;

        foreach (var dir in compass)
        {
            var t = Vector3.Dot(v, dir);
            if (t > maxDot)
            {
                ret = dir;
                maxDot = t;
            }
        }

        return ret;
    }
}