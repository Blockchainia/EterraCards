using TMPro;
using UnityEngine;

public class Corner : MonoBehaviour
{
    [SerializeField] private Vector3 raycastOffset;
    [SerializeField] private Vector3 raycastVector;
    [SerializeField] private cornerName cornerName;
    [field: SerializeField] public SpriteRenderer Background { get; set; }
    [field: SerializeField] public SpriteRenderer Frame { get; set; }

    /// <summary>
    /// Throws a raycast and gets the card component.
    /// </summary>
    /// <returns>Card component if any or null if none.</returns>
    public Card GetTarget()
    {
        LayerMask hitLaterMask = 1 << LayerMask.NameToLayer("PlayerCard") | 1 << LayerMask.NameToLayer("EnemyCard");
        hitLaterMask |= 1 << LayerMask.NameToLayer("Wall");
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + this.raycastOffset, this.raycastVector, 1f, hitLaterMask);
        return hit.transform?.gameObject.GetComponent<Card>();
    }

    /// <summary>
    /// Used to see in the editor the raycast rays.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position + this.raycastOffset, this.raycastVector);
    }
}

public enum cornerName
{
    NORTH_EAST = 0,
    NORTH_WEST = 1, 
    SOUTH_WEST = 2,
    SOUTH_EAST = 3
}
