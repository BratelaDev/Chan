using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public string itemName = "Item";
    public bool isPickedUp = false;
    public Transform handTransform; // Ссылка на объект руки
    private Rigidbody rb; // Ссылка на Rigidbody предмета

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Получаем компонент Rigidbody
    }

    public void PickUp()
    {
        isPickedUp = true;
        Debug.Log("Player picked up " + itemName);

        // Перемещение предмета к руке
        transform.SetParent(handTransform); // Устанавливаем родителя на объект руки
        transform.localPosition = Vector3.zero; // Устанавливаем позицию относительно руки
        transform.localRotation = Quaternion.identity; // Устанавливаем вращение в ноль

        rb.isKinematic = true; // Делаем Rigidbody кинематическим, чтобы избежать влияния физики
        gameObject.SetActive(true); // Убедитесь, что объект активен
    }

    public void Drop(Vector3 force) // Метод для броска предмета
    {
        isPickedUp = false;
        transform.SetParent(null); // Убираем родительский объект
        rb.isKinematic = false; // Включаем физику
        rb.AddForce(force, ForceMode.Impulse); // Применяем силу к предмету
    }
}
