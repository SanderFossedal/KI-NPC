using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcLookAt : MonoBehaviour
{
    public Transform player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            // Beregn retningen mot spilleren
            Vector3 direction = player.position - transform.position;

            // Ignorer høydeforskjellen ved å sette Y-komponenten av retningen til 0
            direction.y = 0;

            // Sjekk at vi ikke har en nullvektor før vi roterer (hvis NPC og spiller er på nøyaktig samme XZ-posisjon)
            if (direction != Vector3.zero)
            {
                // Opprett en rotasjon som ser i den justerte retningen
                Quaternion newRotation = Quaternion.LookRotation(direction);

                // Oppdater NPC-ens rotasjon
                transform.rotation = newRotation;
            }
        }
    }
}
