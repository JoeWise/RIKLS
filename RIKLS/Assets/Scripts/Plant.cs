using UnityEngine;
using System.Collections;

//This is the base class which is
//also known as the Parent class.
public class Plant : MonoBehaviour
{
    public int waterCount;
    public int hydrationGoal;
    public float maturity;
    public float matureRate = 0.05f;
    public float maturityGoal;
    public bool isMaturing = false;
    
    // Constructor
    public Plant()
    {
        // the amount of water this seed has collected
        waterCount = 0;
        // waterCount threshold. once reached, sproutPlant() will be called
        hydrationGoal = 100;

        maturity = 0.0f;
    }

    void Start()
    {

    }

    public virtual void Update()
    {
        maturityGoal = Mathf.Clamp01((float)waterCount / (float)hydrationGoal);

        //bool wasMaturing = isMaturing;

        if (maturity < maturityGoal)
        {
            isMaturing = true;
            maturity += matureRate * Time.deltaTime;

            if (maturity > maturityGoal)
            {
                maturity = maturityGoal;
            }
        }
        else
        {
            isMaturing = false;
        }
    }

    

    public int getWaterCount()
    {
        return waterCount;
    }

    public int getHydrationGoal()
    {
        return hydrationGoal;
    }

    public float getMaturity()
    {
        return maturity;
    }
}