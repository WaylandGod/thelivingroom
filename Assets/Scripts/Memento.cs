﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(ParticleSystem))]
public class Memento : MonoBehaviour
{
    [Range(0, 4)]
    public float timeBeforeTeleport = 1;
    [Range(0, 10)]
    public float resetTime = 1;
    public int theFeels;
    public MementoState mementoState;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;
    private Transform _originalRoot;
    private ParticleSystem _particleSystem;
    private Mover _carrier;

    private bool _triggered = false;

    public void PickUp(Mover mover)
    {
        mementoState = MementoState.PickedUp;
        _carrier = mover;
        transform.SetParent(mover.transform, false);
        transform.rotation = new Quaternion(0, 0, 0, 90);
    }

    public void Drop()
    {
        mementoState = MementoState.Dropped;
        _carrier = null;
        transform.SetParent(_originalRoot, true);
        transform.rotation = _originalRotation;
        transform.localScale = _originalScale;
        _triggered = false;
    }

    public void ResetPosition()
    {
        mementoState = MementoState.Idle;
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
        transform.localScale = _originalScale;
    }

    private void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _originalScale = transform.localScale;
        _originalRoot = transform.parent;
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (_carrier != null)
        {
            transform.Rotate(Vector2.up);
            transform.localPosition = new Vector3(0, 0, 2);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_triggered && collision.tag == "Player")
        {
            switch (mementoState)
            {
                case MementoState.Idle:
                    break;
                case MementoState.PickedUp:
                    //_triggered = true;
                    //Drop();
                    break;
                case MementoState.Dropped:
                    _triggered = true;
                    StartCoroutine(PlayParticalsAndReset());
                    break;
                default:
                    break;
            }
        }
    }

    private IEnumerator PlayParticalsAndReset()
    {
        _particleSystem.Play();
        yield return new WaitForSeconds(timeBeforeTeleport);
        ResetPosition();
        _particleSystem.Play();

        _triggered = false;
    }

    public enum MementoState
    {
        Idle,
        PickedUp,
        Dropped,
    }
}