using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !active)
        {
            StartCoroutine(Attack1());
            active = true;
            Debug.Log("Performed 0");
        }
    }

    private IEnumerator Attack1()
    {
        int before = 2;
        int after = 600;
        bool nextAttack = false;

        yield return new WaitForSecondsRealtime(before);

        while (after >= 0)
        {
            Debug.Log("In While Attack 1");
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Attack2());
                Debug.Log("Performed 1");
                yield return null;
            }

            after--;
        }

        active = false;
    }
    
    private IEnumerator Attack2()
    {
        int before = 2;
        int after = 600;
        bool nextAttack = false;

        yield return new WaitForSecondsRealtime(before);

        while (after >= 0)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Performed 2");
                yield return null;
            }

            after--;
        }
        
        active = false;
    }
}
