using UnityEngine;

public class PlayerInteract : MonoBehaviour {
    public float maxDistance = 3f; // Максимальная дистанция для взаимодействия
    public LayerMask interactableLayer; // Слой, на котором находятся предметы
    public Transform handTransform; // Ссылка на объект руки
    public float throwForce = 10f; // Сила броска

    private PickableItem currentItem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Проверяем нажатие клавиши E для взаимодействия
        {
            PerformRaycast();
        }

        if (Input.GetKeyDown(KeyCode.Q) && currentItem != null)
        {
            ThrowItem();
        }
    }

    void PerformRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Создаем луч из камеры
        RaycastHit hit; // Хранит информацию о столкновении

        if (Physics.Raycast(ray, out hit, maxDistance, interactableLayer)) // Проверяем, попадает ли луч в предмет
        {
            Debug.Log("Hit " + hit.collider.gameObject.name); // Выводим имя предмета
            PickableItem item = hit.collider.GetComponent<PickableItem>(); // Получаем компонент PickableItem

            if (item != null && !item.isPickedUp) // Проверяем, что предмет можно подобрать
            {
                item.handTransform = handTransform; // Устанавливаем ссылку на объект руки
                item.PickUp(); // Вызываем метод подбора
                currentItem = item; // Запоминаем текущий подобранный предмет
            }
        }
    }

    void ThrowItem()
    {
        if (currentItem != null)
        {
            Vector3 throwDirection = Camera.main.transform.forward; // Направление броска
            currentItem.Drop(throwDirection * throwForce); // Бросаем предмет
            currentItem = null; // Обнуляем текущий предмет
        }
    }
}