using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Health : MonoBehaviour
{
   
    public int maxHealth = 200;  
    private int currentHealth;  

    void Start()
    {
        // Initialize health to maximum at the start of the game
        currentHealth = maxHealth;
    }

    
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // Ensure the health doesn't go below zero
        if (currentHealth <= 0)
        {
            Die();
        }

        
        Debug.Log("Player Health: " + currentHealth);
    }

    void Die()
    {
        Debug.Log("Player has died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
