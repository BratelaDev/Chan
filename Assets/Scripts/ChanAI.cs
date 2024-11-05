using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChanAI: MonoBehaviour
{
    public Transform player; // Игрок
    public AudioSource playerFootsteps;
    public PlayerControler playerMovement; 

    [Header("Detection Settings")]
    public float hearingRange = 10f; // Дальность слуха
    public float visionRange = 15f; // Дальность зрения
    public float fieldOfViewAngle = 45f; // Угол зрения

    [Header("Behavior Settings")]
    public float waitTime = 2f; // Время ожидания при реакции на звук
    public float reactionDelay = 1f; // Задержка перед реакцией на звук
    public float rotationSpeed = 5f; // Скорость поворота
    public float chaseSpeed = 5f; // Скорость погони
    public float normalSpeed = 2f; // Нормальная скорость

    private NavMeshAgent agent; // Агент навигации
    private Animator animator; // Аниматор
    private Vector3 originalPosition; // Исходная позиция
    private Vector3 lastHeardPosition; // Последняя слышанная позиция
    private bool isCheckingArea = false; // Проверка области
    private bool isReacting = false; // Реакция на звук
    private bool isChasing = false; // Погоня
    private bool heardSteps = false; // Ушал ли шаги
    private bool playerInSight = false; // Игрок в поле зрения

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        originalPosition = transform.position; // Запоминаем исходную позицию
        agent.speed = normalSpeed; // Устанавливаем нормальную скорость
    }

    void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
            SetAnimationState(isRunning: true, isWalking: false);
        }
        else
        {
            CheckForSound(); // Проверяем звук шагов
            CheckForPlayerSight(); // Проверяем видимость игрока

            UpdateMovementAnimation();

            // Возвращаемся на исходную позицию, если враг не слышит шагов
            if (!heardSteps && !isCheckingArea && !isReacting && IsFarFromOriginalPosition())
            {
                ReturnToOriginalPosition();
            }
        }
    }

    private void CheckForSound()
    {
        // Проверяем, слышны ли шаги игрока
        if (playerFootsteps.isPlaying && IsPlayerInHearingRange() && !playerMovement.IsCrouching())
        {
            heardSteps = true;
            lastHeardPosition = player.position; // Запоминаем позицию, откуда слышен звук
            Debug.Log("Звук шагов игрока услышан.");

            if (!isReacting && !isChasing)
            {
                StartCoroutine(ReactToSound()); // Реакция на звук
            }
        }
    }

    private bool IsPlayerInHearingRange()
    {
        // Проверка на расстояние
        return Vector3.Distance(transform.position, player.position) < hearingRange;
    }

    private void CheckForPlayerSight()
    {
        if (CanSeePlayer())
        {
            if (!isChasing)
            {
                Debug.Log("Игрок замечен в поле зрения — начинаем погоню!");
                StartChase(); // Начинаем погоню
            }
            playerInSight = true; // Игрок в поле зрения
        }
        else
        {
            // Если игрок выходит из поля зрения, продолжаем погоню
            if (isChasing && playerInSight)
            {
                Debug.Log("Игрок вне зоны видимости, но продолжаем погоню.");
            }
        }
    }

    private bool CanSeePlayer()
    {
        // Проверка видимости игрока
        if (Vector3.Distance(transform.position, player.position) < visionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle < fieldOfViewAngle / 2f)
            {
                return RaycastToPlayer(directionToPlayer); // Проверка на преграды
            }
        }
        return false;
    }

    private bool RaycastToPlayer(Vector3 direction)
    {
        // Raycast для проверки препятствий
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, visionRange))
        {
            bool hitPlayer = hit.collider.transform == player;
            Debug.Log("Результат Raycast до игрока: " + hitPlayer);
            return hitPlayer; // Возвращаем результат проверки
        }
        return false;
    }

    private void StartChase()
    {
        isChasing = true; // Враг в состоянии погони
        agent.speed = chaseSpeed; // Устанавливаем скорость погони
        agent.SetDestination(player.position); // Устанавливаем цель
    }

    private void ChasePlayer()
    {
        // Обновляем цель погони
        agent.SetDestination(player.position);
    }

    private void StopChase()
    {
        Debug.Log("Тян прекращает погоню!");
        isChasing = false; // Враг больше не в погоне
        SetAnimationState(isRunning: false, isWalking: false); // Останавливаем анимацию
        agent.speed = normalSpeed; // Возвращаемся к нормальной скорости
    }

    private void ReturnToOriginalPosition()
    {
        agent.SetDestination(originalPosition); // Возвращаемся на исходную позицию
        Debug.Log("Возвращается на место");
    }

    private bool IsFarFromOriginalPosition()
    {
        return Vector3.Distance(transform.position, originalPosition) > 0.5f; // Проверка, далеко ли враг от исходной позиции
    }

    private IEnumerator ReactToSound()
    {
        isReacting = true; // Враг реагирует на звук
        SetAnimationState(isWalking: true, isRunning: false); // Устанавливаем анимацию ходьбы

        yield return StartCoroutine(TurnTowards(lastHeardPosition)); // Поворачиваемся к звуку

        agent.isStopped = true; // Останавливаем агента
        yield return new WaitForSeconds(reactionDelay); // Ждем реакцию

        agent.isStopped = false; // Возобновляем движение
        agent.SetDestination(lastHeardPosition); // Двигаемся к звуку

        while (agent.pathPending || agent.remainingDistance > 1f)
        {
            yield return null; // Ждем, пока не дойдём до цели
        }

        StartCoroutine(CheckArea()); // Проверяем область
        isReacting = false; // Враг больше не реагирует
    }

    private IEnumerator TurnTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed); // Поворачиваемся к звуку
            yield return null;
        }
    }

    private IEnumerator CheckArea()
    {
        isCheckingArea = true; // Начинаем проверку области
        SetAnimationState(isWalking: true, isRunning: false); // Устанавливаем анимацию ходьбы

        yield return new WaitForSeconds(waitTime); // Ждем во время проверки

        heardSteps = false; // Сбрасываем состояние слышимости
        isCheckingArea = false; // Заканчиваем проверку
        SetAnimationState(isWalking: false, isRunning: false); // Останавливаем анимацию
        ReturnToOriginalPosition(); // Возвращаемся на исходную позицию
    }

    private void UpdateMovementAnimation()
    {
        // Устанавливаем анимацию ходьбы, если враг движется
        bool isMoving = agent.velocity.magnitude > 0.1f;
        SetAnimationState(isWalking: isMoving && !isChasing, isRunning: isChasing);
    }

    private void SetAnimationState(bool isWalking, bool isRunning)
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hearingRange); // Отображаем зону слуха

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange); // Отображаем зону зрения
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Player")
        {
            SceneManager.LoadScene("Test");
        }
    }
}
