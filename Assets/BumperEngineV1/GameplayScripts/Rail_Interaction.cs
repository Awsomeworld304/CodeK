using UnityEngine;
using System.Collections;
using SplineMesh;

public class Rail_Interaction : MonoBehaviour {

    public Rail rail { get; set; }
    PlayerBhysics Player;
    ActionManager Actions;
    public Animator CharacterAnimator;

    Quaternion CharRot;
    public Spline RailSpline;

    float ClosestSample;



    void Awake()
    {
        Actions = GetComponent<ActionManager>();
        Player = GetComponent<PlayerBhysics>();
    }

    private void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.CompareTag("Rail"))
        {
            RailSpline = col.gameObject.GetComponentInParent<Spline>();
            if (RailSpline != null)
            {
                Transform ColPos = GetCollisionPoint(col);
                float Range = GetClosestPos(ColPos);
                if (!Actions.Action05.OnRail)
                {
                    Actions.Action05.InitialEvents(Range, RailSpline.transform);
                    Actions.ChangeAction(5);
                }
            }

        }
        
    }

    public Transform GetCollisionPoint(Collision col)
    {
        Transform CollisionPoint = transform;
        foreach (ContactPoint contact in col.contacts)
        {
            //Set Middle Point
            Vector3 pointSum = Vector3.zero;
            for (int i = 0; i < col.contacts.Length; i++)
            {
                pointSum = pointSum + col.contacts[i].point;
            }
            pointSum = pointSum / col.contacts.Length;
            CollisionPoint.position = pointSum;
        }
        return CollisionPoint;
    }


    float GetClosestPos(Transform ColPos)
    {
        //spline.nodes.Count - 1

        //Debug.Log(ColPos.position);
        float CurrentDist = 9999999f;
        for (float n = 0; n < RailSpline.Length; n += Time.deltaTime * 10f)
        {
            float dist = ((RailSpline.GetSampleAtDistance(n).location + RailSpline.transform.position) - ColPos.position).sqrMagnitude;
            if (dist < CurrentDist)
            {
                CurrentDist = dist;
                ClosestSample = n;
            }

        }
        return ClosestSample;
    }
}
