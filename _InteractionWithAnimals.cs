using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems;

public class _InteractionWithAnimals : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int x;
    public int y;
    public bool bonus = false;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        FindFirstObjectByType<_Board>().SelectAnimal(gameObject);
        transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1f, 1f, 1f); 
    }
}
