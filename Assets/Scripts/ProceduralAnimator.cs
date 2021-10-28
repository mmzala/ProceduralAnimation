using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimator : MonoBehaviour
{
    public Leg[] legs;
    public Body body;

    private List<Leg> legGroupOne;
    private List<Leg> legGroupTwo;

    void Start()
    {
        SortLegs();
        if(body != null) body.Init(legs);
    }

    void LateUpdate()
    {
        if(legs.Length > 0) UpdateLegs();
        if(body != null) body.UpdateBody();
    }

    void UpdateLegs()
    {
        if(!AreLegsMoving(legGroupOne))
        {
            MoveLegs(legGroupTwo);
        }
        if(!AreLegsMoving(legGroupTwo))
        {
            MoveLegs(legGroupOne);
        }
    }

    void MoveLegs(List<Leg> legs)
    {
        foreach(Leg leg in legs)
        {
            leg.TryMove();
        }
    }

    bool AreLegsMoving(List<Leg> legs)
    {
        foreach(Leg leg in legs)
        {
            if(leg.IsMoving())
            {
                return true;
            }
        }
        return false;
    }

    void SortLegs()
    {
        legGroupOne = new List<Leg>();
        legGroupTwo = new List<Leg>();

        foreach (Leg leg in legs)
        {
            switch(leg.legGroup)
            {
                case Leg.LegGroup.group1:
                    legGroupOne.Add(leg);
                    break;

                case Leg.LegGroup.group2:
                    legGroupTwo.Add(leg);
                    break;
            }
        }
    }
}
