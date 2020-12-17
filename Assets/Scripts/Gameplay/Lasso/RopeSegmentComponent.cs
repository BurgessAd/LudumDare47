using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class RopeSegmentComponent : MonoBehaviour
{



    private Rigidbody m_Body;

    private RopeSegmentComponent NextSegment;
    private RopeSegmentComponent PreviousSegment;
}
