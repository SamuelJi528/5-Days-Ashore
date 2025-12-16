using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerProximityDetector : MonoBehaviour
{
    public TextMeshProUGUI interactionPromptText;   // UI text for showing interaction hints
    public float grabDelay = 0.4f;                  

    private List<ResourceNode> nearbyNodes = new List<ResourceNode>(); // All resource nodes in trigger range
    private ResourceNode focusedNode = null;        // The node the player is currently focusing on
    private PlayerControl playerControl;            
    private bool isGathering = false;              

    void Start()
    {
        // Get the player control script on this object
        playerControl = GetComponent<PlayerControl>();
    }

    void OnTriggerEnter(Collider other)
    {
        // When entering a trigger, check if it has a resource node
        ResourceNode node = other.GetComponent<ResourceNode>();
        if (node != null) nearbyNodes.Add(node);
    }

    void OnTriggerExit(Collider other)
    {
        // When leaving a trigger, remove that node from the list
        ResourceNode node = other.GetComponent<ResourceNode>();
        if (node != null) nearbyNodes.Remove(node);
    }

    void Update()
    {
        // Always pick the closest node to interact with
        FindClosestNode();

        if (focusedNode != null && !isGathering)
        {
            // Decide what prompt to show based on interaction type
            if (focusedNode.interactionType == ResourceInteractionType.Drink)
            {
                DisplayPrompt("Press E to Drink");
            }
            else
            {
                var drop = focusedNode.GetComponent<ItemDrop>();
                if (drop != null && drop.droppedItem != null)
                    DisplayPrompt(drop.droppedItem.itemName);
                else
                    DisplayPrompt("");
            }

            // Press E to interact with the current node
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartCoroutine(PerformGathering());
            }
        }
        else
        {
            HidePrompt();
        }
    }

    IEnumerator PerformGathering()
    {
        isGathering = true;

        // Make sure node is valid and the player has the right tool
        if (focusedNode == null || !focusedNode.HasCorrectTool())
        {
            isGathering = false;
            yield break;
        }

        string animName = focusedNode.animationTrigger;
        float delay = focusedNode.gatherDelay;

        // Play the gather/interaction animation
        if (playerControl != null)
            playerControl.PlayInteractionAnim(animName);

        // Wait for the gather delay before giving the resource
        yield return new WaitForSeconds(delay);

        // Actually gather the resource 
        if (focusedNode != null)
            focusedNode.GatherResource();

        isGathering = false;
    }

    private void FindClosestNode()
    {
        // Clean up any nodes that were destroyed
        nearbyNodes.RemoveAll(node => node == null);

        focusedNode = null;
        float closestDistance = float.MaxValue;

        // Pick the closest node in range
        foreach (ResourceNode node in nearbyNodes)
        {
            float distance = Vector3.Distance(transform.position, node.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                focusedNode = node;
            }
        }
    }

    private void DisplayPrompt(string text)
    {
        // Show a text prompt on screen
        if (interactionPromptText != null)
        {
            interactionPromptText.text = text;
            interactionPromptText.gameObject.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        // Hide the interaction prompt
        if (interactionPromptText != null)
            interactionPromptText.gameObject.SetActive(false);
    }
}
